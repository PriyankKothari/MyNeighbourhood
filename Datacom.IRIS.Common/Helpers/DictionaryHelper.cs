using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Helpers
{
    public static class DictionaryHelper
    {
        public static void AddEntry<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value, bool addNullValueEntry = false)
        {
            if (value != null || addNullValueEntry)
                dict.Add(key, value);
        }
    }
}
