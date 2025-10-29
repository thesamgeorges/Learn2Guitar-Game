using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatSheetMenu : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
