using UnityEngine;
using System.Collections;

public class CAVE2PlayerCollider : MonoBehaviour {

    public int headID = 1;

    public float bodyRadius = 0.3f;

    CapsuleCollider bodyCollider;
    new Rigidbody rigidbody;
    Vector3 playerHeadPosition;

    public Collider[] playerColliders;

    // Use this for initialization
    void Start () {

        // Setup body collider
        bodyCollider = GetComponent<CapsuleCollider>();
        if(bodyCollider == null)
        {
            bodyCollider = gameObject.AddComponent<CapsuleCollider>();
        }
        rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null )
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        // Ignore collisions between body and any listed child coliders
        // as well as between child colliders
        Collider lastCollider = bodyCollider;
        foreach( Collider c in playerColliders )
        {
            Physics.IgnoreCollision(bodyCollider, c);
            Physics.IgnoreCollision(lastCollider, c);
            lastCollider = c;
        }

        UpdatePlayerCollider();
    }

	void FixedUpdate () {
        UpdatePlayerCollider();
    }

    void UpdatePlayerCollider()
    {
        bodyCollider.radius = bodyRadius;
        playerHeadPosition = CAVE2.GetHeadPosition(1);

        // Prevent collider from height = 0, which causes falling through floors
        if (playerHeadPosition.y < 0.1f)
        {
            playerHeadPosition.y = 0.1f;
        }

        bodyCollider.height = playerHeadPosition.y;
        bodyCollider.center = new Vector3(playerHeadPosition.x, bodyCollider.height / 2.0f, playerHeadPosition.z);
    }
}
