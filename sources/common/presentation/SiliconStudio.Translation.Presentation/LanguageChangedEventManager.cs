// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Windows;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Translation.Presentation
{
    public class LanguageChangedEventManager : WeakEventManager
    {
        public static void AddListener(ITranslationManager source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(ITranslationManager source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        private void OnLanguageChanged(object sender, [NotNull] EventArgs e)
        {
            DeliverEvent(sender, e);
        }

        protected override void StartListening(object source)
        {
            var manager = (ITranslationManager)source;
            manager.LanguageChanged += OnLanguageChanged;
        }

        protected override void StopListening(object source)
        {
            var manager = (ITranslationManager)source;
            manager.LanguageChanged -= OnLanguageChanged;
        }

        [NotNull]
        private static LanguageChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(LanguageChangedEventManager);
                var manager = (LanguageChangedEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new LanguageChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }
    }
}