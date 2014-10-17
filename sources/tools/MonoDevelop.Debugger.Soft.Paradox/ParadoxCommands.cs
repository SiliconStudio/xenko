using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace MonoDevelop.Debugger.Soft.Paradox
{
    public enum Commands
    {
        StartDebug,
    }

    internal class StartDebugServer : CommandHandler
    {
        protected override void Run()
        {
            IdeApp.ProjectOperations.DebugApplication("ParadoxDebugServer");
        }
    }

    internal class StartDebugClient : CommandHandler
    {
        protected override void Run()
        {
            IdeApp.ProjectOperations.DebugApplication("ParadoxDebugClient");
        }
    }
}
