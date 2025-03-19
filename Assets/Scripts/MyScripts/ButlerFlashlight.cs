using System.Collections.Generic;
using UnityEngine;

public class ButlerFlashlight : MonoBehaviour
{
    [SerializeField]
    PlayerManager ownerPlayer;
    public float angle = 25;
    public float depth = 18;
    public float radius = 18;

    Light spotLight;

    public bool lightOn { get; private set; } = true;
    bool insideAnotherObject = false;
    int nTriggers = 0;

    void Start()
    {
        spotLight = GetComponent<Light>();
    }

    void Update()
    {
        if (!spotLight.enabled || !ownerPlayer.isServer)
            return;

        RaycastHit[] coneHits = ConeCastAll(transform.position, radius, transform.forward, depth, angle, "Ghost");

        if (coneHits.Length > 0)
        {
            for (int i = 0; i < coneHits.Length; i++)
            {
                //do something with collider information
                PlayerManager pm = coneHits[i].collider.GetComponent<PlayerManager>();
                if(pm && pm.isServer)
                    pm.ServerFlashlightBanishment(ownerPlayer);
            }
        }
    }

    RaycastHit[] ConeCastAll(Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle, string tag)
    {
        RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - direction * maxDistance, maxRadius, direction, maxDistance);
        List<RaycastHit> coneCastHitList = new List<RaycastHit>();

        if (sphereCastHits.Length > 0)
        {
            for (int i = 0; i < sphereCastHits.Length; i++)
            {
                if (!sphereCastHits[i].collider.gameObject.CompareTag(tag))
                    continue;

                Vector3 hitPoint = sphereCastHits[i].point;
                Vector3 directionToHit = hitPoint - origin;
                float angleToHit = Vector3.Angle(direction, directionToHit);

                if (angleToHit < coneAngle)
                {
                    if (Physics.Raycast(origin, directionToHit, out RaycastHit hit)) //Check if behind wall or other blocking objects
                        if (hit.collider.gameObject.CompareTag(tag))
                            coneCastHitList.Add(sphereCastHits[i]);
                }
            }
        }

        RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
        coneCastHits = coneCastHitList.ToArray();

        return coneCastHits;
    }

    void UpdateFlashlight()
    {
        spotLight.enabled = insideAnotherObject ? false : lightOn;
    }

    public void ToggleFlashlight(bool value)
    {
        lightOn = value;
        UpdateFlashlight();
    }

    private void OnTriggerEnter(Collider other)
    {
        nTriggers++;
        insideAnotherObject = true;
        UpdateFlashlight();
    }
    private void OnTriggerExit(Collider other)
    {
        nTriggers--;
        if (nTriggers == 0)
        {
            insideAnotherObject = false;
            UpdateFlashlight();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * radius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position - transform.forward * depth, .3f);
        Gizmos.DrawLine(transform.position - transform.forward * depth, transform.position - transform.forward * depth + transform.forward * depth);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, angle, 0) * transform.forward * depth);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(angle, 0, 0) * transform.forward * depth);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, -angle, 0) * transform.forward * depth);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(-angle, 0, 0) * transform.forward * depth);
        RaycastHit[] coneHits = ConeCastAll(transform.position, radius, transform.forward, depth, angle, "Ghost");
        if (coneHits.Length > 0)
        {
            for (int i = 0; i < coneHits.Length; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(coneHits[i].point, .3f);
                Gizmos.DrawRay(transform.position, coneHits[i].point - transform.position);
            }
        }
    }
}