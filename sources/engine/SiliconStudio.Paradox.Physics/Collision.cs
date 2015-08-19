using System.Collections.Generic;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Paradox.Physics
{
    public class Collision
    {
        public Collision()
        {
            NewContactChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
            ContactUpdateChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
            ContactEndedChannel = new Channel<ContactPoint> { Preference = ChannelPreference.PreferSender };
        }

        public Collider ColliderA;
        public Collider ColliderB;

        public List<ContactPoint> Contacts;

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
    }
}