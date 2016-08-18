// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    public class Collision
    {
        public Collision(PhysicsComponent colliderA, PhysicsComponent colliderB)
        {
            ColliderA = colliderA;
            ColliderB = colliderB;

            NewContactChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
            ContactUpdateChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
            ContactEndedChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
        }

        public readonly PhysicsComponent ColliderA;
        public readonly PhysicsComponent ColliderB;

        public HashSet<ContactPoint> Contacts = new HashSet<ContactPoint>(ContactPointEqualityComparer.Default);

        internal Channel<ContactPoint> NewContactChannel;

        public ChannelMicroThreadAwaiter<ContactPoint> NewContact()
        {
            return NewContactChannel.Receive();
        }

        internal Channel<ContactPoint> ContactUpdateChannel;

        public ChannelMicroThreadAwaiter<ContactPoint> ContactUpdate()
        {
            return ContactUpdateChannel.Receive();
        }

        internal Channel<ContactPoint> ContactEndedChannel;

        public ChannelMicroThreadAwaiter<ContactPoint> ContactEnded()
        {
            return ContactEndedChannel.Receive();
        }

        public async Task Ended()
        {
            Collision endCollision;
            do
            {
                endCollision = await ColliderA.CollisionEnded();
            }
            while (!endCollision.Equals(this));
        }

        public override bool Equals(object obj)
        {
            var other = (Collision)obj;
            return (other.ColliderA == ColliderA && other.ColliderB == ColliderB) || (other.ColliderB == ColliderA && other.ColliderA == ColliderB);
        }

        public override int GetHashCode()
        {
            return 397 * ColliderA.GetHashCode() * ColliderB.GetHashCode();
        }

        internal bool InternalEquals(PhysicsComponent a, PhysicsComponent b)
        {
            return (ColliderA == a && ColliderB == b) || (ColliderB == a && ColliderA == b);
        }
    }
}
