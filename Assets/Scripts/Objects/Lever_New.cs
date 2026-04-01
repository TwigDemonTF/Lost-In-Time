using UnityEngine;

using UnityEngine;

public class Lever_New : MonoBehaviour, IInteractable
{
    public bool isActivated = false;

    [Header("Target")]
    public GameObject target;

    public void Interact()
    {
        if (isActivated) return;

        isActivated = true;

        if (target != null)
        {
            bool newState = !target.activeSelf;
            target.SetActive(newState);

            Debug.Log($"Lever flipped {target.name} to {newState}");
        }
    }
}