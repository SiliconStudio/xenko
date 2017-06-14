// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL

using System;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    public partial class QueryPool
    {
        internal int[] NativeQueries;

        public bool TryGetData(long[] dataArray)
        {
            for (var index = 0; index < NativeQueries.Length; index++)
            {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES && !SILICONSTUDIO_PLATFORM_IOS
                GL.Ext.GetQueryObject(NativeQueries[index], GetQueryObjectParam.QueryResultAvailable, out long availability);
#elif !SILICONSTUDIO_PLATFORM_IOS
                GL.GetQueryObject(NativeQueries[index], GetQueryObjectParam.QueryResultAvailable, out long availability);
#endif
                if (availability == 0)
                    return false;

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES && !SILICONSTUDIO_PLATFORM_IOS
                GL.Ext.GetQueryObject(NativeQueries[index], GetQueryObjectParam.QueryResult, out dataArray[index]);
#elif !SILICONSTUDIO_PLATFORM_IOS
                GL.GetQueryObject(NativeQueries[index], GetQueryObjectParam.QueryResult, out dataArray[index]);
#endif
            }

            return true;
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
#if !SILICONSTUDIO_PLATFORM_IOS
            GL.DeleteQueries(QueryCount, NativeQueries);
            NativeQueries = null;
#endif
            base.OnDestroyed();
        }

        private void Recreate()
        {
            switch (QueryType)
            {
                case QueryType.Timestamp:
                    break;

                default:
                    throw new NotImplementedException();
            }

            NativeQueries = new int[QueryCount];
#if !SILICONSTUDIO_PLATFORM_IOS
            GL.GenQueries(QueryCount, NativeQueries);
#endif
        }
    }
}

#endif
