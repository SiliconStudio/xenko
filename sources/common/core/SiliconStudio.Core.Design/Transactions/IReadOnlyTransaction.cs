// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Transactions
{
    /// <summary>
    /// An completed interface that cannot be modified anymore, but can be rollbacked or rollforwarded.
    /// </summary>
    public interface IReadOnlyTransaction
    {
        /// <summary>
        /// Gets an unique identifier for the transaction.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the operations executed during the transaction.
        /// </summary>
        [ItemNotNull, NotNull]
        IReadOnlyList<Operation> Operations { get; }

        /// <summary>
        /// Gets the transaction flags.
        /// </summary>
        TransactionFlags Flags { get; }
    }
}
