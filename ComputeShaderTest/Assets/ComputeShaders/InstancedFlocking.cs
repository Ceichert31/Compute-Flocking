using UnityEngine;
using System.Collections.Generic;

public class InstancedFlocking : MonoBehaviour
{
    public struct Boid
    {
        public Vector3 position;
        public Vector3 velocity;
        public float noiseOffset;

        public Boid(Vector3 pos, Vector3 vel, float noise)
        {
            position.x = pos.x;
            position.y = pos.y;
            position.z = pos.z;
            velocity.x = vel.x;
            velocity.y = vel.y;
            velocity.z = vel.z;
            noiseOffset = noise;
        }
    }

    public ComputeShader shader;

    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float boidSpeedVariation = 1f;
    public Mesh boidMesh;
    public Material boidMaterial;
    public int boidsCount;
    public float spawnRadius;
    public Transform target;

    int kernelHandle;

    ComputeBuffer boidsBuffer;

    [SerializeField] Boid[] boidsArray;

    int groupSizeX;

    int numberOfBoids;

    RenderParams renderParams;

    GraphicsBuffer argsBuffer;

    private void Start()
    {
        kernelHandle = shader.FindKernel("CSMain");

        shader.GetKernelThreadGroupSizes(kernelHandle, out uint x, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)boidsCount / (float)x);
        numberOfBoids = groupSizeX * (int)x;

        InitBoids();
        InitShader();

        renderParams = new RenderParams(boidMaterial);
        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
    }
    /// <summary>
    /// Creates an array of boids
    /// </summary>
    void InitBoids()
    {
        boidsArray = new Boid[numberOfBoids];

        //Populate array with boids
        for (int i = 0; i < numberOfBoids; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);

            float offset = Random.value * 1000.0f;
            boidsArray[i] = new Boid(pos, rot.eulerAngles, offset);
        }
    }
    /// <summary>
    /// Creates a compute buffer 
    /// </summary>
    void InitShader()
    {
        //Init boid buffer
        boidsBuffer = new ComputeBuffer(numberOfBoids, 7 * sizeof(float));

        //Cache data on GPU
        boidsBuffer.SetData(boidsArray);

        //Init graphics buffer
        argsBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.IndirectArguments, 
            1, 
            GraphicsBuffer.IndirectDrawIndexedArgs.size);

        //Create data buffer object
        GraphicsBuffer.IndirectDrawIndexedArgs[] data = new GraphicsBuffer.IndirectDrawIndexedArgs[1];

        //Set indexCountPerInstance to vertex count of boid mesh
        data[0].indexCountPerInstance = boidMesh.GetIndexCount(0);

        //Set instance count to the number of boids we want
        data[0].instanceCount = (uint)numberOfBoids;

        //Set buffers data so data is on GPU
        argsBuffer.SetData(data);

        //Set properties
        shader.SetBuffer(kernelHandle, "boidsBuffer", boidsBuffer);
        boidMaterial.SetBuffer("boidsBuffer", boidsBuffer);

        shader.SetInt("boidsCount", numberOfBoids);
        shader.SetFloat("rotationSpeed", rotationSpeed);
        shader.SetFloat("boidSpeed", boidSpeed);
        shader.SetFloat("neighborDistance", neighbourDistance);
        shader.SetFloat("boidSpeedVariation", boidSpeedVariation);
        shader.SetVector("flockPosition", target.transform.position);
    }

    private void Update()
    {
        //Update compute shaders uniform time values
        shader.SetFloat("time", Time.time);
        shader.SetFloat("deltaTime", Time.deltaTime);

        //Dispatch compute shader to GPU
        shader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        //Render updated boids
        Graphics.RenderMeshIndirect(renderParams, boidMesh, argsBuffer);
    }

    private void OnDestroy()
    {
        //Clean buffers on destroy
        if (boidsBuffer != null)
        {
            boidsBuffer.Dispose();
        }
        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}