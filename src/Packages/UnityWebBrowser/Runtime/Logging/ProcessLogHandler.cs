using System;
using System.Diagnostics;
using Newtonsoft.Json;
using UnityEngine.Scripting;
using UnityWebBrowser.Core;
using UnityWebBrowser.Shared;

namespace UnityWebBrowser.Logging
{
    /// <summary>
    ///     Handles UWB logs
    /// </summary>
    [Preserve]
    public class ProcessLogHandler
    {
        private readonly IWebBrowserLogger logger;

        public event Action<string> OnProcessOutputLog;

        public event Action<string> OnProcessErrorLog; 

        internal ProcessLogHandler(WebBrowserClient client)
        {
            logger = client.logger;
        }

        internal void HandleErrorProcessLog(object sender, DataReceivedEventArgs e)
        {
            if(e.Data == null)
                return;
            
            OnProcessErrorLog?.Invoke(e.Data);
        }
        
        internal void HandleOutputProcessLog(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            try
            {
                JsonLogStructure logStructure = ReadJsonLog(e.Data);

                if (logStructure.Level is LogSeverity.Debug or LogSeverity.Info)
                    logger.Debug(logStructure.Message);
                else if (logStructure.Level == LogSeverity.Warn)
                    logger.Warn(logStructure.Message);
                else if (logStructure.Level == LogSeverity.Error)
                    logger.Error(logStructure.Message);
                else if (logStructure.Level == LogSeverity.Error && logStructure.Exception != null)
                    logger.Error($"{logStructure.Exception}\n{logStructure.Exception}");
                else if (logStructure.Level == LogSeverity.Fatal)
                    logger.Error(logStructure.Message);
                else if (logStructure.Level == LogSeverity.Fatal && logStructure.Exception != null)
                    logger.Error(logStructure.Message);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occured with processing a log event from the UWB engine! {ex}");
            }
            
            OnProcessOutputLog?.Invoke(e.Data);
        }

        /// <summary>
        ///     Reads json and deserializes it as <see cref="JsonLogStructure" />
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        internal static JsonLogStructure ReadJsonLog(string json)
        {
            JsonLogStructure logStructure = JsonConvert.DeserializeObject<JsonLogStructure>(json);
            return logStructure;
        }
    }
}