using UnityEngine;

public class InteractableVisual : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private GameObject originObject;

    public void SetHighlight(bool state)
    {
        if (highlightObject != null)
            highlightObject.SetActive(state);
    }
}