using System.Collections.Generic;

namespace YBFramework.Bridge.Data
{
    public sealed class CheckSubNodeExecutionContext : CheckNodeExecutionContext
    {
        public readonly Dictionary<BaseNodeData, CheckNodeExecutionContext> CheckNodeExecutionContexts;

        private CheckSubNodeExecutionContext()
        {
            CheckNodeExecutionContexts = new Dictionary<BaseNodeData, CheckNodeExecutionContext>();
        }

        protected override void OnRelease()
        {
            base.OnRelease();
            foreach (KeyValuePair<BaseNodeData, CheckNodeExecutionContext> kvp in CheckNodeExecutionContexts)
            {
                Release(kvp.Value);
            }
            CheckNodeExecutionContexts.Clear();
        }
    }
}