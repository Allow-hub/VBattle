using System;
using System.Collections.Generic;

namespace TechC.VBattle.Core.Window
{
    public static class WindowRegistry
    {
        public static readonly Dictionary<IntPtr, NativeWindow> ByHwnd = new();
    }
}
