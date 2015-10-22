// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

using System;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using SharpDX.Mathematics.Interop;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{

    // We use MediaFoundation.MediaSession on windows desktop to play SoundMusics.
    // The class has an internal thread to process MediaSessionEvents.
    public partial class AudioEngine
    {
        /// <summary>
        /// This method is called during engine construction to initialize Windows.Desktop specific components.
        /// </summary>
        /// <remarks>Variables do not need to be locked here since there are no concurrent calls before the end of the construction.</remarks>
        private void PlatformSpecificInit()
        {
            // get the GUID of the AudioStreamVolume interface from Class Attributes.
            if (streamAudioVolumeGuid == Guid.Empty)
            {
                streamAudioVolumeGuid = Guid.Parse(typeof(AudioStreamVolume).GetTypeInfo().GetCustomAttributes<GuidAttribute>().First().Value);
            }
        }

        private void PlatformSpecificDispose()
        {
            if (mediaSession != null)
            {
                mediaSession.Close();

                var timeOut = 2000;
                while (mediaSession != null && timeOut > 0)
                {
                    ProccessQueuedMediaSessionEvents();

                    const int sleepTime = 5;
                    Thread.Sleep(sleepTime);
                    timeOut -= sleepTime;
                }

                if (timeOut < 0)
                    throw new AudioSystemInternalException("Audio Engine failed to dispose (Event SessionClose did not came after 2 seconds).");
            }
        }

        /// <summary>
        /// Guid to the StreamAudioVolume interface.
        /// </summary>
        private Guid streamAudioVolumeGuid = Guid.Empty;

        /// <summary>
        /// The Media session used to play the SoundMusic.
        /// </summary>
        private MediaSession mediaSession;

        /// <summary>
        /// AudioStreamVolume interface object used to control the SoundMusic Volume.
        /// </summary>
        /// <remarks>
        /// <para>The SimpleAudioVolume interface can not be used here because it modifies the volume of the whole application (XAudio2 included).</para>
        /// <para>streamVolume is valid only when the media session topology is ready. streamVolume is set to null when not valid.</para>
        /// </remarks>
        private AudioStreamVolume streamVolume;

        /// <summary>
        /// The MediaSource used in the current topology. We need to keep a pointer on it to call shutdown when destroying to MediaSession.
        /// </summary>
        private MediaSource mediaSource;

        /// <summary>
        /// The topology of the media session. Need to be disposed after the Session Closed.
        /// </summary>
        private Topology topology;

        /// <summary>
        /// The media session callback that process the event. Need to be disposed every time the Session is destroyed.
        /// </summary>
        private MediaSessionCallback mediaSessionCallback;

        /// <summary>
        /// The ByteStream associated with .
        /// </summary>
        private ByteStream mediaInputByteStream;

        /// <summary>
        /// Create a topology to be played with a MediaSession from a filepath.
        /// </summary>
        internal static Topology CreateTopology(ByteStream mediaInputStream, out MediaSource mediaSource)
        {
            // collector to dispose all the created Media Foundation native objects.
            var collector = new ObjectCollector();

            // Get the MediaSource object.
            var sourceResolver = new SourceResolver();
            collector.Add(sourceResolver);
            ComObject mediaSourceObject;

            // Try to load music
            try
            {
                mediaSourceObject = sourceResolver.CreateObjectFromStream(mediaInputStream, null, SourceResolverFlags.MediaSource | SourceResolverFlags.ContentDoesNotHaveToMatchExtensionOrMimeType);
            }
            catch (SharpDXException)
            {
                collector.Dispose();
                throw new InvalidOperationException("Music stream format not supported");
            }

            Topology retTopo;

            try
            {
                mediaSource = mediaSourceObject.QueryInterface<MediaSource>();
                collector.Add(mediaSourceObject);

                // Get the PresentationDescriptor
                PresentationDescriptor presDesc;
                mediaSource.CreatePresentationDescriptor(out presDesc);
                collector.Add(presDesc);

                // Create the topology
                MediaFactory.CreateTopology(out retTopo);
                for (var i = 0; i < presDesc.StreamDescriptorCount; i++)
                {
                    RawBool selected;
                    StreamDescriptor desc;
                    presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);
                    collector.Add(desc);

                    if (selected)
                    {
                        // Test that the audio file data is valid and supported.
                        var typeHandler = desc.MediaTypeHandler;
                        collector.Add(typeHandler);

                        var majorType = typeHandler.MajorType;

                        if (majorType != MediaTypeGuids.Audio)
                            throw new InvalidOperationException("The music stream is not a valid audio stream.");

                        for (int mType = 0; mType < typeHandler.MediaTypeCount; mType++)
                        {
                            MediaType type;
                            typeHandler.GetMediaTypeByIndex(mType, out type);
                            collector.Add(type);

                            var nbChannels = type.Get(MediaTypeAttributeKeys.AudioNumChannels);
                            if (nbChannels > 2)
                                throw new InvalidOperationException("The provided audio stream has more than 2 channels.");
                        }

                        // create the topology (source,...)
                        TopologyNode sourceNode;
                        MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);
                        collector.Add(sourceNode);

                        sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
                        sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
                        sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);

                        TopologyNode outputNode;
                        MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);
                        collector.Add(outputNode);

                        Activate activate;
                        MediaFactory.CreateAudioRendererActivate(out activate);
                        collector.Add(activate);
                        outputNode.Object = activate;

                        retTopo.AddNode(sourceNode);
                        retTopo.AddNode(outputNode);
                        sourceNode.ConnectOutput(0, outputNode, 0);
                    }
                }
            }
            finally
            {
                collector.Dispose();
            }

            return retTopo;
        }

        /// <summary>
        /// Load a new music into the media session. That is create a new session and a new topology and set the topology of the session.
        /// </summary>
        /// <param name="music"></param>
        private void LoadNewMusic(SoundMusic music)
        {
            if (currentMusic != null || mediaSession != null)
                throw new AudioSystemInternalException("State of the audio engine invalid at the entry of LoadNewMusic.");

            music.Stream.Position = 0;
            mediaInputByteStream = new ByteStream(music.Stream);
            topology = CreateTopology(mediaInputByteStream, out mediaSource);
            MediaFactory.CreateMediaSession(null, out mediaSession);
            mediaSessionCallback = new MediaSessionCallback(mediaSession, OnMediaSessionEvent);
            mediaSession.SetTopology(SessionSetTopologyFlags.None, topology);

            currentMusic = music;
        }

        private void OnMediaSessionEvent(MediaEvent mEvent)
        {
            var type = mEvent.TypeInfo;
            //Console.WriteLine("MediaEvent {0}", type);
            //Console.Out.Flush();

            switch (type)
            {
                case MediaEventTypes.SessionClosed:
                    musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.MusicPlayerClosed, mEvent));
                    break;
                case MediaEventTypes.SessionEnded:
                    musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.EndOfTrackReached, mEvent));
                    break;
                case MediaEventTypes.SessionTopologyStatus:
                    if (mEvent.Get(EventAttributeKeys.TopologyStatus) == TopologyStatus.Ready)
                        musicMediaEvents.Enqueue(new SoundMusicEventNotification(SoundMusicEvent.ReadyToBePlayed, mEvent));
                    break;
                case MediaEventTypes.Error:
                    break;
                case MediaEventTypes.SourceMetadataChanged:
                    break;
            }
        }
        
        private void UpdateMusicVolume()
        {
            // volume factor used in order to adjust Sound Music and Sound Effect Maximum volumes
            const float volumeAdjustFactor = 0.5f;

            if (streamVolume != null)
            {
                var vol = volumeAdjustFactor * currentMusic.Volume;

                // Query number of channels
                var channelCount = streamVolume.ChannelCount;
                var volumes = new float[channelCount];
                for (int i = 0; i < channelCount; ++i)
                    volumes[i] = vol;

                // Set volumes
                streamVolume.SetAllVolumes(volumes.Length, volumes);
            }
        }
        
        private void PauseCurrentMusic()
        {
            mediaSession.Pause();
        }

        private void StopCurrentMusic()
        {
            mediaSession.Stop();
        }

        private void StartCurrentMusic()
        {
            mediaSession.Start(null, new Variant ());
        }

        private void ResetMusicPlayer()
        {
            mediaSession.Close();
        }
        
        private void RestartCurrentMusic()
        {
            mediaSession.Start(null, new Variant { ElementType = VariantElementType.Long, Type = VariantType.Default, Value = (long)0 });
        }

        private void PlatformSpecificProcessMusicReady()
        {
            // The topology of the MediaSession is ready.

            if (!currentMusic.IsDisposed) // disposal of the music can happen between the call to Play and its ready-to-play state notification
            {
                // Query the Volume interface associated to the new topology
                IntPtr volumeObj;
                MediaFactory.GetService(mediaSession, MediaServiceKeys.StreamVolume, streamAudioVolumeGuid, out volumeObj);
                streamVolume = new AudioStreamVolume(volumeObj);
            }
        }

        private void ProcessMusicError(SoundMusicEventNotification eventNotification)
        {
            var mediaEvent = (MediaEvent)eventNotification.EventData;

            var status = mediaEvent.Status;
            if (status.Failure)
            {
                mediaEvent.Dispose();
                status.CheckError();
            }
        }

        private void ProcessMusicMetaData()
        {
            throw new NotImplementedException();
        }

        private void ProcessPlayerClosed()
        {
            // The session finished to close, we have to dispose all related object.
            currentMusic = null;

            mediaSessionCallback.Dispose();

            if (mediaSource != null) mediaSource.Shutdown();
            if (mediaSession != null) mediaSession.Shutdown();

            if (streamVolume != null) streamVolume.Dispose();
            if (mediaSource != null) mediaSource.Dispose();
            if (topology != null) topology.Dispose();
            if (mediaSession != null) mediaSession.Dispose();
            if (mediaInputByteStream != null) mediaInputByteStream.Dispose();

            topology = null;
            streamVolume = null;
            mediaSession = null;
            mediaSource = null;
            mediaInputByteStream = null;
            mediaSessionCallback = null;
            isMusicPlayerReady = false;
        }
    }
}

#endif
