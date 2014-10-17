// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public class NetworkProxyViewModelContent : ContentBase, IAsyncViewModelContent
    {
        public static readonly PropertyKey<List<NetworkChange>> PendingChanges = new PropertyKey<List<NetworkChange>>("PendingChanges", typeof(NetworkProxyViewModelContent), DefaultValueMetadata.Delegate(delegate { return new List<NetworkChange>(); }));

        // Keep track when last pendingValue was sent over network
        // It will be kept until this packet has been processed on the other end.
        public static readonly PropertyKey<int> PacketIndex = new PropertyKey<int>("PacketIndex", typeof(NetworkProxyViewModelContent));

        private ViewModelContentFlags networkFlags;

        // Latest value available from network (will be moved to cachedValue when UI request refresh)
        private object networkValue;

        // Visible through this.Value
        private object cachedValue;
        
        // Value set by user (override everything)
        // Disappear after a network round-trip
        private object pendingValue;

        // Keep track when last pendingValue was sent over network
        // It will be kept until this packet has been processed on the other end.
        private int pendingValuePacketIndex;

        private bool updated;

        public ViewModelContext context;

        public NetworkProxyViewModelContent(ViewModelContext context, Type type)
            : base(type, null, false)
        {
            this.context = context;
            if (context == null)
            {
                
            }
        }

        /// <inheritdoc/>
        public override ViewModelContentState LoadState
        {
            get
            {
                return base.LoadState;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public override object Value
        {
            get
            {
                // Pending value (waiting to be processed by engine) has higher priority.
                return pendingValue ?? cachedValue;
            }
            set
            {
                // New value will be sent through a network change packet.
                // It should update cachedValue when viewmodel is sent again.
                // 
                var pendingChanges = context.Get(PendingChanges);
                lock (pendingChanges)
                {
                    pendingValue = value;
                    pendingValuePacketIndex = context.Get(PacketIndex);
                    pendingChanges.Add(new NetworkChange { Path = ViewModelController.BuildPath(this.OwnerNode), Type = NetworkChangeType.ValueUpdate, Value = value });
                }
            }
        }

        /// <summary>
        /// Queues the command that will later be sent over network.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void QueueCommand(object parameter)
        {
            var pendingChanges = context.Get(PendingChanges);
            lock (pendingChanges)
            {
                pendingChanges.Add(new NetworkChange { Path = ViewModelController.BuildPath(this.OwnerNode), Type = NetworkChangeType.ActionInvoked, Value = parameter });
            }
        }

        public void RequestLoadContent()
        {
            var pendingChanges = context.Get(PendingChanges);
            lock (pendingChanges)
            {
                pendingChanges.Add(new NetworkChange { Path = ViewModelController.BuildPath(this.OwnerNode), Type = NetworkChangeType.ValueRequestLoad });
            }
        }

        /// <summary>
        /// Updates network value (it will be available when <see cref="PushNetworkValue"/> is called).
        /// </summary>
        /// <param name="lastAckPacketIndex">Index of the last acknowledged packet.</param>
        /// <param name="value">The value.</param>
        public void UpdateNetworkValue(int lastAckPacketIndex, object value, ViewModelContentFlags flags)
        {
            lock (this)
            {
                // If last pending value has been processed by engine, clear it.
                if (lastAckPacketIndex >= pendingValuePacketIndex)
                    pendingValue = null;

                networkFlags = flags;
                networkValue = value;
            }
        }

        private static bool CompareValues(object a, object b)
        {
            if (a != null)
            {
                var enumerableA = a as IEnumerable;
                var enumerableB = b as IEnumerable;
                return enumerableA != null ? enumerableA.SequenceEqual(enumerableB) : a.Equals(b);
            }

            return b == null;
        }

        /// <summary>
        /// Copy pending network value to Value.
        /// </summary>
        public void PushNetworkValue()
        {
            lock (this)
            {
                if (!CompareValues(networkValue, cachedValue))
                {
                    Flags = networkFlags;
                    cachedValue = networkValue;
                    updated = true;
                }
            }
        }

        public override object UpdatedValue
        {
            get
            {
                lock (this)
                {
                    if (updated)
                    {
                        updated = false;
                        return Value;
                    }
                    return ValueNotUpdated;
                }
            }
        }

        public void UpdateNetworkLoadingState(ViewModelContentState loadingState)
        {
            base.LoadState = loadingState;
        }
    }
}