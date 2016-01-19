using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.ActionStack
{
    public class DirtiableManager : IDisposable
    {
        private readonly Dictionary<IDirtiable, List<IDirtiable>> dependenciesMap = new Dictionary<IDirtiable, List<IDirtiable>>();
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

        /// <summary>
        /// Registers a <see cref="IDirtiable"/> as a dependency of another <see cref="IDirtiable"/> regarding its dirty flag.
        /// When the affecting object becomes dirty, the affected object also become dirty.
        /// </summary>
        /// <param name="affectingDirtiable">The dirtiable object that affects the dirtiness of another object.</param>
        /// <param name="affectedDirtiable">The dirtiable object that is affected by the dirtiness of another object.</param>
        /// <exception cref="ArgumentException">The given dirtiable object is already registered.</exception>
        public void RegisterDirtiableDependency(IDirtiable affectingDirtiable, IDirtiable affectedDirtiable)
        {
            List<IDirtiable> dependencies;
            if (dependenciesMap.TryGetValue(affectingDirtiable, out dependencies))
            {
                if (dependencies.Contains(affectedDirtiable))
                    throw new InvalidOperationException("This dependency between IDirtiable objects is already registered.");
                dependencies.Add(affectedDirtiable);
            }
            else
            {
                dependencies = new List<IDirtiable> { affectedDirtiable };
                dependenciesMap.Add(affectingDirtiable, dependencies);
            }
        }

        /// <summary>
        /// Unregisters a <see cref="IDirtiable"/> as a dependency of another <see cref="IDirtiable"/> regarding its dirty flag.
        /// </summary>
        /// <param name="affectingDirtiable">The dirtiable object that doesn't affect anymore the dirtiness of another object.</param>
        /// <param name="affectedDirtiable">The dirtiable object that isn't affected anymore by the dirtiness of another object. If <c>null</c>, all affected objects are unregistered.</param>
        /// <exception cref="ArgumentException">The given dirtiable object is not registered.</exception>
        public void UnregisterDirtiableDependency(IDirtiable affectingDirtiable, IDirtiable affectedDirtiable)
        {
            if (affectedDirtiable == null)
            {
                dependenciesMap.Remove(affectingDirtiable);
            }
            else
            {
                List<IDirtiable> dependencies;
                if (!dependenciesMap.TryGetValue(affectingDirtiable, out dependencies) || !dependencies.Remove(affectedDirtiable))
                    throw new InvalidOperationException("This dependency between IDirtiable objects is not registered.");
            }
        }

        private void UpdateDirtiables(HashSet<IDirtiable> dirtiables)
        {
            Dictionary<IDirtiable, bool> dirtiablesToUpdate = new Dictionary<IDirtiable, bool>();
            var allDependencies = new List<IDirtiable>();

            // First pass, we gather add dependent dirtiable objects to the list of objects to update
            foreach (var dirtiable in dirtiables)
            {
                List<IDirtiable> dependencies;
                if (dependenciesMap.TryGetValue(dirtiable, out dependencies))
                {
                    allDependencies.AddRange(dependencies);
                }
            }
            foreach (var dependency in allDependencies)
                dirtiables.Add(dependency);

            // Second pass, for each dirtiable objects to update we compute its new diryt flag
            foreach (var dirtiable in dirtiables)
            {
                List<DirtiableActionItem> dirtiableActionItems;
                dirtiableActionMap.TryGetValue(dirtiable, out dirtiableActionItems);
                List<DirtiableActionItem> discardedDirtiableActionItems;
                swallowedActionsMap.TryGetValue(dirtiable, out discardedDirtiableActionItems);
                List<IDirtiable> dependencies;
                dependenciesMap.TryGetValue(dirtiable, out dependencies);

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
                
                // Update the dirty status of its dependencies
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        dirtiablesToUpdate[dependency] = dirtiablesToUpdate.ContainsKey(dependency) ? dirtiablesToUpdate[dependency] || isDirty : isDirty;
                    }
                }
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
