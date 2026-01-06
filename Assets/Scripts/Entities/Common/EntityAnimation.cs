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
    
    private int upperLayerIndex;
    private int stateGunHash;
    private int stateToolHash;
    private int stateDraggingHash;
    private int eventShootHash;
    private int eventPickHash;
    private int eventThrowHash;

    void Start()
    {
        animator ??= GetComponent<Animator>();
        if (meshTransform != null)
            startLocalPosition = meshTransform.localPosition;
            
        if (animator != null)
        {
            upperLayerIndex = animator.GetLayerIndex("Upper");
            stateGunHash = Animator.StringToHash("StateGun");
            stateToolHash = Animator.StringToHash("StateTool");
            stateDraggingHash = Animator.StringToHash("StateDragging");
            eventShootHash = Animator.StringToHash("EventShoot");
            eventPickHash = Animator.StringToHash("EventPick");
            eventThrowHash = Animator.StringToHash("EventThrow");
        }
    }

    void Update()
    {
        animator?.SetFloat("Move", currentMoveValue);
        animator?.SetFloat("Speed", currentSpeedValue);
        
        UpdateUpperLayerWeight();
    }
    
    private void UpdateUpperLayerWeight()
    {
        if (animator == null || upperLayerIndex == -1) return;

        var stateInfo = animator.GetCurrentAnimatorStateInfo(upperLayerIndex);
        bool isNeutral = stateInfo.IsName("Neutral");
        
        // If we simply check IsName("Neutral"), we might miss transitions. 
        // We probably want weight=1 if we are NOT in Neutral, or if we are transitioning FROM Neutral.
        // If we are transitioning TO Neutral, we might want to keep it 1 until done? 
        // User said: "instantly go to 1 when it's not in the neutral state anymore".
        
        // Let's assume weight is 1 if state is NOT Neutral.
        // What about transitions? 
        // If (isNeutral && !animator.IsInTransition(upperLayerIndex)) -> Weight 0
        // Else -> Weight 1. (This covers being in other states, or transitioning out of neutral).
        // Wait, if transitioning INTO Neutral, we probably still want weight 1 to show the transition smoothly?
        // Actually, if we cut weight to 0 instantly when entering neutral, it might pop if the transition isn't finished.
        // But usually layers handle mixing. The weight controls if the layer is applied at all.
        // If the layer is in "Neutral" which presumably is an empty state or idle state masked, 
        // maybe it has no animation curves?
        // If Neutral state has "None" motion, then weight 1 is fine too.
        // However, user specifically asked for weight control.
        
        float targetWeight = isNeutral ? 0f : 1f;
        
        // User said "instantly go to 1".
        // Maybe slowly go back to 0?
        // "weight should be 0 and instantly go to 1 when it's not in the neutral state anymore."
        // Implications on return to 0 not specified. I'll make it instant for now as per "instantly go to 1" - maybe he means binary behavior.
        
        animator.SetLayerWeight(upperLayerIndex, targetWeight);
    }
    
    public void SetGunState(bool active) => animator?.SetBool(stateGunHash, active);
    public void SetToolState(bool active) => animator?.SetBool(stateToolHash, active);
    public void SetDraggingState(bool active) => animator?.SetBool(stateDraggingHash, active);
    public void TriggerShoot() => animator?.SetTrigger(eventShootHash);
    public void TriggerPick() => animator?.SetTrigger(eventPickHash);
    public void TriggerThrow() => animator?.SetTrigger(eventThrowHash);


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
