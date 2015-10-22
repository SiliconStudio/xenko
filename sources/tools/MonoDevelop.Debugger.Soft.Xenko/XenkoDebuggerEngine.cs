using System;
using System.Net;
using Mono.Debugging.Soft;
using MonoDevelop.Debugger;
using MonoDevelop.Core.Execution;
using MonoDevelop.Debugger.Soft;
using Mono.Debugging.Client;

namespace MonoDevelop.Debugger.Soft.Xenko
{
	public class XenkoDebuggerEngine : IDebuggerEngine
	{
		public XenkoDebuggerEngine ()
		{
		}
		
	    public bool CanDebugCommand(ExecutionCommand cmd)
		{
			return cmd.CommandString.StartsWith("XenkoDebug");
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
			return new XenkoRemoteSoftDebuggerSession();
		}
	    public ProcessInfo[] GetAttachableProcesses()
		{
			return new ProcessInfo[0];
		}
	}
}

