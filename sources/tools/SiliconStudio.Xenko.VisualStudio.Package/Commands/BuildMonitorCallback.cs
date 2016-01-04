// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SiliconStudio.Xenko.VisualStudio.Commands
{
    public class BuildMonitorCallback : MarshalByRefObject, IBuildMonitorCallback
    {
        private IVsOutputWindowPane buildPane;

        public BuildMonitorCallback()
        {
            var outputWindow = (IVsOutputWindow)Package.GetGlobalService(typeof(SVsOutputWindow));

            // Get Output pane
            Guid buildPaneGuid = VSConstants.GUID_BuildOutputWindowPane;
            outputWindow.GetPane(ref buildPaneGuid, out buildPane);
        }

        public override object InitializeLifetimeService()
        {
            // Infinite lifetime
            return null;
        }

        public void Message(string type, string module, string text)
        {
            if (buildPane != null)
                buildPane.OutputString(string.Format("[BuildEngine] {0}: {1}\r\n", type[0], text));
        }
    }
}