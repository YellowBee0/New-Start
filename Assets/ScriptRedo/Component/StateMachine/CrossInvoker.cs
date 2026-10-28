using System.Collections.Generic;
using UnityEngine;

namespace ScriptRedo
{
    public sealed class CrossInvoker
    {
        private sealed class CrossNode
        {
            public ICross Cross;

            public IReadOnlyList<string> CrossKeys;

            public CrossNode Next;

            public float Percent;
        }

        private CrossNode m_CrossNode;

        private ICross m_CrossIn;

        private float m_Percent;

        private int m_CrossOutCount;

        private float m_TotalWeight;

        private float m_TotalTime;

        private float m_Timer;

        public void Update(float deltaTime)
        {
            if ((m_Timer += deltaTime) <= m_TotalTime)
            {
                CrossNode crossNode = m_CrossNode;
                float deltaWeight = deltaTime / m_TotalTime;
                while (crossNode != null)
                {
                    //-crossNode.Cross.GetWeight()替换为每次修改Cross的列表时获取当时的Weight，然后算出总时间内每帧改的比例
                    crossNode.Cross.OnUpdateCrossOut(crossNode.CrossKeys, crossNode.Percent * deltaWeight);
                    crossNode = crossNode.Next;
                }
                if (m_CrossIn != null)
                {
                    m_CrossIn.OnUpdateCrossIn(deltaWeight * m_Percent);
                }
            }
            else
            {
                CrossNode crossNode = m_CrossNode;
                while (crossNode != null)
                {
                    crossNode.Cross.OnCrossOutEnd(crossNode.CrossKeys);
                    crossNode = crossNode.Next;
                }
            }
        }

        public void SetTime(float totalTime)
        {
            if (totalTime <= 0)
            {
                return;
            }
            m_TotalTime = totalTime;
            m_Timer = 0;
        }

        public void SetCrossIn(ICross cross)
        {
            //还要做是否为CrossOut列表判断
            CrossNode crossNode = m_CrossNode;
            CrossNode preCross = null;
            while (crossNode != null)
            {
                if (crossNode.Cross == cross)
                {
                    //这里要释放移除的CrossNode
                    if (preCross != null)
                    {
                        preCross.Next = crossNode.Next;
                    }
                    else
                    {
                        m_CrossNode = null;
                    }
                }
                crossNode = crossNode.Next;
                preCross = crossNode;
            }
            if (m_CrossIn != cross)
            {
                float percent = 0;
                if (cross != null)
                {
                    float newTotalWeight = cross.GetWeight() + m_TotalWeight;
                    if (newTotalWeight > 1)
                    {
                        Debug.LogError($"can not set cross in target,because it's weight:{cross.GetWeight()} add total weight:{m_TotalWeight} is over than 1");
                        return;
                    }
                    m_TotalWeight = newTotalWeight;
                    cross.OnCrossInBegin();
                    percent = 1 - cross.GetWeight();
                }
                if (m_CrossIn != null)
                {
                    SetCrossOut(m_CrossIn);
                }
                m_CrossIn = cross;
                m_Percent = percent;
            }
        }

        private void SetCrossOut(ICross cross)
        {
            if (cross != null)
            {
                if (m_CrossIn == cross)
                {
                    m_CrossIn = null;
                }
                CrossNode lastCrossNode = m_CrossNode;
                while (lastCrossNode != null)
                {
                    if (lastCrossNode.Cross == cross)
                    {
                        return;
                    }
                    if (lastCrossNode.Next == null)
                    {
                        lastCrossNode.Next = new CrossNode
                        {
                            Cross = cross,
                            //这里获取过渡的keys需要取交集
                            CrossKeys = cross.GetCrossKeys(),
                            Next = null,
                            Percent = -cross.GetWeight()
                        };
                        break;
                    }
                    lastCrossNode = lastCrossNode.Next;
                }
            }
        }
    }
}