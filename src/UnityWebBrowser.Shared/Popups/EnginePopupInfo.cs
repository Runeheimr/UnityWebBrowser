using System;

namespace UnityWebBrowser.Shared.Popups;

/// <summary>
///     Base controls for a popup
/// </summary>
public abstract class EnginePopupInfo : IDisposable
{
    /// <summary>
    ///     Creates a new <see cref="EnginePopupInfo"/>
    /// </summary>
    protected EnginePopupInfo()
    {
        PopupGuid = Guid.NewGuid();
    }

    /// <summary>
    ///     Creates a new <see cref="EnginePopupInfo"/>
    /// </summary>
    internal EnginePopupInfo(Guid guid)
    {
        PopupGuid = guid;
    }

    /// <summary>
    ///     Unique <see cref="Guid"/> related to this popup
    /// </summary>
    public readonly Guid PopupGuid;

    /// <summary>
    ///     Execute JS in this popup
    /// </summary>
    /// <param name="js"></param>
    public abstract void ExecuteJs(string js);

    /// <inheritdoc />
    public virtual void Dispose()
    {
    }
}