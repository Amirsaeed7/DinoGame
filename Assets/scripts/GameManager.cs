using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float initialGameSpeed = 5f;

    public float gameSpeedIncrease = 0.1f;
    public float gameSpeed { get; private set; }

    private Player player;
    private Spawner spawner;

    public TextMeshProUGUI gameOverText;
    public Button retryButton;

    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI highScoreText;
    public GameObject highScoreLabel;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI coinCountText;

    public float score; 
    public int CoinsCollected { get; private set; }
    [Min(1)] public int maxLives = 3;
    [Min(0f)] public float hitSlowDuration = 2f;
    [Range(0.1f, 1f)] public float hitSlowSpeedMultiplier = 0.5f;
    [Min(1f)] public float dashSpeedMultiplier = 2f;

    public int LivesRemaining { get; private set; }

    public bool IsPaused { get; private set; }

    private float hitSlowTimeRemaining;
    private bool isDashing;
    private bool isEnding;
    private bool hasStartedGame;
    private GameMenuUI menus;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        spawner = FindFirstObjectByType<Spawner>();
        InitializeMenus();
        FindHighScoreLabel();
        EnsureLivesText();
        EnsureCoinCountText();
        EnsureRetryButtonListener();
        if (menus == null)
        {
            Debug.LogError("The main menu could not be created; starting the game instead.", this);
            NewGame();
            return;
        }

        ShowMainMenu();
    }

    public void NewGame()
    {
        Obstacle[] obstacles = FindObjectsByType<Obstacle>(FindObjectsSortMode.None);
        foreach (Obstacle obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }

        Coin[] coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        foreach (Coin coin in coins)
        {
            Destroy(coin.gameObject);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }

        gameSpeed = initialGameSpeed;
        hitSlowTimeRemaining = 0f;
        isDashing = false;
        isEnding = false;
        LivesRemaining = maxLives;
        UpdateLivesDisplay();
        CoinsCollected = 0;
        UpdateCoinCountDisplay();
        IsPaused = false;
        Time.timeScale = 1f;
        enabled = true;
        hasStartedGame = true;
        if (menus != null)
        {
            menus.HideMenus();
        }
        SetGameplayHudActive(true);
        player.gameObject.SetActive(true);
        spawner.gameObject.SetActive(true);
        UpdateHighScore();
        score = 0f;
    }

    public void StartGameFromMenu()
    {
        NewGame();
    }

    public void ResumeGame()
    {
        if (!hasStartedGame || isEnding)
        {
            return;
        }

        IsPaused = false;
        ApplyTimeScale();
        if (menus != null)
        {
            menus.HideMenus();
        }
    }

    public void GameOver()
    {
        hitSlowTimeRemaining = 0f;
        isDashing = false;
        isEnding = true;
        IsPaused = false;
        ApplyTimeScale();
        gameSpeed = 0f;
        enabled = false;
        player.gameObject.SetActive(false);
        spawner.gameObject.SetActive(false);
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(true);
        }
        if (menus != null)
        {
            menus.HideMenus();
        }
        SetGameplayHudActive(false);
        UpdateHighScore();
    }

    private void Update()
    {
        if (isEnding)
        {
            return;
        }

        bool dashPressed = player != null && player.IsDashPressed;
        if (isDashing != dashPressed)
        {
            isDashing = dashPressed;
            ApplyTimeScale();
        }

        Keyboard keyboard = Keyboard.current;
        if (hasStartedGame && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (!hasStartedGame || IsPaused)
        {
            return;
        }

        if (hitSlowTimeRemaining > 0f)
        {
            hitSlowTimeRemaining = Mathf.Max(0f, hitSlowTimeRemaining - Time.unscaledDeltaTime);
            if (hitSlowTimeRemaining == 0f)
            {
                ApplyTimeScale();
            }
        }

        gameSpeed += gameSpeedIncrease * Time.deltaTime;
        score += gameSpeed * Time.deltaTime;
        scoreText.text = Mathf.FloorToInt(score).ToString("D5");
    }

    public void PlayerHit()
    {
        if (isEnding || LivesRemaining <= 0)
        {
            return;
        }

        LivesRemaining--;
        UpdateLivesDisplay();

        if (LivesRemaining == 0)
        {
            isEnding = true;
            hitSlowTimeRemaining = 0f;
            isDashing = false;
            IsPaused = false;
            gameSpeed = 0f;
            spawner.gameObject.SetActive(false);
            ApplyTimeScale();

            if (player == null)
            {
                Debug.LogError("The player is missing; cannot play the death feedback.", this);
                GameOver();
                return;
            }

            player.BeginDeathSequence();
            return;
        }

        hitSlowTimeRemaining = hitSlowDuration;
        ApplyTimeScale();
    }

    public void CollectCoin()
    {
        CoinsCollected++;
        UpdateCoinCountDisplay();
    }

    public void TogglePause()
    {
        if (!hasStartedGame || isEnding)
        {
            return;
        }

        IsPaused = !IsPaused;
        ApplyTimeScale();
        if (menus != null)
        {
            if (IsPaused)
            {
                menus.ShowPauseMenu();
            }
            else
            {
                menus.HideMenus();
            }
        }
    }

    private void ShowMainMenu()
    {
        hasStartedGame = false;
        isEnding = false;
        IsPaused = false;
        hitSlowTimeRemaining = 0f;
        isDashing = false;
        gameSpeed = 0f;
        Time.timeScale = 1f;

        if (player != null)
        {
            player.gameObject.SetActive(false);
        }
        if (spawner != null)
        {
            spawner.gameObject.SetActive(false);
        }
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }

        SetGameplayHudActive(false);
        if (menus != null)
        {
            menus.ShowMainMenu();
        }
    }

    private void InitializeMenus()
    {
        Canvas canvas = FindMenuCanvas();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                "Game UI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }
        else if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        EnsureEventSystem();

        TMP_FontAsset font = scoreText != null ? scoreText.font : null;
        if (font == null && livesText != null)
        {
            font = livesText.font;
        }
        if (font == null && gameOverText != null)
        {
            font = gameOverText.font;
        }
        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }
        if (font == null)
        {
            Debug.LogError("Cannot create the game menus because no TextMeshPro font is available.", this);
            return;
        }

        menus = canvas.GetComponent<GameMenuUI>();
        if (menus == null)
        {
            menus = canvas.gameObject.AddComponent<GameMenuUI>();
        }
        menus.Initialize(this, canvas, font);
    }

    private Canvas FindMenuCanvas()
    {
        if (gameOverText != null && gameOverText.canvas != null)
        {
            return gameOverText.canvas;
        }
        if (scoreText != null && scoreText.canvas != null)
        {
            return scoreText.canvas;
        }
        if (highScoreText != null && highScoreText.canvas != null)
        {
            return highScoreText.canvas;
        }
        if (livesText != null && livesText.canvas != null)
        {
            return livesText.canvas;
        }
        if (coinCountText != null && coinCountText.canvas != null)
        {
            return coinCountText.canvas;
        }

        return FindFirstObjectByType<Canvas>();
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            return;
        }

        if (eventSystem.GetComponent<BaseInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void EnsureRetryButtonListener()
    {
        if (retryButton != null && retryButton.onClick.GetPersistentEventCount() == 0)
        {
            retryButton.onClick.AddListener(NewGame);
        }
    }

    private void SetGameplayHudActive(bool isActive)
    {
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(isActive);
        }
        if (highScoreText != null)
        {
            highScoreText.gameObject.SetActive(isActive);
        }
        if (livesText != null)
        {
            livesText.gameObject.SetActive(isActive);
        }
        if (coinCountText != null)
        {
            coinCountText.gameObject.SetActive(isActive);
        }
        if (highScoreLabel != null)
        {
            highScoreLabel.SetActive(isActive);
        }
    }

    private void FindHighScoreLabel()
    {
        if (highScoreLabel != null)
        {
            return;
        }

        Canvas canvas = FindMenuCanvas();
        if (canvas == null)
        {
            return;
        }

        foreach (TMP_Text label in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (string.Equals(label.text.Trim(), "HI", System.StringComparison.OrdinalIgnoreCase))
            {
                highScoreLabel = label.gameObject;
                return;
            }
        }

        foreach (UnityEngine.UI.Text label in canvas.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            if (string.Equals(label.text.Trim(), "HI", System.StringComparison.OrdinalIgnoreCase))
            {
                highScoreLabel = label.gameObject;
                return;
            }
        }
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = IsPaused
            ? 0f
            : (isDashing ? dashSpeedMultiplier : 1f)
                * (hitSlowTimeRemaining > 0f ? hitSlowSpeedMultiplier : 1f);
    }

    private void UpdateLivesDisplay()
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {LivesRemaining}";
        }
    }

    private void UpdateCoinCountDisplay()
    {
        if (coinCountText != null)
        {
            coinCountText.text = $"Coins: {CoinsCollected}";
        }
    }

    private void EnsureLivesText()
    {
        if (livesText != null)
        {
            return;
        }

        Canvas canvas = gameOverText != null ? gameOverText.canvas : null;
        if (canvas == null && scoreText != null)
        {
            canvas = scoreText.canvas;
        }

        if (canvas == null && highScoreText != null)
        {
            canvas = highScoreText.canvas;
        }

        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            Debug.LogError("Cannot create the lives label because the scene has no Canvas.", this);
            return;
        }

        GameObject livesObject = new GameObject(
            "Lives Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        livesObject.transform.SetParent(canvas.transform, false);
        livesText = livesObject.GetComponent<TextMeshProUGUI>();

        TMP_FontAsset font = scoreText != null ? scoreText.font : null;
        if (font == null && highScoreText != null)
        {
            font = highScoreText.font;
        }

        if (font == null && gameOverText != null)
        {
            font = gameOverText.font;
        }

        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        if (font == null)
        {
            Destroy(livesObject);
            livesText = null;
            Debug.LogError("Cannot create the lives label because no TextMeshPro font is available.", this);
            return;
        }

        livesText.font = font;
        livesText.fontSize = scoreText != null ? scoreText.fontSize : 28f;
        livesText.color = scoreText != null ? scoreText.color : Color.black;
        livesText.alignment = TextAlignmentOptions.TopLeft;
        livesText.raycastTarget = false;

        RectTransform rectTransform = livesText.rectTransform;
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(60f, -70f);
        rectTransform.sizeDelta = new Vector2(240f, 50f);
    }

    private void EnsureCoinCountText()
    {
        if (coinCountText != null || livesText == null)
        {
            return;
        }

        coinCountText = Instantiate(livesText, livesText.transform.parent);
        coinCountText.name = "Coins Text";
        coinCountText.rectTransform.anchoredPosition += Vector2.down * 45f;
        UpdateCoinCountDisplay();
    }

    private void UpdateHighScore()
    {
        int highScore = PlayerPrefs.GetInt("", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("", Mathf.FloorToInt(score));
            highScoreText.text = Mathf.FloorToInt(score).ToString("D5");
        }
        else
        {
            highScoreText.text = highScore.ToString("D5");
        }
    }
}
