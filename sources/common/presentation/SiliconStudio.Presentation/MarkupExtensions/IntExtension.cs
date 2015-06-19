using System;
using System.Windows.Markup;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(int))]
    public class IntExtension : MarkupExtension
    {
        public int Value { get; set; }

        public IntExtension(int value)
        {
            Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Value;
        }
    }
}