// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// An interface representing an asynchronous transaction. An asynchronous transaction is a transaction that can be completed asynchronously. It
    /// provides additional safety such as preventing another asynchronous transaction to be created when there is one already in progress.
    /// </summary>
    public interface IAsyncTransaction
    {
    }
}
