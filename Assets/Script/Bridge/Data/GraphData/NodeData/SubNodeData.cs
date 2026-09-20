using System;
using System.Collections.Generic;
using UnityEngine;
using YBFramework.GameLogic.Graph;
#if UNITY_EDITOR
using YBFramework.Bridge.Editor;
#endif

namespace YBFramework.Bridge.Data
{
    [Serializable]
#if UNITY_EDITOR
    [NodeMenu("子图节点", GraphType.Everything)]
#endif
    public sealed class SubNodeData : BaseNodeData
    {
        [SerializeField] private GraphAsset m_SubGraphAsset;

        [SerializeField] private List<SubPortData> m_SubPortsData;

        public GraphAsset GetSubGraphAsset()
        {
            return m_SubGraphAsset;
        }

        public SubPortData FindSubPortDataBySubPortAddress(int subNodeID, int subPortID)
        {
            int index = FindSubPortDataIndexBySubPortAddress(subNodeID, subPortID);
            return index != -1 ? m_SubPortsData[index] : null;
        }

        private int FindSubPortDataIndexBySubPortAddress(int subNodeID, int subPortID)
        {
            for (int i = 0; i < m_SubPortsData.Count; i++)
            {
                SubPortData subPortData = m_SubPortsData[i];
                if (subPortData.GetAsSubNodeID() == subNodeID && subPortData.GetAsSubPortID() == subPortID)
                {
                    return i;
                }
            }
            return -1;
        }

        public override int GetPortsDataCount()
        {
            return m_SubPortsData.Count;
        }

        public override BasePortData PortDataOfIndex(int index)
        {
            return m_SubPortsData[index];
        }

        public override BaseNode CreateRuntimeInstance(NodeCheckResult nodeCheckResult)
        {
            /*ProxyNode proxyNode = new ProxyNode();
            proxyNode.InitializeFromProxyNodeData(this, (SubNodeSliceData)nodeSliceData);
            return proxyNode;*/
            throw new NotImplementedException();
        }

        public override void CheckExecutionEntry(GraphCheckContext graphCheckContext)
        {
            //检查可执行列表中是否有当前节点，没有就添加一个可执行子节点SubNodeCheckResult
            //TODO:这样做存在一个问题：子图如果没有可执行的内容，这个数据就白创建了。需要在创建完毕后检查是否有东西，没有就删除
            if (!graphCheckContext.NodeCheckResults.TryGetValue(this, out NodeCheckResult nodeCheckResult))
            {
                nodeCheckResult = new SubNodeCheckResult(m_SubGraphAsset);
                graphCheckContext.NodeCheckResults.Add(this, nodeCheckResult);
            }
            //设置检查上下文执行的当前节点为this
            graphCheckContext.NodeData = this;
            graphCheckContext.NodeCheckResult = nodeCheckResult;
            //创建子图的检查上下文，并初始化，设置父上下文为参数checkGraphExecutionContext
            GraphCheckContext subGraphCheckContext = new GraphCheckContext
            {
                Parent = graphCheckContext,
                GraphAsset = m_SubGraphAsset,
                NodeCheckResults = ((SubNodeCheckResult)nodeCheckResult).SubNodeCheckResults
            };
            subGraphCheckContext.StartCheck();
        }

