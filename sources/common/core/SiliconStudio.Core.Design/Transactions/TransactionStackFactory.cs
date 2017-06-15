// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// A static factory to create <see cref="ITransactionStack"/> instances.
    /// </summary>
    public static class TransactionStackFactory
    {
        [NotNull]
        public static ITransactionStack Create(int capacity)
        {
            return new TransactionStack(capacity);
        }
    }
}
