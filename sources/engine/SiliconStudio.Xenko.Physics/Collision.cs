// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    public class Collision
    {
        public Collision()
        {
            NewContactChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
            ContactUpdateChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
            ContactEndedChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
        }

        public PhysicsComponent ColliderA;
        public PhysicsComponent ColliderB;

        public TrackingCollection<ContactPoint> Contacts;

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
            while (endCollision != this);
        }
    }
}
