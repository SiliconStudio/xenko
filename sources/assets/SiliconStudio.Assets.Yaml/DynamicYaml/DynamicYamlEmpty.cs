// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Placeholder value to remove keys from <see cref="DynamicYamlMapping"/>.
    /// </summary>
    public class DynamicYamlEmpty : DynamicYamlObject
    {
        public static readonly DynamicYamlEmpty Default = new DynamicYamlEmpty();    
    }
}
