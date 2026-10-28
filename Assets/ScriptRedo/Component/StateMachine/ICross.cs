using System.Collections.Generic;

namespace ScriptRedo
{
    public interface ICross
    {
        float GetWeight();

        IReadOnlyList<string> GetCrossKeys();

        /// <summary>
        /// 当开始过渡进当前状态时调用
        /// </summary>
        void OnCrossInBegin();

        /// <summary>
        /// 当过渡出当前状态结束时调用
        /// </summary>
        /// <param name="crossKeys">参与过渡的key</param>
        void OnCrossOutEnd(IReadOnlyList<string> crossKeys);

        /// <summary>
        /// 过渡到当前状态。
        /// 不需要过渡的key，因为目标就是当前状态，所以当前状态全部的key都会执行
        /// </summary>
        /// <param name="deltaWeight">每帧改变的权重</param>
        void OnUpdateCrossIn(float deltaWeight);

        /// <summary>
        /// 过渡出当前状态。
        /// 需要过渡的key，过渡出时并不知道那些key可以被当前状态控制
        /// </summary>
        /// <param name="crossKeys">过渡的key</param>
        /// <param name="deltaWeight">每帧改变的权重</param>
        void OnUpdateCrossOut(IReadOnlyList<string> crossKeys, float deltaWeight);
    }
}