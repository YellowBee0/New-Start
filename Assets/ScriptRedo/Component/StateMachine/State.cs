namespace ScriptRedo
{
    public sealed class State
    {
        public struct Layer
        {
            public int LayerIndex;
            
            public float LayerWeight;

            public Port[] Ports;
        }
        
        public struct Port
        {
            public int PortIndex;
            
            public float PortWeight;
        }
        
        private string m_Name;

        private Layer[] m_Layers;

        public string GetName()
        {
            return m_Name;
        }

        public Layer[] GetLayers()
        {
            return m_Layers;
        }
        
        //有两个Layer，一个上半身Upper，一个下半身Lower
        //Upper的攻击动画，需要响应动画事件，Lower的跑步动画，需要响应RootMotion和动画事件

        //这个类对应的数据：状态名Idle、Run、Attack或者RunAttack用于索引；需要调用播放的动画（动画数据包括：在哪一个Layer下；在Mixer的哪个端口下（动画索引）；播放权重）；

        //进入时设置每个动画所在Layer权重以及Mixer权重（这个操作发生在StateMachine中，StateMachine读取动画状态，决定进入这个状态时动画是如何的）
    }

    public abstract class BaseStateTest
    {
        public abstract void OnEnter();
        
        public abstract void OnExit();
        
        public abstract void OnUpdate();
    }

    public abstract class OverrideStateTest : BaseStateTest
    {
        protected BaseStateTest ToOverrideStateTest;

        public sealed override void OnEnter()
        {
            ToOverrideStateTest.OnEnter();
            OverrideOnEnter();
        }

        public sealed override void OnExit()
        {
            ToOverrideStateTest.OnExit();
            OverrideOnExit();
        }

        public sealed override void OnUpdate()
        {
            ToOverrideStateTest.OnUpdate();
            OverrideOnUpdate();
        }

        protected abstract void OverrideOnEnter();

        protected abstract void OverrideOnExit();
        
        protected abstract void OverrideOnUpdate();
    }

    public sealed class IdleOverrideStateTest : OverrideStateTest
    {
        protected override void OverrideOnEnter()
        {
            //在这里开启定时器
        }

        protected override void OverrideOnExit()
        {
            
        }

        protected override void OverrideOnUpdate()
        {
            //在这里监听攻击输入
            //idle、move状态下，可以切换到攻击状态
            //空中攻击状态：air状态下，可以切换到攻击
            //dash状态下，可以切换到攻击
        }
    }

    public interface IState<TContext>
    {
        void OnEnter();
        
        void OnExit();

        void OnUpdate();
    }
}