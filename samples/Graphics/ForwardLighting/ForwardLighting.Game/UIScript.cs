using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Events;
using SiliconStudio.Xenko.UI.Panels;

namespace ForwardLighting
{
    /// <summary>
    /// The script in charge of display the UI
    /// </summary>
    public class UIScript : StartupScript
    {
        /// <summary>
        /// The key to get the light component associated to button.
        /// </summary>
        private readonly static PropertyKey<LightComponent> LightKey = new PropertyKey<LightComponent>("LightKey", typeof(UIScript));

        /// <summary>
        /// The key to the shadow on/off button of a light button.
        /// </summary>
        private readonly static PropertyKey<Button> ShadowButtonKey = new PropertyKey<Button>("ShadowButtonKey", typeof(UIScript));
        
        /// <summary>
        /// The font used by the UI
        /// </summary>
        public SpriteFont Font;

        /// <summary>
        /// A reference to the first directional light
        /// </summary>
        public LightComponent DirectionalLight1;

        /// <summary>
        /// A reference to the second directional light
        /// </summary>
        public LightComponent DirectionalLight2;

        /// <summary>
        /// A reference to the point light
        /// </summary>
        public LightComponent PointLight;

        /// <summary>
        /// A reference to the spot light
        /// </summary>
        public LightComponent SpotLight;

        public override void Start()
        {
            base.Start();

            // create the light buttons 
            var buttonLightDirect0 = CreateLightButton(DirectionalLight1);
            var buttonLightDirect1 = CreateLightButton(DirectionalLight2);
            var buttonLightPoint = CreateLightButton(PointLight);
            var buttonLightSpot = CreateLightButton(SpotLight);

            // create the shadow buttons
            var buttonShadowDirectional0 = CreateShadowButton(buttonLightDirect0, DirectionalLight1);
            var buttonShadowDirectional1 = CreateShadowButton(buttonLightDirect1, DirectionalLight2);
            var buttonShadowSpot = CreateShadowButton(buttonLightSpot, SpotLight);
            
            // create the UI stack panel
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                MinimumWidth = 160,
                Children =
                {
                    buttonLightDirect0, buttonShadowDirectional0,
                    buttonLightDirect1, buttonShadowDirectional1,
                    buttonLightPoint,
                    buttonLightSpot, buttonShadowSpot
                }
            };

            Entity.Get<UIComponent>().RootElement = stackPanel;
        }

        public override void Cancel()
        {
            // Cleanup when script is removed
            Entity.Get<UIComponent>().RootElement = null;

            base.Cancel();
        }

        // Function in charge of toggling on/off the lights shadow maps
        private static void ToggleShadowMap(Object sender, RoutedEventArgs args)
        {
            var button = (Button)sender;
            var light = button.DependencyProperties.Get(LightKey);

            var directLight = light != null ? light.Type as IDirectLight : null;
            if (directLight == null)
                return;

            // toggle the shadow and update button text
            directLight.Shadow.Enabled = !directLight.Shadow.Enabled;
            ((TextBlock)button.Content).Text = GetButtonTextOnOffShadow(light);
        }

        // Function in charge of toggling on/off the light themselves
        private void ToggleLight(Object sender, RoutedEventArgs args)
        {
            var button = (Button)sender;
            var light = button.DependencyProperties.Get(LightKey);
            if (light == null)
                return;

            // toggle light and update button text
            light.Enabled = !light.Enabled;
            ((TextBlock)button.Content).Text = GetButtonTextOnOffLight(light);

            var buttonShadow = button.DependencyProperties.Get(ShadowButtonKey);
            if (buttonShadow == null)
                return;

            // disable shadow child button
            buttonShadow.Opacity = light.Enabled ? 1.0f : 0.3f;
            buttonShadow.CanBeHitByUser = light.Enabled;
        }

        // Create a button that can toggle on/off the Light component
        private Button CreateLightButton(LightComponent component)
        {
            var button = new Button
            {
                Name = component.Entity.Name,
                Margin = Thickness.UniformRectangle(5),
                Content = new TextBlock
                {
                    Font = Font, 
                    TextAlignment = TextAlignment.Right,
                    Text = GetButtonTextOnOffLight(component), 
                },
            };
            button.Click += ToggleLight;
            button.DependencyProperties.Set(LightKey, component);

            return button;
        }

        // Create a button that can toggle on/off the light shadow map
        private Button CreateShadowButton(UIElement parentButton, LightComponent component)
        {
            var button = new Button
            {
                Margin = new Thickness(20, 5, 5, 5),
                Name = "Shadow Button " + component.Entity.Name,
                Opacity = component.Enabled ? 1.0f : 0.3f,
                CanBeHitByUser = component.Enabled,
                Content = new TextBlock
                {
                    Font = Font,
                    TextAlignment = TextAlignment.Right,
                    Text = GetButtonTextOnOffShadow(component),
                },
            };

            button.Click += ToggleShadowMap;

            button.DependencyProperties.Set(LightKey, component);
            parentButton.DependencyProperties.Set(ShadowButtonKey, button);

            return button;
        }

        private static string GetButtonTextOnOffShadow(LightComponent component)
        {
            var isEnabled = component.Type is IDirectLight && ((IDirectLight)component.Type).Shadow.Enabled;
            return "Shadow: " + (isEnabled ? "On" : "Off");
        }

        private static string GetButtonTextOnOffLight(LightComponent component)
        {
            return component.Entity.Name + ": " + (component.Enabled ? "On" : "Off");
        }
    }
}