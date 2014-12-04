// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Extensions for <see cref="ParameterCollection"/>.
    /// </summary>
    public static class ParameterCollectionExtensions
    {
        /// <summary>
        /// Clones the specified <see cref="ParameterCollection"/>.
        /// </summary>
        /// <typeparam name="T">Type of the parameter collection</typeparam>
        /// <param name="parameterCollection">The parameter collection.</param>
        /// <returns>A clone of the parameter collection. Values are not cloned.</returns>
        public static T Clone<T>(this T parameterCollection) where T : ParameterCollection, new()
        {
            var newParams = new T();
            parameterCollection.CopyTo(newParams);
            return newParams;
        }

        /// <summary>
        /// Copies the automatic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <param name="parametersTo">The parameters automatic.</param>
        public static void CopyTo<T>(this T parameters, ParameterCollection parametersTo) where T : ParameterCollection
        {
            if (parametersTo == null) throw new ArgumentNullException("parametersTo");
            foreach (var parameter in parameters)
            {
                parametersTo.SetObject(parameter.Key, parameter.Value);
            }
        }


        /// <summary>
        /// Copies the automatic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <param name="parametersTo">The parameters automatic.</param>
        public static void CopySharedTo<T>(this T parameters, ParameterCollection parametersTo) where T : ParameterCollection
        {
            if (parametersTo == null) throw new ArgumentNullException("parametersTo");
            foreach (var parameter in parameters.valueList)
            {
                parameters.CopySharedTo(parameter.Key, null, parametersTo);
            }
        }

        /// <summary>
        /// Determines whether this instance container is the subset of another container.
        /// </summary>
        /// <param name="subset">The container to test as a subset of the 'against' container.</param>
        /// <param name="against">The container superset.</param>
        /// <returns><c>true</c> if the specified against is subset; otherwise, <c>false</c>.</returns>
        public static bool IsSubsetOf(this ParameterCollection subset, ParameterCollection against)
        {
            foreach (var keyValuePair in subset.InternalValues)
            {
                object value = against.GetObject(keyValuePair.Key);

                var innerFrom = keyValuePair.Value.Object as ParameterCollection;
                if (innerFrom != null)
                {
                    var innerTo = value as ParameterCollection;
                    if (innerTo == null)
                    {
                        return false;
                    }

                    if (ReferenceEquals(innerFrom, innerTo))
                    {
                        continue;
                    }

                    if (!innerFrom.IsSubsetOf(innerTo))
                    {
                        return false;
                    }
                }
                else
                {
                    var innerFromArray = keyValuePair.Value.Object as ParameterCollection[];
                    if (innerFromArray != null)
                    {
                        var innerToArray = value as ParameterCollection[];
                        if (innerToArray == null)
                        {
                            return false;
                        }

                        if (innerFromArray.Length != innerToArray.Length)
                        {
                            return false;
                        }

                        for (int i = 0; i < innerFromArray.Length; i++)
                        {
                            if (ReferenceEquals(innerFromArray[i], innerToArray[i]))
                            {
                                continue;
                            }

                            if (innerFromArray[i] == null || innerToArray[i] == null)
                            {
                                return false;
                            }

                            if (!innerFromArray[i].IsSubsetOf(innerToArray[i]))
                            {
                                return false;
                            }
                        }
                    }
                    else if (!Equals(keyValuePair.Value.Object, value))
                    {
                        return false;
                    }
                }
            }

            // If we are here, then 'subset' container is included in 'against'
            return true;
        }
    }
}
