// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading;
using System.Threading.Tasks;

using SiliconStudio.Paradox.Audio.Wave;

namespace SiliconStudio.Paradox.Audio
{
    
    /// <summary>
    /// <para>A dynamic SoundEffectInstance.</para>
    /// <para>This class provides methods, properties and callbacks to enable SoundEffect wav data streaming and generation. 
    /// The user can choose its sound format when creating the <see cref="DynamicSoundEffectInstance"/></para>.
    /// Then he can generate the audio data and submit it to the audio system.
    /// The event <see cref="BufferNeeded"/> is called every time that sound need data.
    /// </summary>
    public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
    {
        internal object WorkerLock = new object();
        internal bool IsDisposing;

        /// <summary>
        /// The wave format of this dynamic sound effect.
        /// </summary>
        private readonly WaveFormat waveFormat;

        internal override WaveFormat WaveFormat
        {
            get { return waveFormat; }
        }

        /// <summary>
        /// This constant represent the number of buffers under which the <see cref="BufferNeeded"/> event should be thrown.
        /// </summary>
        private const int BufferNeededEventNbOfBuffers = 2;

        /// <summary>
        /// Create a dynamic sound effect instance with the given sound properties.
        /// </summary>
        /// <param name="engine">The engine in which the dynamicSoundEffectInstance is created</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio content. Must between 8000 Hz and 48000 Hz</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <param name="encoding">Encoding of a sound data sample</param>
        /// <returns>A new DynamicSoundEffectInstance instance ready to filled with data and then played</returns>
        /// <exception cref="ArgumentOutOfRangeException">This exception is thrown for one of the following reason:
        /// <list type="bullet">
        /// <item>The value specified for sampleRate is less than 8000 Hz or greater than 48000 Hz. </item>
        /// <item>The value specified for channels is something other than mono or stereo. </item>
        /// <item>The value specified for data encoding is something other than 8 or 16 bits. </item>
        /// </list>
        ///  </exception>
        /// <exception cref="ArgumentNullException"><paramref name="engine"/> is null.</exception>
        public DynamicSoundEffectInstance(AudioEngine engine, int sampleRate, AudioChannels channels, AudioDataEncoding encoding)
            : base(engine)
        {
            if (engine == null) 
                throw new ArgumentNullException("engine");

            if (sampleRate < 8000 || 48000 < sampleRate)
                throw new ArgumentOutOfRangeException("sampleRate");

            if(channels != AudioChannels.Mono && channels != AudioChannels.Stereo)
                throw new ArgumentOutOfRangeException("channels");

            if(encoding != AudioDataEncoding.PCM_8Bits && encoding != AudioDataEncoding.PCM_16Bits)
                throw new ArgumentOutOfRangeException("encoding");

            waveFormat = new WaveFormat(sampleRate, (int)encoding, (int)channels);

            Interlocked.Increment(ref totalNbOfInstances);
            Interlocked.Increment(ref numberOfInstances);

            // first instance of dynamic sound effect instance => we create the workerThead and the associated event.
            if (numberOfInstances == 1)
            {
                instancesNeedingBuffer = new ThreadSafeQueue<DynamicSoundEffectInstance>(); // to be sure that there is no remaining request from previous sessions
                awakeWorkerThread = new AutoResetEvent(false);
                CreateWorkerThread();
            }
            
            Name = "Dynamic Sound Effect Instance - "+totalNbOfInstances;

            CreateVoice(WaveFormat);

            InitializeDynamicSound();

            AudioEngine.RegisterSound(this);

            ResetStateToDefault();
        }

        /// <summary>
        /// This represent the total number of instances created since the beginning of the game.
        /// It is used the give a unique name to each Dynamic sound only.
        /// </summary>
        private static int totalNbOfInstances;

        /// <summary>
        /// Submits an audio whole buffer for playback.
        /// </summary>
        /// <param name="buffer">Buffer that contains the audio data. The audio format must be PCM wave data.</param>
        /// <remarks>
        /// The buffer must conform to the format alignment. For example, the buffer length must be aligned to the block alignment value for the audio format type. 
        /// For PCM audio format, the block alignment is calculated as BlockAlignment = BytesPerSample * AudioChannels. 
        /// DynamicSoundEffectInstance supports only PCM 8bits and 16-bit mono or stereo data. 
        /// <para>Submited buffer must not be modified before it finished to play.</para>
        /// <para>Scratches in the sound flow may appears if the submitted buffers are not big enough.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The exception thrown if SubmitBuffer is called after DynamicSoundEffectInstance is disposed.</exception>
        /// <exception cref="ArgumentException">
        /// The exception thrown when buffer is zero length, or does not satisfy format alignment restrictions. 
        /// </exception>
        public void SubmitBuffer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            SubmitBuffer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Submits an audio buffer for playback. Playback begins at the specifed offset, and the byte count determines the size of the sample played. 
        /// </summary>
        /// <param name="buffer">Buffer that contains the audio data. The audio format must be PCM wave data.</param>
        /// <param name="offset">Offset, in bytes, to the starting position of the data.</param>
        /// <param name="byteCount">Amount, in bytes, of data sent.</param>
        /// <remarks>
        /// The buffer must conform to the format alignment. For example, the buffer length must be aligned to the block alignment value for the audio format type. 
        /// For PCM audio format, the block alignment is calculated as BlockAlignment = BytesPerSample * AudioChannels. 
        /// DynamicSoundEffectInstance supports only PCM 8bits and 16-bit mono or stereo data. 
        /// <para>Submited buffer must not be modified before it finished to play.</para>
        /// <para>Scratches in the sound flow may appears if the submitted buffers are not big enough.</para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The exception thrown if SubmitBuffer is called after DynamicSoundEffectInstance is disposed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// The exception thrown when <paramref name="buffer"/> is zero length, or <paramref name="byteCount"/> does not satisfy format alignment restrictions. 
        /// This exception also is thrown if offset is less than zero or is greater than or equal to the size of the buffer. 
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The exception thrown when <paramref name="byteCount"/>"/> is less than or equal to zero, or if the sum of <paramref name="offset"/> and <paramref name="byteCount"/> exceeds the size of the buffer. 
        /// </exception>
        public void SubmitBuffer(byte[] buffer, int offset, int byteCount)
        {
            CheckNotDisposed();

            if(buffer == null)
                throw new ArgumentNullException("buffer");

            if(buffer.Length == 0)
                throw  new ArgumentException("Buffer length is equal to zero.");

            if (byteCount % waveFormat.BlockAlign != 0)
                throw new ArgumentException("The size of data to submit does not satisfy format alignment restrictions.");

            if (offset<0 || offset>=buffer.Length)
                throw new ArgumentOutOfRangeException("offset");

            if(byteCount<0 || offset + byteCount > buffer.Length)
                throw new ArgumentOutOfRangeException("byteCount");

            SubmitBufferImpl(buffer, offset, byteCount);

            CheckAndThrowBufferNeededEvent();
        }

        /// <summary>
        /// Returns the number of buffers that are waiting be to played by the audio system.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The exception thrown if PendingBufferCount is called after DynamicSoundEffectInstance is disposed.</exception>
        public int PendingBufferCount
        {
            get
            {
                CheckNotDisposed();

                return pendingBufferCount;
            }
        }
        /// <summary>
        /// Number of buffer waiting to be played from the user point of view.
        /// </summary>
        private int pendingBufferCount;

        /// <summary>
        /// Number of buffer waiting to be played from the underlying implementation point of view.
        /// </summary>
        private int internalPendingBufferCount;

        
        /// <summary>
        /// Event that occurs when the sound instance is going to run out of audio data. 
        /// </summary>
        /// <remarks>
        /// More precisely, the event is thrown every time that:
        /// <list type="bullet">
        /// <item>the sound is playing and the number of buffers remaining to play is too low.</item>
        /// <item>the number of buffers remaining after a <see cref="SubmitBuffer(byte[])"/> call is still not enough.</item>
        /// </list> 
        /// </remarks>
        public event EventHandler<EventArgs> BufferNeeded;

        /// <summary>
        /// Indicates whether the audio playback of the <see cref="DynamicSoundEffectInstance"/> object is looped. 
        /// </summary>
        /// <remarks>A sound cannot be looped with a <see cref="DynamicSoundEffectInstance"/>. 
        /// So accessing the property always returns false, and setting the property to true always throws the <see cref="InvalidOperationException"/> exception.</remarks>
        /// <exception cref="InvalidOperationException">The exception that is thrown when IsLooped is set to true.</exception>
        /// <exception cref="ObjectDisposedException">The exception that is thrown if IsLooped is called after <see cref="DynamicSoundEffectInstance"/> has been disposed. </exception>
        public override bool IsLooped
        {
            get
            {
                return base.IsLooped; 
            }
            set
            {
                CheckNotDisposed();

                if (value)
                    throw new InvalidOperationException("IsLooped can not be set to true with DynamicSoundEffectInstance.");
            }
        }

        internal override void ExitLoopImpl()
        {
            // there is nothing to do here since DynamicSoundEffectInstance can not be looped.
        }

        public override void Play()
        {
            DataBufferLoaded = true;

            base.Play();

            CheckAndThrowBufferNeededEvent();
        }

        /// <summary>
        /// This method checks if the BufferNeeded event need to be thrown. 
        /// If it is the case, it invokes the methods associated to the event.
        /// </summary>
        private void CheckAndThrowBufferNeededEvent()
        {
            if (internalPendingBufferCount <= BufferNeededEventNbOfBuffers && BufferNeeded != null)
            {
                instancesNeedingBuffer.Enqueue(this);
                awakeWorkerThread.Set();
            }
        }
        
        public override void Stop()
        {
            // submitted buffers need to be cleared even if the music is already stopped (-> overload stop instead of stopImpl)

            lock (WorkerLock) // we lock the worker thread here to avoid to have invalid states due to simultaneous Stop/Submit (via BufferNeeded callback).
            {
                base.Stop();

                pendingBufferCount = 0;
                internalPendingBufferCount = 0;

                ClearBuffersImpl();

                lock (submittedBufferHandles.InternalLock)
                {
                    foreach (var handles in submittedBufferHandles.InternalQueue)
                        handles.FreeHandles();

                    submittedBufferHandles.InternalQueue.Clear();
                }
            }
        }

        /// <summary>
        /// The number of DynamicSoundEffectInstances.
        /// It is used to determine when to create or destroy the working thread.
        /// </summary>
        private static int numberOfInstances;

        /// <summary>
        /// Event used to awake the worker thread when there is some work to perform.
        /// </summary>
        private static AutoResetEvent awakeWorkerThread;

        /// <summary>
        /// A reference to the worker task.
        /// It is used to wait completion of the task before disposing the last instance of Dynamic sound.
        /// </summary>
        private static Task workerTask;

        /// <summary>
        /// Queue of the instances that require a new buffer to be submitted. 
        /// It is used by the worker thread to determine which user callback to execute.
        /// </summary>
        private static ThreadSafeQueue<DynamicSoundEffectInstance> instancesNeedingBuffer;

        internal override void DestroyImpl()
        {
            AudioEngine.UnregisterSound(this);

            IsDisposing = true;

            lock (WorkerLock) // avoid to have simultaneous destroy and submit buffer (via BufferNeeded of working thread).
            {
                base.DestroyImpl();
            }

            Interlocked.Decrement(ref numberOfInstances);

            if (numberOfInstances == 0)
            {
                awakeWorkerThread.Set();
                if (!workerTask.Wait(500))
                    throw new AudioSystemInternalException("The DynamicSoundEffectInstance worker did not complete in allowed time.");
                awakeWorkerThread.Dispose();
            }
        }

        /// <summary>
        /// Create the working thread that will execute the user code on event <see cref="BufferNeeded"/> triggering. 
        /// </summary>
        private static void CreateWorkerThread()
        {
            workerTask = Task.Factory.StartNew(WorkerThread);
        }

        /// <summary>
        /// The worker thread executing the user code on event <see cref="BufferNeeded"/>.
        /// In current implementation the there is only one working thread for all the DynamicSoundEffectInstances.
        /// </summary>
        private static void WorkerThread()
        {
            while (true)
            {
                awakeWorkerThread.WaitOne();

                if (numberOfInstances == 0)
                {
                    return;
                }

                DynamicSoundEffectInstance instanceNeedingBuffer;
                while (instancesNeedingBuffer.TryDequeue(out instanceNeedingBuffer))
                {
                    lock (instanceNeedingBuffer.WorkerLock)
                    {
                        if (!instanceNeedingBuffer.IsDisposing && instanceNeedingBuffer.BufferNeeded != null)
                        {
                            instanceNeedingBuffer.BufferNeeded(instanceNeedingBuffer, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The queue of the handles to free as the buffers are consumed.
        /// </summary>
        private readonly ThreadSafeQueue<SubBufferDataHandles> submittedBufferHandles = new ThreadSafeQueue<SubBufferDataHandles>();

        private void OnBufferEndCommon()
        {
            SubBufferDataHandles bufferHandles;
            if (submittedBufferHandles.TryDequeue(out bufferHandles))
            {
                Interlocked.Decrement(ref internalPendingBufferCount);
                Interlocked.Add(ref pendingBufferCount, -bufferHandles.HandleCount);
                bufferHandles.FreeHandles();
            }

            CheckAndThrowBufferNeededEvent();
        }
    }
}
