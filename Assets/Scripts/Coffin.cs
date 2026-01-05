// Coffin.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coffin is revealed after DigSite completes.
/// Players can grab it; with 1 grabber it's slow (and noisy later), with 2 it's faster.
/// For now: "grab to drag" via joints (physics-friendly).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public sealed class Coffin : MonoBehaviour, IInteractable
{
    [Header("Visibility")]
    [SerializeField] private List<Renderer> visualRenderers;

    [Header("Carry")]
    [SerializeField, Min(1)] private int idealGrabbers = 2;
    [SerializeField, Range(0.1f, 1f)] private float soloSpeedMultiplier = 0.55f;
    [SerializeField, Range(0.1f, 1f)] private float duoSpeedMultiplier = 0.9f;

    [Tooltip("Where on the coffin the joint anchors (local space).")]
    [SerializeField] private Vector3 grabAnchorLocal = Vector3.zero;

    private readonly List<Grabber> grabbers = new List<Grabber>(2);
    private Rigidbody rb;

    private struct Grabber
    {
        public PlayerInteractor interactor;
        public FixedJoint joint;
        public EntityMovement movement;
        public float baseSpeed;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetHidden(bool hidden)
    {
        foreach (var renderer in visualRenderers)
        {
            if (renderer != null) renderer.enabled = !hidden;
        }
        rb.isKinematic = hidden;
        rb.detectCollisions = !hidden;
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        if (interactor == null) return false;

        // Toggle grab/release
        return true;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (interactor == null) return;

        int idx = grabbers.FindIndex(g => g.interactor == interactor);
        if (idx >= 0)
        {
            Release(interactor);
            return;
        }

        Grab(interactor);
    }

    private void Grab(PlayerInteractor interactor)
    {
        var mover = interactor.GetComponent<EntityMovement>();
        var moverRb = interactor.GetComponent<Rigidbody>();
        if (mover == null || moverRb == null) return;

        // Create joint on coffin connecting to player rigidbody.
        var joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = moverRb;
        joint.anchor = grabAnchorLocal;

        var g = new Grabber
        {
            interactor = interactor,
            joint = joint,
            movement = mover,
            baseSpeed = mover.MoveSpeed
        };

        grabbers.Add(g);
        RecomputeCarrySpeed();
    }

    private void Release(PlayerInteractor interactor)
    {
        int idx = grabbers.FindIndex(g => g.interactor == interactor);
        if (idx < 0) return;

        var g = grabbers[idx];

        // Restore speed
        if (g.movement != null)
            g.movement.MoveSpeed = g.baseSpeed;

        if (g.joint != null)
            Destroy(g.joint);

        grabbers.RemoveAt(idx);
        RecomputeCarrySpeed();
    }

    private void RecomputeCarrySpeed()
    {
        int n = grabbers.Count;
        float mult = (n >= idealGrabbers) ? duoSpeedMultiplier : (n >= 1 ? soloSpeedMultiplier : 1f);

        for (int i = 0; i < grabbers.Count; i++)
        {
            var g = grabbers[i];
            if (g.movement != null)
                g.movement.MoveSpeed = g.baseSpeed * mult;
        }
    }
}
