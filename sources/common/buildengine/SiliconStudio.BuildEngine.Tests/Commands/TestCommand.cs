// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.IO;

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.BuildEngine.Tests.Commands
{
    public abstract class TestCommand : Command
    {
        /// <inheritdoc/>
        public override string Title { get { return ToString(); } }

        private static int commandCounter;
        private readonly int commandId;

        public static void ResetCounter()
        {
            commandCounter = 0;
        }

        protected TestCommand()
        {
            commandId = ++commandCounter;
        }

        public override string ToString()
        {
            return GetType().Name + " " + commandId;
        }

        protected override void ComputeParameterHash(BinarySerializationWriter writer)
        {
            base.ComputeParameterHash(writer);

            writer.Write(commandId);
        }
    }
}
