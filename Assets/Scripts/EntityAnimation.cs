using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class EntityAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float smoothSpeed = 5f;
    [Tooltip("Divisor for speed when calculating animation 'Speed' parameter.")]
    [SerializeField, Min(0.1f)] private float speedDivisor = 6f;
    
    [Header("Walk Bob")]
    [SerializeField] private Transform meshTransform;
    [SerializeField] private AnimationCurve bobCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField, Min(0.1f)] private float bobHeight = 0.05f;
    [SerializeField, Min(0.1f)] private float bobSpeed = 8f;
    
    private float currentMoveValue;
    private float currentSpeedValue = 1f;
    private float bobTime;
    private Vector3 startLocalPosition;

    void Start()
    {
        animator ??= GetComponent<Animator>();
        if (meshTransform != null)
            startLocalPosition = meshTransform.localPosition;
    }

    void Update()
    {
        animator?.SetFloat("Move", currentMoveValue);
        animator?.SetFloat("Speed", currentSpeedValue);
    }

    void LateUpdate()
    {
        // Apply walk bob AFTER animator updates (additive, non-destructive)
        if (meshTransform != null && currentMoveValue > 0.1f)
        {
            bobTime += Time.deltaTime * bobSpeed * currentSpeedValue;
            float bobOffset = bobCurve.Evaluate(bobTime % 1f) * bobHeight;
            
            // Apply bob on top of whatever the animator did
            Vector3 currentPos = meshTransform.localPosition;
            meshTransform.localPosition = new Vector3(currentPos.x, startLocalPosition.y + bobOffset, currentPos.z);
        }
        else if (meshTransform != null && bobTime > 0f)
        {
            // Smoothly reset bob when stopping
            bobTime = 0f;
            Vector3 currentPos = meshTransform.localPosition;
            meshTransform.localPosition = new Vector3(currentPos.x, startLocalPosition.y, currentPos.z);
        }
    }

    public void SetFloats(float move, float speed)
    {
        if (speed < 0.05f) move = 0f;
        currentMoveValue = Mathf.Lerp(currentMoveValue, move, Time.deltaTime * smoothSpeed);
        if (move < 0.05f) speed = speedDivisor;
        currentSpeedValue = Mathf.Lerp(currentSpeedValue, speed / speedDivisor, Time.deltaTime * smoothSpeed);
    }
}
