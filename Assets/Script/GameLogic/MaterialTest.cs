using UnityEngine;

public class MaterialTest : MonoBehaviour
{
    public Renderer Renderer;

    private int m_Alpha = Shader.PropertyToID("_Alpha");

    private MaterialPropertyBlock m_PropertyBlock;

    private void Awake()
    {
        m_PropertyBlock = new MaterialPropertyBlock();
    }

    public void InstanceTo1()
    {
        Renderer.material.SetFloat(m_Alpha, 1);
    }

    public void InstanceTo0()
    {
        Renderer.material.SetFloat(m_Alpha, 0);
    }

    public void BlockTo1()
    {
        m_PropertyBlock.SetFloat(m_Alpha, 1);
        Renderer.SetPropertyBlock(m_PropertyBlock);
    }

    public void BlockTo0()
    {
        m_PropertyBlock.SetFloat(m_Alpha, 0);
        Renderer.SetPropertyBlock(m_PropertyBlock);
    }

    public void Log()
    {
        Debug.Log($"instance : {Renderer.material.GetFloat(m_Alpha)}");
        Renderer.GetPropertyBlock(m_PropertyBlock);
        Debug.Log($"block : {m_PropertyBlock.GetFloat(m_Alpha)}");
    }
}