using Assets.Scripts.Layouting.Labels;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles label interactability based on button interactability.
/// </summary>
internal sealed class ButtonLabelHighlighter : LabelHighlighter
{
    private Button button;

    private void Start()
    {
        if (button == null)
        {
            button = GetComponentInParent<Button>();

            if (button == null)
            {
                Debug.LogWarning($"[{nameof(ButtonLabelHighlighter)}] Required button component not found! Disabling script");
                gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (button.interactable != IsEnabled)
        {
            IsEnabled = button.interactable;
        }
    }
}
