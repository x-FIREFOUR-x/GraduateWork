using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenuUi;

    [SerializeField]
    private string mainMenuScene = "MainMenuScene";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPause();
        }
    }

    public void SwitchPause()
    {
        if (pauseMenuUi.activeSelf)
        {
            pauseMenuUi.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            pauseMenuUi.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        Destroy(MapSaver.instance.gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }
}
