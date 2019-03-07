using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    private float timeElapsed;
    private bool levelComplete;

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
