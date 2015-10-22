// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Paradox.Audio
{
    /// <summary>
    /// Interface for a playable sound.
    /// A playable sound can loop (ref <see cref="IsLooped"/>), be played (ref <see cref="Play"/>), be paused (ref <see cref="Pause"/>), be resumed (ref <see cref="Play"/>), 
    /// be stopped (ref <see cref="Stop()"/>) and be attenuated (ref <see cref="Volume"/>).
    /// To query the current state of a sound use the <see cref="PlayState"/> property. 
    /// To stop a sound after its currently loop use <see cref="ExitLoop"/>
    /// </summary>
    public interface IPlayableSound
    {
        /// <summary>
        /// The current state of the sound. 
        /// </summary>
        SoundPlayState PlayState { get; }

        /// <summary>
        /// Does the sound is automatically looping from beginning when it reaches the end.
        /// </summary>
        /// <remarks>If you want to make a sound play continuously until stopped, be sure to set IsLooped to true <bold>before</bold> you call <see cref="Play"/>.
        /// To quit a loop when playing, use <see cref="ExitLoop"/></remarks>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        /// <exception cref="InvalidOperationException">IsLooped cannot be modified after the sound has started to play</exception>
        /// <seealso cref="ExitLoop"/>
        bool IsLooped { get; set; }

        /// <summary>
        /// Start or resume playing the sound.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        /// <remarks>A call to Play when the sound is already playing has no effects.</remarks>
        void Play();

        /// <summary>
        /// Pause the sounds.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        /// <remarks>A call to Pause when the sound is already paused or stopped has no effects.</remarks>
        void Pause();

        /// <summary>
        /// Stop playing the sound immediately and reset the sound to the beginning of the track.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        /// <remarks>A call to Stop when the sound is already stopped has no effects</remarks>
        void Stop();

        /// <summary>
        /// Stop looping. That is, do not start over at the end of the current loop, continue to play until the end of the buffer data and then stop.
        /// </summary>
        /// <remarks>
        /// <para>A call to ExitLoop when the sound is Stopped or when the sound is not looping has no effects. 
        /// That is why a call to ExitLoop directly following a call to <see cref="Play"/> may be ignored (short play latency). 
        /// For a correct behaviour wait that the sound actually started playing to call ExitLoop.</para>
        /// <para>ExitLoop does not modify the value of IsLooped.</para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        void ExitLoop();

        /// <summary>
        /// The global volume at which the sound is played.
        /// </summary>
        /// <remarks>Volume is ranging from 0.0f (silence) to 1.0f (full volume). Values beyond those limits are clamped.</remarks>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        float Volume { get; set; }
    }
}
