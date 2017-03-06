using System;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class GraphNodePresenter
    {
        public static int CompareChildren(INodePresenter a, INodePresenter b)
        {
            // Order has the best priority for comparison, if set.
            if ((a.Order ?? 0) != (b.Order ?? 0))
                return (a.Order ?? 0).CompareTo(b.Order ?? 0);

            // Then we use index, if they are set and comparable.
            if (!a.Index.IsEmpty && !b.Index.IsEmpty)
            {
                if (a.Index.Value.GetType() == b.Index.Value.GetType())
                {
                    return a.Index.CompareTo(b.Index);
                }
            }

            // Then we use name, only if both orders are unset.
            if (a.Order == null && b.Order == null)
            {
                return string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            // Otherwise, the first child would be the one who have an order value.
            return a.Order == null ? 1 : -1;
        }
    }
}
