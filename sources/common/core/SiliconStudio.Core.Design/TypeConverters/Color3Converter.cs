using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Core.TypeConverters
{
    /// <summary>
    /// Defines a type converter for <see cref="Color3"/>.
    /// </summary>
    public class Color3Converter : BaseConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Color3Converter"/> class.
        /// </summary>
        public Color3Converter()
        {
            var type = typeof(Color3);
            Properties = new PropertyDescriptorCollection(new [] 
            { 
                new FieldPropertyDescriptor(type.GetField("R")), 
                new FieldPropertyDescriptor(type.GetField("G")),
                new FieldPropertyDescriptor(type.GetField("B")),
                new FieldPropertyDescriptor(type.GetField("A"))
            });
        }

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));

            if (value is Color3)
            {
                var color = (Color3)value;

                if (destinationType == typeof(string))
                {
                    return color.ToString();
                }
                if (destinationType == typeof(Color))
                {
                    return (Color)color;
                }
                if (destinationType == typeof(Color4))
                {
                    return color.ToColor4();
                }

                if (destinationType == typeof(InstanceDescriptor))
                {
                    var constructor = typeof(Color3).GetConstructor(MathUtil.Array(typeof(float), 4));
                    if (constructor != null)
                        return new InstanceDescriptor(constructor, color.ToArray());
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is Color)
            {
                var color = (Color)value;
                return color.ToColor3();
            }
            if (value is Color4)
            {
                var color = (Color4)value;
                return color.ToColor3();
            }

            var str = value as string;
            if (str != null)
            {
                var colorValue = ColorExtensions.StringToRgba(str);
                return new Color3(colorValue);
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
            return new Color3((float)propertyValues[nameof(Color.R)], (float)propertyValues[nameof(Color.G)], (float)propertyValues[nameof(Color.B)]);
        }
    }
}
