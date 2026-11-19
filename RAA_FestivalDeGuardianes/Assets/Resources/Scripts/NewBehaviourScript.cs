using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    void Start()
    {
        GameObject[] objetos = FindObjectsOfType<GameObject>(true);

        foreach (GameObject obj in objetos)
        {
            Component[] componentes = obj.GetComponents<Component>();

            foreach (Component componente in componentes)
            {
                if (componente == null)
                {
                    Debug.LogError(
                        "SCRIPT PERDIDO EN: " + GetPath(obj.transform),
                        obj
                    );
                }
            }
        }
    }

    string GetPath(Transform t)
    {
        string path = t.name;

        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }

        return path;
    }
}
