// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Link : IIdentifiable, IAssetPartDesign<Link>
    {
        public Link()
        {
            Id = Guid.NewGuid();
        }

        public Link(Slot source, Slot target)
             : this()
        {
            Source = source;
            Target = target;
        }

        public Link(ExecutionBlock source, Slot target)
             : this()
        {
            Source = source.OutputExecution;
            Target = target;
        }

        public Link(Slot source, ExecutionBlock target)
     : this()
        {
            Source = source;
            Target = target.InputExecution;
        }

        public Link(ExecutionBlock source, ExecutionBlock target)
             : this()
        {
            Source = source.OutputExecution;
            Target = target.InputExecution;
        }

        /// <summary>
        /// The function that contains this link.
        /// </summary>
        [DataMemberIgnore]
        public Method Method { get; internal set; }


        [DataMember(-100), Display(Browsable = false)]
        [NonOverridable]
        public Guid Id { get; set; }

        /// <inheritdoc/>
        [DataMember(-90), Display(Browsable = false)]
        [DefaultValue(null)]
        public BasePart Base { get; set; }

        public Slot Source { get; set; }

        public Slot Target { get; set; }

        /// <inheritdoc/>
        Link IAssetPartDesign<Link>.Part { get { return this; } set { throw new InvalidOperationException(); } }
    }
}
