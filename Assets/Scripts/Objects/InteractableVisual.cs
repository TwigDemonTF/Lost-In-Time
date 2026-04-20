using UnityEngine;
using UnityEngine.Tilemaps;

public class InteractableVisual : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private GameObject afterState;

    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;

    private void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    public void SetHighlight(bool state)
    {
        if (highlightObject != null)
            highlightObject.SetActive(state);
    }

    public void CompleteInteraction()
    {
        // Hide the tilemap renderer on this object
        if (tilemapRenderer != null) {
            tilemapRenderer.enabled = false;
            tilemapCollider.enabled = false;
        }
        
        // Hide highlight object
        if (highlightObject != null)
            highlightObject.SetActive(false);

        // Enable the child replacement object
        if (afterState != null)
            afterState.SetActive(true);
    }
}