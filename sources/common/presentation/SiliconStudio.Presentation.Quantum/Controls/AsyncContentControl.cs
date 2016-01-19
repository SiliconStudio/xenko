//// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
//// This file is distributed under GPL v3. See LICENSE.md for details.
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows.Controls;
//using System.Windows;
//using System.Windows.Input;
//using SiliconStudio.Quantum;
//using SiliconStudio.Presentation.Commands;
//using System.Windows.Data;

// TODO: DEPRECATED, KEPT FOR HISTORY

//namespace SiliconStudio.Presentation.Controls
//{
//    public class AsyncContentControl : ContentControl
//    {
//        static AsyncContentControl()
//        {
//            DefaultStyleKeyProperty.OverrideMetadata(typeof(AsyncContentControl), new FrameworkPropertyMetadata(typeof(AsyncContentControl)));
//        }

//        public AsyncContentControl()
//        {
//            this.Loaded += OnLoaded;
//        }

//        protected override void OnInitialized(EventArgs e)
//        {
//            base.OnInitialized(e);

//            UpdateContentStateProperties(LoadState);

//            StoreRealContent();
//            UpdateContentProperties();
//        }

//        private void OnLoaded(object sender, RoutedEventArgs e)
//        {
//            if (LoadState == ViewModelContentState.NotLoaded && LoadContentOnInitialized)
//                RequestContentLoading();
//        }

//        // === LoadContentOnInitialized =========================================================================================

//        public bool LoadContentOnInitialized
//        {
//            get { return (bool)GetValue(LoadContentOnInitializedProperty); }
//            set { SetValue(LoadContentOnInitializedProperty, value); }
//        }

//        public static readonly DependencyProperty LoadContentOnInitializedProperty = DependencyProperty.Register(
//            "LoadContentOnInitialized",
//            typeof(bool),
//            typeof(AsyncContentControl),
//            new PropertyMetadata(false));

//        // === IsContentNotLoaded =========================================================================================

//        public bool IsContentNotLoaded
//        {
//            get { return (bool)GetValue(IsContentNotLoadedProperty); }
//            private set { SetValue(IsContentNotLoadedPropertyKey, value); }
//        }

//        private static readonly DependencyPropertyKey IsContentNotLoadedPropertyKey = DependencyProperty.RegisterReadOnly(
//            "IsContentNotLoaded",
//            typeof(bool),
//            typeof(AsyncContentControl),
//            new PropertyMetadata());
//        public static readonly DependencyProperty IsContentNotLoadedProperty = IsContentNotLoadedPropertyKey.DependencyProperty;

//        // === IsContentLoading =========================================================================================

//        public bool IsContentLoading
//        {
//            get { return (bool)GetValue(IsContentLoadingProperty); }
//            private set { SetValue(IsContentLoadingPropertyKey, value); }
//        }

//        private static readonly DependencyPropertyKey IsContentLoadingPropertyKey = DependencyProperty.RegisterReadOnly(
//            "IsContentLoading",
//            typeof(bool),
//            typeof(AsyncContentControl),
//            new PropertyMetadata());
//        public static readonly DependencyProperty IsContentLoadingProperty = IsContentLoadingPropertyKey.DependencyProperty;

//        // === IsContentLoaded =========================================================================================

//        public bool IsContentLoaded
//        {
//            get { return (bool)GetValue(IsContentLoadedProperty); }
//            private set { SetValue(IsContentLoadedPropertyKey, value); }
//        }

//        private static readonly DependencyPropertyKey IsContentLoadedPropertyKey = DependencyProperty.RegisterReadOnly(
//            "IsContentLoaded",
//            typeof(bool),
//            typeof(AsyncContentControl),
//            new PropertyMetadata());
//        public static readonly DependencyProperty IsContentLoadedProperty = IsContentLoadedPropertyKey.DependencyProperty;

//        // === AwaitingContent =========================================================================================

//        public object AwaitingContent
//        {
//            get { return GetValue(AwaitingContentProperty); }
//            set { SetValue(AwaitingContentProperty, value); }
//        }

//        public static readonly DependencyProperty AwaitingContentProperty = DependencyProperty.Register(
//            "AwaitingContent",
//            typeof(object),
//            typeof(AsyncContentControl));

//        // === AwaitingContentTemplate =========================================================================================

//        public DataTemplate AwaitingContentTemplate
//        {
//            get { return (DataTemplate)GetValue(AwaitingContentTemplateProperty); }
//            set { SetValue(AwaitingContentTemplateProperty, value); }
//        }

//        public static readonly DependencyProperty AwaitingContentTemplateProperty = DependencyProperty.Register(
//            "AwaitingContentTemplate",
//            typeof(DataTemplate),
//            typeof(AsyncContentControl));

//        // === AwaitingContentTemplateSelector =========================================================================================

//        public DataTemplateSelector AwaitingContentTemplateSelector
//        {
//            get { return (DataTemplateSelector)GetValue(AwaitingContentTemplateSelectorProperty); }
//            set { SetValue(AwaitingContentTemplateSelectorProperty, value); }
//        }

//        public static readonly DependencyProperty AwaitingContentTemplateSelectorProperty = DependencyProperty.Register(
//            "AwaitingContentTemplateSelector",
//            typeof(DataTemplateSelector),
//            typeof(AsyncContentControl));

//        // === LoadState =========================================================================================

