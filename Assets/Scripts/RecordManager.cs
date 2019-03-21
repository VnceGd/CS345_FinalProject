using UnityEngine;
using TMPro;

public class RecordManager : MonoBehaviour
{
    public static RecordManager instance;

    public TextMeshProUGUI displayText;
    private float forestTime;
    private float desertTime;
    private float lavaTime;
    private float voidTime;

    // Start is called before the first frame update
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        forestTime = PlayerPrefs.GetFloat("Forest", 0);
        desertTime = PlayerPrefs.GetFloat("Desert", 0);
        lavaTime = PlayerPrefs.GetFloat("Lava", 0);
        voidTime = PlayerPrefs.GetFloat("Void", 0);
    }

    public void DisplayRecords()
    {
        displayText.text = "Forest: " + forestTime.ToString("0.00") + "s\n" +
                           "Desert: " + desertTime.ToString("0.00") + "s\n" +
                           "Lava: " + lavaTime.ToString("0.00") + "s\n" +
                           "Void: " + voidTime.ToString("0.00") + "s\n";
    }

    public void SetRecord(string level, float time)
    {
        PlayerPrefs.SetFloat(level, time);
    }
}
