// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A scene.
    /// </summary>
    [DataContract("Scene")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<Scene>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Scene>), Profile = "Content")]
    public sealed class Scene : PrefabBase
    {
        private Scene parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene()
        {
            Children = new TrackingCollection<Scene>();
            Children.CollectionChanged += ChildrenCollectionChanged;
        }

        [DataMemberIgnore]
        public Scene Parent
        {
            get { return parent; }
            set
            {
                var oldParent = Parent;
                if (oldParent == value)
                    return;

                oldParent?.Children.Remove(this);
                value?.Children.Add(this);
            }
        }

        [DataMemberIgnore]
        public TrackingCollection<Scene> Children { get; }

        public override string ToString()
        {
            return $"Scene {Name}";
        }

        private void ChildrenCollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItem((Scene)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItem((Scene)e.Item);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void AddItem(Scene item)
        {
            if (item.Parent != null)
                throw new InvalidOperationException("This Scene already has a Parent, detach it first.");

            item.parent = this;
        }

        private void RemoveItem(Scene item)
        {
            if (item.Parent != this)
                throw new InvalidOperationException("This Scene's parent is not the expected value.");

            item.parent = null;
        }
    }
}
