using System;
using UnityEngine;
using Random = UnityEngine.Random;

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
            padding.x = 0;
            padding.y = padding.z = 0;
        }
    }

    public struct DebugData
    {
        public Vector3 position;
        public Vector3 velocity;
        public float sampledTerrainHeight;
        public float groundDistance;
        public float isAvoiding;

        public DebugData(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
            sampledTerrainHeight = 0;
            groundDistance = 0;
            isAvoiding = 0;
        }
    }

    [Header("Boid General Settings")]
    public int boidsCount;

    [Header("Boid Movement Values")]
    public float rotationSpeed = 1f;
    public float boidSpeed = 1f;
    public float boidSpeedVariation = 1f;
    public float boidMaxSeparationSpeed = 5f;

    [Header("Boid Avoidance Values")]
    public float neighbourDistance = 1f;
    public float avoidanceDistance = 3f;

    [Header("Boid Weights")]
    /// <summary>
    /// The force at which boids match their velocity
    /// with other nearby boids
    /// </summary>
    public float AlignmentWeight
    {
        get => _alignmentWeight;
        set => PropertyChanged(ref _alignmentWeight, value);
    }

    /// <summary>
    /// <inheritdoc cref="AlignmentWeight"/>
    /// </summary>
    [Range(0, 1000f)]
    [SerializeField]
    private float _alignmentWeight;

    /// <summary>
    /// The force at which boids move towards large groups of boids
    /// </summary>
    public float CohesionWeight
    {
        get => _cohesionWeight;
        set => PropertyChanged(ref _cohesionWeight, value);
    }

    /// <summary>
    /// <inheritdoc cref="CohesionWeight"/>
    /// </summary>
    [Range(0, 1000f)]
    [SerializeField]
    private float _cohesionWeight;

    /// <summary>
    /// The force at which boids move away from each
    /// other after coming in close contact
    /// </summary>
    public float SeperationWeight
    {
        get => _seperationWeight;
        set => PropertyChanged(ref _seperationWeight, value);
    }

    /// <summary>
    /// <inheritdoc cref="SeperationWeight"/>
    /// </summary>
    [Range(0, 1000f)]
    [SerializeField]
    private float _seperationWeight;

    /// <summary>
    /// The force at which a boid returns towards
    /// the center of the flock after going out of bounds
    /// </summary>
    public float CorrectionWeight
    {
        get => _correctionWeight;
        set => PropertyChanged(ref _correctionWeight, value);
    }

    /// <summary>
    /// <inheritdoc cref="CorrectionWeight"/>
    /// </summary>
    [Range(0, 1000f)]
    [SerializeField]
    private float _correctionWeight;

    /// <summary>
    /// The force at which a boid moves
    /// away from the ground
    /// </summary>
    public float GroundAvoidanceWeight
    {
        get => _groundAvoidanceWeight;
        set => PropertyChanged(ref _groundAvoidanceWeight, value);
    }

    /// <summary>
    /// <inheritdoc cref="GroundAvoidanceWeight"/>
    /// </summary>
    [Range(0, 1000f)]
    [SerializeField]
    private float _groundAvoidanceWeight;

    [Header("Boundry Values")]
    public float spawnRadius;
    public float maximumRadius;

    [Header("Animator Values")]
    public float boidFrameSpeed = 10f;
    public bool frameInterpolation = true;
    private int numberOfFrames;

    [Header("Boid References")]
    public ComputeShader shader;
    public Mesh boidMesh;
    public Material boidMaterial;
    public Transform target;

    [SerializeField]
    private Terrain terrain;

    [Header("Animator References")]
    public GameObject boidObject;

    [Tooltip("Animation we want to use")]
    public AnimationClip animationClip;
    private Animator animator;
    private SkinnedMeshRenderer boidSMR;

    private int kernelHandle;

    private ComputeBuffer boidsBuffer;
    private ComputeBuffer vertexAnimationBuffer;

    private Boid[] boidsArray;

    private int groupSizeX;

    private int numberOfBoids;

    private RenderParams renderParams;

    private GraphicsBuffer argsBuffer;

    private const int STRIDE = 11;

    private const int DEBUG_STRIDE = 9;

    [Header("Debug Settings")]
    [Tooltip("WARNING! Will affect performance")]
    public bool isDebugEnabled;
    public bool isTerrainDebugEnabled;
    public float debugRayDist = 5.0f;
    private ComputeBuffer debugBuffer;
    private DebugData[] debugArray;

    /// <summary>
    /// Updates a property in the shader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    private void PropertyChanged<T>(ref T property, object value)
    {
        //Set property
        property = (T)value;

        //Update in shader
        switch (property)
        {
            case float:
                shader.SetFloat(nameof(property), (float)(object)property);
                break;

            case int:
                shader.SetInt(nameof(property), (int)(object)property);
                break;

            case string:
                shader.SetVector(nameof(property), (Vector3)(object)property);
                break;
        }
    }

    private void Awake()
    {
        kernelHandle = shader.FindKernel("CSMain");

        shader.GetKernelThreadGroupSizes(kernelHandle, out uint x, out _, out _);
        groupSizeX = Mathf.CeilToInt(boidsCount / (float)x);
        numberOfBoids = groupSizeX * (int)x;

        InitBoids();
        GenerateSkinnedAnimationForGPUBuffer();
        InitTerrain();
        InitShader();

        renderParams = new RenderParams(boidMaterial)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000),
        };
    }

    private float[,] heights;

    void InitTerrain()
    {
        shader.SetTexture(kernelHandle, "_HeightMap", terrain.terrainData.heightmapTexture);
        shader.SetFloat("_HeightmapResolution", terrain.terrainData.heightmapResolution);
        shader.SetVector("_TerrainSize", terrain.terrainData.size);
        shader.SetVector("_TerrainPosition", terrain.transform.position);
    }

    /// <summary>
    /// Creates an array of boids
    /// </summary>
    void InitBoids()
    {
        boidsArray = new Boid[numberOfBoids];
        debugArray = new DebugData[numberOfBoids];

        //Populate array with boids
        for (int i = 0; i < numberOfBoids; i++)
        {
            //Random boid spawn pos
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;

            //Random boid rotation
            Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);

            //Random offset
            float offset = Random.value * boidsCount;

            //Add boid to array
            boidsArray[i] = new Boid(pos, rot.eulerAngles, offset);
            debugArray[i] = new DebugData(Vector3.zero);
        }
    }

    /// <summary>
    /// Creates a compute buffer
    /// </summary>
    void InitShader()
    {
        // Init graphics buffer
        argsBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.IndirectArguments,
            1,
            GraphicsBuffer.IndirectDrawIndexedArgs.size
        );

        //Create data buffer object
        GraphicsBuffer.IndirectDrawIndexedArgs[] data = new GraphicsBuffer.IndirectDrawIndexedArgs[
            1
        ];

        //Set indexCountPerInstance to vertex count of boid mesh
        data[0].indexCountPerInstance = boidMesh.GetIndexCount(0);

        //Set instance count to the number of boids we want
        data[0].instanceCount = (uint)numberOfBoids;

        //Set buffers data so data is on GPU
        argsBuffer.SetData(data);

        //Init boid buffer
        boidsBuffer = new ComputeBuffer(numberOfBoids, STRIDE * sizeof(float));

        //Cache data on GPU
        boidsBuffer.SetData(boidsArray);

        //Set boid properties
        shader.SetInt("_BoidsCount", numberOfBoids);
        shader.SetFloat("_RotationSpeed", rotationSpeed);
        shader.SetFloat("_BoidSpeed", boidSpeed);
        shader.SetFloat("_NeighborDistance", neighbourDistance);
        shader.SetFloat("_AvoidanceDistance", avoidanceDistance);
        shader.SetFloat("_BoidSpeedVariation", boidSpeedVariation);
        shader.SetVector("_FlockPosition", target.transform.position);
        shader.SetFloat("_MaxSeparationSpeed", boidMaxSeparationSpeed);

        //Set weight properties
        shader.SetFloat("_AlignmentWeight", AlignmentWeight);
        shader.SetFloat("_CohesionWeight", CohesionWeight);
        shader.SetFloat("_SeperationWeight", SeperationWeight);
        shader.SetFloat("_AvoidanceWeight", GroundAvoidanceWeight);
        shader.SetFloat("_CorrectionWeight", CorrectionWeight);

        //Set boundry properties
        shader.SetFloat("_MaximumRadius", maximumRadius);
        shader.SetVector("_SphereCenter", transform.position);

        //Set animation properties
        shader.SetInt("_NumberOfFrames", numberOfFrames);
        shader.SetFloat("_BoidFrameSpeed", boidFrameSpeed);

        //Set buffer properties
        shader.SetBuffer(kernelHandle, "_BoidsBuffer", boidsBuffer);

        //Set Material properties
        boidMaterial.SetBuffer("boidsBuffer", boidsBuffer);
        boidMaterial.SetInt("numberOfFrames", numberOfFrames);

        //Enabling smooth interpolation between frames in litfowardshader
        if (frameInterpolation && !boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION"))
            boidMaterial.EnableKeyword("FRAME_INTERPOLATION");
        if (!frameInterpolation && boidMaterial.IsKeywordEnabled("FRAME_INTERPOLATION"))
            boidMaterial.DisableKeyword("FRAME_INTERPOLATION");

        //Debug properties
        debugBuffer = new ComputeBuffer(numberOfBoids, DEBUG_STRIDE * sizeof(float));
        debugBuffer.SetData(debugArray);

        shader.SetBuffer(kernelHandle, "_DebugBuffer", debugBuffer);
    }

    private void Update()
    {
        //Update compute shaders uniform time values
        shader.SetFloat("_Time", Time.time);
        shader.SetFloat("_DeltaTime", Time.deltaTime);

        //Dispatch compute shader to GPU
        shader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        //Render updated boids
        //Change to instanced rendering eventually
        Graphics.RenderMeshIndirect(renderParams, boidMesh, argsBuffer);

        if (!isDebugEnabled)
            return;
        //Retrive debug data
        debugBuffer.GetData(debugArray);
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
        numberOfFrames = Mathf.ClosestPowerOfTwo(
            (int)(animationClip.frameRate * animationClip.length)
        );

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
        boidsBuffer?.Release();
        argsBuffer?.Release();
        debugBuffer?.Release();
        vertexAnimationBuffer?.Release();
    }

    [SerializeField]
    private float terrainSampleSphereRadius = 0.3f;

    private void OnDrawGizmosSelected()
    {
        /*Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);*/
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maximumRadius);

        if (!isDebugEnabled || debugArray == null)
            return;

        //Render debug visuals for each boid
        for (int i = 0; i < debugArray.Length; ++i)
        {
            //Render velocity and display avoidance
            Debug.DrawRay(
                debugArray[i].position,
                debugArray[i].velocity * debugRayDist,
                debugArray[i].isAvoiding == 1 ? Color.red : Color.green
            );

            if (!isTerrainDebugEnabled)
                continue;

            //Draw line from boid to sampled ground
            if (debugArray[i].isAvoiding == 1)
                Debug.DrawLine(
                    debugArray[i].position,
                    new Vector3(
                        debugArray[i].position.x,
                        debugArray[i].sampledTerrainHeight,
                        debugArray[i].position.z
                    ),
                    Color.yellow
                );
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                new Vector3(
                    debugArray[i].position.x,
                    debugArray[i].sampledTerrainHeight,
                    debugArray[i].position.z
                ),
                terrainSampleSphereRadius
            );
        }
    }
}
