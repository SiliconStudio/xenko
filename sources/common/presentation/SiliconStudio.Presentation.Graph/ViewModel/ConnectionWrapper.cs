// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Data;
using SiliconStudio.Presentation.Graph.Behaviors;

namespace SiliconStudio.Presentation.Graph.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionWrapper : DependencyObject
    {
        public static DependencyProperty BindingProperty = DependencyProperty.Register(
            "Binding", 
            typeof(Binding),
            typeof(NodeGraphBehavior), 
            new PropertyMetadata(OnBindingChanged));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var connectionWrapper = (ConnectionWrapper)d;
            connectionWrapper.OnBindingChanged(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnBindingChanged(DependencyPropertyChangedEventArgs e)
        {
            // nothing
        }

        /// <summary>
        /// 
        /// </summary>
        public Binding Binding { get { return (Binding)GetValue(BindingProperty); } set { SetValue(BindingProperty, value); } }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SingleConnectionWrapper : ConnectionWrapper { }    
}
