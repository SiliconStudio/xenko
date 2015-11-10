using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;

namespace SiliconStudio.Presentation.ValueConverters
{
    [ValueConversion(typeof(string), typeof(FlowDocument))]
    public class TextToFlowDocumentConverter : OneWayValueConverter<TextToFlowDocumentConverter>
    {
        public TextToFlowDocumentConverter()
            : this(null)
        {
        }
        
        public TextToFlowDocumentConverter(Markdown markdown)
        {
            this.Markdown = markdown;
        }

        public Markdown Markdown
        {
            get; set;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var engine = Markdown ?? defaultMarkdown.Value;
            return engine.Transform(value.ToString());
        }

        private readonly Lazy<Markdown> defaultMarkdown = new Lazy<Markdown>(() => new Markdown());
    }
}
