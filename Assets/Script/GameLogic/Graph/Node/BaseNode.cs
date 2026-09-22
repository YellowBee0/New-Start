namespace YBFramework.GameLogic.Graph
{
    //TODO:实现ValueNode<T>，T为可序列化的类型，这个节点可以写入和读取值。
    // 不要再考虑这个节点序列化的值会在运行时被修改，因为根本不会修改到序列化的值。
    // 序列化的值分两种情况：1、值类型：运行时保存的直接是新的值类型副本，修改不影响序列化的值
    // 2、引用类型：运行时保存的是序列化值的引用，直接在运行时节点中修改就只是引用指向了另一个地址，序列化对象不受影响；
    // 如果先获取到这个值的某一个字段，然后再修改，也不会修改序列化值的字段内容。
    // 因为获取字段的时候是一个新的指针指向同一个地址，修改的时候是新的指针指向另外的地址，而不是序列化值得字段指向新的地址。
    // 只要修改不是通过Data.field = xxx，而是referenceField = xxx，就不会修改序列化值的内容


    //TODO:创建节点实例时，同样需要创建端口实例，端口实例是否创建也是通过查找可执行列表中是否存在对应节点的对应端口，存在就创建。
    // 但是通过这个方式BasePortData创建出BasePort后，并不知道应该赋值给哪一个字段。比如TestNode有A，B两个端口类型分别为ActionPort，MethodPort
    // 创建TestNode的时候就得在创建函数中查找TestNodeData的端口AData，BData是否存在，存在就给对应的端口赋值
    public abstract class BaseNode
    {
        public abstract void OnStart();

        public abstract void OnStop();

        public abstract void OnReset();
    }
}