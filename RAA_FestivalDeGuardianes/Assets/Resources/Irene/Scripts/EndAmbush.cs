using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAmbush : MonoBehaviour
{
    public GameObject zombie;

    private ZombieChase chase;

    private void Start()
    {
        chase = zombie.GetComponent<ZombieChase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            chase.StopChasing();
        }
    }
}