//        public ViewModelContentState LoadState
//        {
//            get { return (ViewModelContentState)GetValue(LoadStateProperty); }
//            set { SetValue(LoadStateProperty, value); }
//        }

//        public static readonly DependencyProperty LoadStateProperty = DependencyProperty.Register(
//            "LoadState",
//            typeof(ViewModelContentState),
//            typeof(AsyncContentControl),
//            new FrameworkPropertyMetadata(ViewModelContentState.NotLoaded, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLoadStatePropertyChanged),
//            OnValidateLoadStateValue);

//        public static void OnLoadStatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
//        {
//            var acc = (AsyncContentControl)sender;
//            var oldState = (ViewModelContentState)e.OldValue;
//            var newState = (ViewModelContentState)e.NewValue;

//            acc.UpdateContentStateProperties(newState);
//            acc.UpdateContentProperties();

//            acc.OnLoadStateChanged(oldState, newState);
//        }

//        private void UpdateContentStateProperties(ViewModelContentState newState)
//        {
//            IsContentNotLoaded = newState == ViewModelContentState.NotLoaded;
//            IsContentLoading = newState == ViewModelContentState.Loading;
//            IsContentLoaded = newState == ViewModelContentState.Loaded;
//        }

//        protected virtual void OnLoadStateChanged(ViewModelContentState oldValue, ViewModelContentState newValue)
//        {
//            RaiseEvent(new RoutedPropertyChangedEventArgs<ViewModelContentState>(oldValue, newValue, LoadStateChangedEvent));
//        }

//        public static bool OnValidateLoadStateValue(object value)
//        {
//            return Enum.IsDefined(typeof(ViewModelContentState), value);
//        }

//        public static readonly RoutedEvent LoadStateChangedEvent = EventManager.RegisterRoutedEvent(
//            "LoadStateChanged",
//            RoutingStrategy.Bubble,
//            typeof(DependencyPropertyChangedEventHandler),
//            typeof(AsyncContentControl));

//        public event DependencyPropertyChangedEventHandler LoadStateChanged
//        {
//            add { AddHandler(LoadStateChangedEvent, value); }
//            remove { RemoveHandler(LoadStateChangedEvent, value); }
//        }

//        // === RequestContentLoading =========================================================================================

//        public void RequestContentLoading()
//        {
//            OnRequestContentLoading();
//        }

//        protected virtual void OnRequestContentLoading()
//        {
//            var command = LoadContentCommand;
//            if (command != null && command.CanExecute(null)) // TODO: try/catch CanExecute
//                command.Execute(null); // TODO: try/catch Execute
//        }

//        // === LoadContentCommand =========================================================================================

//        public ICommand LoadContentCommand
//        {
//            get { return (ICommand)GetValue(LoadContentCommandProperty); }
//            set { SetValue(LoadContentCommandProperty, value); }
//        }

//        public static readonly DependencyProperty LoadContentCommandProperty = DependencyProperty.Register(
//            "LoadContentCommand",
//            typeof(ICommand),
//            typeof(AsyncContentControl));

//        // === CancelContentLoadingCommand =========================================================================================

//        public ICommand CancelContentLoadingCommand
//        {
//            get { return (ICommand)GetValue(CancelContentLoadingCommandProperty); }
//            set { SetValue(CancelContentLoadingCommandProperty, value); }
//        }

//        public static readonly DependencyProperty CancelContentLoadingCommandProperty = DependencyProperty.Register(
//            "CancelContentLoadingCommand",
//            typeof(ICommand),
//            typeof(AsyncContentControl));

//        // =====================================================================================

//        private object storedContent;
//        private object storedContentTemplate;
//        private object storedContentTemplateSelector;

//        public void StoreRealContent()
//        {
//            storedContent = GetPropertyValue(ContentProperty);
//            storedContentTemplate = GetPropertyValue(ContentTemplateProperty);
//            storedContentTemplateSelector = GetPropertyValue(ContentTemplateSelectorProperty);
//        }

//        private void UpdateContentProperties()
//        {
//            if (LoadState != ViewModelContentState.Loaded)
//            {
//                SetValue(ContentProperty, AwaitingContent);
//                SetValue(ContentTemplateProperty, AwaitingContentTemplate);
//                SetValue(ContentTemplateSelectorProperty, AwaitingContentTemplateSelector);
//            }
//            else
//            {
//                SetPropertyValue(ContentProperty, storedContent);
//                SetPropertyValue(ContentTemplateProperty, storedContentTemplate);
//                SetPropertyValue(ContentTemplateSelectorProperty, storedContentTemplateSelector);
//            }
//        }

//        private object GetPropertyValue(DependencyProperty property)
//        {
//            object result = null;

//            if (BindingOperations.IsDataBound(this, property))
//            {
//                result = BindingOperations.GetBindingBase(this, property);
//                if (result == null)
//                    result = BindingOperations.GetBindingExpressionBase(this, property);
//            }
//            else
//                result = GetValue(property);

//            return result;
//        }

//        private void SetPropertyValue(DependencyProperty property, object value)
//        {
//            if (value is BindingBase)
//                SetBinding(property, (BindingBase)value);
//            else if (value is BindingExpressionBase)
//                SetBinding(property, ((BindingExpressionBase)value).ParentBindingBase);
//            else
//                SetValue(property, value);
//        }
//    }
//}
