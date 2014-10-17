// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using EnvDTE80;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    class BuildMonitorCallback : MarshalByRefObject, IBuildMonitorCallback
    {
        private EnvDTE.OutputWindowPane buildPane;

        public BuildMonitorCallback(DTE2 dte)
        {
            var vsOutputWindow = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            var outputWindow = (EnvDTE.OutputWindow)vsOutputWindow.Object;
            buildPane = outputWindow.OutputWindowPanes.Item("Build");
        }

        public override object InitializeLifetimeService()
        {
            // Infinite lifetime
            return null;
        }

        public void Message(string type, string module, string text)
        {
            buildPane.OutputString(string.Format("[BuildEngine] {0}: {1}\r\n", type[0], text));
        }
    }
}