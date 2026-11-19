using UnityEngine;
public class WayPoint : MonoBehaviour
{
    public Transform nextWayPoint;
    // dibuja la ruta en la vista Scene para poder verla mientras diseñas
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        if (nextWayPoint != null)
        {
            Gizmos.DrawLine(transform.position, nextWayPoint.position);
        }
    }
}
