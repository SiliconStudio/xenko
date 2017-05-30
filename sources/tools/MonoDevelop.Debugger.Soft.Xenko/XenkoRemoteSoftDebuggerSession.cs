// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using Mono.Debugging.Soft;
using MonoDevelop.Debugger.Soft;
using Mono.Debugging.Client;

namespace MonoDevelop.Debugger.Soft.Xenko
{
	public class XenkoRemoteSoftDebuggerSession : SoftDebuggerSession
	{
		public XenkoRemoteSoftDebuggerSession()
		{
			
		}
		
		protected override void OnRun(DebuggerStartInfo startInfo)
		{
			var dsi = (SoftDebuggerStartInfo)startInfo;
            if (dsi.StartArgs is SoftDebuggerConnectArgs)
			    base.StartConnecting(dsi);
            else if (dsi.StartArgs is SoftDebuggerListenArgs)
                base.StartListening(dsi);
            else
                throw new NotImplementedException();
		}
	}
}

