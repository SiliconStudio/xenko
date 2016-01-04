// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_SOUND_SDL
using SDL2;
using System;

namespace SiliconStudio.Xenko.Audio
{
    public class AudioEngineSDL : AudioEngine
    {
        /// <inheritDoc/>
        internal override void InitializeAudioEngine(AudioDevice device)
        {
            // Initialize SDL Audio part
            SDL.SDL_InitSubSystem(SDL.SDL_INIT_AUDIO);

            // Initialize SDL Mixer part
            SDL_mixer.MIX_InitFlags requestedFlags = SDL_mixer.MIX_InitFlags.MIX_INIT_FLAC | SDL_mixer.MIX_InitFlags.MIX_INIT_MP3 |
                SDL_mixer.MIX_InitFlags.MIX_INIT_OGG | SDL_mixer.MIX_InitFlags.MIX_INIT_MOD |
                SDL_mixer.MIX_InitFlags.MIX_INIT_FLUIDSYNTH;
            int flags = SDL_mixer.Mix_Init(requestedFlags);

            // TODO: Check `flags' to see if underlying platform supports all formats
        }

        /// <inheritDoc/>
        internal override void DestroyAudioEngine()
        {
            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_AUDIO);
        }

        /// <inheritDoc/>
        internal override void LoadMusic(SoundMusic music)
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void PauseMusic()
        { 
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void ProcessMusicReadyImpl()
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void ProcessMusicError(SoundMusicEventNotification eventNotification)
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void ProcessMusicMetaData()
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void ProcessPlayerClosed()
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void RestartMusic()
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void StartMusic()
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void StopMusic()
        {
            throw new NotImplementedException();
        }

        /// <inheritDoc/>
        internal override void UpdateMusicVolume()
        {
            throw new NotImplementedException();
        }

    }
}

#endif
