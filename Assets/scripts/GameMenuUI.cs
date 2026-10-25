using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuUI : MonoBehaviour
{
    private GameObject mainMenuPanel;
    private GameObject pauseMenuPanel;

    public void Initialize(GameManager gameManager, Canvas canvas, TMP_FontAsset font)
    {
        mainMenuPanel = CreateMenuPanel(canvas.transform, "Main Menu", "DINO RUNNER", font, 42f);
        CreateButton(
            mainMenuPanel.transform,
            "Play Button",
            "Play",
            font,
            new Vector2(0f, -20f),
            gameManager.StartGameFromMenu);

        pauseMenuPanel = CreateMenuPanel(canvas.transform, "Pause Menu", "PAUSED", font, 38f);
        CreateButton(
            pauseMenuPanel.transform,
            "Resume Button",
            "Resume",
            font,
            new Vector2(0f, -20f),
            gameManager.ResumeGame);

        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        pauseMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void HideMenus()
    {
        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
    }

    private static GameObject CreateMenuPanel(
        Transform parent,
        string objectName,
        string title,
        TMP_FontAsset font,
        float titleFontSize)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.78f);
        background.raycastTarget = true;

        GameObject titleObject = new GameObject(
            "Title",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 95f);
        titleRect.sizeDelta = new Vector2(700f, 100f);

        TextMeshProUGUI titleText = titleObject.GetComponent<TextMeshProUGUI>();
        titleText.font = font;
        titleText.text = title;
        titleText.fontSize = titleFontSize;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.raycastTarget = false;

        return panel;
    }

    private static void CreateButton(
        Transform parent,
        string objectName,
        string label,
        TMP_FontAsset font,
        Vector2 position,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        buttonObject.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(240f, 64f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.85f, 0.55f, 0.12f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(onClick);

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = labelObject.GetComponent<TextMeshProUGUI>();
        buttonText.font = font;
        buttonText.text = label;
        buttonText.fontSize = 28f;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.raycastTarget = false;
    }
}
