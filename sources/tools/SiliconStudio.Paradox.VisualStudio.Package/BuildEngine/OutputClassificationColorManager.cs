// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using SiliconStudio.Paradox.VisualStudio.Classifiers;

namespace SiliconStudio.Paradox.VisualStudio.BuildEngine
{
    [Export]
    public class OutputClassificationColorManager : ClassificationColorManager
    {
        [ImportingConstructor]
        public OutputClassificationColorManager([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            ClassificationCategory = "output"; // DefGuidList.guidOutputWindowFontCategory

            // Light/Blue theme colors
            var lightColors = new Dictionary<string, ClassificationColor>
            {
                { OutputClassifier.BuildEngineDebug, new ClassificationColor(Color.FromRgb(60, 60, 60)) },
                { OutputClassifier.BuildEngineVerbose, new ClassificationColor(Colors.Black) },
                { OutputClassifier.BuildEngineInfo, new ClassificationColor(Colors.Green) },
                { OutputClassifier.BuildEngineWarning, new ClassificationColor(Colors.DarkOrange) },
                { OutputClassifier.BuildEngineError, new ClassificationColor(Colors.Red) },
                { OutputClassifier.BuildEngineFatal, new ClassificationColor(Colors.Red) },
            };

            themeColors.Add(VisualStudioTheme.Blue, lightColors);
            themeColors.Add(VisualStudioTheme.Light, lightColors);
            themeColors.Add(VisualStudioTheme.Unknown, lightColors);

            // Dark theme colors
            var darkColors = new Dictionary<string, ClassificationColor>
            {
                { OutputClassifier.BuildEngineDebug, new ClassificationColor(Colors.LightGray) },
                { OutputClassifier.BuildEngineVerbose, new ClassificationColor(Colors.White) },
                { OutputClassifier.BuildEngineInfo, new ClassificationColor(Colors.LightGreen) },
                { OutputClassifier.BuildEngineWarning, new ClassificationColor(Colors.Orange) },
                { OutputClassifier.BuildEngineError, new ClassificationColor(Colors.Red) },
                { OutputClassifier.BuildEngineFatal, new ClassificationColor(Colors.Red) },
            };

            themeColors.Add(VisualStudioTheme.Dark, darkColors);
        }
    }
}