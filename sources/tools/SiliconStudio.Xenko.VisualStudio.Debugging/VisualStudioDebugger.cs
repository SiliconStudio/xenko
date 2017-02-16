// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Process = System.Diagnostics.Process;
using Thread = System.Threading.Thread;

namespace SiliconStudio.Xenko.VisualStudio.Debugging
{
    internal class VisualStudioDebugger : IDisposable
    {
        private readonly Context context;
        private readonly DTE dte;

        public int ProcessId { get; private set; }

        private VisualStudioDebugger(Context context, DTE dte, int processId)
        {
            this.context = context;
            this.dte = dte;
            this.ProcessId = processId;
        }

        public static VisualStudioDebugger GetByProcess(int processId)
        {
            var context = new Context();

            var instance = GetFirstOrDefaultDTE(context, x => x.ProcessId == processId);

            return instance.DTE != null ? new VisualStudioDebugger(context, instance.DTE, instance.ProcessId) : null;
        }

        public static VisualStudioDebugger GetAttached()
        {
            var context = new Context();

            if (!System.Diagnostics.Debugger.IsAttached)
                return null;

            var instance = GetFirstOrDefaultDTE(context, x =>
            {
                // Try multiple time, as DTE might report it is busy
                var debugger = x.DTE.Debugger;
                if (debugger.DebuggedProcesses == null)
                    return false;

                return debugger.DebuggedProcesses.OfType<EnvDTE.Process>().Any(debuggedProcess => debuggedProcess.ProcessID == System.Diagnostics.Process.GetCurrentProcess().Id);
            });

            return instance.DTE != null ? new VisualStudioDebugger(context, instance.DTE, instance.ProcessId) : null;
        }

        public void AttachToProcess(int processId)
        {
            context.Execute(() =>
            {
                // Make this DTE attach the newly created process
                MessageFilter.Register();
                var processes = dte.Debugger.LocalProcesses.OfType<EnvDTE.Process>();
                var process = processes.FirstOrDefault(x => x.ProcessID == processId);
                process?.Attach();
                MessageFilter.Revoke();
            });
        }

        public void Attach()
        {
            AttachToProcess(System.Diagnostics.Process.GetCurrentProcess().Id);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        private static Instance GetFirstOrDefaultDTE(Context context, Func<Instance, bool> predicate)
        {
            return context.Execute(() =>
            {
                // Locate all Visual Studio DTE
                var dtes = GetActiveDTEs().ToArray();

                // Find DTE
                MessageFilter.Register();
                var result = dtes.FirstOrDefault(predicate);
                MessageFilter.Revoke();

                return result;
            });
        }

        public static IEnumerable<Process> GetActiveInstances()
        {
            return GetActiveDTEs().Select(x => x.ProcessId).Select(Process.GetProcessById);
        }

        /// <summary>
        /// Gets the instances of active <see cref="EnvDTE.DTE"/>.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Instance> GetActiveDTEs()
        {
            IRunningObjectTable rot;
            if (GetRunningObjectTable(0, out rot) == 0)
            {
                IEnumMoniker enumMoniker;
                rot.EnumRunning(out enumMoniker);

                var moniker = new IMoniker[1];
                while (enumMoniker.Next(1, moniker, IntPtr.Zero) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);
                    string displayName;
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);

                    // Check if it's Visual Studio
                    if (displayName.StartsWith("!VisualStudio"))
                    {
                        object obj;
                        rot.GetObject(moniker[0], out obj);

                        // Cast as DTE
                        var dte = obj as DTE;
                        if (dte != null)
                        {
                            yield return new Instance
                            {
                                DTE = dte,
                                ProcessId = int.Parse(displayName.Split(':')[1])
                            };
                        }
                    }
                }
            }
        }

        private class Context : IDisposable
        {
            private readonly Thread thread;
            private BlockingCollection<Task> tasks;

            public Context()
            {
                tasks = new BlockingCollection<Task>();

                thread = new Thread(() =>
                {
                    foreach (var task in tasks.GetConsumingEnumerable())
                    {
                        task.RunSynchronously();
                    }
                });

                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            public T Execute<T>(Func<T> func)
            {
                var task = new Task<T>(func);

                tasks.Add(task);
                task.Wait();
                return task.Result;
            }

            public void Execute(Action action)
            {
                var task = new Task(action);

                tasks.Add(task);
                task.Wait();
            }

            public void Dispose()
            {
                if (tasks != null)
                {
                    tasks.CompleteAdding();

                    thread.Join();

                    tasks.Dispose();
                    tasks = null;
                }
            }
        }

        private struct Instance
        {
            public DTE DTE;

            public int ProcessId;
        }

        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
    }
}