using System;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Rendering
{
    public class RenderStreamPlugin : RenderPassPlugin
    {
        private DelegateHolder<ThreadContext>.DelegateType startPassAction;
        private DelegateHolder<ThreadContext>.DelegateType endPassAction;

        public RenderStreamPlugin()
        {
            EnableSetTargets = true;
            Parameters.RegisterParameter(RenderTargetKeys.StreamTarget);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable set targets].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable set targets]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSetTargets { get; set; }

        public virtual Buffer StreamTarget
        {
            get
            {
                return Parameters.Get(RenderTargetKeys.StreamTarget);
            }
            set
            {
                Parameters.Set(RenderTargetKeys.StreamTarget, value);
            }
        }


        public override void Load()
        {
 	        base.Load();

            if (OfflineCompilation)
                return;

            if (EnableSetTargets)
            {
                startPassAction = (threadContext) => threadContext.GraphicsDevice.SetStreamTargets(StreamTarget);
                endPassAction = (threadContext) => threadContext.GraphicsDevice.SetStreamTargets(null);

                RenderPass.StartPass += startPassAction;
                RenderPass.EndPass += endPassAction;
            }
        }

        public override void Unload()
        {
            base.Unload();

            if (OfflineCompilation)
                return;

            if (EnableSetTargets)
            {
                RenderPass.StartPass -= startPassAction;
                RenderPass.EndPass -= endPassAction;

                startPassAction = null;
                endPassAction = null;
            }
        }
    }
}