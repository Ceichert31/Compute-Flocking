using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class InstancedFlocking : MonoBehaviour
{

    #region Structs
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
        public float radius;
        
        public DebugData(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
            sampledTerrainHeight = 0;
            groundDistance = 0;
            isAvoiding = 0;
            radius = 0;
        }
    }
    private const int STRIDE = 11;

    private const int DEBUG_STRIDE = 10;
    #endregion
    
    #region Properties
    public int BoidsCount
    {
        get => _boidsCount;
        set => PropertyChanged(ref _boidsCount, value, nameof(_boidsCount));
    }
    public float BoidSpeed
    {
        get => boidSpeed;
        set => PropertyChanged(ref boidSpeed, value, nameof(boidSpeed));
    }
    /// <summary>
    /// The radius in which boids detect one another
    /// </summary>
    public float NeighbourDistance
    {
        get => neighborDistance;
        set => PropertyChanged(ref neighborDistance, value, nameof(neighborDistance));
    }
    /// <summary>
    /// The force at which boids match their velocity
    /// with other nearby boids
    /// </summary>
    public float AlignmentWeight
    {
        get => alignmentWeight;
        set => PropertyChanged(ref alignmentWeight, value, nameof(alignmentWeight));
    }
    /// <summary>
    /// The force at which boids move towards large groups of boids
    /// </summary>
    public float CohesionWeight
    {
        get => cohesionWeight;
        set => PropertyChanged(ref cohesionWeight, value, nameof(cohesionWeight));
    }
    /// <summary>
    /// The force at which boids move away from each
    /// other after coming in close contact
    /// </summary>
    public float SeparationWeight
    {
        get => separationWeight;
        set => PropertyChanged(ref separationWeight, value, nameof(separationWeight));
    }
    /// <summary>
    /// The force at which a boid moves
    /// away from the ground
    /// </summary>
    public float GroundAvoidanceWeight
    {
        get => avoidanceWeight;
        set => PropertyChanged(ref avoidanceWeight, value, nameof(avoidanceWeight));
    }
    /// <summary>
    /// How far boids can go from their starting position
    /// </summary>
    public float MaximumRadius
    {
        get => maximumRadius;
        set => PropertyChanged(ref  maximumRadius, value, nameof(maximumRadius));
    }
    #endregion

    [Header("Boid Movement Values")]
    [SerializeField]
    private int _boidsCount = 3000;
    public float rotationSpeed = 1f;
    
    [SerializeField]
    private float boidSpeed = 3f;
    public float boidSpeedVariation = 1f;
    public float boidMaxSeparationSpeed = 5f;

    [Header("Boid Avoidance Values")]
    [SerializeField]
    private float neighborDistance = 1f;
    [Tooltip("The distance boids start to avoid the ground")]
    public float avoidanceDistance = 3f;

    [Header("Boid Weights")]
    [Range(0, 1000f)]
    [SerializeField]
    private float alignmentWeight;
    [Range(0, 1000f)]
    [SerializeField]
    private float cohesionWeight;
    [Range(0, 1000f)]
    [SerializeField]
    private float separationWeight;
    [Range(0, 1000f)]
    [SerializeField]
    private float avoidanceWeight;

    [Header("Boundary Values")]
    [SerializeField]
    private float spawnRadius;
    [SerializeField]
    private float maximumRadius;

    [Header("Animator Values")]
    [SerializeField]
    private float boidFrameSpeed = 10f;
    [SerializeField]
    private bool frameInterpolation = true;
    private int numberOfFrames;

    [Header("Boid References")]
    [SerializeField]
    private ComputeShader shader;
    [SerializeField]
    private Mesh boidMesh;
    [SerializeField]
    private Material boidMaterial;
    [SerializeField]
    private Transform target;
    [SerializeField]
    private Terrain terrain;

    [Header("Animator References")]
    [SerializeField]
    private GameObject boidObject;

    [Tooltip("Animation we want to use")]
    [SerializeField]
    private AnimationClip animationClip;
    
    private Animator animator;
    private SkinnedMeshRenderer boidSkinnedMeshRenderer;

    //Computation shader private variables
    private int kernelHandle;
    private int groupSizeX;
    private int numberOfBoids;
    
    private ComputeBuffer boidsBuffer;
    private ComputeBuffer vertexAnimationBuffer;
    private Boid[] boidsArray;
    
    //Rendering variables
    private RenderParams renderParams;
    private GraphicsBuffer argsBuffer;

    [Header("Debug Settings")]
    [Tooltip("WARNING! Will affect performance")]
    public bool isDebugEnabled;
    public bool isRadiusEnabled;
    public bool isTerrainDebugEnabled;
    [SerializeField]
    private float debugRayDist = 5.0f;
    [SerializeField]
    private float terrainSampleSphereRadius = 0.3f;
    private ComputeBuffer debugBuffer;
    private DebugData[] debugArray;

    #region Initialization
    private void Awake()
    {
        kernelHandle = shader.FindKernel("CSMain");

        shader.GetKernelThreadGroupSizes(kernelHandle, out uint x, out _, out _);
        groupSizeX = Mathf.CeilToInt(BoidsCount / (float)x);
        numberOfBoids = groupSizeX * (int)x;

        InitBoids();
        GenerateSkinnedAnimationForGPUBuffer();
        InitTerrain();
        InitShader();

        renderParams = new RenderParams(boidMaterial)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000),
        };
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
            float offset = Random.value * BoidsCount;

            //Add boid to array
            boidsArray[i] = new Boid(pos, rot.eulerAngles, offset);
            debugArray[i] = new DebugData(Vector3.zero);
        }
    }
    private void GenerateSkinnedAnimationForGPUBuffer()
    {
        boidObject.SetActive(true);
        
        //Get skinned mesh renderer from prefab
        if (boidSkinnedMeshRenderer == null)
        {
            boidSkinnedMeshRenderer = boidObject.GetComponentInChildren<SkinnedMeshRenderer>();
            //Get starting mesh
            boidMesh = boidSkinnedMeshRenderer.sharedMesh;
        }

        if (animator == null)
        {
            //Get animator from prefab
            animator = boidObject.GetComponent<Animator>();
        }

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
        int vertexCount = boidSkinnedMeshRenderer.sharedMesh.vertexCount;

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
            boidSkinnedMeshRenderer.BakeMesh(bakedMesh);

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
    void InitTerrain()
    {
        shader.SetTexture(kernelHandle, "_heightMap", terrain.terrainData.heightmapTexture);
        shader.SetFloat("_heightmapResolution", terrain.terrainData.heightmapResolution);
        shader.SetVector("_terrainSize", terrain.terrainData.size);
        shader.SetVector("_terrainPosition", terrain.transform.position);
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
        shader.SetInt("_boidsCount", numberOfBoids);
        shader.SetFloat("_rotationSpeed", rotationSpeed);
        shader.SetFloat("_boidSpeed", BoidSpeed);
        shader.SetFloat("_neighborDistance", NeighbourDistance);
        shader.SetFloat("_avoidanceDistance", avoidanceDistance);
        shader.SetFloat("_boidSpeedVariation", boidSpeedVariation);
        shader.SetVector("_flockPosition", target.transform.position);
        shader.SetFloat("_maxSeparationSpeed", boidMaxSeparationSpeed);

        //Set weight properties
        shader.SetFloat("_alignmentWeight", AlignmentWeight);
        shader.SetFloat("_cohesionWeight", CohesionWeight);
        shader.SetFloat("_seperationWeight", SeparationWeight);
        shader.SetFloat("_avoidanceWeight", GroundAvoidanceWeight);

        //Set boundry properties
        shader.SetFloat("_maximumRadius", maximumRadius);
        shader.SetVector("_sphereCenter", transform.position);

        //Set animation properties
        shader.SetInt("_numberOfFrames", numberOfFrames);
        shader.SetFloat("_boidFrameSpeed", boidFrameSpeed);

        //Set buffer properties
        shader.SetBuffer(kernelHandle, "_boidsBuffer", boidsBuffer);

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

        shader.SetBuffer(kernelHandle, "_debugBuffer", debugBuffer);
    }
    #endregion
    
    #region Helper Methods
    /// <summary>
    /// Updates a property in the shader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="property"></param>
    private void PropertyChanged<T>(ref T property, object value, string variableName)
    {
        //Set property
        property = (T)value;

        //Update in shader
        switch (property)
        {
            case float:
                shader.SetFloat("_" + variableName, (float)(object)property);
                break;

            case int:
                shader.SetInt("_" + variableName, (int)(object)property);
                break;

            case string:
                shader.SetVector("_" + variableName, (Vector3)(property as object));
                break;
        }
    }
    /// <summary>
    /// Re-initializes boids
    /// </summary>
    public void ResetBoids()
    {
        //Clean buffers on destroy
        boidsBuffer?.Release();
        argsBuffer?.Release();
        debugBuffer?.Release();
        vertexAnimationBuffer?.Release();
        
        kernelHandle = shader.FindKernel("CSMain");

        shader.GetKernelThreadGroupSizes(kernelHandle, out uint x, out _, out _);
        groupSizeX = Mathf.CeilToInt(BoidsCount / (float)x);
        numberOfBoids = groupSizeX * (int)x;

        InitBoids();
        GenerateSkinnedAnimationForGPUBuffer();
        InitTerrain();
        InitShader();

        renderParams = new RenderParams(boidMaterial)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000),
        };
    }
    #endregion
    private void Update()
    {
        //Update compute shaders uniform time values
        shader.SetFloat("_time", Time.time);
        shader.SetFloat("_deltaTime", Time.deltaTime);

        //Dispatch compute shader to GPU
        shader.Dispatch(kernelHandle, groupSizeX, 1, 1);

        //Render updated boids
        //Change to instanced rendering eventually
        Graphics.RenderMeshIndirect(renderParams, boidMesh, argsBuffer);

        if (!isDebugEnabled)
            return;
        //Retrieve debug data
        debugBuffer.GetData(debugArray);
    }
    private void OnDestroy()
    {
        //Clean buffers on destroy
        boidsBuffer?.Release();
        argsBuffer?.Release();
        debugBuffer?.Release();
        vertexAnimationBuffer?.Release();
    }
    private void OnDrawGizmosSelected()
    {
        if (!isDebugEnabled || debugArray == null)
            return;
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, maximumRadius);

        //Render debug visuals for each boid
        for (int i = 0; i < debugArray.Length; ++i)
        {
            //Render velocity and display terrain avoidance
            Debug.DrawRay(
                debugArray[i].position,
                debugArray[i].velocity.normalized * debugRayDist,
                debugArray[i].isAvoiding == 1 ? Color.red : Color.green
            );
            
            //Render detection radius
            if (isRadiusEnabled)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(debugArray[i].position, debugArray[i].radius);
            }

            //Render terrain sample positions
            if (!isTerrainDebugEnabled) continue;
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
