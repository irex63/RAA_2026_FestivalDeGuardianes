using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
// Va en el GameObject Cinematic (el que tiene el Playable Director)
public class CinematicGaby : MonoBehaviour
{
 public PlayableDirector director; // el Playable Director de Cinematic
 public string nextScene = "BaseLobby"; // escena que se carga al terminar
 private void OnEnable()
 {
 if (director != null)
 director.stopped += OnCinematicStopped;
 }
 private void OnDisable()
 {
 if (director != null)
 director.stopped -= OnCinematicStopped;
 }
 // Se llama sola cuando el Timeline llega al final
 private void OnCinematicStopped(PlayableDirector finished)
 {
 SceneManager.LoadScene(nextScene);
 }
}
