using UnityEngine;

public class ItemPickupAnimation : MonoBehaviour
{
    [SerializeField] private AnimationCurve swayCurve = AnimationCurve.Linear(0, -1, 1, 1);
    [SerializeField] private float swayAmplitude = 0.5f;
    [SerializeField] private float swayFrequency = 1f;
    [SerializeField] private float rotationSpeed = 30f;
    
    private float positionOffset;
    private float currentCycleTime;
    private Vector3 previousOffset;

    void Start()
    {
        positionOffset = transform.position.x + transform.position.z;
        currentCycleTime = positionOffset;
        previousOffset = Vector3.zero;
    }

    void Update()
    {
        currentCycleTime += Time.deltaTime * swayFrequency;
        
        if (currentCycleTime >= 1f)
        {
            currentCycleTime -= 1f;
        }
        
        float curveValue = swayCurve.Evaluate(currentCycleTime);
        
        Vector3 offset = new Vector3(0, curveValue * swayAmplitude, 0);
        transform.position = transform.position - previousOffset + offset;
        previousOffset = offset;
        
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
