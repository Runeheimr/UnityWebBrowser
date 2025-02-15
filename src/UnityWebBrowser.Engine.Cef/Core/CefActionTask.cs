﻿using System;
using Xilium.CefGlue;

namespace UnityWebBrowser.Engine.Cef.Core;

public class CefActionTask : CefTask
{
    private Action action;

    public CefActionTask(Action action)
    {
        this.action = action;
    }

    protected override void Execute()
    {
        action();
        action = null;
    }
}