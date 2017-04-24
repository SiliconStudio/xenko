// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;

namespace SiliconStudio.NugetPackageRewriter
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine(string.Join(", ", args));

            var exeName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            string nupkgSuffix = null;
            var showHelp = false;

            // Note: For now, this tool simply replace -beta into -alpha.
            // Later, we might want more control and/or customizations (what to change, which files to apply to, etc...)
            var p = new OptionSet
                {
                    "Copyright (C) 2011-2013 Silicon Studio Corporation. All Rights Reserved",
                    "Xenko Transform Alpha to Beta: "
                    +
                    String.Format(
                        "{0}.{1}.{2}",
                        typeof(Program).Assembly.GetName().Version.Major,
                        typeof(Program).Assembly.GetName().Version.Minor,
                        typeof(Program).Assembly.GetName().Version.Build) + string.Empty,
                    string.Format("Usage: {0} inputFile", exeName),
                    string.Empty,
                    "=== Options ===",
                    string.Empty,
                    { "h|help", "Show this message and exit", v => showHelp = v != null },
                    { "nupkgsuffix=", "Nupkg suffix", v => nupkgSuffix = "-" + v },
                };

            try
            {
                var inputFiles = p.Parse(args);
                if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                    return 0;
                }

                if (inputFiles.Count == 0)
                    throw new OptionException("Expect at least one input file", "");

                foreach (var inputFileGroup in inputFiles)
                {
                    var path = Path.GetDirectoryName(inputFileGroup);
                    var filename = Path.GetFileName(inputFileGroup);
                    if (string.IsNullOrEmpty(path))
                        path = ".";

                    foreach (var inputFile in System.IO.Directory.GetFiles(path, filename))
                    {
                        if (nupkgSuffix != null)
                        {
                            var outputFile = inputFile.Replace("-beta", nupkgSuffix);
                            var filesToUpdates = new[] { "Xenko.nuspec" };

                            Console.WriteLine("Rename {0} into {1}", inputFile, outputFile);
                            File.Copy(inputFile, outputFile, true);

                            using (var zipToOpen = File.Open(outputFile, FileMode.Open, FileAccess.ReadWrite))
                            {
                                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                                {
                                    foreach (var fileToUpdate in filesToUpdates)
                                    {
                                        var fileEntry = archive.GetEntry(fileToUpdate);
                                        var fileStream = fileEntry.Open();
                                        string fileContent;

                                        var reader = new StreamReader(fileStream, Encoding.UTF8);
                                        fileContent = reader.ReadToEnd();

                                        fileContent = fileContent.Replace("-beta", nupkgSuffix);

                                        fileStream.SetLength(0);

                                        var writer = new StreamWriter(fileStream);
                                        writer.Write(fileContent);
                                        writer.Flush();

                                        fileStream.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}: {1}", exeName, e);
                if (e is OptionException)
                    p.WriteOptionDescriptions(Console.Out);
                return 1;
            }
        }
    }
}
