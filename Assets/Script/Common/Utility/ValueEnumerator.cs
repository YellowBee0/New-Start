namespace YBFramework.Common
{
    public struct ValueEnumerator<TValue>
    {
        private readonly IValueIterator<TValue> m_ValueIterator;

        private TValue m_Current;

        private int m_Index;

        public TValue Current => m_Current;

        public ValueEnumerator(IValueIterator<TValue> valueIterator)
        {
            m_ValueIterator = valueIterator;
            m_Current = default;
            m_Index = 0;
        }

        public bool MoveNext()
        {
            return m_ValueIterator.Iterator(m_Index++, out m_Current);
        }
    }
}