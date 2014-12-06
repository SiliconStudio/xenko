// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

using SiliconStudio.Paradox.VisualStudio.Commands;

namespace NShader
{
    internal class NShaderViewFilter : ViewFilter
    {
        private readonly NShaderLanguageService langService;

        public NShaderViewFilter(NShaderLanguageService langService, CodeWindowManager mgr, IVsTextView view)
            : base(mgr, view)
        {
            this.langService = langService;
        }

        protected override int QueryCommandStatus(ref Guid guidCmdGroup, uint nCmdId)
        {
            if (guidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                if (IsGoToDefinition(nCmdId)) 
                { 
                    return (int)OLECMDF.OLECMDF_SUPPORTED | (int)OLECMDF.OLECMDF_ENABLED;
                }
            }

            return base.QueryCommandStatus(ref guidCmdGroup, nCmdId);
        }

        protected override int ExecCommand(ref Guid guidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (guidCmdGroup == VSConstants.GUID_VSStandardCommandSet97)
            {
                if (IsGoToDefinition(nCmdId))
                {
                    GoToDefinition();
                    return 0;
                }
            }
            unchecked { return (int)Constants.OLECMDERR_E_NOTSUPPORTED; }
        }

        private bool IsGoToDefinition(uint nCmdId)
        {
            var cmd = (VSConstants.VSStd97CmdID)nCmdId;
            switch (cmd)
            {
                case VSConstants.VSStd97CmdID.GotoDefn:
                case VSConstants.VSStd97CmdID.GotoDecl:
                case VSConstants.VSStd97CmdID.GotoRef:
                    return true;
            }
            return false;
        }

        private void GoToDefinition()
        {
            int line;
            int column;
            TextView.GetCaretPos(out line, out column);

            IVsTextLines buffer;
            TextView.GetBuffer(out buffer);

            var span = new TextSpan();
            buffer.GetLastLineIndex(out span.iEndLine, out span.iEndIndex);
           
            string text;
            buffer.GetLineText(span.iStartLine, span.iStartIndex, span.iEndLine, span.iEndIndex, out text);


            try
            {
                var remoteCommands = ParadoxCommandsProxy.GetProxy();
                var location = new SourceLocation()
                {
                    File = this.Source.GetFilePath(),
                    Column = column + 1,
                    Line = line + 1
                };
                var spanFound = remoteCommands.GoToDefinition(text, location);

                langService.GotoLocation(spanFound);
            }
            catch (Exception ex)
            {
                // TODO handle errors
            }
        }
    }
}