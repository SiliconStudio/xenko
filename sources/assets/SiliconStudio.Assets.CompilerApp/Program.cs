// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Mono.Options;

using System.Reflection;
using System.Text;
using System.Diagnostics;

using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Assets.Model;
using SiliconStudio.Paradox.Assets.SpriteFont;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.ProceduralModels;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Assets.CompilerApp
{
    class Program
    {
        private static Stopwatch clock;
        private static string FormatLog(ILogMessage message)
        {
            //$filename($row,$column): $error_type $error_code: $error_message
            //C:\Code\Paradox\sources\assets\SiliconStudio.Assets.CompilerApp\PackageBuilder.cs(89,13,89,70): warning CS1717: Assignment made to same variable; did you mean to assign something else?
            var builder = new StringBuilder();
            builder.Append(message.Module);
            builder.Append(": ");
            builder.Append(message.Type.ToString().ToLowerInvariant()).Append(" ");
            builder.Append((clock.ElapsedMilliseconds * 0.001).ToString("0.000"));
            builder.Append("s: ");
            builder.Append(message.Text);
            return builder.ToString();
        }

        private static int Main(string[] args)
        {
            clock = Stopwatch.StartNew();

            // TODO this is hardcoded. Check how to make this dynamic instead.
            RuntimeHelpers.RunModuleConstructor(typeof(IProceduralModel).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(MaterialKeys).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(SpriteFontAsset).Module.ModuleHandle);
            RuntimeHelpers.RunModuleConstructor(typeof(ModelAsset).Module.ModuleHandle);
            //var project = new Package();
            //project.Save("test.pdxpkg");

            //Thread.Sleep(10000);
            //var spriteFontAsset = StaticFontAsset.New();
            //Asset.Save("test.pdxfnt", spriteFontAsset);
            //project.Refresh();

            //args = new string[] { "test.pdxpkg", "-o:app_data", "-b:tmp", "-t:1" };

            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var showHelp = false;
            var options = new PackageBuilderOptions(new ForwardingLoggerResult(GlobalLogger.GetLogger("BuildEngine")));

            var p = new OptionSet
                {
                    "Copyright (C) 2011-2014 Silicon Studio Corporation. All Rights Reserved",
                    "Paradox Build Tool - Version: "
                    +
                    String.Format(
                        "{0}.{1}.{2}",
                        typeof(Program).Assembly.GetName().Version.Major,
                        typeof(Program).Assembly.GetName().Version.Minor,
                        typeof(Program).Assembly.GetName().Version.Build) + string.Empty,
                    string.Format("Usage: {0} inputPackageFile [options]* -b buildPath", exeName),
                    string.Empty,
                    "=== Options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
                    { "v|verbose", "Show more verbose progress logs", v => options.Verbose = v != null },
                    { "d|debug", "Show debug logs (imply verbose)", v => options.Debug = v != null },
                    { "log", "Enable file logging", v => options.EnableFileLogging = v != null },
                    { "p|profile=", "Profile name", v => options.BuildProfile = v },
                    { "project-configuration=", "Project configuration", v => options.ProjectConfiguration = v },
                    { "platform=", "Platform name", v => options.Platform = (PlatformType)Enum.Parse(typeof(PlatformType), v) },
                    { "graphics-platform=", "Graphics Platform name", v => options.GraphicsPlatform = (GraphicsPlatform)Enum.Parse(typeof(GraphicsPlatform), v) },
                    { "solution-file=", "Solution File Name", v => options.SolutionFile = v },
                    { "package-id=", "Package Id from the solution file", v => options.PackageId = Guid.Parse(v) },
                    { "package-file=", "Input Package File Name", v => options.PackageFile = v },
                    { "o|output-path=", "Output path", v => options.OutputDirectory = v },
                    { "b|build-path=", "Build path", v => options.BuildDirectory = v },
                    { "log-file=", "Log build in a custom file.", v =>
                        {
                            options.EnableFileLogging = v != null;
                            options.CustomLogFileName = v;
                        } },
                    { "log-pipe=", "Log pipe.", v =>
                        {
                            if (!string.IsNullOrEmpty(v))
                                options.LogPipeNames.Add(v);
                        } },
                    { "monitor-pipe=", "Monitor pipe.", v =>
                        {
                            if (!string.IsNullOrEmpty(v))
                                options.MonitorPipeNames.Add(v);
                        } },
                    { "slave=", "Slave pipe", v => options.SlavePipe = v }, // Benlitz: I don't think this should be documented
                    { "t|threads=", "Number of threads to create. Default value is the number of hardware threads available.", v => options.ThreadCount = int.Parse(v) },
                    { "test=", "Run a test session.", v => options.TestName = v },
                    { "property:", "Properties. Format is name1=value1;name2=value2", v =>
                        {
                            if (!string.IsNullOrEmpty(v))
                            {
                                foreach (var nameValue in v.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var equalIndex = nameValue.IndexOf('=');
                                    if (equalIndex == -1)
                                        throw new OptionException("Expect name1=value1;name2=value2 format.", "property");

                                    options.Properties.Add(nameValue.Substring(0, equalIndex), nameValue.Substring(equalIndex + 1));
                                }
                            }
                        }
                    },
                    { "compile-property:", "Compile properties. Format is name1=value1;name2=value2", v =>
                        {
                            if (!string.IsNullOrEmpty(v))
                            {
                                if (options.ExtraCompileProperties == null)
                                    options.ExtraCompileProperties = new Dictionary<string, string>();

                                foreach (var nameValue in v.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var equalIndex = nameValue.IndexOf('=');
                                    if (equalIndex == -1)
                                        throw new OptionException("Expect name1=value1;name2=value2 format.", "property");

                                    options.ExtraCompileProperties.Add(nameValue.Substring(0, equalIndex), nameValue.Substring(equalIndex + 1));
                                }
                            }
                        }
                    },
                };

            TextWriterLogListener fileLogListener = null;

            // Output logs to the console with colored messages
            if (options.SlavePipe == null)
            {
                var consoleLogListener = new ConsoleLogListener { TextFormatter = FormatLog, LogMode = ConsoleLogMode.Always };
                GlobalLogger.GlobalMessageLogged += consoleLogListener;
            }

            BuildResultCode exitCode;

            try
            {
                var unexpectedArgs = p.Parse(args);
                if (unexpectedArgs.Any())
                {
                    throw new OptionException("Unexpected arguments [{0}]".ToFormat(string.Join(", ", unexpectedArgs)), "args");
                }
                try
                {
                    options.ValidateOptions();
                }
                catch (ArgumentException ex)
                {
                    throw new OptionException(ex.Message, ex.ParamName);
                }

                // Also write logs from master process into a file
                if (options.SlavePipe == null)
                {
                    if (options.EnableFileLogging)
                    {
                        string logFileName = options.CustomLogFileName;
                        if (string.IsNullOrEmpty(logFileName))
                        {
                            string inputName = Path.GetFileNameWithoutExtension(options.PackageFile);
                            logFileName = "Logs/Build-" + inputName + "-" + DateTime.Now.ToString("yy-MM-dd-HH-mm") + ".txt";
                        }

                        string dirName = Path.GetDirectoryName(logFileName);
                        if (dirName != null)
                            Directory.CreateDirectory(dirName);

                        fileLogListener = new TextWriterLogListener(new FileStream(logFileName, FileMode.Create)) { TextFormatter = FormatLog };
                        GlobalLogger.GlobalMessageLogged += fileLogListener;
                    }
                    options.Logger.Info("BuildEngine arguments: " + string.Join(" ", args));
                    options.Logger.Info("Starting builder.");
                }

                if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                    exitCode = BuildResultCode.Successful;
                }
                else if (!string.IsNullOrEmpty(options.TestName))
                {
                    var test = new TestSession();
                    test.RunTest(options.TestName, options.Logger);
                    exitCode = BuildResultCode.Successful;
                }
                else
                {
                    var builder = new PackageBuilder(options);
                    Console.CancelKeyPress += (_, e) => e.Cancel = builder.Cancel();
                    exitCode = builder.Build();
                }
            }
            catch (OptionException e)
            {
                options.Logger.Error("Command option '{0}': {1}", e.OptionName, e.Message);
                exitCode = BuildResultCode.CommandLineError;
            }
            catch (Exception e)
            {
                options.Logger.Error("Unhandled exception: {0}", e, e.Message);
                exitCode = BuildResultCode.BuildError;
            }
            finally
            {
                if (fileLogListener != null)
                    fileLogListener.LogWriter.Close();

            }
            return (int)exitCode;
        }
    }
}
