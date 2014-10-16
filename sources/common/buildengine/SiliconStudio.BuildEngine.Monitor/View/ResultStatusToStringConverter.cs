// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows.Data;

using SiliconStudio.Presentation.ValueConverters;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    /// <summary>
    /// A converter that returns an user-friendly string equals each other, and null otherwise
    /// </summary>
    [ValueConversion(typeof(ResultStatus), typeof(string))]
    class ResultStatusToString : OneWayValueConverter<ResultStatusToString>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ResultStatus)value;
            switch (status)
            {
                case ResultStatus.Successful:
                    return "Successful";
                case ResultStatus.Failed:
                    return "Failed";
                case ResultStatus.Cancelled:
                    return "Cancelled";
                case ResultStatus.NotTriggeredWasSuccessful:
                    return "Skipped - Successful";
                case ResultStatus.NotTriggeredPrerequisiteFailed:
                    return "Not triggered - Failed";
                default:
                    return "In progress";
            }
        }
    }
}
