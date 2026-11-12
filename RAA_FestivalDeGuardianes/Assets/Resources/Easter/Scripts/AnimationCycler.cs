using System.Collections;
using UnityEngine;

public class AnimationCycler : MonoBehaviour
{
    private Animator animator;

    [Header("Configuración de Loops")]
    //Repeticiones de la primera
    [SerializeField] private int loopsAnimation1 = 2;
    //Repeticiones de la segunda
    [SerializeField] private int loopsAnimation2 = 1;
    

    [Header("Nombres de los Triggers o Estados")]
    [SerializeField] private string anim1Trigger = "PlayAnim1";
    [SerializeField] private string anim2Trigger = "PlayAnim2";

    [SerializeField] private float anim1Duration =4f;
    [SerializeField] private float anim2Duration = 4f;
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(AnimationSequenceRoutine());
    }

    private IEnumerator AnimationSequenceRoutine()
    {
        while (true)
        {   
            //Empezamos la animación uno
            animator.SetTrigger(anim1Trigger);

            //Esperamos a que acabe
            yield return new WaitForSeconds(anim1Duration * loopsAnimation1);

            //Empezamos la animación dos
            animator.SetTrigger(anim2Trigger);

            //Esperamos a que acabe
            yield return new WaitForSeconds(anim2Duration * loopsAnimation2);
        }
    }
}