using System.Collections.Generic;
using UnityEngine;

public class HunterFlashlight : MonoBehaviour
{
    public float angle = 25;
    public float depth = 10;
    public float radius = 10;

    void Update()
    {
        RaycastHit[] coneHits = ConeCastAll(transform.position, radius, transform.forward, depth, angle, "Ghost");

        if (coneHits.Length > 0)
        {
            for (int i = 0; i < coneHits.Length; i++)
            {
                //do something with collider information
                coneHits[i].collider.GetComponent<PlayerManager>().FlashlightBanishment();
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
                    coneCastHitList.Add(sphereCastHits[i]);
                }
            }
        }

        RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
        coneCastHits = coneCastHitList.ToArray();

        return coneCastHits;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.forward * depth, radius);
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
            }
        }
    }
}