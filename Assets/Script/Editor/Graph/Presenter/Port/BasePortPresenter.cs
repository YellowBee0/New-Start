using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    //TODO:基类改成abstract，然后用一个CommonPortPresenter实现现在的内容，不在保存BasePortData、PortView和PortContentView字段，而是使用三个抽象函数，具体实现子类去做
    public class BasePortPresenter
    {
        private static readonly Dictionary<Type, Stack<BasePortPresenter>> s_PortPresenters = new();

        public static BasePortPresenter AllocatePortPresenter(Type portDataType)
        {
            Type portPresenterType = RuntimeToEditorMap.GetInstance().GetDrawerType(portDataType);
            if (portPresenterType == null)
            {
                return null;
            }
            if (!s_PortPresenters.TryGetValue(portPresenterType, out Stack<BasePortPresenter> portPresenters))
            {
                portPresenters = new Stack<BasePortPresenter>();
                s_PortPresenters.Add(portPresenterType, portPresenters);
            }
            return portPresenters.Count > 0 ? portPresenters.Pop() : Activator.CreateInstance(portPresenterType) as BasePortPresenter;
        }

        public static void ReleasePortPresenter(BasePortPresenter portPresenter)
        {
            Type portPresenterType = portPresenter.GetType();
            if (s_PortPresenters.TryGetValue(portPresenterType, out Stack<BasePortPresenter> portPresenters))
            {
                portPresenters.Push(portPresenter);
            }
        }

        protected BasePortData m_PortData;

        protected PortView m_PortView;

        protected VisualElement m_PortContentView;

        public virtual void Initialize(BasePortData portData, SerializedProperty portSerializedProperty)
        {
            m_PortData = portData;
            m_PortView = new PortView(portData, portData.GetPortName(), portData.GetDirection(), portData.GetCapacity(), portData.GetPortColor());
            m_PortContentView = m_PortView;
        }

        public BasePortData GetPortData()
        {
            return m_PortData;
        }

        public PortView GetPortView()
        {
            return m_PortView;
        }

        public VisualElement GetPortContentView()
        {
            return m_PortContentView;
        }

        public virtual void OnRelease()
        {
            ReleasePortPresenter(this);
        }
    }
}