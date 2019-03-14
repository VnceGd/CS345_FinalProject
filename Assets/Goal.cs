using UnityEngine;

public class Goal : MonoBehaviour
{
    private GameManager gameManager;

    // Start is called before the first frame update
    private void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (gameManager)
            {
                gameManager.FinishLevel();
            }
        }
    }
}
