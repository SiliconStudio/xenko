// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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