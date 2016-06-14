// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Represents a 3D audio listener in the audio scene. 
    /// This object, used in combination with an <see cref="AudioEmitter"/>, can simulate 3D audio localization effects for a sound implemention the <see cref="IPositionableSound"/> interface.
    /// For more details take a look at the <see cref="IPositionableSound.Apply3D"/> function.
    /// </summary>
    /// <seealso cref="IPositionableSound.Apply3D"/>
    /// <seealso cref="AudioEmitter"/>
    public class AudioListener : IDisposable
    {
        public AudioListener(AudioEngine engine)
        {
            Listener = OpenAl.ListenerCreate(engine.AudioDevice);
        }

        /// <summary>
        /// The position of the listener in the 3D world.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The velocity of the listener in the 3D world. 
        /// </summary>
        /// <remarks>This is only used to calculate the doppler effect on the sound effect</remarks>
        public Vector3 Velocity;


        /// <summary>
        /// The orientation of the listener in the 3D world. 
        /// </summary>
        /// <remarks>This is only used to calculate the doppler effect on the sound effect</remarks>
        public Quaternion Orientation;

        /// <summary>
        /// Internal OpenAL object that represents a device context actually, this is to allow multiple listeners
        /// </summary>
        internal OpenAl.Listener Listener;

        public void Dispose()
        {
            OpenAl.ListenerDestroy(Listener);
        }

        public unsafe void Update(Vector3 position, Quaternion orientation, Vector3 velocity)
        {
            OpenAl.ListenerPush3D(Listener, (float*)Interop.Fixed(ref position), (float*)Interop.Fixed(ref orientation), (float*)Interop.Fixed(ref velocity));
        }
    }
}
