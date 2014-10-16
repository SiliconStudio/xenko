// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Forms;

namespace SiliconStudio.LauncherApp
{
    public static class ControlHelper
    {
        public static void InvokeSafe<T>(this T control, Action action) where T : Control
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (control.InvokeRequired)
            {
                control.Invoke((MethodInvoker)delegate () { action(); });
            }
            else
            {
                action();
            }            
        }
    }
}