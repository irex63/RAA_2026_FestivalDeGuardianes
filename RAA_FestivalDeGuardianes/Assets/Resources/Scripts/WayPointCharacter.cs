using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class WayPointCharacter : MonoBehaviour
{
    public Transform target; // primer waypoint de la ruta
    public float speed = 2f;
    public float turnSpeed = 5f;
    public float gravity = -9.81f;
    public bool disappearAtEnd = true;
    private CharacterController controller;
    private float verticalVelocity = 0f;
    private bool finished = false;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    void Update()
    {
        if (finished || target == null) return;
        // mira hacia el objetivo, girando poco a poco
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
        // gravedad
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        // avanza hacia delante
        Vector3 velocity = transform.forward * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
    // al tocar el waypoint que perseguimos, pasamos al siguiente
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("WayPoint")) return;
        if (target == null || other.gameObject != target.gameObject) return;
        WayPoint wp = other.GetComponent<WayPoint>();
        if (wp != null && wp.nextWayPoint != null)
        {
            target = wp.nextWayPoint;
        }
        else
        {
            // fin de la ruta: el conejo se cuela en la madriguera
            finished = true;
            if (disappearAtEnd) gameObject.SetActive(false);
        }
    }
}