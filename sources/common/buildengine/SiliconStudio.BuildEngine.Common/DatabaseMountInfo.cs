// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.BuildEngine
{
    [Serializable]
    public struct DatabaseMountInfo
    {
        public string DatabaseMountPoint;

        public DatabaseMountInfo(string databaseMountPoint)
        {
            DatabaseMountPoint = databaseMountPoint;
        }
    }
}
