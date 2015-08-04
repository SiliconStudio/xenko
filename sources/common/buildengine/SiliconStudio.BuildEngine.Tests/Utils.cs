// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Reflection;
using System.Text;

using NUnit.Framework;
using SiliconStudio.BuildEngine.Tests.Commands;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;

namespace SiliconStudio.BuildEngine.Tests
{
    public static class Utils
    {
        private static bool loggerHandled;

        private const string FileSourceFolder = "source";

        public static string BuildPath { get { return Path.Combine(PlatformFolders.ApplicationBinaryDirectory, Assembly.GetEntryAssembly() == null? TestContext.CurrentContext.Test.Name: "data/"+Assembly.GetEntryAssembly().GetName().Name); } }

        private static StringBuilder logCollecter;

        public static Logger CleanContext()
        {
            // delete previous build data
            if(Directory.Exists(BuildPath))
                Directory.Delete(BuildPath, true);

            // Create database directory
            ((FileSystemProvider)VirtualFileSystem.ApplicationData).ChangeBasePath(BuildPath);
            VirtualFileSystem.CreateDirectory("/data");
            VirtualFileSystem.CreateDirectory("/data/db");

            // Delete source folder if exists
            if (Directory.Exists(FileSourceFolder))
                Directory.Delete(FileSourceFolder, true);

            TestCommand.ResetCounter();
            if (!loggerHandled)
            {
                GlobalLogger.GlobalMessageLogged += new ConsoleLogListener();
                loggerHandled = true;
            }

            return GlobalLogger.GetLogger("UnitTest");
        }

        public static Builder CreateBuilder()
        {
            var logger = GlobalLogger.GetLogger("Builder");
            logger.ActivateLog(LogMessageType.Debug);
            var builder = new Builder(BuildPath, "Windows", "index", "inputHashes", logger) { BuilderName = "TestBuilder", SlaveBuilderPath = @"SiliconStudio.BuildEngine.exe" };
            return builder;
        }

        public static void GenerateSourceFile(string filename, string content, bool overwrite = false)
        {
            string filepath = GetSourcePath(filename);

            if (!Directory.Exists(FileSourceFolder))
                Directory.CreateDirectory(FileSourceFolder);

            if (!overwrite && File.Exists(filepath))
                throw new IOException("File already exists");

            File.WriteAllText(filepath, content);
        }

        public static string GetSourcePath(string filename)
        {
            // TODO: return a path in the temporary folder
            return Path.Combine(FileSourceFolder, filename);
        }

        public static void StartCapturingLog()
        {
            if (logCollecter != null)
                throw new InvalidOperationException("Log are already being captured");
            logCollecter = new StringBuilder();

            GlobalLogger.GlobalMessageLogged += CaptureLog;
        }

        public static string StopCapturingLog()
        {
            if (logCollecter == null)
                throw new InvalidOperationException("Log are not being captured");

            GlobalLogger.GlobalMessageLogged -= CaptureLog;

            string result = logCollecter.ToString();
            logCollecter = null;
            return result;
        }

        private static void CaptureLog(ILogMessage obj)
        {
            logCollecter.AppendLine(obj.Text);
        }
    }
}
