using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.Editor.Graph
{
    public sealed class NewNodeView : Node
    {
        public readonly BaseNodeDataPresenter NodeDataPresenter;

        public NewNodeView(BaseNodeDataPresenter nodeDataPresenter)
        {
            NodeDataPresenter = nodeDataPresenter;
            title = nodeDataPresenter.GetNodeData().NodeName;
            SetPosition(new Rect(nodeDataPresenter.GetNodeData().Position, Vector2.one));
        }

        public void AddPortContentView(VisualElement portContentView, Direction direction)
        {
            portContentView.style.borderBottomColor = Color.black;
            portContentView.style.borderBottomWidth = .2f;
            if (direction == Direction.Input)
            {
                inputContainer.Add(portContentView);
            }
            else
            {
                outputContainer.Add(portContentView);
            }
        }

        public void RemovePortContentView(VisualElement portContentView, Direction direction)
        {
            CustomGraphView graphView = NodeDataPresenter.GetGraphAssetPresenter().GetGraphView();
            if (direction == Direction.Input)
            {
                inputContainer.Remove(portContentView);
            }
            else
            {
                outputContainer.Remove(portContentView);
            }
        }

        public void RefreshPortContainerDisplay()
        {
            inputContainer.style.display = inputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            outputContainer.style.display = outputContainer.childCount == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}