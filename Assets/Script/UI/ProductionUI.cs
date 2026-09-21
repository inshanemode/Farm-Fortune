using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI for factory and processing facilities.
/// </summary>
public class ProductionUI : MonoBehaviour
{
    public static ProductionUI Instance { get; private set; }

    [SerializeField] private GameObject productionPanel;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (closeButton != null)
        {
            FarmUITheme.ApplyButtonStyle(closeButton, FarmUITheme.ButtonState.Warning);
            closeButton.onClick.AddListener(Hide);
        }
    }

    public void Toggle()
    {
        if (productionPanel == null) return;
        productionPanel.SetActive(!productionPanel.activeSelf);
    }

    public void Hide()
    {
        if (productionPanel != null) productionPanel.SetActive(false);
    }
}
