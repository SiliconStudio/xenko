// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    [Description("Output enumeration")]
    [Obsolete("This class is not maintained.")]
    public class OutputEnumerationBuildStep : EnumerableBuildStep
    {
        public BuildStep Template { get { return template; } set { template = value; if (template != null) template.Parent = this; } }
        private BuildStep template;

        public List<string> SearchTags { get; set; }

        public IEnumerable<string> Urls { get; protected set; }

        public OutputEnumerationBuildStep()
        {
            SearchTags = new List<string>();
            Urls = Enumerable.Empty<string>();
        }

        public override string ToString()
        {
            lock (Urls)
            {
                return "Output enumeration" + (Urls.FirstOrDefault() != null ? " (" + Urls.Count() + " urls)" : "");
            }
        }

        public override BuildStep Clone()
        {
            var clone = new OutputEnumerationBuildStep();
            if (template != null)
                clone.Template = template.Clone();
            clone.SearchTags = SearchTags.ToList();
            return clone;
        }

        public override async Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext)
        {
            Steps = new List<BuildStep>();

            var urls = Enumerable.Empty<string>();

            BuildStep parentList = Parent;
            while (!(parentList is ListBuildStep))
            {
                parentList = parentList.Parent;
            }

            var parentListBuildStep = (ListBuildStep)parentList;


            foreach (string tag in SearchTags)
            {
                urls = urls.Concat(parentListBuildStep.OutputObjects.Where(x => x.Value.Tags.Contains(tag)).Select(x => x.Key.ToString()));
            }

            var buildStepToWait = new List<BuildStep>();

            lock (Urls)
            {
                Urls = urls;
                foreach (string url in Urls)
                {
                    executeContext.Variables["URL"] = url;
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
