// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class SingleNodeViewModel : NodeViewModel
    {
        protected string[] DisplayNameDependentProperties;
        protected Func<string> DisplayNameProvider;
        private List<DependencyPath> dependencies;

        static SingleNodeViewModel()
        {
            typeof(SingleNodeViewModel).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleNodeViewModel"/> class.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="GraphViewModel"/> that owns the new <see cref="SingleNodeViewModel"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <see cref="Index.Empty"/> must be passed otherwise</param>
        protected SingleNodeViewModel(GraphViewModel ownerViewModel, Type type, string baseName, Index index)
            : base(ownerViewModel, type, index)
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
        /// The reference expansion policy chosen while generating children for this node.
        /// </summary>
        /// <remarks>
        /// This can be customized by <see cref="IPropertiesProviderViewModel.ShouldConstructChildren"/>.
        /// </remarks>
        public ExpandReferencePolicy ExpandReferencePolicy { get; protected set; } = ExpandReferencePolicy.None;

        /// <inheritdoc/>
        public override void Destroy()
        {
            if (dependencies != null)
            {
                Owner.NodeValueChanged -= DependencyNodeValueChanged;
                dependencies = null;
            }
            base.Destroy();
        }

        /// <inheritdoc cref="NodeViewModel.AddCommand"/>
        public new void AddCommand([NotNull] INodeCommandWrapper command)
        {
            base.AddCommand(command);
        }

        /// <inheritdoc cref="NodeViewModel.RemoveCommand"/>
        public new bool RemoveCommand([NotNull] INodeCommandWrapper command)
        {
            return base.RemoveCommand(command);
        }

        /// <summary>
        /// Adds a dependency to the node represented by the given path.
        /// </summary>
        /// <param name="nodePath">The path to the node that should be a dependency of this node.</param>
        /// <param name="refreshOnNestedNodeChanges">If true, this node will also be refreshed when one of the child node of the dependency node changes.</param>
        /// <remarks>A node that is a dependency to this node will trigger a refresh of this node each time its value is modified (or the value of one of its parent).</remarks>
        public void AddDependency(string nodePath, bool refreshOnNestedNodeChanges)
        {
            if (string.IsNullOrEmpty(nodePath)) throw new ArgumentNullException(nameof(nodePath));

            if (dependencies == null)
            {
                dependencies = new List<DependencyPath>();
                Owner.NodeValueChanged += DependencyNodeValueChanged;
            }

            dependencies.Add(new DependencyPath(nodePath, refreshOnNestedNodeChanges));
        }

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

        public VirtualNodeViewModel CreateVirtualChild(string name, Type contentType, bool isPrimitive, int? order, Index index, Func<object> getter, Action<object> setter, IReadOnlyDictionary<string, object> nodeAssociatedData = null)
        {
            var child = new VirtualNodeViewModel(Owner, contentType, name, isPrimitive, order, index, getter, setter);
            nodeAssociatedData?.ForEach(x => child.AddAssociatedData(x.Key, x.Value));
            child.FinalizeInitialization();
            AddChild(child);
            return child;
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
            else if (!index.IsEmpty)
            {
                // TODO: make a better interface for custom naming specification
                var propertyKey = index.Value as PropertyKey;
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
                    if (index.IsInt)
                        Name = "Item " + index.ToString().Replace(".", "-");
                    else
                        Name = index.ToString().Replace(".", "-");

                    DisplayName = Name;
                }
            }

            Name = EscapeName(Name);
        }

        private void DependencyNodeValueChanged(object sender, GraphViewModelNodeValueChanged e)
        {
            if (dependencies?.Any(x => x.ShouldRefresh(e.NodePath)) ?? false)
            {
                Refresh();
            }
        }

        private struct DependencyPath
        {
            private readonly string path;
            private readonly bool refreshOnNestedNodeChanges;

            public DependencyPath(string path, bool refreshOnNestedNodeChanges)
            {
                this.path = path;
                this.refreshOnNestedNodeChanges = refreshOnNestedNodeChanges;
            }

            public bool ShouldRefresh(string modifiedNodePath)
            {
                if (IsContainingPath(modifiedNodePath, path))
                {
                    // The node that has changed is the dependent node or one of its parent, let's refresh
                    return true;
                }
                if (refreshOnNestedNodeChanges && IsContainingPath(path, modifiedNodePath))
                {
                    // The node that has changed is a child of the dependent node, and we asked for recursive dependencies, let's refresh
                    return true;
                }

                return false;

            }

            private static bool IsContainingPath(string containerPath, string containedPath)
            {
                if (!containedPath.StartsWith(containerPath))
                    return false;

                // Check if the strings are actually identical, or if the next character in the contained path is a property separator ('.')
                return containedPath.Length == containerPath.Length || containedPath[containerPath.Length] == '.';
            }

        }
    }
}
