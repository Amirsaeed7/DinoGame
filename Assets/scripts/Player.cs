using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private CharacterController character;
    private SpriteRenderer spriteRenderer;
    private Vector3 direction;
    private Color originalColor;
    private Coroutine hitFeedbackCoroutine;
    private bool isDying;

    public float gravity = 9.81f * 2f;

    public float jumpForce = 8f;
    public float hitFeedbackDuration = 0.2f;
    public Color hitColor = Color.red;
    [Min(1)] public int deathBlinkCount = 6;
    [Min(0.01f)] public float deathBlinkInterval = 0.08f;

    public bool IsDashPressed =>
        Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

    private void Awake()
    {
        character = GetComponent<CharacterController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        direction = Vector3.zero;
        isDying = false;
    }

    private void OnDisable()
    {
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
            hitFeedbackCoroutine = null;
        }

        spriteRenderer.color = originalColor;
    }

    private void Update()
    {
        if (isDying || (GameManager.Instance != null && GameManager.Instance.IsPaused))
        {
            return;
        }

        bool jumpHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        direction += Vector3.down * gravity * Time.deltaTime;
        if (character.isGrounded)
        {
            direction = Vector3.down;

            if (jumpHeld)
            {
                direction = Vector3.up * jumpForce;
            }
        }

        character.Move(direction * Time.deltaTime);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDying)
        {
            return;
        }

        Coin coin = other.GetComponentInParent<Coin>();
        if (coin != null)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("A coin was touched by the player, but no GameManager is active.", this);
                return;
            }

            if (coin.TryCollect())
            {
                GameManager.Instance.CollectCoin();
            }

            return;
        }

        Obstacle obstacle = other.GetComponentInParent<Obstacle>();
        if (obstacle == null || !obstacle.TryRegisterHit())
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("An obstacle hit the player, but no GameManager is active.", this);
            return;
        }

        PlayHitFeedback();
        GameManager.Instance.PlayerHit();
    }

    public void BeginDeathSequence()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
        }

        hitFeedbackCoroutine = StartCoroutine(ShowDeathFeedback());
    }

    private void PlayHitFeedback()
    {
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
        }

        hitFeedbackCoroutine = StartCoroutine(ShowHitFeedback());
    }

    private IEnumerator ShowHitFeedback()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFeedbackDuration);
        spriteRenderer.color = originalColor;
        hitFeedbackCoroutine = null;
    }

    private IEnumerator ShowDeathFeedback()
    {
        for (int i = 0; i < deathBlinkCount; i++)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSecondsRealtime(deathBlinkInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSecondsRealtime(deathBlinkInterval);
        }

        hitFeedbackCoroutine = null;
        GameManager.Instance.GameOver();
    }
}
