using System;
using System.Collections.Generic;
using UnityEngine;

namespace YBFramework.BlueprintV2
{
    /// <summary>
    /// 可扩展的节点模型。只保存可序列化业务数据，不持有任何 Editor View 或 Presenter。
    /// </summary>
    [Serializable]
    public class BlueprintNodeData
    {
        [SerializeField] private BlueprintNodeId m_Id;
        [SerializeField] private string m_Title = "Node";
        [SerializeField] private Vector2 m_Position;
        // 修改节点显示相关数据时递增；该值本身也会被 Unity Undo 恢复。
        [SerializeField, HideInInspector] private int m_ViewRevision;
        // 节点可以混合保存不同的端口派生类型。
        [SerializeReference] private List<BlueprintPortData> m_Ports = new();

        // 以下字段都是可重建缓存，不能参与资产持久化或对象身份判断。
        [NonSerialized] private BlueprintAsset m_Asset;
        [NonSerialized] private Dictionary<BlueprintPortId, BlueprintPortData> m_PortIndex;

        public BlueprintNodeData()
        {
        }

        public BlueprintNodeData(string title, Vector2 position)
        {
            m_Id = BlueprintNodeId.Create();
            m_Title = title;
            m_Position = position;
        }

        public BlueprintNodeId Id => m_Id;

        public string Title => m_Title;

        public Vector2 Position => m_Position;

        public BlueprintAsset Asset => m_Asset;

        public int ViewRevision => m_ViewRevision;

        public IReadOnlyList<BlueprintPortData> Ports
        {
            get
            {
                m_Ports ??= new List<BlueprintPortData>();
                return m_Ports;
            }
        }

        public bool TryGetPort(BlueprintPortId portId, out BlueprintPortData port)
        {
            EnsurePortIndexInternal();
            return m_PortIndex.TryGetValue(portId, out port);
        }

        /// <summary>
        /// 供派生节点在构造阶段声明固定端口。运行期结构修改仍应经过 BlueprintEditService。
        /// </summary>
        protected void AddDeclaredPort(BlueprintPortData port)
        {
            if (port == null)
            {
                throw new ArgumentNullException(nameof(port));
            }
            m_Ports ??= new List<BlueprintPortData>();
            m_Ports.Add(port);
        }

        internal void EnsureIdentityInternal()
        {
            if (!m_Id.IsValid)
            {
                m_Id = BlueprintNodeId.Create();
            }
        }

        internal void AttachInternal(BlueprintAsset asset)
        {
            m_Asset = asset;
            EnsureIdentityInternal();
            RebuildPortIndexInternal();
        }

        internal void RebuildPortIndexInternal()
        {
            m_Ports ??= new List<BlueprintPortData>();
            m_PortIndex ??= new Dictionary<BlueprintPortId, BlueprintPortData>();
            m_PortIndex.Clear();
            for (int i = 0; i < m_Ports.Count; i++)
            {
                BlueprintPortData port = m_Ports[i];
                if (port == null)
                {
                    continue;
                }
                port.AttachInternal(this);
                // 重复 ID 保留第一项供查询，具体错误交给 Validator 报告，避免加载资产时直接抛异常。
                if (!m_PortIndex.ContainsKey(port.Id))
                {
                    m_PortIndex.Add(port.Id, port);
                }
            }
        }

        internal bool AddPortInternal(BlueprintPortData port)
        {
            if (port == null)
            {
                return false;
            }
            m_Ports ??= new List<BlueprintPortData>();
            port.AttachInternal(this);
            EnsurePortIndexInternal();
            if (m_PortIndex.ContainsKey(port.Id))
            {
                return false;
            }
            m_Ports.Add(port);
            m_PortIndex.Add(port.Id, port);
            return true;
        }

        internal bool RemovePortInternal(BlueprintPortId portId)
        {
            m_Ports ??= new List<BlueprintPortData>();
            for (int i = 0; i < m_Ports.Count; i++)
            {
                BlueprintPortData port = m_Ports[i];
                if (port != null && port.Id == portId)
                {
                    m_Ports.RemoveAt(i);
                    m_PortIndex?.Remove(portId);
                    return true;
                }
            }
            return false;
        }

        internal void SetPositionInternal(Vector2 position)
        {
            m_Position = position;
            IncrementViewRevisionInternal();
        }

        internal void SetTitleInternal(string title)
        {
            m_Title = title;
            IncrementViewRevisionInternal();
        }

        internal void IncrementViewRevisionInternal()
        {
            unchecked
            {
                m_ViewRevision++;
            }
        }

        private void EnsurePortIndexInternal()
        {
            if (m_PortIndex == null)
            {
                RebuildPortIndexInternal();
            }
        }
    }
}
