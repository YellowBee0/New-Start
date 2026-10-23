using System;
using UnityEditor;
using UnityEngine;

namespace DistributedAnimationStates.Editor
{
    [CustomEditor(typeof(AnimationStateDefinition))]
    public sealed class AnimationStateDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StateName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Layers"), true);
            serializedObject.ApplyModifiedProperties();

            var state = (AnimationStateDefinition)target;
            try
            {
                state.CreateSnapshot();
                if (state.Layers.Length == 0)
                    EditorGUILayout.HelpBox("Empty state: all managed layer/clip weights become zero.", MessageType.Info);
                foreach (var layer in state.Layers)
                {
                    float sum = 0;
                    foreach (var clip in layer.Clips) sum += clip.Weight;
                    if (layer.Weight > 0 && Mathf.Abs(sum - 1) > 0.001f)
                        EditorGUILayout.HelpBox(layer.Layer.name + ": clip weights sum to " + sum +
                            ". Inputs are applied as authored; normally a blend sums to 1.", MessageType.Warning);
                }
            }
            catch (Exception exception)
            {
                EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
            }
        }
    }
}
