using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Xuf.UI
{
    public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip Settings")]
        [SerializeField] private GameObject tooltipPrefab;
        [SerializeField] private Vector2 offset = new Vector2(40, 40);
        [SerializeField] private int sortingOrder = 100;

        [Header("Tooltip Content")]
        [SerializeField] private string tooltipText;

        // Each tooltip instance has its own panel
        private GameObject tooltipPanel;
        private TextMeshProUGUI tooltipTextComponent;
        private Canvas tooltipCanvas;

        private void Awake()
        {
            CreateTooltipPanel();
            // Hide tooltip initially
            HideTooltip();
        }

        private void CreateTooltipPanel()
        {
            // Check if we already have a tooltip panel as a child
            Transform existingPanel = transform.Find("TooltipPanel");

            // If we found an existing panel, use it
            if (existingPanel != null)
            {
                tooltipPanel = existingPanel.gameObject;
                tooltipTextComponent = tooltipPanel.GetComponentInChildren<TextMeshProUGUI>();
                tooltipCanvas = tooltipPanel.GetComponent<Canvas>();

                // Update the sorting order in case it changed
                if (tooltipCanvas != null)
                {
                    tooltipCanvas.sortingOrder = sortingOrder;
                }

                // Update position in case offset changed
                RectTransform tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
                if (tooltipRectTransform != null)
                {
                    tooltipRectTransform.localPosition = new Vector3(offset.x, offset.y, 0);
                }


                return;
            }

            // If we already have a tooltip panel reference but it's not a child, clean it up
            if (tooltipPanel != null)
            {
                DestroyImmediate(tooltipPanel);
            }

            // Use provided prefab if available
            if (tooltipPrefab != null)
            {
                tooltipPanel = Instantiate(tooltipPrefab);
                tooltipPanel.name = "TooltipPanel"; // Ensure consistent naming
                tooltipTextComponent = tooltipPanel.GetComponentInChildren<TextMeshProUGUI>();

                if (tooltipTextComponent == null)
                {
                    Debug.LogWarning("Tooltip prefab does not contain a TextMeshProUGUI component!");
                }
            }
            else
            {
                // Create default tooltip panel
                tooltipPanel = new GameObject("TooltipPanel");

                // Add required components
                RectTransform rectTransform = tooltipPanel.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(100, 40);

                // Add background image
                Image background = tooltipPanel.AddComponent<Image>();
                background.color = new Color(0, 0, 0, 0.8f);

                // Add text component
                GameObject textObj = new GameObject("TooltipText");
                textObj.transform.SetParent(tooltipPanel.transform, false);

                RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
                textRectTransform.anchorMin = new Vector2(0, 0);
                textRectTransform.anchorMax = new Vector2(1, 1);
                textRectTransform.offsetMin = new Vector2(5, 5);
                textRectTransform.offsetMax = new Vector2(-5, -5);

                tooltipTextComponent = textObj.AddComponent<TextMeshProUGUI>();
                tooltipTextComponent.alignment = TextAlignmentOptions.Center;
                tooltipTextComponent.fontSize = 14f;
            }

            // Set up the tooltip with its own canvas
            tooltipPanel.transform.SetParent(transform, false);

            // Add a canvas component to control sorting
            tooltipCanvas = tooltipPanel.AddComponent<Canvas>();
            tooltipCanvas.overrideSorting = true;
            tooltipCanvas.sortingOrder = sortingOrder;

            // Add required components for the canvas to work
            tooltipPanel.AddComponent<CanvasRenderer>();
            tooltipPanel.AddComponent<GraphicRaycaster>();

            // Just apply the position offset
            RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            tooltipRect.localPosition = new Vector3(offset.x, offset.y, 0);
        }

        // Method to set tooltip text dynamically
        public void SetTooltipText(string text)
        {
            tooltipText = text;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(tooltipText))
            {
                tooltipTextComponent.text = tooltipText;

                // Show tooltip (position is already set in CreateTooltipPanel)
                tooltipPanel.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (tooltipPanel != null)
            {
                Destroy(tooltipPanel);
                tooltipPanel = null;
            }
        }
    }
}