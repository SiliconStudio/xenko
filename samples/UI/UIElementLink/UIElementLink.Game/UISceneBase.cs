using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace UIElementLink
{
    public abstract class UISceneBase : SyncScript
    {
        protected Game UIGame;

        protected bool IsRunning;

        protected bool SceneCreated;

        public override void Start()
        {
            IsRunning = true;

            UIGame = (Game)Services.GetServiceAs<IGame>();

            AdjustVirtualResolution(this, EventArgs.Empty);
            Game.Window.ClientSizeChanged += AdjustVirtualResolution;

            CreateScene();
        }

        public override void Update()
        {
            UpdateScene();
        }

        protected virtual void UpdateScene()
        {
        }

        public override void Cancel()
        {
            Game.Window.ClientSizeChanged -= AdjustVirtualResolution;

            IsRunning = false;
            SceneCreated = false;
        }

        private void AdjustVirtualResolution(object sender, EventArgs e)
        {
            var backBufferSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);
            Entity.Get<UIComponent>().Resolution = new Vector3(backBufferSize, 1000);
        }

        protected void CreateScene()
        {
            if (!SceneCreated)
                LoadScene();

            SceneCreated = true;
        }

        protected abstract void LoadScene();
    }
}