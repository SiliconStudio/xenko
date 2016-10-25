// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Android;

namespace SiliconStudio.Xenko.Input
{
    public class InputSourceAndroid : InputSourceBase
    {
        private AndroidXenkoGameView uiControl;

        private readonly List<SensorDeviceBase> sensors = new List<SensorDeviceBase>();
        private KeyboardAndroid keyboard;
        private PointerAndroid pointer;

        public override void Initialize(InputManager inputManager)
        {
            var context = inputManager.Game.Context as GameContextAndroid;
            uiControl = context.Control;

            keyboard = new KeyboardAndroid(uiControl);
            pointer = new PointerAndroid(uiControl);
            RegisterDevice(keyboard);
            RegisterDevice(pointer);

            // List of sensors to try and create for android
            List<SensorDeviceBase> sensorsToCreate = new List<SensorDeviceBase>
            {
                new AccelerometerAndroid(),
                new GyroscopeSensorAndroid(),
                new UserAccelerationSensorAndroid(),
                new GravitySensorAndroid(),
                new OrientationSensorAndroid(),
                new CompassSensorAndroid(this),
            };

            // Try to enable sensors to check if they are supported
            foreach (var sensor in sensorsToCreate)
            {
                if (sensor.Enable())
                {
                    sensors.Add(sensor);
                    RegisterDevice(sensor);
                }
            }

            // Disable sensors by default after checking if they are supported
            foreach (var sensor in sensors)
            {
                sensor.Disable();
            }
        }

        public SensorDeviceBase TryGetSensor(Type type)
        {
            return sensors.FirstOrDefault(x => type.IsAssignableFrom(x.GetType()));
        }

        public override bool IsEnabled(GameContext gameContext)
        {
            return gameContext.ContextType == AppContextType.Android;
        }
    }
}

#endif