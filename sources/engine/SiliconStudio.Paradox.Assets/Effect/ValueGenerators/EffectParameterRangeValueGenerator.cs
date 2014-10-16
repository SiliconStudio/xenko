// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Effect.ValueGenerators
{
    /// <summary>
    /// An value range generator.
    /// </summary>
    [DataContract("!fxparam.range")]
    public class EffectParameterRangeValueGenerator : IEffectParameterValueGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffectParameterRangeValueGenerator"/> class.
        /// </summary>
        public EffectParameterRangeValueGenerator()
        {
            Step = 1;
        }

        /// <summary>
        /// Gets or sets 'from' value.
        /// </summary>
        /// <value>From.</value>
        [DataMember(10)]
        public double From { get; set; }

        /// <summary>
        /// Gets or sets 'to' value.
        /// </summary>
        /// <value>The automatic.</value>
        [DataMember(20)]
        public double To { get; set; }

        /// <summary>
        /// Gets or sets the step.
        /// </summary>
        /// <value>The step.</value>
        [DataMember(30)]
        [DefaultValue(1)]
        public double Step { get; set; }

        IEnumerable<object> IEffectParameterValueGenerator.GenerateValues(ParameterKey key)
        {
            if (!((From < To && Step <= 0) || (From > To && Step >= 0))) // condition to have a finite set of value within the range
            {
                //var converType = key.PropertyType;
                // TODO: code is not safe if step is zero or (from-to)/step is very high
                if (From == To)
                {
                    yield return key.ConvertValue(From);//Convert.ChangeType(From, converType));
                }
                else if (From < To)
                {
                    for (var value = From; value - To < 0.000001; value += Step)
                        yield return key.ConvertValue(value);//Convert.ChangeType(value, converType);
                }
                else if (From > To)
                {
                    for (var value = From; To - value < 0.000001; value += Step)
                        yield return key.ConvertValue(value);//Convert.ChangeType(value, converType);
                }
            }
        }

        void IEffectParameterValueGenerator.AddValue(ParameterKey key, object value)
        {
            // do nothing
        }
    }
}