        public override void CheckExecutionFlow(GraphCheckContext graphCheckContext, int portID)
        {
            //先找到可执行节点数据
            NodeCheckResult nodeCheckResult;
            if (graphCheckContext.NodeData == this)
            {
                nodeCheckResult = graphCheckContext.NodeCheckResult;
            }
            else
            {
                if (!graphCheckContext.NodeCheckResults.TryGetValue(this, out nodeCheckResult))
                {
                    nodeCheckResult = new SubNodeCheckResult(m_GraphAsset);
                    graphCheckContext.NodeCheckResults.Add(this, nodeCheckResult);
                }
            }
            SubPortData subPortData = null;
            for (int i = 0; i < m_SubPortsData.Count; i++)
            {
                SubPortData portData = m_SubPortsData[i];
                if (portData.GetPortID() == portID)
                {
                    subPortData = portData;
                    break;
                }
            }
            if (subPortData == null)
            {
                Debug.LogError("不可能为null，除非数据丢失");
                return;
            }
            //如果可执行节点数据添加到HashSet，就对端口执行检查
            if (nodeCheckResult.PortCheckResults.Add(subPortData))
            {
                //1、执行端口在当前蓝图的检查
                subPortData.CheckExecutionFlow(graphCheckContext);
                //2、执行子蓝图中实际的端口在子蓝图的检查
                //因为同时执行了1、2两个步骤，就会导致子图检查流程执行回父图的端口时，父图的端口会执行一次在父图的检查，然后又执行到子图的端口，第二步就是多余的。
                //所以每一个端口需要先判断是否添加到可执行列表，再执行检查后面的内容，不然会在这里形成无限递归。
                BaseNodeData asSubNodeData = m_SubGraphAsset.FindNodeData(subPortData.GetAsSubNodeID());
                if (asSubNodeData != null)
                {
                    graphCheckContext.NodeData = this;
                    graphCheckContext.NodeCheckResult = nodeCheckResult;
                    //创建子图的检查上下文，并初始化，设置父上下文为参数checkGraphExecutionContext
                    GraphCheckContext subGraphCheckContext = new GraphCheckContext
                    {
                        Parent = graphCheckContext,
                        GraphAsset = m_SubGraphAsset,
                        NodeCheckResults = ((SubNodeCheckResult)nodeCheckResult).SubNodeCheckResults
                    };
                    asSubNodeData.CheckExecutionFlow(subGraphCheckContext, subPortData.GetAsSubPortID());
                }
            }
        }
#if UNITY_EDITOR
        [SerializeField] private int m_SourcePortID;

        private static ExposeNodeData[] s_ExposeNodesData = new ExposeNodeData[2];

        private static ExposeNodeData[] InitializeExposePortsData(GraphAsset subGraphAsset)
        {
            for (int i = 0; i < s_ExposeNodesData.Length; i++)
            {
                s_ExposeNodesData[i] = null;
            }
            if (subGraphAsset != null)
            {
                int count = 0;
                IReadOnlyList<BaseNodeData> nodesData = subGraphAsset.GetNodesData();
                for (int i = 0; i < nodesData.Count; i++)
                {
                    if (nodesData[i] is ExposeNodeData exposeNodeData)
                    {
                        //确保初始化非序列化数据
                        exposeNodeData.InitializePortData();
                        s_ExposeNodesData[count++] = exposeNodeData;
                        if (count > 1)
                        {
                            break;
                        }
                    }
                }
            }
            return s_ExposeNodesData;
        }

        private static ExposePortData FindExposePortData(ExposeNodeData[] exposeNodesData, int asSubNodeID, int asSubPortID)
        {
            for (int i = 0; i < exposeNodesData.Length; i++)
            {
                ExposeNodeData exposeNodeData = exposeNodesData[i];
                if (exposeNodeData != null)
                {
                    IReadOnlyList<ExposePortData> exposePortsData = exposeNodeData.GetExposePortsData();
                    for (int j = 0; j < exposePortsData.Count; j++)
                    {
                        ExposePortData exposePortData = exposePortsData[j];
                        PortConnectionData exposePortAddress = exposePortData.GetToExposePortAddress();
                        if (exposePortAddress.NodeID == asSubNodeID && exposePortAddress.PortID == asSubPortID)
                        {
                            return exposePortData;
                        }
                    }
                }
            }
            return null;
        }

