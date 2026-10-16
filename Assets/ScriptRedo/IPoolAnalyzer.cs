using System;

namespace ScriptRedo
{
    public interface IPoolAnalyzer
    {
        /// <summary>
        /// 获取对象池管理对象类型
        /// </summary>
        /// <returns>对象类型</returns>
        Type GetObjectType();

        /// <summary>
        /// 获取对象池管理的总个数（已分配的对象+空闲的对象）。但是不代表当前程序运行时创建出来的对象总个数，因为有的对象有可能不是通过对象池创建的。
        /// </summary>
        /// <returns>管理的总个数</returns>
        int GetTotalObjectCount();

        /// <summary>
        /// 获取对象池空闲对象的个数
        /// </summary>
        /// <returns>空闲对象个数</returns>
        int GetIdleObjectCount();
    }
}