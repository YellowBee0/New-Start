using System.Collections.Generic;

namespace YBFramework.Common
{
    public static class CustomArrayUtility
    {
        public static TValue FindOtherListValueAtSelfIndexOfKey<TKey, TValue>( IReadOnlyList<TKey> keys, IReadOnlyList<TValue> values, TKey key)
        {
            if (keys == null || values == null || keys.Count != values.Count)
            {
                return default;
            }
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Equals(key))
                {
                    return values[i];
                }
            }
            return default;
        }
    }
}