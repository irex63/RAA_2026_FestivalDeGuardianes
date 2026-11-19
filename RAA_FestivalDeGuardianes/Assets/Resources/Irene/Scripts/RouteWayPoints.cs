
using UnityEngine;

public class RouteWayPoints : MonoBehaviour
{
    [Header("Siguiente waypoint")]
    public Transform nextWayPoint;

    // Dibujar los waypoints y sus conexiones en Scene.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (nextWayPoint != null)
        {
            Gizmos.DrawLine(
                transform.position,
                nextWayPoint.position
            );
        }
    }
}
