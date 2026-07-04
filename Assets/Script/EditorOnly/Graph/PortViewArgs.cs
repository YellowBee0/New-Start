#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace YBFramework.EditorOnly
{
    public readonly struct PortViewArgs
    {
        public readonly string Name;

        public readonly Direction Direction;

        public readonly Port.Capacity Capacity;

        public readonly Color Color;

        public PortViewArgs(string name, Direction direction, Port.Capacity capacity, Color color)
        {
            Name = name;
            Direction = direction;
            Capacity = capacity;
            Color = color;
        }
    }
}
#endif