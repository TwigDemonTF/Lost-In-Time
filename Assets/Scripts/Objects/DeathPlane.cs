using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    public Transform respawnPoint;
    [SerializeField] private GameObject deathPanel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        if (other.CompareTag("Player"))
        {
            other.transform.position = respawnPoint.position;
        }
    }
}