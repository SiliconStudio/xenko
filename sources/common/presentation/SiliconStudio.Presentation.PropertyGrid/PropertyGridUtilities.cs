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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using SiliconStudio.Presentation.Controls.PropertyGrid.Editors;

using System.Windows.Controls;

namespace SiliconStudio.Presentation.Controls.PropertyGrid
{
    internal class PropertyGridUtilities
    {
        private static Style _noBorderControlStyle;
        private static Style _propertyGridComboBoxStyle;

        internal static Style NoBorderControlStyle
        {
            get
            {
                if (_noBorderControlStyle == null)
                {
                    var style = new Style(typeof(Control));
                    var trigger = new MultiTrigger();
                    trigger.Conditions.Add(new Condition(UIElement.IsKeyboardFocusWithinProperty, false));
                    trigger.Conditions.Add(new Condition(UIElement.IsMouseOverProperty, false));
                    trigger.Setters.Add(
                      new Setter(Control.BorderBrushProperty, new SolidColorBrush(Colors.Transparent)));
                    style.Triggers.Add(trigger);

                    _noBorderControlStyle = style;
                }

                return _noBorderControlStyle;
            }
        }

        internal static Style ComboBoxStyle
        {
            get
            {
                if (_propertyGridComboBoxStyle == null)
                {
                    var style = new Style(typeof(Control));
                    var trigger = new MultiTrigger();
                    trigger.Conditions.Add(new Condition(UIElement.IsKeyboardFocusWithinProperty, false));
                    trigger.Conditions.Add(new Condition(UIElement.IsMouseOverProperty, false));
                    trigger.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(Colors.Transparent)));
                    trigger.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.Transparent)));
                    style.Triggers.Add(trigger);

                    _propertyGridComboBoxStyle = style;
                }
                return _propertyGridComboBoxStyle;
            }
        }

        internal static T GetAttribute<T>(PropertyDescriptor property) where T : Attribute
        {
            return property.Attributes.OfType<T>().FirstOrDefault();
        }

        internal static ITypeEditor CreateDefaultEditor(Type propertyType, TypeConverter typeConverter)
        {
            return (typeConverter != null && typeConverter.CanConvertFrom(typeof(string))) ? (ITypeEditor)new TextBoxEditor() : new TextBlockEditor();
        }
    }
}
