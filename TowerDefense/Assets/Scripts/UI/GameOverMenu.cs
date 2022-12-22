using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    private bool isGameOver = false;

    [SerializeField]
    private GameObject endBuildingPrefab;
    [SerializeField]
    private GameObject gameOverMenuUI;

    void Update()
    {
        if (endBuildingPrefab.GetComponent<EndBuilding>().health <= 0)
        {
            gameOver();
        }
    }

    void gameOver()
    {
        isGameOver = true;

        gameOverMenuUI.SetActive(true);
    }

    public void GoToMenu()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Go to menu");
    }
}
