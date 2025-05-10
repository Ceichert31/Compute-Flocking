using UnityEngine;

public class TestShader : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;

    int kernelHandle;

    void Start()
    {
        kernelHandle = shader.FindKernel("Test");


    }


    void Update()
    {
        shader.Dispatch(kernelHandle, 8, 8, 1);
    }
}
