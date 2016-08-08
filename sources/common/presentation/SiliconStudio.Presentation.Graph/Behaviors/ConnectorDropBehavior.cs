// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Graph.Behaviors
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ConnectorDropBehavior : Behavior<FrameworkElement>
    {
        #region IDropHandler Interface
        /// <summary>
        /// 
        /// </summary>
        public interface IDropHandler
        {
            void OnDragOver(object sender, DragEventArgs e);
            void OnDrop(object sender, DragEventArgs e);
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DropHandlerProperty = DependencyProperty.Register("DropHandler", typeof(IDropHandler), typeof(ConnectorDropBehavior));
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ConnectorDropBehavior()
        {
            // nothing
        }
        #endregion

        #region Attach & Detach Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            
            AssociatedObject.Drop += OnDropEvent;
            AssociatedObject.DragOver += OnDragOverEvent;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.Drop -= OnDropEvent;
            AssociatedObject.DragOver -= OnDragOverEvent;
            base.OnDetaching();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDropEvent(object sender, DragEventArgs e)
        {

            if (DropHandler != null)
            {
                DropHandler.OnDrop(sender, e);                
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDragOverEvent(object sender, DragEventArgs e)
        {
            if (DropHandler != null)
            {
                DropHandler.OnDragOver(sender, e);
                
            }
        }
        #endregion

        #region Properties
        public IDropHandler DropHandler { get { return (IDropHandler)GetValue(DropHandlerProperty); } set { SetValue(DropHandlerProperty, value); } }
        #endregion
    }
}
