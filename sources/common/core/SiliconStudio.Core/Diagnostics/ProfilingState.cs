// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// A profiler state contains information of a portion of code being profiled. See remarks.
    /// </summary>
    /// <remarks>
    /// This struct is not intended to be used directly but only through <see cref="Profiler.Begin()"/>.
    /// You can still attach some attributes to it while profiling a portion of code.
    /// </remarks>
    public struct ProfilingState : IDisposable
    {
        private static readonly Logger Logger = Profiler.Logger;
        private readonly int profilingId;
        private readonly ProfilingKey profilingKey;
        private bool isEnabled;
        private ProfilerDisposeEventDelegate disposeProfileDelegate;
        private Dictionary<object, object> attributes;
        private long startTime;
        private string beginText;

        internal ProfilingState(int profilingId, ProfilingKey profilingKey, bool isEnabled)
        {
            this.profilingId = profilingId;
            this.profilingKey = profilingKey;
            this.isEnabled = isEnabled;
            this.disposeProfileDelegate = null;
            attributes = null;
            beginText = null;
            startTime = 0;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        public bool IsInitialized
        {
            get
            {
                return profilingKey != null;
            }
        }

        /// <summary>
        /// Gets the profiling unique identifier.
        /// </summary>
        /// <value>The profiling unique identifier.</value>
        public int ProfilingId
        {
            get
            {
                return profilingId;
            }
        }

        /// <summary>
        /// Gets the profiling key.
        /// </summary>
        /// <value>The profiling key.</value>
        public ProfilingKey ProfilingKey
        {
            get
            {
                return profilingKey;
            }
        }

        /// <summary>
        /// Gets or sets the dispose profile delegate.
        /// </summary>
        /// <value>The dispose profile delegate.</value>
        public ProfilerDisposeEventDelegate DisposeDelegate
        {
            get
            {
                return disposeProfileDelegate;
            }
            set
            {
                disposeProfileDelegate = value;
            }
        }

        /// <summary>
        /// Checks if the profiling key is enabled and update this instance. See remarks.
        /// </summary>
        /// <remarks>
        /// This can be used for long running profiling that are using markers and want to log markers if 
        /// the profiling was activated at runtime.
        /// </remarks>
        public void CheckIfEnabled()
        {
            isEnabled = Profiler.IsEnabled(profilingKey);
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Value of a key.</returns>
        /// <remarks>If profiling was not enabled for this profile key, the attribute is not stored</remarks>
        public object GetAttribute(string key)
        {
            if (attributes == null)
            {
                return null;
            }
            object result;
            attributes.TryGetValue(key, out result);
            return result;
        }

        /// <summary>
        /// Sets the attribute value for a specified key. See remarks.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <remarks>If profiling was not enabled for this profile key, the attribute is not stored</remarks>
        public void SetAttribute(string key, object value)
        {
            // If profiling is not enabled, doesn't store anything
            if (!isEnabled) return;

            if (attributes == null)
            {
                attributes = new Dictionary<object, object>();
            }
            attributes[key] = value;
        }

        public void Dispose()
        {
            // Perform a Start event only if the profiling is running
            if (!isEnabled) return;

            // Give a chance to the profiling to end and put some property in this profiler state
            if (disposeProfileDelegate != null)
            {
                disposeProfileDelegate(ref this);
            }

            End();
        }

        /// <summary>
        /// Emits a Begin profiling event.
        /// </summary>
        public void Begin()
        {
            EmitEvent(ProfilingMessageType.Begin);
        }

        /// <summary>
        /// Emits a Begin profiling event with the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Begin(string text)
        {
            EmitEvent(ProfilingMessageType.Begin, text);
        }

        /// <summary>
        /// Emits a Begin profiling event with the specified formatted text.
        /// </summary>
        /// <param name="textFormat">The text format.</param>
        /// <param name="textFormatArguments">The text format arguments.</param>
        public void Begin(string textFormat, params object[] textFormatArguments)
        {
            EmitEvent(ProfilingMessageType.Begin, textFormat, textFormatArguments);
        }

        public void Begin(int value0)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { IntValue = value0 }, null, null, null);
        }

        public void Begin(int value0, int value1)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, null, null);
        }

        public void Begin(int value0, int value1, int value2)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, new ProfilingCustomValue { IntValue = value2 }, null);
        }

        public void Begin(int value0, int value1, int value2, int value3)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, new ProfilingCustomValue { IntValue = value2 }, new ProfilingCustomValue { IntValue = value3 });
        }

        public void Begin(float value0)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { FloatValue = value0 }, null, null, null);
        }

        public void Begin(float value0, float value1)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, null, null);
        }

        public void Begin(float value0, float value1, float value2)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, new ProfilingCustomValue { FloatValue = value2 }, null);
        }

        public void Begin(float value0, float value1, float value2, float value3)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, new ProfilingCustomValue { FloatValue = value2 }, new ProfilingCustomValue { FloatValue = value3 });
        }

        public void Begin(long value0)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { LongValue = value0 }, null, null, null);
        }

        public void Begin(long value0, long value1)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, null, null);
        }

        public void Begin(long value0, long value1, long value2)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, new ProfilingCustomValue { LongValue = value2 }, null);
        }

        public void Begin(long value0, long value1, long value2, long value3)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, new ProfilingCustomValue { LongValue = value2 }, new ProfilingCustomValue { LongValue = value3 });
        }

        public void Begin(double value0)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { DoubleValue = value0 }, null, null, null);
        }

        public void Begin(double value0, double value1)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, null, null);
        }

        public void Begin(double value0, double value1, double value2)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, new ProfilingCustomValue { DoubleValue = value2 }, null);
        }

        public void Begin(double value0, double value1, double value2, double value3)
        {
            EmitEvent(ProfilingMessageType.Begin, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, new ProfilingCustomValue { DoubleValue = value2 }, new ProfilingCustomValue { DoubleValue = value3 });
        }

        /// <summary>
        /// Emits a Mark event.
        /// </summary>
        public void Mark()
        {
            EmitEvent(ProfilingMessageType.Mark);
        }

        /// <summary>
        /// Emits a Mark event with the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Mark(string text)
        {
            EmitEvent(ProfilingMessageType.Mark, text);
        }

        /// <summary>
        /// Emits a Mark event with the specified formatted text.
        /// </summary>
        /// <param name="textFormat">The text format.</param>
        /// <param name="textFormatArguments">The text format arguments.</param>
        public void Mark(string textFormat, params object[] textFormatArguments)
        {
            EmitEvent(ProfilingMessageType.Mark, textFormat, textFormatArguments);
        }

        public void Mark(int value0)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { IntValue = value0 }, null, null, null);
        }

        public void Mark(int value0, int value1)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, null, null);
        }

        public void Mark(int value0, int value1, int value2)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, new ProfilingCustomValue { IntValue = value2 }, null);
        }

        public void Mark(int value0, int value1, int value2, int value3)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, new ProfilingCustomValue { IntValue = value2 }, new ProfilingCustomValue { IntValue = value3 });
        }

        public void Mark(float value0)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { FloatValue = value0 }, null, null, null);
        }

        public void Mark(float value0, float value1)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, null, null);
        }

        public void Mark(float value0, float value1, float value2)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, new ProfilingCustomValue { FloatValue = value2 }, null);
        }

        public void Mark(float value0, float value1, float value2, float value3)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, new ProfilingCustomValue { FloatValue = value2 }, new ProfilingCustomValue { FloatValue = value3 });
        }

        public void Mark(long value0)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { LongValue = value0 }, null, null, null);
        }

        public void Mark(long value0, long value1)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, null, null);
        }

        public void Mark(long value0, long value1, long value2)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, new ProfilingCustomValue { LongValue = value2 }, null);
        }

        public void Mark(long value0, long value1, long value2, long value3)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, new ProfilingCustomValue { LongValue = value2 }, new ProfilingCustomValue { LongValue = value3 });
        }

        public void Mark(double value0)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { DoubleValue = value0 }, null, null, null);
        }

        public void Mark(double value0, double value1)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, null, null);
        }

        public void Mark(double value0, double value1, double value2)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, new ProfilingCustomValue { DoubleValue = value2 }, null);
        }

        public void Mark(double value0, double value1, double value2, double value3)
        {
            EmitEvent(ProfilingMessageType.Mark, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, new ProfilingCustomValue { DoubleValue = value2 }, new ProfilingCustomValue { DoubleValue = value3 });
        }

        /// <summary>
        /// Emits a End profiling event.
        /// </summary>
        public void End()
        {
            EmitEvent(ProfilingMessageType.End);
        }

        /// <summary>
        /// Emits a End profiling event with the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void End(string text)
        {
            EmitEvent(ProfilingMessageType.End, text);
        }

        /// <summary>
        /// Emits a End profiling event with the specified formatted text.
        /// </summary>
        /// <param name="textFormat">The text format.</param>
        /// <param name="textFormatArguments">The text format arguments.</param>
        public void End(string textFormat, params object[] textFormatArguments)
        {
            EmitEvent(ProfilingMessageType.End, textFormat, textFormatArguments);
        }

        public void End(int value0)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { IntValue = value0 }, null, null, null);
        }

        public void End(int value0, int value1)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, null, null);
        }

        public void End(int value0, int value1, int value2)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, new ProfilingCustomValue { IntValue = value2 }, null);
        }

        public void End(int value0, int value1, int value2, int value3)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { IntValue = value0 }, new ProfilingCustomValue { IntValue = value1 }, new ProfilingCustomValue { IntValue = value2 }, new ProfilingCustomValue { IntValue = value3 });
        }

        public void End(float value0)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { FloatValue = value0 }, null, null, null);
        }

        public void End(float value0, float value1)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, null, null);
        }

        public void End(float value0, float value1, float value2)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, new ProfilingCustomValue { FloatValue = value2 }, null);
        }

        public void End(float value0, float value1, float value2, float value3)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { FloatValue = value0 }, new ProfilingCustomValue { FloatValue = value1 }, new ProfilingCustomValue { FloatValue = value2 }, new ProfilingCustomValue { FloatValue = value3 });
        }

        public void End(long value0)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { LongValue = value0 }, null, null, null);
        }

        public void End(long value0, long value1)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, null, null);
        }

        public void End(long value0, long value1, long value2)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, new ProfilingCustomValue { LongValue = value2 }, null);
        }

        public void End(long value0, long value1, long value2, long value3)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { LongValue = value0 }, new ProfilingCustomValue { LongValue = value1 }, new ProfilingCustomValue { LongValue = value2 }, new ProfilingCustomValue { LongValue = value3 });
        }

        public void End(double value0)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { DoubleValue = value0 }, null, null, null);
        }

        public void End(double value0, double value1)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, null, null);
        }

        public void End(double value0, double value1, double value2)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, new ProfilingCustomValue { DoubleValue = value2 }, null);
        }

        public void End(double value0, double value1, double value2, double value3)
        {
            EmitEvent(ProfilingMessageType.End, new ProfilingCustomValue { DoubleValue = value0 }, new ProfilingCustomValue { DoubleValue = value1 }, new ProfilingCustomValue { DoubleValue = value2 }, new ProfilingCustomValue { DoubleValue = value3 });
        }

        private void EmitEvent(ProfilingMessageType profilingType, string text = null)
        {
            // Perform a Mark event only if the profiling is running
            if (!isEnabled) return;

            var timeStamp = Stopwatch.GetTimestamp();

            // In the case of begin/end, reuse the text from the `begin`event 
            // if the text is null for `end` event.
            if (text == null && profilingType != ProfilingMessageType.Mark)
                text = beginText;

            if (profilingType == ProfilingMessageType.Begin)
            {
                startTime = timeStamp;
                beginText = text;
            }
            else if (profilingType == ProfilingMessageType.End)
            {
                beginText = null;
            }

            // Create profiler event
            // TODO ideally we should make a copy of the attributes
            var profilerEvent = new ProfilingEvent(profilingId, profilingKey, profilingType, timeStamp, timeStamp - startTime, text, attributes);

            // Send profiler event to Profiler
            Profiler.ProcessEvent(ref profilerEvent);
        }

        private void EmitEvent(ProfilingMessageType profilingType, string textFormat, params object[] textFormatArguments)
        {
            // Perform a Mark event only if the profiling is running
            if (!isEnabled) return;

            var timeStamp = Stopwatch.GetTimestamp();

            // In the case of begin/end, reuse the text from the `begin`event 
            // if the text is null for `end` event.
            var text = textFormat != null ? string.Format(textFormat, textFormatArguments) : profilingType == ProfilingMessageType.Mark ? null : beginText;

            if (profilingType == ProfilingMessageType.Begin)
            {
                startTime = timeStamp;
                beginText = text;
            }
            else if (profilingType == ProfilingMessageType.End)
            {
                beginText = null;
            }

            // Create profiler event
            // TODO ideally we should make a copy of the attributes
            var profilerEvent = new ProfilingEvent(profilingId, profilingKey, profilingType, timeStamp, timeStamp - startTime, text, attributes);

            // Send profiler event to Profiler
            Profiler.ProcessEvent(ref profilerEvent);
        }

        private void EmitEvent(ProfilingMessageType profilingType, ProfilingCustomValue? value0, ProfilingCustomValue? value1, ProfilingCustomValue? value2, ProfilingCustomValue? value3)
        {
            // Perform a Mark event only if the profiling is running
            if (!isEnabled) return;

            var timeStamp = Stopwatch.GetTimestamp();

            if (profilingType == ProfilingMessageType.Begin)
            {
                startTime = timeStamp;
            }
            else if (profilingType == ProfilingMessageType.End)
            {
                beginText = null;
            }

            // Create profiler event
            // TODO ideally we should make a copy of the attributes
            var profilerEvent = new ProfilingEvent(profilingId, profilingKey, profilingType, timeStamp, timeStamp - startTime, null, attributes, value0, value1, value2, value3);

            // Send profiler event to Profiler
            Profiler.ProcessEvent(ref profilerEvent);
        }

        private TimeSpan GetElapsedTime()
        {
            var delta = Stopwatch.GetTimestamp() - startTime;
            return new TimeSpan((delta * 10000000) / Stopwatch.Frequency);
        }
    }
}