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
    public class Symbol : IIdentifiable, IAssetPartDesign<Symbol>
    {
        public Symbol()
        {
            Id = Guid.NewGuid();
        }

        public Symbol(string type, string name)
            : this()
        {
            Name = name;
            Type = type;
        }

        [DataMember(-100), Display(Browsable = false)]
        [NonOverridable]
        public Guid Id { get; set; }

        /// <inheritdoc/>
        [DataMember(-90), Display(Browsable = false)]
        [DefaultValue(null)]
        public BasePart Base { get; set; }

        /// <summary>
        /// Gets or sets the name of that variable.
        /// </summary>
        [DataMember(-50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of that variable.
        /// </summary>
        [DataMember(-40)]
        public string Type { get; set; }

        Symbol IAssetPartDesign<Symbol>.Part { get { return this; } set { throw new InvalidOperationException(); } }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
}
