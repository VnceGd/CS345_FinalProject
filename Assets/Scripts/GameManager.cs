using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI timeText;
    private float timeElapsed;
    public bool levelComplete;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!levelComplete)
        {
            timeElapsed += Time.deltaTime;
            timeText.text = timeElapsed.ToString("0.00");
        }
    }

    public bool FinishLevel()
    {
        bool previouslyCompleted = false;
        if (!levelComplete)
        {
            levelComplete = true;
            string levelName = SceneManager.GetActiveScene().name;
            RecordManager.instance.SetRecord(levelName, timeElapsed);
        }
        else
        {
            previouslyCompleted = true;
        }
        return previouslyCompleted;
    }
}
