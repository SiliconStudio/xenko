/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    internal class ListConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null)
                return null;

            var names = value as string;

            var list = new List<object>();
            if (names == null)
            {
                list.Add(value);
            }
            else
            {
                list.AddRange(names.Split(',').Select(x => x.Trim()));
            }

            return new ReadOnlyCollection<object>(list);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string))
                throw new InvalidOperationException("Can only convert to string.");


            var strs = (IList)value;

            if (strs == null)
                return null;

            var sb = new StringBuilder();
            bool first = true;
            foreach (object o in strs)
            {
                if (o == null)
                    throw new InvalidOperationException("Property names cannot be null.");

                var s = o as string;
                if (s == null)
                    throw new InvalidOperationException("Does not support serialization of non-string property names.");

                if (s.Contains(','))
                    throw new InvalidOperationException("Property names cannot contain commas.");

                if (s.Trim().Length != s.Length)
                    throw new InvalidOperationException("Property names cannot start or end with whitespace characters.");

                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;

                sb.Append(s);
            }

            return sb.ToString();
        }
    }
}
