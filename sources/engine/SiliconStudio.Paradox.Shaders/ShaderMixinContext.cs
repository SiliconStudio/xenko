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
        private readonly Dictionary<object, object> strippedPropertyContainers = new Dictionary<object, object>();
        private readonly Stack<ShaderMixinParameters> currentUsedParameters = new Stack<ShaderMixinParameters>();
        private readonly List<ShaderMixinParameters> finalUsedParameters = new List<ShaderMixinParameters>();
        private readonly Stack<string> shaderNames = new Stack<string>();
        private readonly Stack<HashSet<ParameterKey>> blackListKeys = new Stack<HashSet<ParameterKey>>();
        private readonly Stack<ParameterCollection> currentPropertyContainers = new Stack<ParameterCollection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinContext"/> class.
        /// </summary>
        /// <param name="defaultPropertyContainer">The default property container.</param>
        /// <param name="registeredBuilders">The registered builders.</param>
        /// <param name="shaderBaseName">The name of the base shader.</param>
        /// <exception cref="System.ArgumentNullException">
        /// defaultPropertyContainer
        /// or
        /// registeredBuilders
        /// </exception>
        public ShaderMixinContext(ParameterCollection defaultPropertyContainer, Dictionary<string, IShaderMixinBuilder> registeredBuilders, string shaderBaseName)
        {
            if (defaultPropertyContainer == null)
                throw new ArgumentNullException("defaultPropertyContainer");

            if (registeredBuilders == null)
                throw new ArgumentNullException("registeredBuilders");

            // TODO: use a copy of the defaultPropertyContainer?
            this.defaultPropertyContainer = defaultPropertyContainer;
            this.registeredBuilders = registeredBuilders;

            strippedPropertyContainers.Add(defaultPropertyContainer, new ShaderMixinParameters());

            currentUsedParameters.Push(new ShaderMixinParameters(shaderBaseName));
            shaderNames.Push(shaderBaseName);
            blackListKeys.Push(new HashSet<ParameterKey>());
            currentPropertyContainers.Push(defaultPropertyContainer);
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

            ParameterCollection sourcePropertyContainer = null;

            T value;
            // Try to get a value from registered containers
            foreach (var propertyContainer in propertyContainers)
            {
                if (propertyContainer.TryGet(key, out value))
                {
                    sourcePropertyContainer = propertyContainer;
                    goto valueFound; // Use goto to speedup the code and avoid usage of additionnal bool state
                }
            }

            // Else gets the value (or default value) from the default property container
            sourcePropertyContainer = defaultPropertyContainer;
            value = currentPropertyContainers.Peek().Get(key);
            // do not store the keys behind a ParameterKey<ShaderMixinParameters>, only when it comes from defaultPropertyContainer
            if (!blackListKeys.Peek().Contains(key))
                currentUsedParameters.Peek().Set(key, value);

        valueFound:

            // Cache the strip property container
            var stripPropertyContainer = (ParameterCollection)ReplicateContainer(sourcePropertyContainer);

            if (!stripPropertyContainer.ContainsKey(key))
            {
                var stripValue = value;
                if (IsPropertyContainer(value))
                {
                    stripValue = (T)ReplicateContainer(value);
                }
                stripPropertyContainer.Set(key, stripValue);
            }

            return value;
        }

        /// <summary>
        /// Gets all parameters used by this context when mixin a shader.
        /// </summary>
        /// <returns>ShaderMixinParameters.</returns>
        public ShaderMixinParameters GetMainUsedParameters()
        {
            return (ShaderMixinParameters)strippedPropertyContainers[defaultPropertyContainer];
        }

        public List<ShaderMixinParameters> GetUsedParameters()
        {
            while (currentUsedParameters.Count > 0)
                finalUsedParameters.Add(currentUsedParameters.Pop());
            return finalUsedParameters;
        }

        private bool IsPropertyContainer(object source)
        {
            return source is PropertyContainer || source is PropertyContainer[];
        }

        private object ReplicateContainer(object source)
        {
            object objectToReplicate = null;
            if (!strippedPropertyContainers.TryGetValue(source, out objectToReplicate))
            {
                if (source is ParameterCollection)
                {
                    objectToReplicate = new ShaderMixinParameters();
                }
                else
                {
                    var containers = source as ParameterCollection[];
                    if (containers != null)
                    {
                        var subPropertyContainers = new ShaderMixinParameters[containers.Length];
                        for (int i = 0; i < containers.Length; i++)
                        {
                            subPropertyContainers[i] = (ShaderMixinParameters)ReplicateContainer(containers[i]);
                        }
                        objectToReplicate = containers;
                    }
                }

                strippedPropertyContainers.Add(source, objectToReplicate);
            }
            return objectToReplicate;
        }

        public void SetParam<T>(ParameterKey<T> key, T value)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var propertyContainer = propertyContainers.Count > 0 ? propertyContainers.Peek() : currentPropertyContainers.Peek();
            propertyContainer.Set(key, value);

            if (propertyContainers.Count == 0) // in currentDefaultContainer?
                blackListKeys.Peek().Add(key);
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
        /// Mixins a <see cref="ShaderClassSource"/> identified by its name/generic parameters into the specified mixin tree.
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

            if (builder != null)
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
            var parentName = shaderNames.Peek();
            var childName = parentName + "." + subMixin.Name;
            currentUsedParameters.Push(new ShaderMixinParameters(childName));
            shaderNames.Push(childName);

            // copy the keys that have been set
            var blk = new HashSet<ParameterKey>();
            foreach (var blackKey in blackListKeys.Peek())
                blk.Add(blackKey);
            blackListKeys.Push(blk);

            // copy the set of parameters
            var pc = new ParameterCollection();
            currentPropertyContainers.Peek().CopyTo(pc);
            currentPropertyContainers.Push(pc);
        }

        /// <summary>
        /// Copy the properties of the parent to the calling clone.
        /// </summary>
        public void CloneProperties()
        {
            if (currentUsedParameters.Count > 1)
            {
                var childUsedParameters = currentUsedParameters.Pop();
                var parentUsedParameters = currentUsedParameters.Peek();
                parentUsedParameters.CopyTo(childUsedParameters);
                currentUsedParameters.Push(childUsedParameters);
            }
        }

        /// <summary>
        /// Ends the computation of the child mixin and store the used parameters.
        /// </summary>
        public void EndChild()
        {
            shaderNames.Pop();
            blackListKeys.Pop();
            currentPropertyContainers.Pop();
            finalUsedParameters.Add(currentUsedParameters.Pop());
        }

        /// <summary>
        /// Mixins a <see cref="ShaderMixinSource"/> into the specified mixin tree.
        /// </summary>
        /// <param name="mixinTree">The mixin tree.</param>
        /// <param name="shaderMixinSource">The shader mixin source.</param>
        public void Mixin(ShaderMixinSourceTree mixinTree, ShaderMixinSource shaderMixinSource)
        {
            mixinTree.Mixin.CloneFrom(shaderMixinSource);
        }
    }
}