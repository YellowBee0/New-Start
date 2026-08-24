using System.Collections.Generic;

namespace YBFramework.Bridge.NewData
{
    public sealed class CheckValidStack
    {
        private static readonly Stack<CheckValidStack> s_Pool = new();

        public static CheckValidStack Allocate(GraphAsset graphAsset)
        {
            CheckValidStack checkValidStack = s_Pool.Count > 0 ? s_Pool.Pop() : new CheckValidStack();
            checkValidStack.m_CurrentGraphAsset = graphAsset;
            return checkValidStack;
        }

        public static void Free(CheckValidStack checkValidStack)
        {
            checkValidStack.m_CurrentGraphAsset = null;
            checkValidStack.m_CurrentNodeData = null;
            checkValidStack.m_ParentStack = null;
            s_Pool.Push(checkValidStack);
        }

        private GraphAsset m_CurrentGraphAsset;

        private BaseNodeData m_CurrentNodeData;

        private CheckValidStack m_ParentStack;

        public GraphAsset GetCurrentGraphAsset()
        {
            return m_CurrentGraphAsset;
        }

        public BaseNodeData GetCurrentNodeData()
        {
            return m_CurrentNodeData;
        }

        public CheckValidStack GetParentStack()
        {
            return m_ParentStack;
        }

        public void SetCurrentNodeData(BaseNodeData nodeData)
        {
            m_CurrentNodeData = nodeData;
        }

        public void SetParentStack(CheckValidStack parentStack)
        {
            m_ParentStack = parentStack;
        }
    }
}