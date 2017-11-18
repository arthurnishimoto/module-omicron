using UnityEngine;
using System.Collections;

public class CAVE2PlayerCollider : MonoBehaviour {

    [SerializeField]
    int headID = 1;

    [SerializeField]
    float bodyRadius = 0.3f;

    [SerializeField]
    CapsuleCollider bodyCollider;

    new Rigidbody rigidbody;
    Vector3 playerHeadPosition;

    [SerializeField]
    Collider[] playerColliders;

    // Use this for initialization
    void Start () {

        // Setup body collider
        if(bodyCollider == null)
        {
            bodyCollider = gameObject.AddComponent<CapsuleCollider>();
        }
        rigidbody = bodyCollider.GetComponent<Rigidbody>();
        if (rigidbody == null )
        {
            rigidbody = bodyCollider.gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        // Ignore collisions between body and any listed child coliders
        // as well as between child colliders
        Collider lastCollider = bodyCollider;
        foreach( Collider c in playerColliders )
        {
            Debug.Log("IgnoreCollision: " + bodyCollider.name + " " + c.name);
            Debug.Log("IgnoreCollision: " + lastCollider.name + " " + c.name);

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
