using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    public abstract class BasePortDataPresenter
    {
        private static readonly Dictionary<Type, Stack<BasePortDataPresenter>> s_PortPresenters = new();

        public static BasePortDataPresenter AllocatePortPresenter(Type portDataType)
        {
            Type portDataPresenterType = RuntimeToEditorMap.GetInstance().GetDrawerType(portDataType);
            if (portDataPresenterType == null)
            {
                return null;
            }
            if (!s_PortPresenters.TryGetValue(portDataPresenterType, out Stack<BasePortDataPresenter> portDataPresenters))
            {
                portDataPresenters = new Stack<BasePortDataPresenter>();
                s_PortPresenters.Add(portDataPresenterType, portDataPresenters);
            }
            return portDataPresenters.Count > 0 ? portDataPresenters.Pop() : Activator.CreateInstance(portDataPresenterType) as BasePortDataPresenter;
        }

        public static void ReleasePortPresenter(BasePortDataPresenter portDataPresenter)
        {
            if (s_PortPresenters.TryGetValue(portDataPresenter.GetType(), out Stack<BasePortDataPresenter> portDataPresenters))
            {
                portDataPresenter.OnRelease();
                portDataPresenters.Push(portDataPresenter);
            }
        }

        protected BaseNodeDataPresenter m_NodeDataPresenter;

        public virtual void Initialize(BaseNodeDataPresenter nodeDataPresenter, BasePortData portData, SerializedProperty portSerializedProperty)
        {
            m_NodeDataPresenter = nodeDataPresenter;
        }

        public abstract BasePortData GetPortData();

        public abstract PortView GetPortView();

        public abstract VisualElement GetPortContentView();

        protected virtual void OnRelease()
        {
        }
    }
}