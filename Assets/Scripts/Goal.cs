using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameManager gameManager;

    // Start is called before the first frame update
    private void Start()
    {
        gameManager = GameManager.instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!gameManager.FinishLevel())
            {
                collision.gameObject.GetComponent<PlayerController>().FinishLevel();
            }
        }
    }
}
