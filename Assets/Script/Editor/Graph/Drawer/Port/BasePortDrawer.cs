using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;
using YBFramework.Bridge.Editor;

namespace YBFramework.Editor
{
    [GraphDrawer(typeof(BasePortData))]
    public class BasePortDrawer
    {
        public virtual VisualElement CreatePortContentView(BasePortData portData, SerializedProperty serializedProperty, out PortView portView)
        {
            PortViewArgs portViewArgs = portData.GetPortViewArgs();
            portView = new PortView(portData, portViewArgs.Name, portViewArgs.Direction, portViewArgs.Capacity, portViewArgs.Color);
            return portView;
        }
    }
}