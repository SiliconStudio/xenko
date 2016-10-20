// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input
{
    public interface IInputDevice : IDisposable
    {
        string DeviceName { get; }

        Guid Id { get; }

        // TODO: Move to a more specific subclass?
        void Update();
    }
}