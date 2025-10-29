using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // load different scenes from main menu
    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadEasyMode()
    {
        PlayerPrefs.SetString("GameMode", "Easy");
        SceneManager.LoadScene("GameScene");
    }

    public void LoadHardMode()
    {
        PlayerPrefs.SetString("GameMode", "Hard");
        SceneManager.LoadScene("GameScene");
    }

    public void LoadCheatSheet()
    {
        SceneManager.LoadScene("CheatSheet");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game closed (won’t exit in editor).");
    }
}
