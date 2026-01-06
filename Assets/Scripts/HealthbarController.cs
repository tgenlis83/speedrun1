using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class HealthbarController : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

    [Header("Visuals")]
    [SerializeField] private Gradient healthGradient;
    [SerializeField, Range(0f, 1f)] private float lowHealthThreshold = 0.2f;
    [SerializeField] private float minGlowIntensity = 1f;
    [SerializeField] private float maxGlowIntensity = 2.5f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float fadeDelay = 5f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Shake")]
    [SerializeField] private float shakeMagnitude = 0.2f;
    [SerializeField] private float shakeDecay = 5f;
    
    // Optional: Reference to follow logic if it's world space
    [SerializeField] private Vector3 offset;
    private Transform target;
    private Material instancedMaterial;
    private float currentPercent = 1f;
    private float fullHealthTimer = 1f;
    private float currentAlpha = 0f; 
    private Color targetColor = Color.white;
    private float shakeAmount = 0f;

    private void Awake()
    {
        if (healthText != null)
        {
            // Create a unique material instance so we don't affect other healthbars
            instancedMaterial = new Material(healthText.fontMaterial);
            instancedMaterial.EnableKeyword("GLOW_ON");
            healthText.fontMaterial = instancedMaterial;
        }
    }

    public void Initialize(Transform targetEntity)
    {
        target = targetEntity;
    }

    public void UpdateHealth(float current, float max)
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(current)}";
            
            float pct = Mathf.Clamp01(current / max);
            
            if (pct < currentPercent) shakeAmount = 1f;

            currentPercent = pct;
            targetColor = healthGradient.Evaluate(pct);

            // If taking damage (or simply not full), reset timer and show immediately.
            // Using 0.999f to avoid float precision issues with "full life"
            if (pct < 0.999f) 
            {
                fullHealthTimer = 0f;
                currentAlpha = 1f; // Snap to visible on damage
            }
        }
    }

    public InputActionReference click;

    private void Update()
    {
        // if click remove 5 health
        if (click != null && click.action.WasPerformedThisFrame())
        {
            // Just for testing purposes
            UpdateHealth(currentPercent * 100f - 5f, 100f);
        }
        // 1. Handle Full Health Fade Logic
        if (currentPercent >= 0.999f)
        {
            fullHealthTimer += Time.deltaTime;
        }
        else
        {
            fullHealthTimer = 0f;
        }
        
        // Decay shake
        shakeAmount = Mathf.MoveTowards(shakeAmount, 0f, Time.deltaTime * shakeDecay);

        float targetAlpha = (fullHealthTimer >= fadeDelay) ? 0f : 1f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // 2. Animate glow if low health
        float glowIntensity = 1.5f;
        if (currentPercent <= lowHealthThreshold)
        {
            float sine = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
            glowIntensity = Mathf.Lerp(minGlowIntensity, maxGlowIntensity, sine);
        }

        // 3. Apply Colors
        if (healthText != null)
        {
            Color finalTextColor = targetColor;
            finalTextColor.a = currentAlpha;
            healthText.color = finalTextColor;

            if (instancedMaterial != null)
            {
                // Scale glow by alpha so it fades out too
                Color glowColor = targetColor * glowIntensity * currentAlpha;
                instancedMaterial.SetColor("_GlowColor", glowColor);
                healthText.ForceMeshUpdate();
            }
        }
    }

    void LateUpdate()
    {
        // Follow the target if assigned
        if (target != null)
        {
            Vector3 shakeOffset = Vector3.zero;
            if (shakeAmount > 0.01f)
            {
                shakeOffset = Random.insideUnitSphere * (shakeAmount * shakeMagnitude);
            }

            transform.position = target.position + offset + shakeOffset;
            
            // Face up for 3d effect
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
        }
        else
        {
            // If target is destroyed, destroy bar
            Destroy(gameObject);
        }
    }
}
