using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace YBFramework.BlueprintV2.Editor
{
    /// <summary>
    /// 用于验证基础框架的最小窗口；具体节点菜单、Inspector 和业务工具栏后续通过扩展实现。
    /// </summary>
    public sealed class BlueprintEditorWindow : EditorWindow
    {
        [SerializeField] private BlueprintAsset m_Asset;

        private ObjectField m_AssetField;
        private VisualElement m_GraphHost;
        private BlueprintEditorSession m_Session;

        [MenuItem("Window/Blueprint V2/Editor")]
        public static void Open()
        {
            GetWindow<BlueprintEditorWindow>("Blueprint V2");
        }

        public void CreateGUI()
        {
            rootVisualElement.style.flexGrow = 1f;

            Toolbar toolbar = new Toolbar();
            m_AssetField = new ObjectField("Asset")
            {
                objectType = typeof(BlueprintAsset),
                allowSceneObjects = false
            };
            m_AssetField.style.minWidth = 260f;
            m_AssetField.SetValueWithoutNotify(m_Asset);
            m_AssetField.RegisterValueChangedCallback(evt => OpenAsset(evt.newValue as BlueprintAsset));
            toolbar.Add(m_AssetField);
            toolbar.Add(new ToolbarButton(AddNode) { text = "Add Node" });
            toolbar.Add(new ToolbarButton(() => AddPort(BlueprintPortDirection.Input)) { text = "Add Input" });
            toolbar.Add(new ToolbarButton(() => AddPort(BlueprintPortDirection.Output)) { text = "Add Output" });
            toolbar.Add(new ToolbarButton(ValidateAsset) { text = "Validate" });
            rootVisualElement.Add(toolbar);

            m_GraphHost = new VisualElement { style = { flexGrow = 1f } };
            rootVisualElement.Add(m_GraphHost);
            OpenAsset(m_Asset);
        }

        private void OnDisable()
        {
            DisposeSession();
        }

        private void OpenAsset(BlueprintAsset asset)
        {
            DisposeSession();
            m_Asset = asset;
            m_AssetField?.SetValueWithoutNotify(asset);
            m_GraphHost?.Clear();
            if (asset == null || m_GraphHost == null)
            {
                return;
            }

            m_Session = new BlueprintEditorSession(asset);
            m_GraphHost.Add(m_Session.GraphView);
        }

        private void AddNode()
        {
            if (m_Session == null)
            {
                return;
            }

            Vector2 position = m_Session.GraphView.contentViewContainer.WorldToLocal(
                m_Session.GraphView.worldBound.center);
            if (!float.IsFinite(position.x) || !float.IsFinite(position.y))
            {
                position = new Vector2(100f, 100f);
            }
            ReportFailure(m_Session.EditService.AddNode(new BlueprintNodeData("Node", position)));
        }

        private void AddPort(BlueprintPortDirection direction)
        {
            BlueprintNodeView node = GetSelectedNode();
            if (m_Session == null || node == null)
            {
                return;
            }

            string displayName = direction == BlueprintPortDirection.Input ? "Input" : "Output";
            BlueprintPortCapacity capacity = direction == BlueprintPortDirection.Input
                ? BlueprintPortCapacity.Single
                : BlueprintPortCapacity.Multiple;
            ReportFailure(m_Session.EditService.AddPort(
                node.NodeId,
                new BlueprintPortData(displayName, direction, capacity)));
        }

        private BlueprintNodeView GetSelectedNode()
        {
            if (m_Session == null)
            {
                return null;
            }
            foreach (ISelectable selectable in m_Session.GraphView.selection)
            {
                if (selectable is BlueprintNodeView node)
                {
                    return node;
                }
            }
            return null;
        }

        private void ValidateAsset()
        {
            if (m_Asset == null)
            {
                return;
            }
            BlueprintValidationReport report = BlueprintValidator.Validate(m_Asset);
            if (report.Issues.Count == 0)
            {
                Debug.Log("Blueprint V2 validation passed.", m_Asset);
                return;
            }
            for (int i = 0; i < report.Issues.Count; i++)
            {
                BlueprintValidationIssue issue = report.Issues[i];
                if (issue.Severity == BlueprintValidationSeverity.Error)
                {
                    Debug.LogError(issue.Message, m_Asset);
                }
                else
                {
                    Debug.LogWarning(issue.Message, m_Asset);
                }
            }
        }

        private void DisposeSession()
        {
            m_Session?.Dispose();
            m_Session = null;
        }

        private void ReportFailure(BlueprintEditResult result)
        {
            if (!result.Succeeded)
            {
                Debug.LogWarning(result.Error, m_Asset);
            }
        }
    }
}
