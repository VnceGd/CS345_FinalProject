using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Load a scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
