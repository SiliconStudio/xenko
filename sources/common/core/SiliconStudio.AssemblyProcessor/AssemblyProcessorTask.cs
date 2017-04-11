// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// Execute a custom tasks that will log a simple message
    /// </summary>
    public class AssemblyProcessorTask : Task
    {
        // Required property indicated by Required attribute
        [Required]
        public string Arguments { get; set; }

        /// <summary>
        /// This method is called automatically when the task is run.
        /// </summary>
        /// <returns>Boolean to indicate if the task was sucessful.</returns>
        public override bool Execute()
        {
            // Override assembly resolve (cf comments in CurrentDomain_AssemblyResolve)
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            try
            {
                var args = ParseArguments(Arguments);
                var processor = new AssemblyProcessorProgram();
                var redirectLogger = new RedirectLogger(Log);
                var result = processor.Run(args.ToArray(), redirectLogger);

                if (result != 0)
                {
                    Log.LogError($"Failed to run assembly processor with parameters: {Arguments}");
                    Log.LogError("Check the previous logs");
                }

                return result == 0;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            // Note: Ideally we would just like to use AssemblyProcessor app.config temporarily
            // Those are workaround to get MSBuild to load our dependencies properly

            // MSBuild has its own version of System.Collection.Immutables (1.2.1.0 right now)
            // However, there is no binding redirect, but Reflection.Metadata 1.4.1.0 needs it (it references 1.2.0.0)
            // This following peace of code intent is to act as if we had:
            // <dependentAssembly>
            //   <assemblyIdentity name="System.Collections.Immutable" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
            //   <bindingRedirect oldVersion="0.0.0.0-1.2.1.0" newVersion="1.2.1.0" />
            // </dependentAssembly>
            var immutableCollectionAssembly = typeof(ImmutableArray<byte>).Assembly;
            var immutableCollectionAssemblyName = immutableCollectionAssembly.GetName();
            if (assemblyName.Name == immutableCollectionAssemblyName.Name
                && assemblyName.Version <= immutableCollectionAssemblyName.Version)
            {
                return typeof(ImmutableArray<byte>).Assembly;
            }

            // Also try to resolve assemblies from current folder
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName.Name + ".dll");
            if (File.Exists(path))
            {
                return Assembly.LoadFrom(path);
            }

            return null;
        }

        /// <summary>
        /// Recompose command line arguments as they were passed to a console app.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static List<string> ParseArguments(string parameters)
        {
            var args = new List<string>();
            bool isInString = false;
            var builder = new StringBuilder();
            for (int i = 0; i < parameters.Length; i++)
            {
                var c = parameters[i];

                if (c == '"')
                {
                    if (isInString)
                    {
                        args.Add(builder.ToString());
                        builder.Clear();
                        isInString = false;
                    }
                    else
                    {
                        isInString = true;
                    }
                }
                else if (!char.IsWhiteSpace(c) || isInString)
                {
                    builder.Append(c);
                }
                else if (char.IsWhiteSpace(c) && builder.Length > 0)
                {
                    args.Add(builder.ToString());
                    builder.Clear();
                }
            }
            if (builder.Length > 0)
            {
                args.Add(builder.ToString());
            }
            return args;
        }

        /// <summary>
        /// Heler class to redirect logs
        /// </summary>
        private class RedirectLogger : TextWriter
        {
            private readonly TaskLoggingHelper taskLogger;
            private readonly StringBuilder content = new StringBuilder();

            public RedirectLogger(TaskLoggingHelper taskLogger)
            {
                this.taskLogger = taskLogger;
            }

            public override void Write(char value)
            {
                if (value == '\r')
                {
                    return;
                }
                if (value != '\n')
                {
                    content.Append(value);
                }
                else
                {
                    taskLogger.LogMessage(MessageImportance.High, content.ToString());
                    content.Clear();
                }
            }

            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}