using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Extensions
{
    public static class CollectionElementsConvertExtension
    {
        /// <summary>
        /// Convert a collection of objects or primitives, and convert (or unbox) each element to a double.
        /// Incompatible elements are not added to the list, therefore the resulting collection Count might differ.
        /// </summary>
        /// <param name="collection">source enumerable, can be a system array of primitives, or a collection of boxed numeric types</param>
        /// <returns>Converted collection</returns>
        [NotNull]
        public static List<double> ToListOfDoubles([NotNull] this IList collection)
        {
            var result = new List<double>(collection.Count);
            foreach (var v in collection)
            {
                if (v.GetType().IsPrimitive)
                    result.Add((double)v);
                else
                {
                    var unboxed = (double)System.Convert.ChangeType(v, typeof(double));
                    if (!double.IsNaN(unboxed))
                        result.Add(unboxed);
                }
            }
            return result;
        }
    }
}
