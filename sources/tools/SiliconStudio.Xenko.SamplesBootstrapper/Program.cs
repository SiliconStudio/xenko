using SiliconStudio.Assets;
using SiliconStudio.Assets.Templates;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.Presentation.Templates;
using System;
using System.Linq;

namespace SiliconStudio.Xenko.SamplesBootstrapper
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            Console.WriteLine(@"Bootstrapping: " + args[0]);

            var xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");
            var xenkoPkgPath = UPath.Combine(xenkoDir, new UFile("Xenko.xkpkg"));

            var session = PackageSession.Load(xenkoPkgPath);

            var generator = TemplateSampleGenerator.Default;

            var logger = new LoggerResult();

            var parameters = new SessionTemplateGeneratorParameters { Session = session.Session };
            TemplateSampleGenerator.SetDontAskForPlatforms(parameters, true);
            TemplateSampleGenerator.SetPlatforms(parameters, AssetRegistry.SupportedPlatforms.ToList());

            var outputPath = UPath.Combine(new UDirectory(xenkoDir), new UDirectory("samplesGenerated"));
            outputPath = UPath.Combine(outputPath, new UDirectory(args[0]));

            var xenkoTemplates = session.Session.Packages.First().Templates;
            parameters.Description = xenkoTemplates.First(x => x.Group.StartsWith("Samples") && x.Id == new Guid(args[1]));
            parameters.Name = args[0];
            parameters.Namespace = args[0];
            parameters.OutputDirectory = outputPath;
            parameters.Logger = logger;

            if (!generator.PrepareForRun(parameters))
                logger.Error("PrepareForRun returned false for the TemplateSampleGenerator");

            if (!generator.Run(parameters))
                logger.Error("Run returned false for the TemplateSampleGenerator");

            var updaterTemplate = xenkoTemplates.First(x => x.FullPath.ToString().EndsWith("UpdatePlatforms.xktpl"));
            parameters.Description = updaterTemplate;

            Console.WriteLine(logger.ToText());

            return logger.HasErrors ? 1 : 0;
        }
    }
}
