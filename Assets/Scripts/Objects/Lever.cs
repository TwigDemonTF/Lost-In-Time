using UnityEngine;

public class SimpleInteract : MonoBehaviour
{
    public GameObject[] enableObjects;
    public GameObject[] disableObjects;

    public void Interact()
    {
        foreach (var obj in enableObjects)
            obj.SetActive(true);

        foreach (var obj in disableObjects)
            obj.SetActive(false);
    }
}