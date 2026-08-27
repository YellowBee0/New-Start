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

        public abstract void Initialize(BasePortData portData, SerializedProperty portSerializedProperty);

        /// <summary>
        /// 在连接两个端口时调用。原本存在连接，打开蓝图重建连接不会调用这个函数
        /// </summary>
        /// <param name="other">连接的其他port data presenter</param>
        public virtual void OnConnect(BasePortDataPresenter other)
        {
        }

        /// <summary>
        /// 在断开两个端口连接时调用。
        /// </summary>
        /// <param name="other">断开连接的其他port data presenter</param>
        public virtual void OnDisconnect(BasePortDataPresenter other)
        {
        }
        
        public abstract BasePortData GetPortData();

        public abstract PortView GetPortView();

        public abstract VisualElement GetPortContentView();

        protected virtual void OnRelease()
        {
        }
    }
}