// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// A context used when mixin <see cref="ShaderSource"/>.
    /// </summary>
    public class ShaderMixinContext
    {
        private readonly ParameterCollection defaultPropertyContainer;
        private readonly Stack<ParameterCollection> propertyContainers = new Stack<ParameterCollection>();
        private readonly Dictionary<string, IShaderMixinBuilder> registeredBuilders;
        private ShaderMixinSourceTree currentMixinSourceTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinContext" /> class.
        /// </summary>
        /// <param name="defaultPropertyContainer">The default property container.</param>
        /// <param name="registeredBuilders">The registered builders.</param>
        /// <exception cref="System.ArgumentNullException">defaultPropertyContainer
        /// or
        /// registeredBuilders</exception>
        public ShaderMixinContext(ParameterCollection defaultPropertyContainer, Dictionary<string, IShaderMixinBuilder> registeredBuilders)
        {
            if (defaultPropertyContainer == null)
                throw new ArgumentNullException("defaultPropertyContainer");

            if (registeredBuilders == null)
                throw new ArgumentNullException("registeredBuilders");

            // TODO: use a copy of the defaultPropertyContainer?
            this.defaultPropertyContainer = defaultPropertyContainer;
            this.registeredBuilders = registeredBuilders;
            this.propertyContainers = new Stack<ParameterCollection>();
        }

        /// <summary>
        /// Pushes the current parameters collection being used.
        /// </summary>
        /// <typeparam name="T">Type of the parameter collection</typeparam>
        /// <param name="propertyContainer">The property container.</param>
        public void PushParameters<T>(T propertyContainer) where T : ParameterCollection
        {
            propertyContainers.Push(propertyContainer);
        }

        /// <summary>
        /// Pops the parameters collection.
        /// </summary>
        public void PopParameters()
        {
            propertyContainers.Pop();
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
        /// <param name="key">The key.</param>
        /// <returns>The value or default value associated to this parameter key.</returns>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public T GetParam<T>(ParameterKey<T> key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var sourcePropertyContainer = defaultPropertyContainer;

            T value;
            // Try to get a value from registered containers
            foreach (var propertyContainer in propertyContainers)
            {
                if (propertyContainer.TryGet(key, out value))
                {
                    sourcePropertyContainer = propertyContainer;
                    break;
                }
            }

            value = sourcePropertyContainer.Get(key);
            var tree = currentMixinSourceTree;
            while (tree != null)
            {
                tree.UsedParameters.Set(key, value);
                tree = tree.Parent;
            }

            return value;
        }

        public void SetParam<T>(ParameterKey<T> key, T value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var propertyContainer = propertyContainers.Count > 0 ? propertyContainers.Peek() : defaultPropertyContainer;
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