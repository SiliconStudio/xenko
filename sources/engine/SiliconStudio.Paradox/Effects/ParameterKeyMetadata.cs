// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects
{
    public abstract class ParameterKeyMetadata : PropertyKeyMetadata
    {
        public abstract object GetDefaultValue();

        public abstract void SetupDefaultValue(ParameterCollection parameterCollection, ParameterKey parameterKey, bool addDependencies);

        public abstract ParameterDynamicValue DefaultDynamicValue { get; }
    }

    /// <summary>
    /// Metadata used for <see cref="ParameterKey"/>
    /// </summary>
    public class ParameterKeyMetadata<T> : ParameterKeyMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKeyMetadata"/> class.
        /// </summary>
        public ParameterKeyMetadata()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKeyMetadata"/> class.
        /// </summary>
        /// <param name="setupDelegate">The setup delegate.</param>
        public ParameterKeyMetadata(T defaultValue)
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterKeyMetadata&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="defaultDynamicValue">The default dynamic value.</param>
        public ParameterKeyMetadata(ParameterDynamicValue<T> defaultDynamicValue)
        {
            DefaultDynamicValueT = defaultDynamicValue;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public readonly T DefaultValue;

        protected ParameterDynamicValue<T> DefaultDynamicValueT { get; set; }

        public override ParameterDynamicValue DefaultDynamicValue
        {
            get { return DefaultDynamicValueT; }
        }

        public override object GetDefaultValue()
        {
            return DefaultValue;
        }

        public override void SetupDefaultValue(ParameterCollection parameterCollection, ParameterKey parameterKey, bool addDependencies)
        {
            if (DefaultDynamicValueT != null)
            {
                if (addDependencies)
                {
                    foreach (var dependencyKey in DefaultDynamicValueT.Dependencies)
                        parameterCollection.RegisterParameter(dependencyKey, addDependencies);
                }
                parameterCollection.AddDynamic((ParameterKey<T>)parameterKey, DefaultDynamicValueT);
            }
            else
            {
                parameterCollection.Set((ParameterKey<T>)parameterKey, DefaultValue);
            }
        }
    }
}

