// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// This interface can be used to create run-time plug ins for games, that can run from module initializers for example and need access to the whole Game.
    /// </summary>
    public interface IGamePlugin
    {
        void Initialize(Game game, string packageName);
        void Destroy(Game game);
    }
}