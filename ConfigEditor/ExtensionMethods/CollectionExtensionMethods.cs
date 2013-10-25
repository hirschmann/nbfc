using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ConfigEditor
{
    public static class CollectionExtensionMethods
    {
        public static int Replace<TSource>(this Collection<TSource> oc, TSource oldItem, TSource newItem)
        {
            int idx = oc.IndexOf(oldItem);

            if (idx >= 0)
            {
                oc[idx] = newItem;
                return idx;
            }
            else
            {
                throw new InvalidOperationException("The collection does not contain oldItem");
            }
        }
    }
}
