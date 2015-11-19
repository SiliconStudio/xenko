// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Represents an <see cref="IContent"/> type that must refresh node references when its value is modified.
    /// </summary>
    public interface IUpdatableContent
    {
        void RegisterOwner(IGraphNode node);
    }
}