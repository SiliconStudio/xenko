// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets.Visitors;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Contains different default merge policies to use with <see cref="AssetMerge"/>.
    /// </summary>
    public static class AssetMergePolicies
    {
        /// <summary>
        /// The merge policy is expecting <c>asset2</c> to be the new base of <c>asset1</c>, and <c>base</c> the current base of <c>asset1</c>. 
        /// In case of conflicts:
        /// <ul>
        /// <li>If there is a type conflict, leave as-is</li>
        /// <li>If there is a <see cref="Diff3ChangeType.Conflict"/>:
        ///     <ul>
        ///      <li>If it is on a member, and both <c>asset1</c> and <c>asset2</c> are not null and changes were done 
        ///      in <c>asset2</c> but not in <c>asset1</c>, select <c>asset2</c> else select <c>asset1</c></li>
        ///      <li>If it is on a list item, we can't resolve it and leave the conflict as-is</li>
        ///      <li>If it is on a array item, we select <c>asset2</c></li>
        ///     </ul>
        /// </li>
        /// </ul>
        /// </summary>
        /// <param name="diff3Node">The diff3 node.</param>
        /// <returns>.</returns>
        public static Diff3ChangeType MergePolicyAsset2AsNewBaseOfAsset1(Diff3Node diff3Node)
        {
            var hasConflicts = diff3Node.ChangeType > Diff3ChangeType.Conflict;
            if (hasConflicts)
            {
                return diff3Node.ChangeType;
            }

            switch (diff3Node.ChangeType)
            {
                case Diff3ChangeType.Conflict:
                case Diff3ChangeType.MergeFromAsset2:

                    var dataNode = diff3Node.Asset2Node ?? diff3Node.Asset1Node ?? diff3Node.BaseNode;

                    if (dataNode is DataVisitMember)
                    {
                        if (diff3Node.Asset1Node != null && diff3Node.Asset2Node != null && diff3Node.ChangeType == Diff3ChangeType.MergeFromAsset2)
                        {
                            return Diff3ChangeType.MergeFromAsset2;
                        }

                        return Diff3ChangeType.MergeFromAsset1;
                    }

                    if (dataNode is DataVisitListItem)
                    {
                        // If we have a conflict in a list, we can't really resolve it here, so we are breaking out of the loop
                        if (diff3Node.ChangeType == Diff3ChangeType.Conflict)
                        {
                            return Diff3ChangeType.Conflict;
                        }
                        return Diff3ChangeType.MergeFromAsset2;
                    }

                    if (dataNode is DataVisitDictionaryItem)
                    {
                        return Diff3ChangeType.MergeFromAsset2;
                    }

                    if (dataNode is DataVisitArrayItem)
                    {
                        if (diff3Node.Asset1Node != null && ((DataVisitArrayItem)diff3Node.Asset1Node).Index == ((DataVisitArrayItem)dataNode).Index)
                        {
                            return Diff3ChangeType.MergeFromAsset2;
                        }
                    }
                    break;
            }

            return diff3Node.ChangeType;
        }
    }
}