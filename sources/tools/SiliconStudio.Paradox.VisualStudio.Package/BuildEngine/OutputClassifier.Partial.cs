// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiliconStudio.Paradox.VisualStudio
{
    public partial class OutputClassifier
    {
        private Dictionary<char, string> classificationTypes = new Dictionary<char, string>();

        private void InitializeClassifiers()
        {
            classificationTypes.Add('D', BuildEngineDebug);
            classificationTypes.Add('V', BuildEngineVerbose);
            classificationTypes.Add('I', BuildEngineInfo);
            classificationTypes.Add('W', BuildEngineWarning);
            classificationTypes.Add('E', BuildEngineError);
            classificationTypes.Add('F', BuildEngineFatal);
        }

        public const string BuildEngineDebug = "pdx.buildengine.debug";
        public const string BuildEngineVerbose = "pdx.buildengine.verbose";
        public const string BuildEngineInfo = "pdx.buildengine.info";
        public const string BuildEngineWarning = "pdx.buildengine.warning";
        public const string BuildEngineError = "pdx.buildengine.error";
        public const string BuildEngineFatal = "pdx.buildengine.fatal";

        [Export]
        [Name(BuildEngineDebug)]
        internal static ClassificationTypeDefinition buildEngineDebug = null;

        [Export]
        [Name(BuildEngineVerbose)]
        internal static ClassificationTypeDefinition buildEngineVerbose = null;

        [Export]
        [Name(BuildEngineInfo)]
        internal static ClassificationTypeDefinition buildEngineInfo = null;

        [Export]
        [Name(BuildEngineWarning)]
        internal static ClassificationTypeDefinition buildEngineWarning = null;

        [Export]
        [Name(BuildEngineError)]
        internal static ClassificationTypeDefinition buildEngineError = null;

        [Export]
        [Name(BuildEngineFatal)]
        internal static ClassificationTypeDefinition buildEngineFatal = null;

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineDebug)]
        [Name(BuildEngineDebug)]
        internal sealed class BuildEngineDebugFormat : ClassificationFormatDefinition
        {
            public BuildEngineDebugFormat()
            {
                this.ForegroundColor = Colors.DarkGray;
                this.IsBold = false;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineVerbose)]
        [Name(BuildEngineVerbose)]
        internal sealed class BuildEngineVerboseFormat : ClassificationFormatDefinition
        {
            public BuildEngineVerboseFormat()
            {
                this.ForegroundColor = Colors.Gray;
                this.IsBold = false;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineInfo)]
        [Name(BuildEngineInfo)]
        internal sealed class BuildEngineInfoFormat : ClassificationFormatDefinition
        {
            public BuildEngineInfoFormat()
            {
                this.ForegroundColor = Colors.Green;
                this.IsBold = false;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineWarning)]
        [Name(BuildEngineWarning)]
        internal sealed class BuildEngineWarningFormat : ClassificationFormatDefinition
        {
            public BuildEngineWarningFormat()
            {
                this.ForegroundColor = Colors.DarkOrange;
                this.IsBold = false;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineError)]
        [Name(BuildEngineError)]
        internal sealed class BuildEngineErrorFormat : ClassificationFormatDefinition
        {
            public BuildEngineErrorFormat()
            {
                this.ForegroundColor = Colors.Red;
                this.IsBold = true;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineFatal)]
        [Name(BuildEngineFatal)]
        internal sealed class BuildEngineFatalFormat : ClassificationFormatDefinition
        {
            public BuildEngineFatalFormat()
            {
                this.ForegroundColor = Colors.Red;
                this.IsBold = true;
            }
        }

    }
}
