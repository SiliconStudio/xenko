// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
//
//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiliconStudio.Paradox.VisualStudio.Assets
{
	#region Format definition
	[Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = AssetObjectDefinitions.AnchorClassificationName)]
	[Name("Paradox.YamlAnchorFormat")]
	[UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlAnchorFormat : ClassificationFormatDefinition
	{
		public YamlAnchorFormat()
		{
			DisplayName = "Paradox YAML Anchor"; //human readable version of the name
			ForegroundColor = Color.FromRgb(255, 128, 64);
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = AssetObjectDefinitions.AliasClassificationName)]
    [Name("Paradox.YamlAliasFormat")]
    [UserVisible(true)] //this should be visible to the end user
	[Order(Before = Priority.Default)] //set the priority to be after the default classifiers
	internal sealed class YamlAliasFormat : ClassificationFormatDefinition
	{
		public YamlAliasFormat()
		{
            DisplayName = "Paradox YAML Alias"; //human readable version of the name
			ForegroundColor = Color.FromRgb(115, 141, 0);
		}
	}

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = AssetObjectDefinitions.KeyClassificationName)]
    [Name("Paradox.YamlKeyFormat")]
    [UserVisible(true)] //this should be visible to the end user
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class YamlKeyFormat : ClassificationFormatDefinition
    {
        public YamlKeyFormat()
        {
            DisplayName = "Paradox YAML Key"; //human readable version of the name
            ForegroundColor = Color.FromRgb(0, 64, 96);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = AssetObjectDefinitions.NumberClassificationName)]
    [Name("Paradox.YamlNumberFormat")]
    [UserVisible(true)] //this should be visible to the end user
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class YamlNumberFormat : ClassificationFormatDefinition
    {
        public YamlNumberFormat()
        {
            DisplayName = "Paradox YAML Number"; //human readable version of the name
            ForegroundColor = Color.FromRgb(128, 64, 0);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = AssetObjectDefinitions.ErrorClassificationName)]
    [Name("Paradox.YamlErrorFormat")]
    [UserVisible(true)] //this should be visible to the end user
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class YamlErrorFormat : ClassificationFormatDefinition
    {
        public YamlErrorFormat()
        {
            DisplayName = "Paradox YAML Error"; //human readable version of the name
            BackgroundColor = Color.FromRgb(255, 0, 0);
        }
    }
	#endregion //Format definition
}
