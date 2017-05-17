// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// A pool holding <see cref="Query"/> with a specific <see cref="QueryType"/>.
    /// </summary>
    public partial class QueryPool : GraphicsResourceBase
    {
        private const QueryType queryType = QueryType.Timestamp;

        private Query[] queries;
        private int capacity;
        private bool isInUse;

        /// <summary>
        /// <see cref="QueryType"/> for this QueryPool.
        /// </summary>
        public QueryType QueryType => queryType;

        /// <summary>
        /// Capacity of this pool.
        /// </summary>
        public int Capacity => capacity;

        /// <summary>
        /// Whether or not the pool is currently used by a <see cref="CommandList"/>
        /// </summary>
        public bool IsInUse
        {
            get
            {
                return isInUse;
            }
            set
            {
                if (!isInUse)
                {
                    isInUse = value;
                }
            }
        }

        public QueryPool()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryPool"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>.</param>
        internal QueryPool(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        /// <summary>
        /// Creates a new <see cref="QueryPool" /> instance.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="queryType">The <see cref="QueryType"/> of the pool.</param>
        /// <param name="queryCount">The capacity of the pool.</param>
        /// <returns>An instance of a new <see cref="QueryPool" /></returns>
        public static QueryPool New(CommandList commandList, QueryType queryType, int queryCount)
        {
            return new QueryPool(commandList.GraphicsDevice).InitializeImpl(commandList, queryType, queryCount);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public QueryPool Clone(CommandList commandList)
        {
            return new QueryPool(GraphicsDevice).InitializeImpl(commandList, queryType, capacity);
        }

        protected internal override bool OnRecreate()
        {
            base.OnRecreate();

            OnRecreateImpl();

            return true;
        }

        /// <summary>
        /// Releases this instance.
        /// </summary>
        /// <param name="commandList">The <see cref="CommandList"/>.</param>
        public void Release(CommandList commandList)
        {
            Reset(commandList);
            isInUse = false;
        }
    }
}