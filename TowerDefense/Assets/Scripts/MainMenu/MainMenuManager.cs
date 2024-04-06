using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private string defenderGameScene = "DefenderGameScene";
    [SerializeField]
    private string attackerGameScene = "AttackerGameScene";
    [SerializeField]
    private string mapConstrucorScene = "MapConstructorScene";

    public void PlayDefenderGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(defenderGameScene);
    }

    public void PlayAttackerGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(attackerGameScene);
    }

    public void Constructor()
    {
        SceneManager.LoadScene(mapConstrucorScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
