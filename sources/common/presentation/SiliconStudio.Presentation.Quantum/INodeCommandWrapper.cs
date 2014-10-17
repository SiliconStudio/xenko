// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows.Input;

using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum
{
    public interface INodeCommandWrapper : ICommand
    {
        string Name { get; }

        CombineMode CombineMode { get; }
    }
}