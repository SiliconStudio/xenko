using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.VisualStudio.BuildEngine;

namespace SiliconStudio.Xenko.VisualStudio
{
    public partial class OutputClassifier
    {
        private Dictionary<string, string> classificationTypes = new Dictionary<string, string>();

        private void InitializeClassifiers()
        {
            // IMPORTANT NOTE: KEEP old ones as string if LogMessageType entries are removed/changed! (VSPackage supports older version of Xenko)
            classificationTypes.Add(nameof(LogMessageType.Debug), BuildEngineDebug);
            classificationTypes.Add(nameof(LogMessageType.Verbose), BuildEngineVerbose);
            classificationTypes.Add(nameof(LogMessageType.Info), BuildEngineInfo);
            classificationTypes.Add(nameof(LogMessageType.Warning), BuildEngineWarning);
            classificationTypes.Add(nameof(LogMessageType.Error), BuildEngineError);
            classificationTypes.Add(nameof(LogMessageType.Fatal), BuildEngineFatal);
        }

        public const string BuildEngineDebug = "xk.buildengine.debug";
        public const string BuildEngineVerbose = "xk.buildengine.verbose";
        public const string BuildEngineInfo = "xk.buildengine.info";
        public const string BuildEngineWarning = "xk.buildengine.warning";
        public const string BuildEngineError = "xk.buildengine.error";
        public const string BuildEngineFatal = "xk.buildengine.fatal";

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
        [UserVisible(true)] //this should be visible to the end user
        [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
        internal sealed class BuildEngineDebugFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public BuildEngineDebugFormat(OutputClassificationColorManager colorManager)
            {
                DisplayName = "Xenko BuildEngine Debug";
                this.IsBold = false;
                var classificationColor = colorManager.GetClassificationColor(BuildEngineDebug);
                ForegroundColor = classificationColor.ForegroundColor;
                BackgroundColor = classificationColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineVerbose)]
        [Name(BuildEngineVerbose)]
        [UserVisible(true)] //this should be visible to the end user
        [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
        internal sealed class BuildEngineVerboseFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public BuildEngineVerboseFormat(OutputClassificationColorManager colorManager)
            {
                DisplayName = "Xenko BuildEngine Verbose";
                this.IsBold = false;
                var classificationColor = colorManager.GetClassificationColor(BuildEngineVerbose);
                ForegroundColor = classificationColor.ForegroundColor;
                BackgroundColor = classificationColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineInfo)]
        [Name(BuildEngineInfo)]
        [UserVisible(true)] //this should be visible to the end user
        [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
        internal sealed class BuildEngineInfoFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public BuildEngineInfoFormat(OutputClassificationColorManager colorManager)
            {
                DisplayName = "Xenko BuildEngine Info";
                this.IsBold = false;
                var classificationColor = colorManager.GetClassificationColor(BuildEngineInfo);
                ForegroundColor = classificationColor.ForegroundColor;
                BackgroundColor = classificationColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineWarning)]
        [Name(BuildEngineWarning)]
        [UserVisible(true)] //this should be visible to the end user
        [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
        internal sealed class BuildEngineWarningFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public BuildEngineWarningFormat(OutputClassificationColorManager colorManager)
            {
                DisplayName = "Xenko BuildEngine Warning";
                this.IsBold = false;
                var classificationColor = colorManager.GetClassificationColor(BuildEngineWarning);
                ForegroundColor = classificationColor.ForegroundColor;
                BackgroundColor = classificationColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineError)]
        [Name(BuildEngineError)]
        [UserVisible(true)] //this should be visible to the end user
        [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
        internal sealed class BuildEngineErrorFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public BuildEngineErrorFormat(OutputClassificationColorManager colorManager)
            {
                DisplayName = "Xenko BuildEngine Error";
                this.IsBold = true;
                var classificationColor = colorManager.GetClassificationColor(BuildEngineError);
                ForegroundColor = classificationColor.ForegroundColor;
                BackgroundColor = classificationColor.BackgroundColor;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = BuildEngineFatal)]
        [Name(BuildEngineFatal)]
        [UserVisible(true)] //this should be visible to the end user
        [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
        internal sealed class BuildEngineFatalFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public BuildEngineFatalFormat(OutputClassificationColorManager colorManager)
            {
                DisplayName = "Xenko BuildEngine Fatal";
                this.IsBold = true;
                var classificationColor = colorManager.GetClassificationColor(BuildEngineFatal);
                ForegroundColor = classificationColor.ForegroundColor;
                BackgroundColor = classificationColor.BackgroundColor;
            }
        }

    }
}
