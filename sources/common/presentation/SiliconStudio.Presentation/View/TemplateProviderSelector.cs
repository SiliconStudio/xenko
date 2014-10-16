// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Presentation.View
{
    /// <summary>
    /// An implementation of <see cref="DataTemplateSelector"/> that can select a template from a set of statically registered <see cref="ITemplateProvider"/> objects.
    /// </summary>
    /// <remarks>This class is a singleton and cannot be instanced. To reference this selector, use the static member <see cref="Instance"/>.</remarks>
    public class TemplateProviderSelector : DataTemplateSelector
    {
        /// <summary>
        /// The singleton instance of the <see cref="TemplateProviderSelector"/> class.
        /// </summary>
        public static readonly TemplateProviderSelector Instance = new TemplateProviderSelector();
        
        /// <summary>
        /// The list of all template providers registered for the <see cref="TemplateProviderSelector"/>, indexed by their name.
        /// </summary>
        private static readonly List<ITemplateProvider> TemplateProviders = new List<ITemplateProvider>();

        /// <summary>
        /// A hashset of template provider names, used only to ensure unicity.
        /// </summary>
        private static readonly HashSet<string> TemplateProviderNames = new HashSet<string>();

        /// <summary>
        /// A map of all providers that have already been used for each object, indexed by <see cref="Guid"/>.
        /// </summary>
        private static ConditionalWeakTable<object, List<string>> usedProviders = new ConditionalWeakTable<object, List<string>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateProviderSelector"/> class.
        /// </summary>
        /// <remarks>This constructor is private because this class is a singleton.</remarks>
        private TemplateProviderSelector()
        {
        }

        /// <summary>
        /// Registers the given template into the static <see cref="TemplateProviderSelector"/>.
        /// </summary>
        /// <param name="templateProvider"></param>
        public static void RegisterTemplateProvider(ITemplateProvider templateProvider)
        {
            if (templateProvider == null) throw new ArgumentNullException("templateProvider");

            if (TemplateProviderNames.Contains(templateProvider.Name))
                throw new InvalidOperationException("A template provider with the same name has already been registered in this template selector.");

            InsertTemplateProvider(TemplateProviders, templateProvider, new List<ITemplateProvider>());
            TemplateProviderNames.Add(templateProvider.Name);
        }

        /// <summary>
        /// Unregisters the given template into the static <see cref="TemplateProviderSelector"/>.
        /// </summary>
        /// <param name="templateProvider"></param>
        public static void UnregisterTemplateProvider(ITemplateProvider templateProvider)
        {
            TemplateProviderNames.Remove(templateProvider.Name);
            TemplateProviders.Remove(templateProvider);
        }

        /// <summary>
        /// Resets the list of used template providers, allowing to fully re-template observable nodes.
        /// </summary>
        public static void ResetProviders()
        {
            usedProviders = new ConditionalWeakTable<object, List<string>>();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            var element = container as FrameworkElement;
            if (element == null)
                throw new ArgumentException(@"Container must be of type FrameworkElement", "container");

            var provider = FindTemplateProvider(item);
            var template = provider.Template;
            // We set the template we found into the content presenter itself to avoid re-entering the template selector if the property is refreshed.
            var contentPresenter = container as ContentPresenter;
            if (contentPresenter == null)
                throw new ArgumentException("The container of an TemplateProviderSelector must be a ContentPresenter.");

            contentPresenter.ContentTemplate = template;
            return template;
        }
        
        private static void InsertTemplateProvider(List<ITemplateProvider> list, ITemplateProvider templateProvider, List<ITemplateProvider> movedItems)
        {
            movedItems.Add(templateProvider);
            // Find the first index where we can insert
            int insertIndex = 1 + list.LastIndexOf(x => x.CompareTo(templateProvider) < 0);
            list.Insert(insertIndex, templateProvider);
            // Every following providers may have an override rule against the new template provider, we must potentially resort them.
            for (int i = insertIndex + 1; i < list.Count; ++i)
            {
                if (list[i].CompareTo(list[insertIndex]) < 0)
                {
                    if (!movedItems.Contains(list[i]))
                    {
                        list.RemoveAt(i);
                        InsertTemplateProvider(list, list[i], movedItems);
                    }
                }
            }
        }

        /// <summary>
        /// Obtains a template provider for the given object, that has not been used yet since the last call to <see cref="ResetProviders"/>, or
        /// null if no (more) template provider matches the given object.
        /// </summary>
        /// <param name="item">The object for which to find a template provider.</param>
        /// <returns> a template provider for the given object, that has not been used yet since the last call to <see cref="ResetProviders"/>, or null if no (more) template provider matches the given object.</returns>
        private static ITemplateProvider FindTemplateProvider(object item)
        {
            List<string> userProvidersForItem = usedProviders.GetOrCreateValue(item);

            var availableSelectors = TemplateProviders.Where(x => x.Match(item)).ToList();
            ITemplateProvider result = availableSelectors.FirstOrDefault(selector => !userProvidersForItem.Contains(selector.Name));
            if (result == null)
            {
                // No unused template found, lets use the first available template and reset the used provider list.
                result = availableSelectors.FirstOrDefault();
                userProvidersForItem.Clear();
            }
            if (result != null)
            {
                userProvidersForItem.Add(result.Name);
            }
            return result;
        }
    }
}
