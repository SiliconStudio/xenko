// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Quantum.Legacy
{
    /// <inheritdoc/>
    public class ViewModelNode : IViewModelNode
    {
        /// <inheritdoc/>
        public string Name { get { return name; } set { name = value; } }

        /// <inheritdoc/>
        public Guid Guid { get; set; }

        /// <inheritdoc/>
        public virtual IContent Content { get { return content; } set { content = value; content.OwnerNode = this; } }

        /// <inheritdoc/>
        public virtual IViewModelNode Parent { get; set; }

        /// <inheritdoc/>
        public IList<IViewModelNode> Children { get; private set; }

        private IContent content;

        private string name;

        protected ViewModelNode()
        {
            Guid = Guid.NewGuid();
            var properties = new ObservableCollection<IViewModelNode>();
            properties.CollectionChanged += ChildrenOnCollectionChanged;
            Children = properties;
        }

        public ViewModelNode(string name, IContent content)
            : this()
        {
            if (name == null) throw new ArgumentNullException("name");
            if (content == null) throw new ArgumentNullException("content");
            this.content = content;
            this.content.OwnerNode = this;
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0}: [{1}]", Name, Content.Value);
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems != null)
            {
                foreach (IViewModelNode item in notifyCollectionChangedEventArgs.NewItems)
                    item.Parent = this;
            }
            if (notifyCollectionChangedEventArgs.OldItems != null)
            {
                foreach (IViewModelNode item in notifyCollectionChangedEventArgs.OldItems)
                    item.Parent = null;
            }
        }

        public class ViewModelNodeSerializer : DataSerializer<ViewModelNode>
        {
            public override void Serialize(ref ViewModelNode obj, ArchiveMode mode, SerializationStream stream)
            {
                // TODO: wtf?
                stream.Serialize(ref obj.name);
                stream.Serialize(ref obj.name);
            }
        }
    }
}