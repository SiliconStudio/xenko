// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Presentation.Commands;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public interface INodeCommandWrapper : ICommandBase
    {
        string ActionName { get; }

        string Name { get; }

        CombineMode CombineMode { get; }
    }
}
