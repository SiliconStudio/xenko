// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Theme Coloring Source: https://github.com/fsprojects/VisualFSharpPowerTools
//
// Copyright 2014 F# Software Foundation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;

namespace SiliconStudio.Paradox.VisualStudio.Classifiers
{
    public class ClassificationColorManager : IDisposable
    {
        private VisualStudioTheme currentTheme = VisualStudioTheme.Unknown;
        protected string ClassificationCategory = "text"; // DefGuidList.guidTextEditorFontCategory

        protected readonly Dictionary<VisualStudioTheme, IDictionary<string, ClassificationColor>> themeColors =
            new Dictionary<VisualStudioTheme, IDictionary<string, ClassificationColor>>();

        private VisualStudioThemeEngine themeEngine;

        [Import]
        private IClassificationFormatMapService classificationFormatMapService = null;

        [Import]
        private IClassificationTypeRegistryService classificationTypeRegistry = null;

        protected ClassificationColorManager(IServiceProvider serviceProvider)
        {
            // Initialize theme engine
            themeEngine = new VisualStudioThemeEngine(serviceProvider);
            themeEngine.OnThemeChanged += themeEngine_OnThemeChanged;
        }

        public void Dispose()
        {
            themeEngine.OnThemeChanged -= themeEngine_OnThemeChanged;
            themeEngine.Dispose();
        }

        protected VisualStudioTheme GetCurrentTheme()
        {
            return themeEngine.GetCurrentTheme();
        }

        void themeEngine_OnThemeChanged(object sender, EventArgs e)
        {
            UpdateColors();
        }

        private void UpdateColors()
        {
            var theme = themeEngine.GetCurrentTheme();

            // Did theme change?
            if (theme != currentTheme)
            {
                currentTheme = theme;

                var colors = themeColors[theme];
                var formatMap = classificationFormatMapService.GetClassificationFormatMap(ClassificationCategory);

                // TODO: It seems this approach doesn't update Fonts & Colors settings
                try
                {
                    formatMap.BeginBatchUpdate();
                    foreach (var pair in colors)
                    {
                        string type = pair.Key;
                        var color = pair.Value;

                        var classificationType = classificationTypeRegistry.GetClassificationType(type);
                        var oldProp = formatMap.GetTextProperties(classificationType);

                        var foregroundBrush =
                            color.ForegroundColor == null
                                ? null
                                : new SolidColorBrush(color.ForegroundColor.Value);

                        var backgroundBrush =
                            color.BackgroundColor == null
                                ? null
                                : new SolidColorBrush(color.BackgroundColor.Value);

                        var newProp = TextFormattingRunProperties.CreateTextFormattingRunProperties(
                            foregroundBrush, backgroundBrush, oldProp.Typeface, null, null, oldProp.TextDecorations,
                            oldProp.TextEffects, oldProp.CultureInfo);

                        formatMap.SetTextProperties(classificationType, newProp);
                    }
                }
                finally
                {
                    formatMap.EndBatchUpdate();
                }
            }
        }

        public ClassificationColor GetClassificationColor(string classificationName)
        {
            var theme = GetCurrentTheme();

            ClassificationColor classificationColor;
            themeColors[theme].TryGetValue(classificationName, out classificationColor);

            return classificationColor;
        }
    }
}