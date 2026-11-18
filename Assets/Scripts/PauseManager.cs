using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseButtom;

    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseButtom.SetActive(false);
        pauseMenu.SetActive(true);
        AudioManager.Instance?.SetPaused(true);   // snapshot "Pausa"
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseButtom.SetActive(true);
        pauseMenu.SetActive(false);
        AudioManager.Instance?.SetPaused(false);  // vuelve a "Gameplay"
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
}