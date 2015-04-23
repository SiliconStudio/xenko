// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Storage;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Assets.Tests
{
    static class TestCommon
    {
        /// <summary>
        /// Initializes the asset database. Simlar to <see cref="Game.InitializeAssetDatabase"/>, but accessible without using internals.
        /// </summary>
        internal static void InitializeAssetDatabase()
        {
            using (var profile = Profiler.Begin(GameProfilingKeys.ObjectDatabaseInitialize))
            {
                // Create and mount database file system
                var objDatabase = new ObjectDatabase("/data/db", "index", "/local/db");

                // Only set a mount path if not mounted already
                var mountPath = VirtualFileSystem.ResolveProviderUnsafe("/asset", true).Provider == null ? "/asset" : null;
                var databaseFileProvider = new DatabaseFileProvider(objDatabase, mountPath);

                AssetManager.GetFileProvider = () => databaseFileProvider;
            }
        }
    }
}