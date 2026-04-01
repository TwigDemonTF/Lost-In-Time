using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class DynamicKeybindMenu : MonoBehaviour
{
    [Header("References")]
    public InputActionAsset inputActions;
    public Transform contentRoot;

    [Header("Prefabs")]
    public GameObject rowPrefab;

    private Dictionary<string, string> currentBindings = new Dictionary<string, string>();

    void OnEnable()
    {
        GenerateMenu();
        LoadBindings();
    }

    void GenerateMenu()
    {
        // Clear old
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];

                    // Skip composites container ("2D Vector")
                    if (binding.isComposite) continue;

                    CreateRow(action, i);
                }
            }
        }
    }

    void CreateRow(InputAction action, int bindingIndex)
    {
        GameObject row = Instantiate(rowPrefab, contentRoot);

        TMP_Text actionText = row.transform.Find("ActionText").GetComponent<TMP_Text>();
        TMP_Text bindText = row.transform.Find("BindText").GetComponent<TMP_Text>();
        Button rebindButton = row.transform.Find("RebindButton").GetComponent<Button>();

        actionText.text = action.name;
        bindText.text = action.GetBindingDisplayString(bindingIndex);

        rebindButton.onClick.AddListener(() =>
        {
            StartRebind(action, bindingIndex, bindText);
        });
    }

    void StartRebind(InputAction action, int bindingIndex, TMP_Text bindText)
    {
        action.Disable();

        bindText.text = "Press a key...";

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(callback =>
            {
                action.Enable();
                callback.Dispose();

                // Prevent duplicates
                if (IsDuplicate(action, bindingIndex))
                {
                    action.RemoveBindingOverride(bindingIndex);
                    bindText.text = "Duplicate!";
                }
                else
                {
                    bindText.text = action.GetBindingDisplayString(bindingIndex);
                    SaveBindings();
                }
            })
            .OnCancel(callback =>
            {
                action.Enable();
                callback.Dispose();
                bindText.text = action.GetBindingDisplayString(bindingIndex);
            })
            .Start();
    }

    bool IsDuplicate(InputAction action, int bindingIndex)
    {
        string newPath = action.bindings[bindingIndex].effectivePath;

        foreach (var map in inputActions.actionMaps)
        {
            foreach (var act in map.actions)
            {
                foreach (var bind in act.bindings)
                {
                    if (bind.effectivePath == newPath && act != action)
                        return true;
                }
            }
        }

        return false;
    }

    void SaveBindings()
    {
        PlayerPrefs.SetString("rebinds", inputActions.SaveBindingOverridesAsJson());
    }

    void LoadBindings()
    {
        if (PlayerPrefs.HasKey("rebinds"))
        {
            inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds"));
        }
    }
}