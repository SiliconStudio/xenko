using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets;
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
        private static int Main(string[] args)
        {
            Console.WriteLine(@"Bootstrapping: " + args[0]);

            var xenkoDir = Environment.GetEnvironmentVariable("SiliconStudioXenkoDir");
            var xenkoPkgPath = UPath.Combine(xenkoDir, new UFile("Xenko.xkpkg"));

            var session = PackageSession.Load(xenkoPkgPath);

            var generator = TemplateSampleGenerator.Default;

            var logger = new LoggerResult();

            var parameters = new TemplateGeneratorParameters { Session = session.Session };

            var outputPath = UPath.Combine(new UDirectory(xenkoDir), new UDirectory("samplesGenerated"));
            outputPath = UPath.Combine(outputPath, new UDirectory(args[0]));

            var xenkoTemplates = session.Session.Packages.First().Templates;
            parameters.Description = xenkoTemplates.First(x => x.Group.StartsWith("Samples") && x.Id == new Guid(args[1]));
            parameters.Name = args[0];
            parameters.Namespace = args[0];
            parameters.OutputDirectory = outputPath;
            parameters.Logger = logger;

            generator.Generate(parameters);

            var updaterTemplate = xenkoTemplates.First(x => x.FullPath.ToString().EndsWith("UpdatePlatforms.xktpl"));
            parameters.Description = updaterTemplate;

            var updater = UpdatePlatformsTemplateGenerator.Default;

            var gameSettingsAsset = session.Session.Packages.Last().GetGameSettingsAsset();
            var renderingSettings = gameSettingsAsset.Get<RenderingSettings>();

            var updateParams = new GameTemplateParameters
            {
                Common = parameters,
                ForcePlatformRegeneration = true,
                GraphicsProfile = renderingSettings.DefaultGraphicsProfile,
                IsHDR = false,
                Orientation = renderingSettings.DisplayOrientation,
                Platforms = AssetRegistry.SupportedPlatforms.ToList()
            };

            updater.Generate(updateParams);

            Console.WriteLine(logger.ToText());

            return logger.HasErrors ? 1 : 0;
        }
    }
}