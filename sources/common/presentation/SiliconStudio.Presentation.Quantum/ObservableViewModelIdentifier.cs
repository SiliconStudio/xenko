// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Presentation.Quantum
{
    public struct ObservableViewModelIdentifier : IEquatable<ObservableViewModelIdentifier>
    {
        private readonly HashSet<Guid> guids;

 
        internal ObservableViewModelIdentifier(Guid guid)
        {
            guids = new HashSet<Guid> { guid };
        }

        internal ObservableViewModelIdentifier(IEnumerable<Guid> guids)
        {
            this.guids = new HashSet<Guid>();
            foreach (var guid in guids)
                this.guids.Add(guid);
        }

        public bool IsCombined { get { return guids.Count > 1; } }

        public bool Match(ObservableViewModelIdentifier identifier)
        {
            return guids.Count == identifier.guids.Count && guids.All(guid => identifier.guids.Contains(guid));
        }

        public bool Equals(ObservableViewModelIdentifier identifier)
        {
            return Match(identifier);
        }

        public override bool Equals(object obj)
        {
            return obj is ObservableViewModelIdentifier && Match((ObservableViewModelIdentifier)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return guids.Aggregate(0, (current, guid) => (guid.GetHashCode() * 397) ^ current);
            }
        }
    }
}