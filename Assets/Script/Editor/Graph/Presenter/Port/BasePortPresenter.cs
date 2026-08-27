using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using YBFramework.Bridge.Data;

namespace YBFramework.Editor.Graph
{
    //TODO:基类改成abstract，然后用一个CommonPortPresenter实现现在的内容，不在保存BasePortData、PortView和PortContentView字段，而是使用三个抽象函数，具体实现子类去做
    public abstract class BasePortPresenter
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
            if (s_PortPresenters.TryGetValue(portPresenter.GetType(), out Stack<BasePortPresenter> portPresenters))
            {
                portPresenter.OnRelease();
                portPresenters.Push(portPresenter);
            }
        }

        public abstract void Initialize(BasePortData portData, SerializedProperty portSerializedProperty);

        public abstract BasePortData GetPortData();

        public abstract PortView GetPortView();

        public abstract VisualElement GetPortContentView();

        protected virtual void OnRelease()
        {
        }
    }
}