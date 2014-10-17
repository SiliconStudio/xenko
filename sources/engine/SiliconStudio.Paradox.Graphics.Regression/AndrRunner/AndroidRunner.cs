//
// Copyright 2011-2012 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Widget;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnitLite;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using NUnit.Framework.Internal.WorkItems;

namespace Android.NUnitLite {
	
	public class AndroidRunner : ITestListener {
		
		private AndroidRunner ()
		{
		}
		
		public bool AutoStart { get; set; }

		public bool TerminateAfterExecution { get; set; }
		
		#region writer
		
		public TextWriter Writer { get; set; }
		
		public bool OpenWriter (string message, Context activity)
		{
			DateTime now = DateTime.Now;
			// let the application provide it's own TextWriter to ease automation with AutoStart property
			if (Writer == null) {
				Writer = Console.Out;
			}

			Writer.WriteLine ("[Runner executing:\t{0}]", message);
			// FIXME
			Writer.WriteLine ("[M4A Version:\t{0}]", "???");
			
			Writer.WriteLine ("[Board:\t\t{0}]", Android.OS.Build.Board);
			Writer.WriteLine ("[Bootloader:\t{0}]", Android.OS.Build.Bootloader);
			Writer.WriteLine ("[Brand:\t\t{0}]", Android.OS.Build.Brand);
			Writer.WriteLine ("[CpuAbi:\t{0} {1}]", Android.OS.Build.CpuAbi, Android.OS.Build.CpuAbi2);
			Writer.WriteLine ("[Device:\t{0}]", Android.OS.Build.Device);
			Writer.WriteLine ("[Display:\t{0}]", Android.OS.Build.Display);
			Writer.WriteLine ("[Fingerprint:\t{0}]", Android.OS.Build.Fingerprint);
			Writer.WriteLine ("[Hardware:\t{0}]", Android.OS.Build.Hardware);
			Writer.WriteLine ("[Host:\t\t{0}]", Android.OS.Build.Host);
			Writer.WriteLine ("[Id:\t\t{0}]", Android.OS.Build.Id);
			Writer.WriteLine ("[Manufacturer:\t{0}]", Android.OS.Build.Manufacturer);
			Writer.WriteLine ("[Model:\t\t{0}]", Android.OS.Build.Model);
			Writer.WriteLine ("[Product:\t{0}]", Android.OS.Build.Product);
			Writer.WriteLine ("[Radio:\t\t{0}]", Android.OS.Build.Radio);
			Writer.WriteLine ("[Tags:\t\t{0}]", Android.OS.Build.Tags);
			Writer.WriteLine ("[Time:\t\t{0}]", Android.OS.Build.Time);
			Writer.WriteLine ("[Type:\t\t{0}]", Android.OS.Build.Type);
			Writer.WriteLine ("[User:\t\t{0}]", Android.OS.Build.User);
			Writer.WriteLine ("[VERSION.Codename:\t{0}]", Android.OS.Build.VERSION.Codename);
			Writer.WriteLine ("[VERSION.Incremental:\t{0}]", Android.OS.Build.VERSION.Incremental);
			Writer.WriteLine ("[VERSION.Release:\t{0}]", Android.OS.Build.VERSION.Release);
			Writer.WriteLine ("[VERSION.Sdk:\t\t{0}]", Android.OS.Build.VERSION.Sdk);
			Writer.WriteLine ("[VERSION.SdkInt:\t{0}]", Android.OS.Build.VERSION.SdkInt);
			Writer.WriteLine ("[Device Date/Time:\t{0}]", now); // to match earlier C.WL output
			
			// FIXME: add data about how the app was compiled (e.g. ARMvX, LLVM, Linker options)

			return true;
		}
		
		public void CloseWriter ()
		{
			Writer.Close ();
			Writer = null;
		}

		#endregion
		
		public void TestStarted (ITest test)
		{
			if (test is TestSuite) {
				Writer.WriteLine ();
				Writer.WriteLine (test.Name);
			}
		}

	    public void TestFinished(ITestResult result)
	    {
            AndroidRunner.Results[result.Test.FullName ?? result.Test.Name] = result;

            if (result.Test is TestSuite)
            {
                if (!result.IsFailure() && !result.IsSuccess() && !result.IsInconclusive() && !result.IsIgnored())
                    Writer.WriteLine("\t[INFO] {0}", result.Message);

                string name = result.Test.Name;
                if (!String.IsNullOrEmpty(name))
                    Writer.WriteLine("{0} : {1} ms", name, result.Duration.TotalMilliseconds);
            }
            else
            {
                if (result.IsSuccess())
                {
                    Writer.Write("\t[PASS] ");
                }
                else if (result.IsIgnored())
                {
                    Writer.Write("\t[IGNORED] ");
                }
                else if (result.IsFailure())
                {
                    Writer.Write("\t[FAIL] ");
                }
                else if (result.IsInconclusive())
                {
                    Writer.Write("\t[INCONCLUSIVE] ");
                }
                else
                {
                    Writer.Write("\t[INFO] ");
                }
                Writer.Write(result.Test.Name);

                string message = result.Message;
                if (!String.IsNullOrEmpty(message))
                {
                    Writer.Write(" : {0}", message.Replace("\r\n", "\\r\\n"));
                }
                Writer.WriteLine();

                string stacktrace = result.StackTrace;
                if (!String.IsNullOrEmpty(result.StackTrace))
                {
                    string[] lines = stacktrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                        Writer.WriteLine("\t\t{0}", line);
                }
            }
	    }

	    public void TestOutput(TestOutput testOutput)
	    {
	    }

	    Stack<DateTime> time = new Stack<DateTime> ();
			
		public void TestFinished (TestResult result)
		{
			
		}
		
		static AndroidRunner runner = new AndroidRunner ();
		
		static public AndroidRunner Runner {
			get { return runner; }
		}
		
		static List<TestSuite> top = new List<TestSuite> ();
		static Dictionary<string,TestSuite> suites = new Dictionary<string, TestSuite> ();
		static Dictionary<string,ITestResult> results = new Dictionary<string, ITestResult> ();
		
		static public IList<TestSuite> AssemblyLevel {
			get { return top; }
		}
		
		static public IDictionary<string,TestSuite> Suites {
			get { return suites; }
		}
		
		static public IDictionary<string,ITestResult> Results {
			get { return results; }
		}

        public Task<TestResult> Run(NUnit.Framework.Internal.Test test)
        {
            return Task.Run(() =>
            {
                TestExecutionContext current = TestExecutionContext.CurrentContext;
                current.WorkDirectory = Environment.CurrentDirectory;
                //current.Listener = this; // Internal on Android
                current.GetType().GetField("listener", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(current, this);
                current.TestObject = test is TestSuite ? null : Reflect.Construct((test as TestMethod).Method.ReflectedType, null);
                WorkItem wi = test.CreateWorkItem(TestFilter.Empty);

                wi.Execute(current);
                return wi.Result;
            });
        }
	}
}