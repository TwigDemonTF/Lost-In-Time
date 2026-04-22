using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonEvents : MonoBehaviour
{
    public GameObject keybindCanvas;
    public GameObject mainCanvas;
    public void GameScene()
    {
        SceneManager.LoadScene("Game");
    }

    public void OpenKeybindMenu()
    {
        mainCanvas.SetActive(false);
        keybindCanvas.SetActive(true);
    }
    public void CloseKeybindMenu()
    {
        mainCanvas.SetActive(true);
        keybindCanvas.SetActive(false);
    }
}