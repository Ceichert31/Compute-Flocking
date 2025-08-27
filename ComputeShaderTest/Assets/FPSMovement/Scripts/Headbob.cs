using UnityEngine;
public class Headbob : MonoBehaviour
{
    [Header("Head bob References")]
    [Tooltip("Camera holder which will have movement applied to it")]
    [SerializeField] private Transform headbobTarget;

    [Header("Head bob Settings")]
    [Tooltip("How fast the x and y of the camera bob")]
    [SerializeField] private Vector2 headbobSpeed;

    [Tooltip("How far the x and y of the camera will bob")]
    [SerializeField] private Vector2 headbobIntensity;

    [Tooltip("The vertical path the camera will take when bobbing")]
    [SerializeField] private AnimationCurve headbobCurveY;

    [Tooltip("The horizontal path the camera will take when bobbing")]
    [SerializeField] private AnimationCurve headbobCurveX;

    private Vector2 _currentPos;
    private Vector2 _currentTime;

    private InputController _inputController;
    private Rigidbody _rb;

    private float _smoothTime;
    private Vector3 _velocity;

    private float SpeedFactor
    {
        get
        {
            return _rb.linearVelocity.magnitude;
        }
    }
    private bool ApplyMovementEffects
    {
        get
        {
            return _inputController.ApplyMovementEffects;
        }
    }
    private bool IsGrounded
    {
        get
        {
            return _inputController.IsGrounded;
        }
    }
    private bool IsMoving
    {
        get
        {
            return _inputController.IsMoving;
        }
    }

    private void Start()
    {
        _inputController = GetComponent<InputController>();

        _rb = GetComponent<Rigidbody>();

        if (headbobTarget == null)
            headbobTarget = transform.GetChild(0);
    }
    private void Update()
    {
        if (ApplyMovementEffects) return;

        UpdateHeadBob();
    }
    private void FixedUpdate()
    {
        //Vector3 targetPos = new Vector3(_currentPos.x, _currentPos.y, 0);
        Vector3 desiredPos = Vector3.SmoothDamp(headbobTarget.localPosition, _currentPos, ref _velocity, _smoothTime);

        headbobTarget.localPosition = desiredPos;
    }

    /// <summary>
    ///     Updates the position of the camera holder based on movement speed
    /// </summary>
    private void UpdateHeadBob()
    {
        //If player is stationary
        if (!IsGrounded || !IsMoving)
        {
            _currentTime = Vector2.zero;
            //Set camera to default position
            _currentPos.Set(headbobCurveX.Evaluate(_currentTime.x), headbobCurveY.Evaluate(_currentTime.y));
            _smoothTime = 0.2f;
        }
        else if (IsMoving)
        {
            _currentTime.x += headbobSpeed.x / 10 * Time.deltaTime * SpeedFactor;
            _currentTime.y += headbobSpeed.y / 10 * Time.deltaTime * SpeedFactor;
            _currentPos.x = headbobCurveX.Evaluate(_currentTime.x) * headbobIntensity.x;
            _currentPos.y = headbobCurveY.Evaluate(_currentTime.y) * headbobIntensity.y;

            _smoothTime = 0.1f;
        }
    }
}
