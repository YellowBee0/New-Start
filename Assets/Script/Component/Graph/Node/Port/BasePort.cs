namespace YBFramework.Component
{
    public abstract class BasePort
    {
        protected ushort m_PortID;

        public ushort GetPortID()
        {
            return m_PortID;
        }
    }
}