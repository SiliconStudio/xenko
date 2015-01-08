// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// A context used when mixin <see cref="ShaderSource"/>.
    /// </summary>
    public class ShaderMixinContext
    {
        private readonly ParameterCollection compilerParameters;
        private readonly Stack<ParameterCollection> parameterCollections = new Stack<ParameterCollection>();
        private readonly Dictionary<string, IShaderMixinBuilder> registeredBuilders;
        private readonly Stack<int> compositionIndices = new Stack<int>();
        private readonly StringBuilder compositionStringBuilder = new StringBuilder();

        private string compositionString = null;

        private ShaderMixinSourceTree currentMixinSourceTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinContext" /> class.
        /// </summary>
        /// <param name="compilerParameters">The default property container.</param>
        /// <param name="registeredBuilders">The registered builders.</param>
        /// <exception cref="System.ArgumentNullException">compilerParameters
        /// or
        /// registeredBuilders</exception>
        public ShaderMixinContext(ParameterCollection compilerParameters, Dictionary<string, IShaderMixinBuilder> registeredBuilders)
        {
            if (compilerParameters == null)
                throw new ArgumentNullException("compilerParameters");

            if (registeredBuilders == null)
                throw new ArgumentNullException("registeredBuilders");

            // TODO: use a copy of the compilerParameters?
            this.compilerParameters = compilerParameters;
            this.registeredBuilders = registeredBuilders;
            this.parameterCollections = new Stack<ParameterCollection>();
        }

        /// <summary>
        /// Pushes the current parameters collection being used.
        /// </summary>
        /// <typeparam name="T">Type of the parameter collection</typeparam>
        /// <param name="parameterCollection">The property container.</param>
        public void PushParameters<T>(T parameterCollection) where T : ParameterCollection
        {
            parameterCollections.Push(parameterCollection);
        }

        /// <summary>
        /// Pops the parameters collection.
        /// </summary>
        public void PopParameters()
        {
            parameterCollections.Pop();
        }

        public ShaderMixinSource CurrentMixin
        {
            get
            {
                return currentMixinSourceTree.Mixin;
            }
        }

        /// <summary>
        /// Gets a parameter value for the specified key.
        /// </summary>
        /// <typeparam name="T">Type of the parameter value</typeparam>
        /// <param name="paramKey">The parameter key.</param>
        /// <returns>The value or default value associated to this parameter key.</returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public T GetParam<T>(ParameterKey<T> paramKey)
        {
            if (paramKey == null)
                throw new ArgumentNullException("paramKey");

            var globalKey = paramKey;
            var composeKey = GetComposeKey(paramKey);
            var selectedKey = globalKey;
            ParameterCollection sourceParameters = null;

            // Try first if a composite key with a value is available for the key
            if (composeKey != globalKey)
            {
                sourceParameters = FindKeyValue(composeKey, out selectedKey);
            }

            // Else try using global key
            if (sourceParameters == null)
            {
                sourceParameters = FindKeyValue(globalKey, out selectedKey);
            }

            // If nothing found, use composeKey and global compiler parameters
            if (sourceParameters == null)
            {
                selectedKey = composeKey;
                sourceParameters = compilerParameters;
            }

            // Gets the value from a source parameters
            var value = sourceParameters.Get(selectedKey);

            // Sore only used parameters when they are taken from compilerParameters
            if (sourceParameters == compilerParameters)
            {
                currentMixinSourceTree.UsedParameters.Set(selectedKey, value);
            }

            return value;
        }

        private ParameterCollection FindKeyValue<T>(ParameterKey<T> key, out ParameterKey<T> selectedKey)
        {
            // Try to get a value from registered containers
            selectedKey = null;
            foreach (var parameterCollection in parameterCollections)
            {
                if (parameterCollection.ContainsKey(key))
                {
                    selectedKey = key;
                    return parameterCollection;
                }
            }
            if (compilerParameters.ContainsKey(key))
            {
                selectedKey = key;
                return compilerParameters;
            }
            
            return null;
        }

        private ParameterKey<T> GetComposeKey<T>(ParameterKey<T> key)
        {
            if (compositionString == null)
            {
                return key;
            }
            key = key.ComposeWith(compositionString);
            return key;
        }

        public void SetParam<T>(ParameterKey<T> key, T value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var propertyContainer = parameterCollections.Count > 0 ? parameterCollections.Peek() : compilerParameters;
            propertyContainer.Set(key, value);
        }

        /// <summary>
        /// Removes the specified mixin from this instance.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="name">The name.</param>
        public void RemoveMixin(ShaderMixinSourceTree mixinTree, string name)
        {
            var mixinParent = mixinTree.Mixin;
            for (int i = mixinParent.Mixins.Count - 1; i >= 0; i--)
            {
                var mixin = mixinParent.Mixins[i];
                if (mixin.ClassName == name)
                {
                    mixinParent.Mixins.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Mixins a <see cref="ShaderMixinSource" /> into the specified mixin tree.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="name">The name.</param>
        public void Mixin(ShaderMixinSourceTree mixinTree, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", "Invalid null mixin name");
            }

            IShaderMixinBuilder builder;
            if (!registeredBuilders.TryGetValue(name, out builder))
            {
                // Else simply add the name of the shader
                mixinTree.Mixin.Mixins.Add(new ShaderClassSource(name));
            }
            else if (builder != null)
            {
                builder.Generate(mixinTree, this);
            }
        }

        /// <summary>
        /// Mixins a <see cref="ShaderClassSource" /> identified by its name/generic parameters into the specified mixin tree.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="name">The name.</param>
        /// <param name="genericParameters">The generic parameters.</param>
        /// <exception cref="System.InvalidOperationException">If the class source doesn't support generic parameters</exception>
        public void Mixin(ShaderMixinSourceTree mixinTree, string name, params object[] genericParameters)
        {
            IShaderMixinBuilder builder;
            if (!registeredBuilders.TryGetValue(name, out builder))
            {
                // Else simply add the name of the shader
                mixinTree.Mixin.Mixins.Add(new ShaderClassSource(name, genericParameters));
            } 
            else if (builder != null)
            {
                if (genericParameters.Length != 0)
                {
                    throw new InvalidOperationException(string.Format("Generic Parameters are not supported with [{0}]", builder.GetType().GetTypeInfo().Name));
                }
                builder.Generate(mixinTree, this);
            }
        }

        public void PushComposition(ShaderMixinSourceTree mixin, string compositionName, ShaderMixinSourceTree composition)
        {
            mixin.Mixin.AddComposition(compositionName, composition.Mixin);

            compositionIndices.Push(compositionStringBuilder.Length);
            if (compositionString != null)
            {
                compositionStringBuilder.Insert(0, '.');
            }

            compositionStringBuilder.Insert(0, compositionName);

            compositionString = compositionStringBuilder.ToString();
        }

        public void PushCompositionArray(ShaderMixinSourceTree mixin, string compositionName, ShaderMixinSourceTree composition)
        {
            int arrayIndex = mixin.Mixin.AddCompositionToArray(compositionName, composition.Mixin);

            compositionIndices.Push(compositionStringBuilder.Length);
            if (compositionString != null)
            {
                compositionStringBuilder.Insert(0, '.');
            }

            compositionStringBuilder.Insert(0, ']');
            compositionStringBuilder.Insert(0, arrayIndex);
            compositionStringBuilder.Insert(0, '[');
            compositionStringBuilder.Insert(0, compositionName);

            compositionString = compositionStringBuilder.ToString();
        }

        public void PopComposition()
        {
            var compositionIndex = compositionIndices.Pop();
            compositionStringBuilder.Length = compositionIndex;
            compositionString = compositionIndex == 0 ? null : compositionStringBuilder.ToString();
        }

        /// <summary>
        /// Creates a new ParameterCollection for a child shader.
        /// </summary>
        public void BeginChild(ShaderMixinSourceTree subMixin)
        {
            var parent = currentMixinSourceTree;
            subMixin.Parent = parent;
            subMixin.UsedParameters = new ShaderMixinParameters();
            currentMixinSourceTree = subMixin;

            if (parent != null)
            {
                subMixin.Parent.Children.Add(subMixin);
                // Copy used parameters
                subMixin.Parent.UsedParameters.CopyTo(currentMixinSourceTree.UsedParameters);

                // TODO: cache ParameterCollection
                PushParameters(new ParameterCollection());
            }
        }

        /// <summary>
        /// Copy the properties of the parent to the calling clone.
        /// </summary>
        public void CloneParentMixinToCurrent()
        {
            var parentTree = currentMixinSourceTree.Parent;
            if (parentTree == null)
            {
                throw new InvalidOperationException("Invalid `mixin clone;` from mixin [{0}]. A clone can only be used if the mixin is a child of another mixin".ToFormat(currentMixinSourceTree.Name));
            }

            // Copy mixin informations
            currentMixinSourceTree.Mixin.CloneFrom(parentTree.Mixin);
        }

        /// <summary>
        /// Ends the computation of the child mixin and store the used parameters.
        /// </summary>
        public void EndChild()
        {
            currentMixinSourceTree = currentMixinSourceTree.Parent;
            if (currentMixinSourceTree != null)
            {
                PopParameters();
            }
        }

        /// <summary>
        /// Mixins a <see cref="ShaderMixinSource" /> into the specified mixin tree.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="shaderMixinSource">The shader mixin source.</param>
        public void Mixin(ShaderMixinSourceTree mixinTree, ShaderMixinSource shaderMixinSource)
        {
            mixinTree.Mixin.CloneFrom(shaderMixinSource);
        }
    }
}