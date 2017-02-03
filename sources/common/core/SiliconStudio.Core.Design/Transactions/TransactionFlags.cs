using System;

namespace SiliconStudio.Core.Transactions
{
    [Flags]
    public enum TransactionFlags
    {
        None = 0,

        /// <summary>
        /// Keep parent transaction alive (useful to start async inner transactions).
        /// </summary>
        KeepParentsAlive = 1,
    }
}