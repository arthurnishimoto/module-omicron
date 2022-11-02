using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelSpaceRaycast : MonoBehaviour
{
    public LayerMask wandLayerMask = -1;
    public LayerMask wandLayerMask2 = -1;

    [SerializeField]
    bool wandHit;

    [SerializeField]
    CAVE2Display display;

    [SerializeField]
    Vector3 hitPoint;

    CAVE2WandInteractor wand;

    [SerializeField]
    bool wandPointing;

    [SerializeField]
    GameObject wandPointingHit = null;

    [SerializeField]
    Vector2 displayHitOffset;

    [SerializeField]
    LineRenderer laserLine = null;

    Vector3 wandOffset;

    ParticleSystem laserParticle;

    // Start is called before the first frame update
    void Start()
    {
        wand = GetComponent<CAVE2WandInteractor>();

        if(laserLine)
        {

            laserLine.startWidth = 0.02f;
            laserLine.endWidth = 0.02f;

            laserLine.useWorldSpace = true;

            laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            laserLine.receiveShadows = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        RaycastHit hit;

        // Get the first collider that was hit by the ray
        wandHit = Physics.Raycast(ray, out hit, 100, wandLayerMask);
        Debug.DrawLine(ray.origin, hit.point); // Draws a line in the editor

        if(wandHit)
        {
            display = hit.collider.GetComponentInParent<CAVE2Display>();

            hitPoint = hit.collider.transform.InverseTransformPoint(hit.point);

            // HitPoint is centered at 0, 0. Shift to edge
            hitPoint.x += 0.5f;
            hitPoint.y += 0.5f;

            // Invert x
            hitPoint.x = 1.0f - hitPoint.x;

            if (display)
            {
                // Perform a raycast from virtual camera's space into world space
                Ray displayRay = display.DisplayRaycast(hitPoint);

                // Mostly copied from CAVE2WandInteractor
                RaycastHit displayRayHit;

                Debug.DrawRay(displayRay.origin, displayRay.direction * 100, Color.yellow);

                wandPointing = Physics.Raycast(displayRay, out displayRayHit, 100, wandLayerMask2);

                bool laserButtonPressed = CAVE2.Input.GetButton(wand.GetWandID(), CAVE2.Button.Button5);

                if (laserButtonPressed)
                {
                    laserLine.startWidth = 0.02f * 8;
                    laserLine.endWidth = 0.02f * 8;
                }
                else
                {
                    laserLine.startWidth = 0.02f * 4;
                    laserLine.endWidth = 0.02f * 4;
                }

                // Draw line in CAVE2 sim mode - OLD
                //laserLine.SetPosition(0, displayRay.origin + wandOffset + displayRay.direction * 4);
                //laserLine.SetPosition(1, displayRay.origin + wandOffset + displayRay.direction * 100);

                // Dont draw line in CAVE2 sim mode
                laserLine.SetPosition(0, Vector3.zero);
                laserLine.SetPosition(1, Vector3.zero);


                // Use laser particle instead
                if (laserParticle != null)
                {
                    laserParticle.transform.position = hit.point;
                    laserParticle.Emit(1);
                }
                
                if (wandPointing) // The wand is pointed at a collider
                {
                    CAVE2.WandEvent playerInfo = new CAVE2.WandEvent(wand.GetPlayerID(), wand.GetWandID(), CAVE2.Button.None, CAVE2.InteractionType.Pointing);

                    wandPointingHit = displayRayHit.collider.gameObject;

                    // Send a message to the hit object telling it that the wand is hovering over it
                    displayRayHit.collider.gameObject.SendMessage("OnWandPointing", playerInfo, SendMessageOptions.DontRequireReceiver);

                    CAVE2WandInteractor.ProcessButtons(wand.GetWandID(), displayRayHit.collider.gameObject, playerInfo);
                }
                else
                {
                    wandPointingHit = null;
                }
            }
        }
        else
        {
            display = null;

            laserLine.SetPosition(0, Vector3.zero);
            laserLine.SetPosition(1, Vector3.zero);
        }
    }

    public void SetWandOffset(Vector3 offset)
    {
        wandOffset = offset;
    }

    public void SetLaserParticle(ParticleSystem particleSystem)
    {
        laserParticle = particleSystem;
    }
}
