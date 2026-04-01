using UnityEngine;

public class FutureObject : MonoBehaviour
{
    private bool isActivated = false;

    public bool IsActivated => isActivated;

    public void Activate()
    {
        if (isActivated) return;

        isActivated = true;

        gameObject.SetActive(false);
        Debug.Log("FutureObject activated: " + gameObject.name);
    }
}