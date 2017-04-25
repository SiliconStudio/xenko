// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// A group that is used to distinguish between different agent types with it's <see cref="Id"/> used at run-time to acquire the navigation mesh for a group
    /// </summary>
    [DataContract]
    [ObjectFactory(typeof(NavigationMeshGroupFactory))]
    [InlineProperty]
    public class NavigationMeshGroup : IIdentifiable
    {
        [DataMember(-10)]
        [Display(Browsable = false)]
        [NonOverridable]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Display name
        /// </summary>
        [DataMember(0)]
        [InlineProperty]
        public string Name { get; set; }

        /// <summary>
        /// Agent settings for this group
        /// </summary>
        [DataMember(5)]
        public NavigationAgentSettings AgentSettings;
        
        protected bool Equals(NavigationMeshGroup other)
        {
            return string.Equals(Name, other.Name) && Equals(AgentSettings, other.AgentSettings) && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NavigationMeshGroup)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AgentSettings != null ? AgentSettings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
    
    public class NavigationMeshGroupFactory : IObjectFactory
    {
        public object New(Type type)
        {
            return new NavigationMeshGroup
            {
                Name = "New group",
                AgentSettings = ObjectFactoryRegistry.NewInstance<NavigationAgentSettings>(),
            };
        }
    }
}
