// ExitZone.cs
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ExitZone : MonoBehaviour
{
    public event System.Action<Coffin> OnCoffinExited;

    private void OnTriggerEnter(Collider other)
    {
        var coffin = other.GetComponentInParent<Coffin>();
        if (coffin != null)
            OnCoffinExited?.Invoke(coffin);
    }
}
