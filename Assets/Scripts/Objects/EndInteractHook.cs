using UnityEngine;

public class EndInteractHook : MonoBehaviour
{
    public EndTriggerManager manager;
    private bool hasTriggered = false;

    public void OnInteracted()
    {
        if (hasTriggered) return;

        hasTriggered = true;
        manager.RegisterInteraction();
    }
}