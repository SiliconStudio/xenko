// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Actual definition of IParameterCollectionInheritance (hidden in internal interface).
    /// This could be moved directly in IParameterCollectionInheritance if it becomes public.
    /// </summary>
    internal interface IParameterCollectionInheritanceInternal : IParameterCollectionInheritance
    {
        int GetInternalValueCount();
        ParameterCollection.InternalValue GetInternalValue(ParameterKey key);
        IEnumerable<KeyValuePair<ParameterKey, ParameterCollection.InternalValue>> GetInternalValues();
        ParameterCollection GetParameterCollection();
        ParameterCollection.OnUpdateValueDelegate GetUpdateValueDelegate(ParameterCollection.OnUpdateValueDelegate original);
    }
}