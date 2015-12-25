// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class SingleObservableNode : ObservableNode
    {
        protected string[] DisplayNameDependentProperties;
        protected Func<string> DisplayNameProvider;

        static SingleObservableNode()
        {
            typeof(SingleObservableNode).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleObservableNode"/> class.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="ObservableViewModel"/> that owns the new <see cref="SingleObservableNode"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
        protected SingleObservableNode(ObservableViewModel ownerViewModel, string baseName, object index = null)
            : base(ownerViewModel, index)
        {
            if (baseName == null && index == null)
                throw new ArgumentException("baseName and index can't be both null.");

            CombineMode = CombineMode.CombineOnlyForAll;
            SetName(baseName);
        }

        /// <summary>
        /// Gets or sets the <see cref="CombineMode"/> of this single node.
        /// </summary>
        public CombineMode CombineMode { get; set; }

        /// <summary>
        /// Registers a function that can compute the display name of this node. If the function uses some children of this node to compute
        /// the display name, the name of these children can be passed so the function is re-evaluated each time one of these children value changes.
        /// </summary>
        /// <param name="provider">A function that can compute the display name of this node.</param>
        /// <param name="dependentProperties">The names of children that should trigger the re-evaluation of the display name when they are modified.</param>
        public void SetDisplayNameProvider(Func<string> provider, params string[] dependentProperties)
        {
            DisplayNameProvider = provider;
            DisplayNameDependentProperties = dependentProperties?.Select(EscapeName).ToArray();
            if (provider != null)
                DisplayName = provider();
        }

        public VirtualObservableNode CreateVirtualChild(string name, Type contentType, bool isPrimitive, int? order, object index, Func<object> getter, Action<object> setter, IReadOnlyDictionary<string, object> nodeAssociatedData = null)
        {
            var observableChild = VirtualObservableNode.Create(Owner, name, contentType, isPrimitive, order, index, getter, setter);
            nodeAssociatedData?.ForEach(x => observableChild.AddAssociatedData(x.Key, x.Value));
            observableChild.FinalizeChildrenInitialization();
            AddChild(observableChild);
            return observableChild;
        }

        public Deprecated_VirtualObservableNode CreateVirtualChild(string name, Type contentType, int? order, bool isPrimitive, object initialValue, object index, NodeCommandWrapperBase valueChangedCommand, IReadOnlyDictionary<string, object> nodeAssociatedData)
        {
            var observableChild = Deprecated_VirtualObservableNode.Create(Owner, name, order, isPrimitive, contentType, initialValue, index, valueChangedCommand);
            if (nodeAssociatedData != null)
            {
                foreach (var data in nodeAssociatedData)
                {
                    observableChild.AddAssociatedData(data.Key, data.Value);
                }
            }
            observableChild.FinalizeChildrenInitialization();
            AddChild(observableChild);
            return observableChild;
        }

        protected override void OnPropertyChanged(params string[] propertyNames)
        {
            base.OnPropertyChanged(propertyNames);
            if (DisplayNameProvider != null && DisplayNameDependentProperties != null)
            {
                if (propertyNames.Any(x => DisplayNameDependentProperties.Contains(x)))
                {
                    DisplayName = DisplayNameProvider();
                }
            }
        }

        private void SetName(string nodeName)
        {
            var index = Index;
            nodeName = nodeName?.Replace(".", "-");
            
            if (!string.IsNullOrWhiteSpace(nodeName))
            {
                Name = nodeName;
                DisplayName = Utils.SplitCamelCase(nodeName);
            }
            else if (index != null)
            {
                // TODO: make a better interface for custom naming specification
                var propertyKey = index as PropertyKey;
                if (propertyKey != null)
                {
                    string name = propertyKey.Name.Replace(".", "-");

                    if (name == "Key")
                        name = propertyKey.PropertyType.Name.Replace(".", "-");

                    Name = name;
                    var parts = propertyKey.Name.Split('.');
                    DisplayName = parts.Length == 2 ? $"{parts[1]} ({parts[0]})" : name;
                }
                else
                {
                    if (index.GetType().IsNumeric())
                        Name = "Item " + index.ToString().Replace(".", "-");
                    else
                        Name = index.ToString().Replace(".", "-");

                    DisplayName = Name;
                }
            }

            Name = EscapeName(Name);
        }
    }
}
