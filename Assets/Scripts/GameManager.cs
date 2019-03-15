using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextMeshProUGUI timeText;
    private float timeElapsed;
    private bool levelComplete;

    // Start is called before the first frame update
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
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

    public void FinishLevel()
    {
        levelComplete = true;
    }
}
