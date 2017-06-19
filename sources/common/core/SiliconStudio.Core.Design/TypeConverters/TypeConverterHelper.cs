// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.ComponentModel;

namespace SiliconStudio.Core.TypeConverters
{
    public static class TypeConverterHelper
    {
        /// <summary>
        /// Tries to convert the <paramref name="source"/> to the <paramref name="targetType"/>.
        /// </summary>
        /// <param name="source">The object to convert</param>
        /// <param name="targetType">The type to convert to</param>
        /// <param name="target">The converted object</param>
        /// <returns><c>true</c> if the <paramref name="source"/> could be converted to the <paramref name="targetType"/>; otherwise, <c>false</c>.</returns>
        public static bool TryConvert(object source, Type targetType, out object target)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if(source != null)
            {
                try
                {
                    // Already same type or inherited (also works with interface)
                    if (targetType.IsInstanceOfType(source))
                    {
                        target = source;
                        return true;
                    }

                    if (source is IConvertible)
                    {
                        var typeCode = Type.GetTypeCode(targetType);
                        if (typeCode != TypeCode.Object)
                        {
                            target = Convert.ChangeType(source, targetType);
                            return true;
                        }
                    }

                    var sourceType = source.GetType();
                    // Try to convert using the source type converter
                    var converter = TypeDescriptor.GetConverter(sourceType);
                    if (converter.CanConvertTo(targetType))
                    {
                        target = converter.ConvertTo(source, targetType);
                        return true;
                    }
                    // Try to convert using the target type converter
                    converter = TypeDescriptor.GetConverter(targetType);
                    if (converter.CanConvertFrom(sourceType))
                    {
                        target = converter.ConvertFrom(source);
                        return true;
                    }
                }
                catch (InvalidCastException) { }
                catch (InvalidOperationException) { }
                catch (FormatException) { }
                catch (NotSupportedException) { }
                catch (OverflowException) { }
                catch (Exception ex) when (ex.InnerException is InvalidCastException) { }
                catch (Exception ex) when (ex.InnerException is InvalidOperationException) { }
                catch (Exception ex) when (ex.InnerException is FormatException) { }
                catch (Exception ex) when (ex.InnerException is NotSupportedException) { }
                catch (Exception ex) when (ex.InnerException is OverflowException) { }
            }

            // Incompatible type and no conversion available
            target = null;
            return false;
        }
    }
}
