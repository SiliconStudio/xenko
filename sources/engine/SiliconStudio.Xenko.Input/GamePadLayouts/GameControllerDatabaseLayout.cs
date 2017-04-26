// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Layout that matches a name or product Id
    /// </summary>
    public class GameControllerDatabaseLayout : GamePadLayout
    {
        public string Name;
        public Guid ProductId;

        public GameControllerDatabaseLayout(Guid productId, string name)
        {
            Name = name;
            ProductId = productId;
        }

        public override bool MatchDevice(IInputSource source, IGameControllerDevice device)
        {
            return device.ProductId == ProductId || device.Name == Name;
        }
    }
}