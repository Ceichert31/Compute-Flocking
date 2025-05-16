using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class InstancedFlocking : MonoBehaviour
{
    public struct Boid
    {
        public Vector3 position;
        public Vector3 velocity;
        public float noiseOffset;

        //Animation
        public float frame;
        public Vector3 padding;

        public Boid(Vector3 pos, Vector3 vel, float noise)
        {
            position = pos;
            velocity = vel;
            noiseOffset = noise;
            frame = 0;
            padding.x = 0; padding.y = padding.z = 0;
        }
    }

    [Header("Boid References")]
    public ComputeShader shader;
    public Mesh boidMesh;
    public Material boidMaterial;
    public Transform target;
    [SerializeField] Terrain terrain;

    [Header("Boid Values")]
    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float neighbourDistance = 1f;
    public float terrainDistance = 3f;
    public float boidSpeedVariation = 1f;
    public int boidsCount;
    public float spawnRadius;
    public float maximumRadius;

    //Animator variables
    [Header("Animator References")]
    public GameObject boidObject;
    [Tooltip("Animation we want to use")]
    public AnimationClip animationClip;
    private Animator animator;
    private SkinnedMeshRenderer boidSMR;

    [Header("Animator Values")]
    public float boidFrameSpeed = 10f;
    public bool frameInterpolation = true;
    int numberOfFrames;

    int kernelHandle;

    ComputeBuffer boidsBuffer;
    ComputeBuffer vertexAnimationBuffer;

    Boid[] boidsArray;

    int groupSizeX;

    int numberOfBoids;

    RenderParams renderParams;

    GraphicsBuffer argsBuffer;

    const int STRIDE = 11;

    private void Awake()
    {
        kernelHandle = shader.FindKernel("CSMain");

        shader.GetKernelThreadGroupSizes(kernelHandle, out uint x, out _, out _);
        groupSizeX = Mathf.CeilToInt(boidsCount / (float)x);
        numberOfBoids = groupSizeX * (int)x;

        InitBoids();
        GenerateSkinnedAnimationForGPUBuffer();
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
        boidsBuffer = new ComputeBuffer(numberOfBoids, STRIDE * sizeof(float));

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

        //Set buffer properties
        shader.SetBuffer(kernelHandle, "_BoidsBuffer", boidsBuffer);
        boidMaterial.SetBuffer("boidsBuffer", boidsBuffer);

        //Set terrain properties
        shader.SetTexture(kernelHandle, "_HeightMap", terrain.terrainData.heightmapTexture);
        shader.SetFloat("_HeightmapResolution", terrain.terrainData.heightmapResolution);
        shader.SetVector("_TerrainSize", terrain.terrainData.size);
        shader.SetVector("_TerrainPosition", terrain.transform.position);

        //Set boid properties
        shader.SetInt("_BoidsCount", numberOfBoids);
        shader.SetFloat("_RotationSpeed", rotationSpeed);
        shader.SetFloat("_BoidSpeed", boidSpeed);
        shader.SetFloat("_NeighborDistance", neighbourDistance);
        shader.SetFloat("_TerrainDistance", terrainDistance);
        shader.SetFloat("_BoidSpeedVariation", boidSpeedVariation);
        shader.SetVector("_FlockPosition", target.transform.position);

        //Set boundry properties
        shader.SetFloat("_MaximumRadius", maximumRadius);
        shader.SetVector("_SphereCenter", transform.position);

        //Set animation properties
        shader.SetInt("_NumberOfFrames", numberOfFrames);
        boidMaterial.SetInt("numberOfFrames", numberOfFrames);
        shader.SetFloat("_BoidFrameSpeed", boidFrameSpeed);

        //Enabling smooth interpolation between frames in litfowardshader
        if (frameInterpolation && !boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION"))
            boidMaterial.EnableKeyword("FRAME_INTERPOLATION");
        if (!frameInterpolation && boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION"))
            boidMaterial.DisableKeyword("FRAME_INTERPOLATION");
    }

    private void Update()
    {
        //Update compute shaders uniform time values
        shader.SetFloat("_Time", Time.time);
        shader.SetFloat("_DeltaTime", Time.deltaTime);

        //Dispatch compute shader to GPU
        shader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        //Render updated boids
        Graphics.RenderMeshIndirect(renderParams, boidMesh, argsBuffer);
    }
    private void GenerateSkinnedAnimationForGPUBuffer()
    {
        //Get skinned mesh renderer from prefab
        boidSMR = boidObject.GetComponentInChildren<SkinnedMeshRenderer>();

        //Get starting mesh
        boidMesh = boidSMR.sharedMesh;

        //Get animator from prefab
        animator = boidObject.GetComponent<Animator>();

        //Set animation we want to use to first layer in animator
        int iLayer = 0;

        AnimatorStateInfo aniStateInfo = animator.GetCurrentAnimatorStateInfo(iLayer);

        Mesh bakedMesh = new Mesh();
        float sampleTime = 0;

        //Calculate the number of frames in the animation
        //We use closest power of two because we will be using this value often
        numberOfFrames = Mathf.ClosestPowerOfTwo((int)(animationClip.frameRate * animationClip.length));

        //Calculate time per frame
        float perFrameTime = animationClip.length / numberOfFrames;
        int vertexCount = boidSMR.sharedMesh.vertexCount;

        //Generate new compute buffer
        vertexAnimationBuffer = new ComputeBuffer(vertexCount * numberOfFrames, 16);

        //Create a new array of vector fours to store vertex data
        Vector4[] vertexAnimationData = new Vector4[vertexCount * numberOfFrames];
        
        //Cache vertex positions of each frame
        for (int i = 0; i < numberOfFrames; ++i)
        {
            //Play animation
            animator.Play(aniStateInfo.shortNameHash, iLayer, sampleTime);

            //Load next frame
            animator.Update(0.0f);

            //Bake mesh with new frame
            boidSMR.BakeMesh(bakedMesh);

            //Update verticies
            for (int j = 0; j < vertexCount; ++j)
            {
                Vector4 vertex = bakedMesh.vertices[j];
                vertex.w = 1;
                vertexAnimationData[(j * numberOfFrames) + i] = vertex;
            }
            //Increase sample time to update to next frame
            sampleTime += perFrameTime;
        }

        //Send data to GPU
        vertexAnimationBuffer.SetData(vertexAnimationData);
        boidMaterial.SetBuffer("vertexAnimation", vertexAnimationBuffer);

        //Disable object
        boidObject.SetActive(false);
    }

    private void OnDestroy()
    {
        //Clean buffers on destroy
        boidsBuffer?.Dispose();
        argsBuffer?.Dispose();
        vertexAnimationBuffer?.Dispose();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maximumRadius);
    }
}