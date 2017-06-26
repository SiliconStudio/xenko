// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.VisualStudio.Commands
{
    public class BuildMonitorCallback : MarshalByRefObject, IBuildMonitorCallback
    {
        private const string MessageHeader = "[Xenko.AssetCompiler]";
        private readonly IVsOutputWindowPane buildPane;

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
            buildPane?.OutputString($"{MessageHeader} {type}: {text}\r\n");
        }

        public static bool FilterMessage(string message, out string messageType)
        {
            messageType = null;

            if (!message.StartsWith(MessageHeader))
                return false;

            var typeStartIndex = MessageHeader.Length + 1;
            var typeEndIndex = message.IndexOf(":", typeStartIndex);
            if (typeEndIndex == -1)
                return false;

            messageType = message.Substring(typeStartIndex, typeEndIndex - typeStartIndex);
            return true;
        }
    }
}
