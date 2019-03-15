using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject menuPanel;

    // Load a scene
    public void LoadScene(string sceneName)
    {
        if (GameManager.instance != null)
        {
            DontDestroyOnLoad(GameManager.instance);
        }
        SceneManager.LoadScene(sceneName);
    }

    // Toggle the "pause" menu
    public void ToggleMenu()
    {
        if (menuPanel.activeSelf)
        {
            menuPanel.SetActive(false);
        }
        else
        {
            menuPanel.SetActive(true);
        }
    }
}
