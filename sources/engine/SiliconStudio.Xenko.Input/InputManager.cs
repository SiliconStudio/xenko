// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    public abstract class InputManager : InputManagerBase
    {
        protected const float G = 9.81f;

        /// <summary>
        /// Does InputManager support raw input? By default true.
        /// </summary>
        public static bool UseRawInput = true;

        protected InputManager(IServiceRegistry registry) : base(registry)
        {
        }
    }

    internal abstract class InputManager<TK> : InputManager
    {
        protected InputManager(IServiceRegistry registry) : base(registry)
        {
        }

        protected TK Control;

        public sealed override void Initialize()
        {
            base.Initialize();
            var context = Game.Context as GameContext<TK>;
            if (context != null)
            {
                Initialize(context);
            }
            else
            {
                throw new InvalidOperationException("Incompatible Context and InputManager.");
            }
        }

        /// <summary>
        /// Type safe version of Initialize.
        /// </summary>
        /// <param name="context">Matching context type.</param>
        public abstract void Initialize(GameContext<TK> context);
    }
}