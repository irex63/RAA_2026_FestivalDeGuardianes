using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class LobbyMovement : MonoBehaviour
{
    public float speed = 2.5f; // velocidad de paseo (m/s)
    public float acceleration = 12f; // arranque y parada suaves
    public float minStepTime = 0.3f; // un toque breve dura al menos este tiempo
    public float gravity = -9.81f;
    public KeyCode forwardButton = KeyCode.JoystickButton0; // botón A del mando
    public bool useVisorButton = true; // el botón del visor también hace avanzar
    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private Vector3 stepDirection;
    private float stepTimer;
    private float verticalVelocity;
    private bool levelEnded;


    public string hallowenScene = "Halloween";
    public string easterScene = "Easter";
    public string endScene = "End";

    public AudioSource source;
    public AudioClip clip;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (levelEnded) return;
        // 1) entrada: stick izquierdo / flechas / WASD (ejes Horizontal y Vertical)
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (Input.GetKey(forwardButton) || (useVisorButton && Input.GetMouseButton(0)))
        {
            input.z = 1f;
        }
        input = Vector3.ClampMagnitude(input, 1f);
        // 2) dirección: relativa a hacia dónde mira la cabeza, sin componente vertical
        if (input.sqrMagnitude > 0.01f && Camera.main != null)
        {
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = Camera.main.transform.right;
            right.y = 0f;
            right.Normalize();
            stepDirection = (forward * input.z + right * input.x).normalized;
            stepTimer = minStepTime; // se renueva mientras se mantiene la entrada
        }
        else
        {
            stepTimer -= Time.deltaTime;
        }
        // 3) velocidad horizontal con aceleración suave
        Vector3 target = (stepTimer > 0f) ? stepDirection * speed : Vector3.zero;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, acceleration * Time.deltaTime);
        // 4) gravedad: el Character Controller no la incluye
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        // 5) una única llamada a Move por fotograma
        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
    }
    private void ReproducirError()
    {
        source.PlayOneShot(clip);
    }

    // --- Tags (const para poder usarlos en el switch) ---
    private const string hallowenTag = "HalloweenPortal";
    private const string easterTag = "EasterPortal";
    private const string endTag = "EndPortal";

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case easterTag:
                ChangeScene(PlayerPrefsNames.easter, easterScene);
                break;
            case hallowenTag:
                ChangeScene(PlayerPrefsNames.hallowen, hallowenScene);
                break;
            case endTag:
                if (CanPlayEndScene())
                    SceneManager.LoadScene(endScene);
                else
                    ReproducirError();
                break;
        }
    }

    private bool CanPlayEndScene()
    {
        bool hallowenDone = PlayerPrefs.GetInt(PlayerPrefsNames.hallowen, PlayerPrefsNames.noPlayed) == PlayerPrefsNames.played;
        bool easterDone = PlayerPrefs.GetInt(PlayerPrefsNames.easter, PlayerPrefsNames.noPlayed) == PlayerPrefsNames.played;
        return hallowenDone && easterDone;
    }

    private void SavePreference(string key, int mode)
    {
        if (mode == PlayerPrefsNames.noPlayed || mode == PlayerPrefsNames.played)
        {
            PlayerPrefs.SetInt(key, mode);
            PlayerPrefs.Save();
            Debug.Log("(PlayerPrefs) Preferencia guardada: " + key + " = " + mode);
        }
        else
        {
            Debug.LogWarning("(PlayerPrefs) Valor no válido: " + mode);
        }
    }

    private void ChangeScene(string levelPrefs, string nextScene)
    {
        //Miramos si ya se jugo
        int jugado = PlayerPrefs.GetInt(levelPrefs, PlayerPrefsNames.noPlayed);
        //Si NO se jugó
        if (jugado == PlayerPrefsNames.noPlayed)
        {
            //Cambiamos de escene según el tipo
            int tipo = PlayerPrefs.GetInt(PlayerPrefsNames.dialogueKey, PlayerPrefsNames.audio);

            SavePreference(levelPrefs, PlayerPrefsNames.played);
            if (tipo == PlayerPrefsNames.audio)
            {
                SceneManager.LoadScene(nextScene + "Audio");
            }
            else
            {
                SceneManager.LoadScene(nextScene + "Text");
            }
        }
        else
        {
            ReproducirError();
        }


    }
}