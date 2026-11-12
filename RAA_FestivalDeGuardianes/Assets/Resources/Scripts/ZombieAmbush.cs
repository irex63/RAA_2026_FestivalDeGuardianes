using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAmbush : MonoBehaviour
{
    public GameObject zombie;
    public AudioSource scareSound;
    public float delay = 0.05f; // Delay casi nulo para que arranque de inmediato

    private Animator animator;
    private ZombieChase chase;
    private bool triggered;

    private void Start()
    {
        animator = zombie.GetComponent<Animator>();
        chase = zombie.GetComponent<ZombieChase>();

        animator.speed = 0f;
        animator.Update(0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggered)
        {
            StartCoroutine(AmbushWithoutSound());
            return;
        }

        triggered = true;
        StartCoroutine(Ambush());
    }

    private IEnumerator Ambush()
    {
        yield return new WaitForSeconds(delay);

        if (scareSound != null) scareSound.Play();
        animator.speed = 1f;

        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        chase.StartChasing();
    }

    private IEnumerator AmbushWithoutSound()
    {
        yield return new WaitForSeconds(delay);
        chase.StartChasing();
    }
}