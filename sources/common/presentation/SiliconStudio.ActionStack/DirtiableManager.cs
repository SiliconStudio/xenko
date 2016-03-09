using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.ActionStack
{
    public class DirtiableManager : IDisposable
    {
        private readonly Dictionary<IDirtiable, List<DirtiableActionItem>> dirtiableActionMap = new Dictionary<IDirtiable, List<DirtiableActionItem>>();
        private readonly Dictionary<IDirtiable, List<DirtiableActionItem>> swallowedActionsMap = new Dictionary<IDirtiable, List<DirtiableActionItem>>();

        private IActionStack actionStack;

        public DirtiableManager(IActionStack actionStack)
        {
            if (actionStack == null) throw new ArgumentNullException(nameof(actionStack));
            this.actionStack = actionStack;
            actionStack.Undone += ActionItemsAddedOrChanged;
            actionStack.Redone += ActionItemsAddedOrChanged;
            actionStack.ActionItemsAdded += ActionItemsAddedOrChanged;
            actionStack.ActionItemsCleared += ActionItemsCleared;
            actionStack.ActionItemsDiscarded += ActionItemsDiscarded;
        }

        public void Dispose()
        {
            dirtiableActionMap.Clear();
            actionStack.Undone -= ActionItemsAddedOrChanged;
            actionStack.Redone -= ActionItemsAddedOrChanged;
            actionStack.ActionItemsAdded -= ActionItemsAddedOrChanged;
            actionStack.ActionItemsCleared -= ActionItemsCleared;
            actionStack.ActionItemsDiscarded -= ActionItemsDiscarded;
            actionStack = null;
        }

        private void UpdateDirtiables(HashSet<IDirtiable> dirtiables)
        {
            Dictionary<IDirtiable, bool> dirtiablesToUpdate = new Dictionary<IDirtiable, bool>();

            // For each dirtiable objects to update we compute its new dirty flag
            foreach (var dirtiable in dirtiables)
            {
                List<DirtiableActionItem> dirtiableActionItems;
                dirtiableActionMap.TryGetValue(dirtiable, out dirtiableActionItems);
                List<DirtiableActionItem> discardedDirtiableActionItems;
                swallowedActionsMap.TryGetValue(dirtiable, out discardedDirtiableActionItems);

                bool isDirty = false;
                // Check if it is dirty regarding to action currently in the action stack
                if (dirtiableActionItems != null)
                {
                    isDirty = dirtiableActionItems.Any(x => x.IsSaved != x.IsDone);
                }
                // Check if it is dirty regarding to actions swallowed but still unsaved.
                if (discardedDirtiableActionItems != null)
                {
                    isDirty = isDirty || discardedDirtiableActionItems.Count > 0;
                }

                // Update its dirty status according to the computed flag and a previously determinated update (from dependencies)
                dirtiablesToUpdate[dirtiable] = dirtiablesToUpdate.ContainsKey(dirtiable) ? dirtiablesToUpdate[dirtiable] || isDirty : isDirty;
            }

            // Finally propagate the update
            foreach (var dirtiable in dirtiablesToUpdate)
            {
                dirtiable.Key.UpdateDirtiness(dirtiable.Value);
            }
        }

        private void ActionItemsAddedOrChanged(object sender, ActionItemsEventArgs<IActionItem> e)
        {
            var dirtiables = new HashSet<IDirtiable>();
            foreach (var actionItem in e.ActionItems.SelectMany(GetDirtiableItems))
            {
                foreach (var dirtiable in actionItem.Dirtiables)
                {
                    List<DirtiableActionItem> dirtiableActionItems;
                    if (!dirtiableActionMap.TryGetValue(dirtiable, out dirtiableActionItems))
                    {
                        dirtiableActionItems = new List<DirtiableActionItem>();
                        dirtiableActionMap.Add(dirtiable, dirtiableActionItems);
                    }
                    dirtiableActionItems.Add(actionItem);
                    dirtiables.Add(dirtiable);
                }
            }
            UpdateDirtiables(dirtiables);
        }


        private void ActionItemsCleared(object sender, EventArgs e)
        {
            dirtiableActionMap.Clear();
        }

        private void ActionItemsDiscarded(object sender, DiscardedActionItemsEventArgs<IActionItem> e)
        {
            var dirtiables = new HashSet<IDirtiable>();
            switch (e.Type)
            {
                case ActionItemDiscardType.Swallowed:
                    foreach (var actionItem in e.ActionItems.SelectMany(GetDirtiableItems))
                    {
                        foreach (var dirtiable in actionItem.Dirtiables)
                        {
                            List<DirtiableActionItem> dirtiableActionItems;
                            if (!dirtiableActionMap.TryGetValue(dirtiable, out dirtiableActionItems))
                            {
                                dirtiableActionItems = new List<DirtiableActionItem>();
                                dirtiableActionMap.Add(dirtiable, dirtiableActionItems);
                            }
                            dirtiableActionItems.Remove(actionItem);

                            if (!swallowedActionsMap.TryGetValue(dirtiable, out dirtiableActionItems))
                            {
                                dirtiableActionItems = new List<DirtiableActionItem>();
                                swallowedActionsMap.Add(dirtiable, dirtiableActionItems);
                            }
                            dirtiableActionItems.Add(actionItem);
                        }
                    }
                    break;
                case ActionItemDiscardType.Disbranched:
                    foreach (var actionItem in e.ActionItems.SelectMany(GetDirtiableItems))
                    {
                        foreach (var dirtiable in actionItem.Dirtiables)
                        {
                            List<DirtiableActionItem> dirtiableActionItems;
                            if (!dirtiableActionMap.TryGetValue(dirtiable, out dirtiableActionItems))
                            {
                                dirtiableActionItems = new List<DirtiableActionItem>();
                                dirtiableActionMap.Add(dirtiable, dirtiableActionItems);
                            }
                            dirtiableActionItems.Remove(actionItem);
                            dirtiables.Add(dirtiable);
                        }
                    }
                    break;
                case ActionItemDiscardType.UndoRedoInProgress:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            UpdateDirtiables(dirtiables);
        }

        private static IEnumerable<DirtiableActionItem> GetDirtiableItems(IActionItem actionItem)
        {
            var dirtiableActionItem = actionItem as DirtiableActionItem;
            if (dirtiableActionItem != null)
                return new[] { dirtiableActionItem };

            var aggegateActionItem = actionItem as AggregateActionItem;
            if (aggegateActionItem != null)
                return aggegateActionItem.GetInnerActionItems().OfType<DirtiableActionItem>();

            return Enumerable.Empty<DirtiableActionItem>();
        }

        public void NotifySave()
        {
            // TODO: we could add an event in ActionStack and subscribe it to avoid to have to call this
            var dirtiables = new HashSet<IDirtiable>(dirtiableActionMap.Keys);
            swallowedActionsMap.Clear();
            UpdateDirtiables(dirtiables);
        }
    }
}
