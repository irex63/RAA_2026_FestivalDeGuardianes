using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieChase : MonoBehaviour
{
    public float attackDistance = 1.2f;
    public float originReachDistance = 0.5f;
    public float moveSpeed = 3.5f;
    public bool reloadOnCatch = true;
    public float reloadDelay = 1.5f;
    public float settleTime = 0.5f;      // tiempo en colocarse y girar al origen
    public float repathInterval = 0.25f;
    public HalloweenMovement playerMovement;

    // NO lo pongas como hijo del zombi, o se moverá con él
    public Transform origin;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    private Vector3 originPos;
    private Quaternion originRot;
    private Vector3 originNavPos;        // punto del NavMesh más cercano al origen

    private bool chasing, caught, end;
    private float nextRepath;
    private Coroutine settleRoutine;

    private static readonly int ChasingHash = Animator.StringToHash("Chasing");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.enabled = false;

        // Si no hay origen asignado, usa donde empieza el zombi
        originPos = origin != null ? origin.position : transform.position;
        originRot = origin != null ? origin.rotation : transform.rotation;

        originNavPos = NavMesh.SamplePosition(originPos, out NavMeshHit h, 5f, NavMesh.AllAreas)
            ? h.position : originPos;
    }

    public void StartChasing()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null) return;
        player = p.transform;

        if (settleRoutine != null) { StopCoroutine(settleRoutine); settleRoutine = null; }

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Zombi sin NavMesh cerca: revisa el bake.");
            return;
        }

        if (!agent.enabled)
        {
            agent.enabled = true;
            agent.Warp(hit.position);
        }

        agent.stoppingDistance = 0f;
        agent.speed = moveSpeed;
        agent.isStopped = false;

        chasing = true;
        end = false;
        caught = false;
        animator.SetBool(ChasingHash, true);
    }

    public void StopChasing()
    {
        if (!chasing || end) return;

        end = true;
        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(originNavPos);
        animator.SetBool(ChasingHash, true);
    }

    private void Update()
    {
        if (!chasing || caught) return;

        if (end)
        {
            Vector3 diff = originNavPos - transform.position;
            diff.y = 0f;

            if (diff.magnitude <= originReachDistance)
                ArriveAtOrigin();
            return;
        }

        if (player == null) return;

        if (Time.time >= nextRepath)
        {
            nextRepath = Time.time + repathInterval;
            agent.SetDestination(player.position);
        }

        Vector3 d = player.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude <= attackDistance * attackDistance)
        {
            caught = true;
            agent.isStopped = true;
            animator.SetBool(ChasingHash, false);
            if (playerMovement != null) playerMovement.frozen = true;
            if (reloadOnCatch) StartCoroutine(Reload());
        }
    }

    private void ArriveAtOrigin()
    {
        chasing = false;
        end = false;

        agent.ResetPath();
        agent.isStopped = true;
        agent.enabled = false;

        animator.SetBool(ChasingHash, false);   // pasa a Idle
        settleRoutine = StartCoroutine(SettleAtOrigin());
    }

    private IEnumerator SettleAtOrigin()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / settleTime;
            float k = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, originPos, k);
            transform.rotation = Quaternion.Slerp(startRot, originRot, k);
            yield return null;
        }

        transform.SetPositionAndRotation(originPos, originRot);
        settleRoutine = null;
    }

    private IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}