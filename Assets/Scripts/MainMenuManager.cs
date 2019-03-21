using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject levelCompletePanel;
    private bool cursorLocked;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            cursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Load a scene
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    // Toggle the "pause" menu
    public void ToggleMenu()
    {
        if (levelCompletePanel.activeSelf)
        {
            ContinuePlaying();
            return;
        }

        if (cursorLocked)
        {
            cursorLocked = false;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            cursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (menuPanel.activeSelf)
        {
            Time.timeScale = 1f;
            menuPanel.SetActive(false);
        }
        else
        {
            Time.timeScale = 0f;
            menuPanel.SetActive(true);
        }
    }

    public void RestartLevel()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void FinishLevel()
    {
        Time.timeScale = 0f;
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        levelCompletePanel.SetActive(true);
    }

    public void ContinuePlaying()
    {
        Time.timeScale = 1f;
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        levelCompletePanel.SetActive(false);
    }
}
