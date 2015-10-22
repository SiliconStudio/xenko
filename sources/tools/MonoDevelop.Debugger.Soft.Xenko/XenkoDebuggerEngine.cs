using System;
using System.Net;
using Mono.Debugging.Soft;
using MonoDevelop.Debugger;
using MonoDevelop.Core.Execution;
using MonoDevelop.Debugger.Soft;
using Mono.Debugging.Client;

namespace MonoDevelop.Debugger.Soft.Paradox
{
	public class ParadoxDebuggerEngine : IDebuggerEngine
	{
		public ParadoxDebuggerEngine ()
		{
		}
		
	    public bool CanDebugCommand(ExecutionCommand cmd)
		{
			return cmd.CommandString.StartsWith("ParadoxDebug");
		}
	    public DebuggerStartInfo CreateDebuggerStartInfo(ExecutionCommand cmd)
	    {
	        var dsi =
	            cmd.CommandString.EndsWith("Client")
	                ? new SoftDebuggerStartInfo(new SoftDebuggerConnectArgs("TestApp", IPAddress.Loopback, 13332))
	                : new SoftDebuggerStartInfo(new SoftDebuggerListenArgs("TestApp", IPAddress.Any, 13332));
			return dsi;
		}
	    public DebuggerSession CreateSession()
		{
			return new ParadoxRemoteSoftDebuggerSession();
		}
	    public ProcessInfo[] GetAttachableProcesses()
		{
			return new ProcessInfo[0];
		}
	}
}

