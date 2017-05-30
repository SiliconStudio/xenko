// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core.Yaml.Events;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Objects that can't be loaded as valid Yaml will be converted to a proxy object implementing this interface by <see cref="ErrorRecoverySerializer"/>.
    /// </summary>
    public interface IUnloadable
    {
        string TypeName { get; }

        string AssemblyName { get; }

        string Error { get; }

        List<ParsingEvent> ParsingEvents { get; }
    }
}
