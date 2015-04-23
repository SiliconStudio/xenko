// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;
using SiliconStudio.Paradox.Animations;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    public class TestAnimationChannel
    {
        [Test, Ignore]
        public void TestFitting()
        {
            // Make a sinus between T = 0s to 10s at 60 FPS
            var animationChannel = new AnimationChannel();
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.Zero, Value = 0.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(10.0), Value = 0.0f });

            var maxErrorThreshold = 0.05f;
            var timeStep = CompressedTimeSpan.FromSeconds(1.0f / 60.0f);
            Func<CompressedTimeSpan, float> curve = x =>
                {
                    if (x.Ticks == 196588)
                    {
                    }
                    return (float)Math.Sin(x.Ticks / (double)CompressedTimeSpan.FromSeconds(10.0).Ticks * Math.PI * 2.0);
                };
            animationChannel.Fitting(
                curve,
                CompressedTimeSpan.FromSeconds(1.0f / 60.0f),
                maxErrorThreshold);

            var evaluator = new AnimationChannel.Evaluator(animationChannel.KeyFrames);
            for (var time = CompressedTimeSpan.Zero; time < CompressedTimeSpan.FromSeconds(10.0); time += timeStep)
            {
                var diff = Math.Abs(curve(time) - evaluator.Evaluate(time));
                Assert.That(diff, Is.LessThanOrEqualTo(maxErrorThreshold));
            }
        }

        [Test]
        public void TestDiscontinuity()
        {
            var animationChannel = new AnimationChannel();
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.Zero, Value = 0.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(1.0), Value = 0.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(1.0), Value = 0.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(1.0), Value = 1.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(1.0), Value = 1.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(2.0), Value = 1.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(2.0), Value = 1.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(2.0), Value = 0.0f });
            animationChannel.KeyFrames.Add(new KeyFrameData<float> { Time = CompressedTimeSpan.FromSeconds(2.0), Value = 0.0f });

            var evaluator = new AnimationChannel.Evaluator(animationChannel.KeyFrames);
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(0.0)), Is.EqualTo(0.0f));
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(0.999999)), Is.EqualTo(0.0f));
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(1.0)), Is.EqualTo(1.0f));
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(1.000001)), Is.EqualTo(1.0f));
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(1.999999)), Is.EqualTo(1.0f));
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(2.0)), Is.EqualTo(0.0f));
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(2.000001)), Is.EqualTo(0.0f));
            Assert.That(evaluator.Evaluate(CompressedTimeSpan.FromSeconds(2.5)), Is.EqualTo(0.0f));
        }
    }
}