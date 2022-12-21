using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private bool isGameOver = false;

    [SerializeField]
    private GameObject endBuildingPrefab;
    [SerializeField]
    private GameObject gameOverUI;

    void Update()
    {
        if(endBuildingPrefab.GetComponent<EndBuilding>().health <= 0)
        {
            gameOver();
        }
    }

    void gameOver()
    {
        isGameOver = true;

        gameOverUI.SetActive(true);
    }
}
