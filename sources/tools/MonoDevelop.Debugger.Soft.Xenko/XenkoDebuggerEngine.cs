// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Net;
using Mono.Debugging.Soft;
using MonoDevelop.Core.Execution;
using Mono.Debugging.Client;

namespace MonoDevelop.Debugger.Soft.Xenko
{
	public class XenkoDebuggerEngine : DebuggerEngineBackend
	{
		public XenkoDebuggerEngine ()
		{
		}

        public override bool CanDebugCommand(ExecutionCommand cmd)
	    {
	        var processCmd = cmd as ProcessExecutionCommand;
	        if (processCmd == null)
	            return false;

            return processCmd.Command.StartsWith("XenkoDebug");
		}

        public override DebuggerStartInfo CreateDebuggerStartInfo(ExecutionCommand cmd)
	    {
	        var processCmd = cmd as ProcessExecutionCommand;
	        if (processCmd == null)
	            return null;

	        var dsi =
	            processCmd.Command.EndsWith("Client")
	                ? new SoftDebuggerStartInfo(new SoftDebuggerConnectArgs("TestApp", IPAddress.Loopback, 13332))
	                : new SoftDebuggerStartInfo(new SoftDebuggerListenArgs("TestApp", IPAddress.Any, 13332));
			return dsi;
		}

        public override DebuggerSession CreateSession()
        {
			return new XenkoRemoteSoftDebuggerSession();
		}
	}
}

