using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityWebBrowser.Core.Popups;
using UnityWebBrowser.Logging;
using UnityWebBrowser.Shared;
using UnityWebBrowser.Shared.Core;
using UnityWebBrowser.Shared.Events;
using UnityWebBrowser.Shared.Popups;
using UnityWebBrowser.Shared.ReadWriters;
using VoltRpc.Communication;

namespace UnityWebBrowser.Core
{
    /// <summary>
    ///     Handles the RPC methods and two-way communication between the UWB engine and Unity
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    internal class WebBrowserCommunicationsManager : IEngineControls, IClientControls, IDisposable
    {
        private static ProfilerMarker sendEventMarker = new("UWB.SendEvent");
        public readonly WebBrowserClient client;

        private readonly IEngineControls engineProxy;

        public readonly IWebBrowserLogger logger;
        
        private readonly object threadLock;
        private readonly SynchronizationContext unityThread;

        private readonly Host ipcHost;
        public readonly Client ipcClient;
        
        public readonly PixelsEventTypeReader pixelsEventTypeReader;

        /// <summary>
        ///     Creates a new <see cref="WebBrowserCommunicationsManager" /> instance
        /// </summary>
        /// <param name="browserClient"></param>
        public WebBrowserCommunicationsManager(WebBrowserClient browserClient)
        {
            threadLock = new object();
            unityThread = SynchronizationContext.Current;

            logger = browserClient.logger;
            client = browserClient;

            ipcClient = browserClient.communicationLayer.CreateClient();
            ipcHost = browserClient.communicationLayer.CreateHost();

            WebBrowserPopupService popupService = new(this);

            ReadWriterUtils.AddBaseTypeReadWriters(ipcHost.TypeReaderWriterManager);
            ipcHost.AddService<IClientControls>(this);
            ipcHost.AddService<IPopupEngineControls>(popupService);

            ReadWriterUtils.AddBaseTypeReadWriters(ipcClient.TypeReaderWriterManager);

            pixelsEventTypeReader = new PixelsEventTypeReader(browserClient.nextTextureData);
            ipcClient.TypeReaderWriterManager.AddType(pixelsEventTypeReader);
            
            ipcClient.AddService<IEngineControls>();
            ipcClient.AddService<IPopupClientControls>();
            engineProxy = new EngineControls(ipcClient);
        }

        /// <summary>
        ///     Is our client connected to the UWB engine
        /// </summary>
        public bool IsConnected => ipcClient.IsConnected;

        public void Dispose()
        {
            lock (threadLock)
            {
                ipcHost?.Dispose();
                ipcClient?.Dispose();
            }
        }

        public PixelsEvent GetPixels()
        {
            using (sendEventMarker.Auto())
            {
                lock (threadLock)
                {
                    return engineProxy.GetPixels();
                }
            }
        }

        public void Shutdown()
        {
            lock (threadLock)
            {
                engineProxy.Shutdown();
            }
        }

        public void SendKeyboardEvent(KeyboardEvent keyboardEvent)
        {
            ExecuteTask(() => engineProxy.SendKeyboardEvent(keyboardEvent));
        }

        public void SendMouseMoveEvent(MouseMoveEvent mouseMoveEvent)
        {
            ExecuteTask(() => engineProxy.SendMouseMoveEvent(mouseMoveEvent));
        }

        public void SendMouseClickEvent(MouseClickEvent mouseClickEvent)
        {
            ExecuteTask(() => engineProxy.SendMouseClickEvent(mouseClickEvent));
        }

        public void SendMouseScrollEvent(MouseScrollEvent mouseScrollEvent)
        {
            ExecuteTask(() => engineProxy.SendMouseScrollEvent(mouseScrollEvent));
        }

        public Vector2 GetScrollPosition()
        {
            using (sendEventMarker.Auto())
            {
                lock (threadLock)
                {
                    return engineProxy.GetScrollPosition();
                }
            }
        }

        public void GoForward()
        {
            ExecuteTask(() => engineProxy.GoForward());
        }

        public void GoBack()
        {
            ExecuteTask(() => engineProxy.GoBack());
        }

        public void Refresh()
        {
            ExecuteTask(() => engineProxy.Refresh());
        }

        public void LoadUrl(string url)
        {
            ExecuteTask(() => engineProxy.LoadUrl(url));
        }

        public void LoadHtml(string html)
        {
            ExecuteTask(() => engineProxy.LoadHtml(html));
        }

        public void ExecuteJs(string js)
        {
            ExecuteTask(() => engineProxy.ExecuteJs(js));
        }

        public void Resize(Resolution resolution)
        {
            ExecuteTask(() => engineProxy.Resize(resolution));
        }

        public void Connect()
        {
            ipcClient.Connect();
        }

        public void Listen()
        {
            ipcHost.StartListening();
        }

        private void ExecuteOnUnity(Action action, [CallerMemberName] string memberName = "")
        {
            unityThread.Post(_ =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occured in {memberName}! {ex}");
                }
            }, null);
        }

        internal void ExecuteTask(Action action, [CallerMemberName] string memberName = "")
        {
            if (!IsConnected)
                return;

            UniTask.Run(() =>
            {
                sendEventMarker.Begin();
                try
                {
                    lock (threadLock)
                    {
                        action.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occured while executing task {memberName}! {ex}");
                }

                sendEventMarker.End();
            });
        }

        #region Client Events

        public void UrlChange(string url)
        {
            ExecuteOnUnity(() => client.InvokeUrlChanged(url));
        }

        public void LoadStart(string url)
        {
            ExecuteOnUnity(() => client.InvokeLoadStart(url));
        }

        //TODO: Remove
        public void OnPopup(string url)
        {
            //ExecuteOnUnity(() => client.InvokeOnPopup(url));
        }

        public void LoadFinish(string url)
        {
            ExecuteOnUnity(() => client.InvokeLoadFinish(url));
        }

        public void TitleChange(string title)
        {
            ExecuteOnUnity(() => client.InvokeTitleChange(title));
        }

        public void ProgressChange(double progress)
        {
            ExecuteOnUnity(() => client.InvokeLoadProgressChange(progress));
        }

        public void Fullscreen(bool fullScreen)
        {
            ExecuteOnUnity(() => client.InvokeFullscreen(fullScreen));
        }

        public void Ready()
        {
            client.EngineReady().Forget();
        }

        #endregion
    }
}