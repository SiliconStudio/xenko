// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets.Diff
{
    public enum Diff3ChangeType
    {
        None,

        Children,

        MergeFromAsset1,

        MergeFromAsset2,

        MergeFromAsset1And2,

        // After Conflict enum, all enums are considered as conflicts. Insert conflict after Conflict

        Conflict,

        ConflictType,

        ConflictArraySize,

        InvalidNodeType,
    }
}