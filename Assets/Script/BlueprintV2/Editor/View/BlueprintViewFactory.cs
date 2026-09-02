namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 模型类型到具体 View 类型的扩展点，业务节点和端口无需修改投影层。
    /// </summary>
    public interface IBlueprintViewFactory
    {
        BlueprintNodeView CreateNode(BlueprintNodeData node);

        BlueprintPortView CreatePort(BlueprintPortData port);

        BlueprintEdgeView CreateEdge(BlueprintConnectionData connection, BlueprintPortReference owner);
    }

    public sealed class DefaultBlueprintViewFactory : IBlueprintViewFactory
    {
        public BlueprintNodeView CreateNode(BlueprintNodeData node)
        {
            return new BlueprintNodeView(node);
        }

        public BlueprintPortView CreatePort(BlueprintPortData port)
        {
            return new BlueprintPortView(port);
        }

        public BlueprintEdgeView CreateEdge(BlueprintConnectionData connection, BlueprintPortReference owner)
        {
            return new BlueprintEdgeView(connection, owner);
        }
    }
}
