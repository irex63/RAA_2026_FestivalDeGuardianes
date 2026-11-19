using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Wander : MonoBehaviour
{
    public float wanderRadius = 15f; // radio del paseo alrededor del punto de partida
    public float minWait = 2f;
    public float maxWait = 5f;
    public float maxWalkTime = 20f; // seguridad: si no llega, desiste
    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 home;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        home = transform.position;
        StartCoroutine(GoWander());
    }

    // Corrutina: GoWander (retorna IEnumerator y contiene yield return)
    IEnumerator GoWander()
    {
        while (true)
        {
            Vector3 destination;
            if (RandomPoint(home, wanderRadius, out destination))
            {
                agent.SetDestination(destination);
                animator.SetBool("isWalking", true);
                // esperamos, fotograma a fotograma, a que llegue
                float timer = 0f;
                while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                && timer < maxWalkTime)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                animator.SetBool("isWalking", false);
            }
            // descansa un rato antes de elegir otro destino
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        }
    }

    // busca un punto aleatorio que esté sobre el NavMesh
    bool RandomPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 candidate = center + Random.insideUnitSphere * radius;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }
}

