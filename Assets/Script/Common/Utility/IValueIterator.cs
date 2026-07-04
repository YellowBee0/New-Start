namespace YBFramework.Common
{
    /// <summary>
    /// 提供无GC的foreach循环
    /// </summary>
    /// <typeparam name="TValue">循环元素的类型</typeparam>
    public interface IValueIterator<TValue>
    {
        /// <summary>
        /// 迭代函数
        /// </summary>
        /// <param name="index">迭代的索引，从0开始</param>
        /// <param name="current">迭代的值</param>
        /// <returns>是否能进行下一次循环</returns>
        bool Iterator(int index, out TValue current);

        /// <summary>
        /// foreach模式匹配的函数
        /// </summary>
        /// <returns>迭代器（结构体）</returns>
        ValueEnumerator<TValue> GetEnumerator()
        {
            return new ValueEnumerator<TValue>(this);
        }
    }
}