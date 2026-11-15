using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void VictoryGame()
    {
        SceneManager.LoadScene("Victory");
    }
    public void DefeatGame()
    {
        SceneManager.LoadScene("Defeat");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}