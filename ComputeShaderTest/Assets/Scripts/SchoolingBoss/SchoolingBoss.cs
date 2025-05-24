using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class SchoolingBoss : MonoBehaviour
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

    [Header("Boid General Settings")]
    public int boidsCount;
    public float spawnRadius;
    [SerializeField]
    private bool isMeshState;
    [SerializeField]
    private int vertexSkip = 4;

    [Header("Boid Movement Values")]
    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float boidSpeedVariation = 1f;

    [Header("Boid Avoidance Values")]
    public float neighbourDistance = 1f;
    public float avoidanceDistance = 3f;

    [Header("Boid Weights")]
    [Range(0, 1000f)]
    public float alignmentWeight;
    [Range(0, 1000f)]
    public float cohesionWeight;
    [Range(0, 1000f)]
    public float seperationWeight;
    [Range(0, 1000f)]
    public float groundAvoidanceWeight;

    [Header("Animator Values")]
    public float boidFrameSpeed = 10f;
    public bool frameInterpolation = true;
    int numberOfFrames;

    [Header("Boid References")]
    public ComputeShader boidShader;
    public Mesh boidMesh;
    public MeshFilter targetShape;
    public Material boidMaterial;
    public Transform target;
    [SerializeField] Terrain terrain;

    [Header("Animator References")]
    public GameObject boidObject;
    [Tooltip("Animation we want to use")]
    public AnimationClip animationClip;
    private Animator animator;
    private SkinnedMeshRenderer boidSMR;

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
        boidsCount = targetShape.mesh.vertexCount;

        kernelHandle = boidShader.FindKernel("BoidLogic");

        boidShader.GetKernelThreadGroupSizes(kernelHandle, out uint x, out _, out _);
        groupSizeX = Mathf.CeilToInt(boidsCount / (float)x);
        numberOfBoids = groupSizeX * (int)x;

        InitTerrain();
        InitBoids();
        GenerateSkinnedAnimationForGPUBuffer();
        InitShader();

        //Assign each boid a vertex to adhere to
        for (int i = 0; i < boidsCount; i += vertexSkip)
        {
            //Assign a vertex 
        }

        renderParams = new RenderParams(boidMaterial);
        renderParams.worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000);
    }

    void InitTerrain()
    {
        boidShader.SetTexture(kernelHandle, "_HeightMap", terrain.terrainData.heightmapTexture);
        boidShader.SetFloat("_HeightmapResolution", terrain.terrainData.heightmapResolution);
        boidShader.SetVector("_TerrainSize", terrain.terrainData.size);
        boidShader.SetVector("_TerrainPosition", terrain.transform.position);
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
            //Random boid spawn pos
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;

            //Random boid rotation
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);

            //Random offset
            float offset = Random.value * 1000.0f;

            //Add boid to array
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
        boidShader.SetBuffer(kernelHandle, "_BoidsBuffer", boidsBuffer);
        boidMaterial.SetBuffer("boidsBuffer", boidsBuffer);

        //Set boid properties
        boidShader.SetInt("_BoidsCount", numberOfBoids);
        boidShader.SetFloat("_RotationSpeed", rotationSpeed);
        boidShader.SetFloat("_BoidSpeed", boidSpeed);
        boidShader.SetFloat("_NeighborDistance", neighbourDistance);
        boidShader.SetFloat("_AvoidanceDistance", avoidanceDistance);
        boidShader.SetFloat("_BoidSpeedVariation", boidSpeedVariation);
        boidShader.SetVector("_FlockPosition", target.transform.position);

        //Set weight properties
        boidShader.SetFloat("_AlignmentWeight", alignmentWeight);
        boidShader.SetFloat("_CohesionWeight", cohesionWeight);
        boidShader.SetFloat("_SeperationWeight", seperationWeight);
        boidShader.SetFloat("_AvoidanceWeight", groundAvoidanceWeight);

        //Set animation properties
        boidShader.SetInt("_NumberOfFrames", numberOfFrames);
        boidMaterial.SetInt("numberOfFrames", numberOfFrames);
        boidShader.SetFloat("_BoidFrameSpeed", boidFrameSpeed);

        //Enabling smooth interpolation between frames in litfowardshader
        if (frameInterpolation && !boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION"))
            boidMaterial.EnableKeyword("FRAME_INTERPOLATION");
        if (!frameInterpolation && boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION"))
            boidMaterial.DisableKeyword("FRAME_INTERPOLATION");
    }

    private void Update()
    {
        //Update compute shaders uniform time values
        boidShader.SetFloat("_Time", Time.time);
        boidShader.SetFloat("_DeltaTime", Time.deltaTime);

        //Set verticies 

        //Update shader


        //Dispatch compute shader to GPU
        boidShader.Dispatch(kernelHandle, groupSizeX, 1, 1);

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
}