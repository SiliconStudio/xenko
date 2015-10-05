// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SiliconStudio.AssemblyProcessor
{
    public class GenerateUserDocumentationProcessor : IAssemblyDefinitionProcessor
    {
        private readonly string inputFile;

        public GenerateUserDocumentationProcessor(string inputFile)
        {
            if (inputFile == null) throw new ArgumentNullException("inputFile");
            this.inputFile = inputFile;
        }

        public bool Process(AssemblyProcessorContext context)
        {
            var basePath = Path.Combine(Path.GetDirectoryName(inputFile) ?? "", Path.GetFileNameWithoutExtension(inputFile) ?? "");
            var xmlFile = basePath + ".xml";
            var targetFile = basePath + ".usrdoc";

            // No xml documentation file available, stop here.
            if (!File.Exists(xmlFile))
                return false;

            var result = new Dictionary<string, string>();

            var document = XElement.Load(xmlFile);
            foreach (var member in document.Descendants("member"))
            {
                var nameAttribute = member.Attribute("name");
                if (nameAttribute == null)
                    continue;
                
            
                foreach (var userdocElement in member.Descendants("userdoc"))
                {
                    string userdoc = null;
                    var key = nameAttribute.Value;

                    var docOverride = userdocElement.Attribute("override");
                    if (docOverride != null && key.StartsWith("T")) // if on top of the class we have some overrides we must process them now
                    {
                        key = "P" + key.Substring(1) + "." + docOverride.Value; //replace T with M
                    }

                    if (result.ContainsKey(key))
                    {
                        LogLine("Warning: the member {0} has multiple userdoc, only the first one will be used.", key);
                        continue;
                    }
                    if (userdocElement.Descendants().Any())
                    {
                        LogLine("Warning: the userdoc of member {0} has descendant nodes, which is not supported.", key);
                        continue;
                    }

                    userdoc = userdocElement.Value;
                    userdoc = userdoc.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Trim();
                    // Removes double space.
                    var regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                    userdoc = regex.Replace(userdoc, @" ");

                    result.Add(key, userdoc);
                }
            }

            using (var writer = new StreamWriter(targetFile))
            {
                foreach (var entry in result)
                {
                    writer.WriteLine("{0}={1}", entry.Key, entry.Value);
                }
            }

            return true;
        }

        private static void LogLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}