        private static bool CheckCircleReference(GraphAsset targetGraphAsset, GraphAsset CheckGraphAsset)
        {
            if (targetGraphAsset == null || CheckGraphAsset == null)
            {
                return false;
            }
            if (targetGraphAsset == CheckGraphAsset)
            {
                return true;
            }
            IReadOnlyList<BaseNodeData> nodesData = CheckGraphAsset.GetNodesData();
            for (int i = 0; i < nodesData.Count; i++)
            {
                BaseNodeData nodeData = nodesData[i];
                if (nodeData is SubNodeData subNodeData)
                {
                    if (CheckCircleReference(targetGraphAsset, subNodeData.m_SubGraphAsset))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TrySetSubGraphAsset(GraphAsset subGraphAsset)
        {
            if (m_SubGraphAsset != subGraphAsset)
            {
                if (!CheckCircleReference(m_GraphAsset, subGraphAsset))
                {
                    for (int i = 0; i < m_SubPortsData.Count; i++)
                    {
                        m_SubPortsData[i].DisconnectAll();
                    }
                    m_SubPortsData.Clear();
                    //确保蓝图引用初始化
                    if (subGraphAsset != null)
                    {
                        ExposeNodeData[] exposeNodesData = InitializeExposePortsData(subGraphAsset);
                        for (int i = 0; i < exposeNodesData.Length; i++)
                        {
                            ExposeNodeData exposeNodeData = exposeNodesData[i];
                            if (exposeNodeData != null)
                            {
                                IReadOnlyList<ExposePortData> exposePortsData = exposeNodeData.GetExposePortsData();
                                for (int j = 0; j < exposePortsData.Count; j++)
                                {
                                    ExposePortData exposePortData = exposePortsData[j];
                                    BasePortData asSubPortData = exposePortData.GetToExposePortData();
                                    if (asSubPortData != null)
                                    {
                                        SubPortData subPortData = new(asSubPortData.CreateSubPortData(), exposePortData.GetToExposePortAddress().NodeID,
                                            exposePortData.GetToExposePortAddress().PortID);
                                        subPortData.SetPortID(++m_SourcePortID);
                                        subPortData.SetNodeData(this);
                                        m_SubPortsData.Add(subPortData);
                                    }
                                }
                            }
                        }
                    }
                    m_SubGraphAsset = subGraphAsset;
                    return true;
                }
                Debug.LogError($"{subGraphAsset.name} makes a circle reference to {m_GraphAsset.name}");
            }
            return false;
        }

        public void OnExposePortDataConnectionChanged(ExposePortData exposePortData, int asSubNodeID, int asSubPortID, bool isConnect)
        {
            if (isConnect)
            {
                SubPortData subPortData = new(exposePortData.GetToExposePortData().CreateSubPortData(), asSubNodeID, asSubPortID);
                subPortData.SetPortID(++m_SourcePortID);
                subPortData.SetNodeData(this);
                m_SubPortsData.Add(subPortData);
            }
            else
            {
                int index = FindSubPortDataIndexBySubPortAddress(asSubNodeID, asSubPortID);
                if (index != -1)
                {
                    m_SubPortsData[index].DisconnectAll();
                    m_SubPortsData.RemoveAt(index);
                }
            }
        }

        public void InitializeSubPortsData()
        {
            ExposeNodeData[] exposePortsData = InitializeExposePortsData(m_SubGraphAsset);
            for (int i = 0; i < m_SubPortsData.Count; i++)
            {
                SubPortData subPortData = m_SubPortsData[i];
                ExposePortData exposePortData = FindExposePortData(exposePortsData, subPortData.GetAsSubNodeID(), subPortData.GetAsSubPortID());
                InitializeSubPortsData(subPortData, exposePortData, i);
            }
        }

        private void InitializeSubPortsData(SubPortData subPortData, ExposePortData exposePortData, int index)
        {
            if (subPortData.GetAsSubPortData() is IFieldPath fieldPath)
            {
                fieldPath.SetFieldPath($"{nameof(m_SubPortsData)}.Array.data[{index}].{SubPortData.AS_SUB_PORT_FIELD_NAME}");
            }
            if (exposePortData != null)
            {
                BasePortData toExposePortData = exposePortData.GetToExposePortData();
                subPortData.SetPortName(string.IsNullOrEmpty(exposePortData.ExposePortDisplayName) ? toExposePortData.GetPortName() : exposePortData.ExposePortDisplayName);
                subPortData.RevertNonSerializedData(toExposePortData);
            }
            else
            {
                Debug.LogError(
                    $"{nameof(SubNodeData)} node id:{m_NodeID} port id:{subPortData.GetPortID()} has saved a missing port data,address:node id:{subPortData.GetAsSubNodeID()} port id:{subPortData.GetAsSubPortID()}");
            }
        }

        public override void InitializeSerializedData()
        {
            m_SubPortsData = new List<SubPortData>();
        }

        protected override void OnInitializePortData()
        {
            InitializeSubPortsData();
        }
#endif
    }
}