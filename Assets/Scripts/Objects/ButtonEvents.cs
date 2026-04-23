using UnityEngine;
using UnityEngine.SceneManagement;

public class MainButtonEvents : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel;

    public void CloseDeathScreen()
    {
        deathPanel.SetActive(false);
    }
}