using SiliconStudio.Assets;
using SiliconStudio.Assets.Templates;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.Presentation.Templates;
using SiliconStudio.Xenko.Graphics;
using System;
using System.IO;
using System.Linq;

namespace SiliconStudio.Xenko.SamplesBootstrapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");
            var xenkoPkgPath = UPath.Combine(xenkoDir, new UFile("Xenko.xkpkg"));

            var session = PackageSession.Load(xenkoPkgPath);

            var generator = TemplateSampleGenerator.Default;

            var logger = GlobalLogger.GetLogger("SamplesBootstrapper");

            var parameters = new TemplateGeneratorParameters { Session = session.Session };

            var xenkoTemplates = session.Session.Packages.First().Templates;
            parameters.Description = xenkoTemplates.Last(x => x.Group.StartsWith("Samples"));
            parameters.Name = "BootstrapTest";
            parameters.Namespace = "BootstrapTest";
            parameters.OutputDirectory = Directory.GetCurrentDirectory();
            parameters.Logger = logger;

            generator.Generate(parameters);

            var updaterTemplate = xenkoTemplates.First(x => x.FullPath.ToString().EndsWith("UpdatePlatforms.xktpl"));
            parameters.Description = updaterTemplate;

            var updater = UpdatePlatformsTemplateGenerator.Default;

            var updateParams = new GameTemplateParameters
            {
                Common = parameters,
                ForcePlatformRegeneration = true,
                GraphicsProfile = GraphicsProfile.Level_9_1,
                IsHDR = false,
                Orientation = DisplayOrientation.Default,
                Platforms = AssetRegistry.SupportedPlatforms.ToList()
            };

            updater.Generate(updateParams);
        }
    }
}