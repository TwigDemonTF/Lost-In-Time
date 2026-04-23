using UnityEngine;

public class EndTriggerManager : MonoBehaviour
{
    public int requiredInteractions = 2;
    private int currentInteractions = 0;

    [SerializeField] private GameObject endScreen;

    public void RegisterInteraction()
    {
        currentInteractions++;
        Debug.Log(currentInteractions);

        if (currentInteractions >= requiredInteractions)
        {
            TriggerEnd();
        }
    }

    private void TriggerEnd()
    {
        if (endScreen != null)
            endScreen.SetActive(true);

        Debug.Log("End triggered!");
    }
}