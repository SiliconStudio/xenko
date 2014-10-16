// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.LauncherApp
{
    public class LoadingEventArgs
    {
        public LoadingEventArgs(string package, string version)
        {
            Package = package;
            Version = version;
        }

        public string Package { get; private set; }

        public string Version { get; private set; }
    }
}