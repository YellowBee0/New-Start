namespace YBFramework.Bridge.Editor
{
    public enum GraphType
    {
        None = 0,

        Test1 = 1 << 0,

        Test2 = 1 << 1,

        Test3 = 1 << 2,

        Everything = Test1 | Test2 | Test3
    }
}