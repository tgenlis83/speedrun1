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
    [SerializeField] private float coffinMass = 80f;

    [Tooltip("Where on the coffin the joint anchors (local space).")]
    [SerializeField] private Vector3 grabAnchorLocal = Vector3.zero;

    private readonly List<Grabber> grabbers = new List<Grabber>(2);
    private readonly Dictionary<PlayerInteractor, Vector3> touchingInteractors = new Dictionary<PlayerInteractor, Vector3>();
    private Rigidbody rb;

    private struct Grabber
    {
        public PlayerInteractor interactor;
        public ConfigurableJoint joint;
        public Transform grabPointHelper;
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
        
        if (hidden)
        {
            touchingInteractors.Clear();
        }
        else
        {
            // Scale mass based on player count to suit lobby size
            int playerCount = Mathf.Max(1, PlayerRegistry.Players.Count);
            rb.mass = coffinMass * playerCount;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var interactor = collision.gameObject.GetComponentInParent<PlayerInteractor>();
        if (interactor != null) 
        {
            Vector3 point = collision.GetContact(0).point;
            touchingInteractors[interactor] = transform.InverseTransformPoint(point);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var interactor = collision.gameObject.GetComponentInParent<PlayerInteractor>();
        if (interactor != null) touchingInteractors.Remove(interactor);
    }

    public bool CanInteract(PlayerInteractor interactor)
    {
        if (interactor == null) return false;

        // If already grabbing, we can always interact (to release)
        if (grabbers.FindIndex(g => g.interactor == interactor) >= 0) return true;

        // Otherwise need direct physical contact
        return touchingInteractors.ContainsKey(interactor);
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (interactor == null || interactor.Inventory.CurrentCarryItem != null) return;

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
        var moverRb = gameObject.GetComponent<Rigidbody>();
        if (moverRb == null) return;

        // Create joint on coffin connecting to player rigidbody.
        var joint = interactor.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = moverRb;
        joint.anchor = grabAnchorLocal;
        joint.autoConfigureConnectedAnchor = true;
        // joint.connectedAnchor = Vector3.zero; // Attach to center of player
        
        // Lock all motion
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Free;
        joint.zMotion = ConfigurableJointMotion.Locked;
        
        // Allow X rotation, lock Y and Z rotation
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        joint.enableCollision = true; // Prevent player colliding with coffin while pulling?

        // Determine grab point
        Transform grabPoint = this.transform; // Fallback
        Transform helper = null;

        if (touchingInteractors.TryGetValue(interactor, out Vector3 localHit))
        {
            var go = new GameObject($"GrabPoint_{interactor.gameObject.name}");
            go.transform.SetParent(transform);
            go.transform.localPosition = localHit;
            go.transform.localRotation = Quaternion.identity;
            helper = go.transform;
            grabPoint = helper;
        }

        // Update animation & movement orientation
        var anim = interactor.GetComponent<EntityAnimation>();
        if (anim != null) anim.SetDraggingState(true);

        var move = interactor.GetComponent<EntityMovement>();
        if (move != null) move.SetDragging(true, grabPoint);

        var g = new Grabber
        {
            interactor = interactor,
            joint = joint,
            grabPointHelper = helper
        };

        grabbers.Add(g);
    }

    private void Release(PlayerInteractor interactor)
    {
        int idx = grabbers.FindIndex(g => g.interactor == interactor);
        if (idx < 0) return;

        var g = grabbers[idx];

        if (g.joint != null)
            Destroy(g.joint);

        if (g.grabPointHelper != null)
            Destroy(g.grabPointHelper.gameObject);

        // Update animation & movement orientation
        var anim = g.interactor.GetComponent<EntityAnimation>();
        if (anim != null) anim.SetDraggingState(false);

        var move = g.interactor.GetComponent<EntityMovement>();
        if (move != null) move.SetDragging(false);

        grabbers.RemoveAt(idx);
    }
}
