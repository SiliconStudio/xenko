// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageReadback<T> : ImageEffectBase where T : struct
    {
        private readonly List<Texture> stagingTargets;

        private readonly List<bool> stagingUsed;
        private int currentStagingIndex;
        private T[] result;

        private int ringCount;

        public ImageReadback(ImageEffectContext context) : base(context)
        {
            stagingUsed = new List<bool>();
            stagingTargets = new List<Texture>();
            RingCount = 16;
        }

        public int RingCount
        {
            get
            {
                return ringCount;
            }
            set
            {
                if (ringCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Expecting a value > 0");
                }

                ringCount = value;
            }
        }

        public bool IsResultAvailable { get; private set; }

        public T[] Result
        {
            get
            {
                return result;
            }
        }

        protected override void DrawCore()
        {
            var input = GetSafeInput(0);

            // Copy to staging resource
            GraphicsDevice.Copy(input, stagingTargets[currentStagingIndex]);
            stagingUsed[currentStagingIndex] = true;

            // Read-back to CPU using a ring of staging buffers
            for (int i = stagingTargets.Count - 1; i >= 1; i--)
            {
                var oldStagingIndex = (currentStagingIndex + i) % stagingTargets.Count;
                var oldLuminanceStaging = stagingTargets[oldStagingIndex];
                if (stagingUsed[oldStagingIndex] && oldLuminanceStaging.GetData(result, 0, 0, true))
                {
                    IsResultAvailable = true;
                    //Debug.WriteLine(string.Format("Buffer {0}:{1} Lum: {2}", currentStagingIndex, oldStagingIndex, AverageLuminance));
                    break;
                }
            }
            currentStagingIndex = (currentStagingIndex + 1) % stagingTargets.Count;            
        }

        private void EnsureStaging(Texture input)
        {
            if (stagingTargets.Count != RingCount)
            {
                //stagingTargets = new RenderTarget[RingCount];
            }
            throw new NotImplementedException();
        }
    }
}