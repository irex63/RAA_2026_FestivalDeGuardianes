using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GazeInput : MonoBehaviour
{
    public string nextScene;

    // TIMER
    private bool isLooked = false;
    public float timerDuration = 3f;
    private float lookTimer = 0f;

    void Start()
    {
        // Disable screen dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Update()
    {
        if (isLooked)
        {
            lookTimer += Time.deltaTime;

            if (lookTimer > timerDuration)
            {
                lookTimer = 0f;
                Debug.Log("Object timer click");
                OnPointerClick();
            }
        }
        else
        {
            lookTimer = 0f;
        }
    }

    public void setIsLooked(bool looked)
    {
        isLooked = looked;
    }

    public void OnPointerClick()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsNames.hallowen);
        PlayerPrefs.DeleteKey(PlayerPrefsNames.easter);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nextScene); // el nombre real de tu escena
    }

    private void SaveDialoguePreference(int mode)
    {
        if (mode == PlayerPrefsNames.audio || mode == PlayerPrefsNames.text)
        {
            PlayerPrefs.SetInt(PlayerPrefsNames.dialogueKey, mode);
            PlayerPrefs.Save();
            Debug.Log("(PlayerPrefs) Preferencia guardada: " + mode);
        }
        else
        {
            Debug.LogWarning("(PlayerPrefs) Valor no válido: " + mode);
        }
    }

    public void OnAudioClick()
    {
        SaveDialoguePreference(PlayerPrefsNames.audio);
    }

    public void OnTextClick()
    {
        SaveDialoguePreference(PlayerPrefsNames.text);
    }
}