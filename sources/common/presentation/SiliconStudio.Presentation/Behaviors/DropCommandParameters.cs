// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// Information about a drag & drop command.
    /// </summary>
    // TODO: Move this in a ViewModel-dedicated assembly
    [DataContract]
    public class DropCommandParameters
    {
        public string DataType { get; set; }
        public object Data { get; set; }
        public object Parent { get; set; }
        public int SourceIndex { get; set; }
        public int TargetIndex { get; set; }
        public object Sender { get; set; }
    }
}