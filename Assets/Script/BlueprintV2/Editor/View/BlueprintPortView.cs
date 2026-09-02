using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 端口的纯视图对象。方向或容量改变时需要替换，因为它们由 Port 构造函数确定。
    /// </summary>
    public class BlueprintPortView : Port
    {
        private readonly Type m_ModelType;
        private bool m_HasConnector;

        public BlueprintPortView(BlueprintPortData port)
            : base(
                Orientation.Horizontal,
                ToViewDirection(port.Direction),
                ToViewCapacity(port.Capacity),
                null)
        {
            if (port == null)
            {
                throw new ArgumentNullException(nameof(port));
            }

            PortReference = port.Reference;
            m_ModelType = port.GetType();
            Refresh(port);
        }

        public BlueprintPortReference PortReference { get; }

        public bool Matches(BlueprintPortData port)
        {
            return port != null &&
                   port.Reference == PortReference &&
                   port.GetType() == m_ModelType &&
                   direction == ToViewDirection(port.Direction) &&
                   capacity == ToViewCapacity(port.Capacity);
        }

        public virtual void Refresh(BlueprintPortData port)
        {
            portName = port.DisplayName;
            portColor = port.Color;
        }

        internal void AttachConnector(IEdgeConnectorListener listener)
        {
            // View 可能被多次刷新，但拖线 Manipulator 只能安装一次，避免重复回调。
            if (m_HasConnector)
            {
                return;
            }

            m_EdgeConnector = new EdgeConnector<Edge>(listener);
            this.AddManipulator(m_EdgeConnector);
            m_HasConnector = true;
        }

        private static Direction ToViewDirection(BlueprintPortDirection direction)
        {
            return direction == BlueprintPortDirection.Input ? Direction.Input : Direction.Output;
        }

        private static Capacity ToViewCapacity(BlueprintPortCapacity capacity)
        {
            return capacity == BlueprintPortCapacity.Single ? Capacity.Single : Capacity.Multi;
        }
    }
}
