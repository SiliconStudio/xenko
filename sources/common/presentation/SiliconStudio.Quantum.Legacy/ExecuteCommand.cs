// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy
{
    /// <summary>
    /// Executes a command through the view model library.
    /// </summary>
    /// <param name="viewModelNode">The view model node.</param>
    /// <param name="parameter">The parameter.</param>
    public delegate void ExecuteCommand(IViewModelNode viewModelNode, object parameter);
}