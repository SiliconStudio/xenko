// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Shaders.Utility
{
    public partial class MessageCode
    {
        // Errors
        public static readonly MessageCode ErrorMatrixInvalidMemberReference    = new MessageCode("E0100", "Invalid member reference [{0}] for matrix type");
        public static readonly MessageCode ErrorMatrixInvalidIndex              = new MessageCode("E0101", "Invalid index [{0}] for matrix type member access. Must be in the range [{1},{2}]  member for array type");
    }
}
