namespace YBFramework.Common
{
    public interface IValueIterator<TValue>
    {
        bool Iterator(int index, out TValue current);

        ValueEnumerator<TValue> GetEnumerator()
        {
            return new ValueEnumerator<TValue>(this);
        }
    }
}