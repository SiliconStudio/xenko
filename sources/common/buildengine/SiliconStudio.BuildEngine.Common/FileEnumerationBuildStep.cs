// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    [Description("File enumeration")]
    [Obsolete("This class is not maintained.")]
    public class FileEnumerationBuildStep : EnumerableBuildStep
    {
        public BuildStep Template { get { return template; } set { template = value; if (template != null) template.Parent = this; } }
        private BuildStep template;

        public List<string> SearchPattern { get; set; }

        public List<string> ExcludePattern { get; set; }

        public IEnumerable<string> Files { get; protected set; }

        public FileEnumerationBuildStep()
        {
            SearchPattern = new List<string>();
            ExcludePattern = new List<string>();
            Files = Enumerable.Empty<string>();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            lock (Files)
            {
                return "File enumeration" + (Files.FirstOrDefault() != null ? " (" + Files.Count() + " files)" : "");
            }
        }

        /// <inheritdoc/>
        public override BuildStep Clone()
        {
            var clone = new FileEnumerationBuildStep();
            if (template != null)
                clone.Template = template.Clone();
            clone.SearchPattern = SearchPattern.ToList();
            clone.ExcludePattern = ExcludePattern.ToList();
            return clone;
        }

        public override async Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            Steps = new List<BuildStep>();
            var files = Enumerable.Empty<string>();

            foreach (string pattern in SearchPattern)
            {
                string path = Path.GetDirectoryName(pattern);
                string filePattern = Path.GetFileName(pattern);
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(filePattern))
                {
                    files = files.Concat(Directory.EnumerateFiles(path, filePattern));
                }
                else
                {
                    files = files.Concat(Directory.EnumerateFiles(pattern));
                }
            }

            var excludes = Enumerable.Empty<string>();

            foreach (string pattern in ExcludePattern)
            {
                string path = Path.GetDirectoryName(pattern);
                string filePattern = Path.GetFileName(pattern);
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(filePattern))
                {
                    excludes = excludes.Concat(Directory.EnumerateFiles(path, filePattern));
                }
                else
                {
                    excludes = excludes.Concat(Directory.EnumerateFiles(pattern));
                }
            }

            var buildStepToWait = new List<BuildStep>();
            
            lock (Files)
            {
                Files = files.Where(x => !excludes.Contains(x));
                foreach (string file in Files)
                {
                    executeContext.Variables["FILE"] = file;
                    var fileBuildStep = Template.Clone();
                    ((List<BuildStep>)Steps).Add(fileBuildStep);
                    buildStepToWait.Add(fileBuildStep);
                    executeContext.ScheduleBuildStep(fileBuildStep);
                }
            }

            await CompleteCommands(executeContext, buildStepToWait);

            return ResultStatus.Successful;
        }
    }
}
