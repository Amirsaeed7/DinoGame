using UnityEngine;

public class Coin : MonoBehaviour
{
    private static Sprite coinSprite;
    private float leftEdge;
    private bool hasBeenCollected;

    public static Coin Create(Vector3 position)
    {
        GameObject coinObject = new GameObject("Coin");
        coinObject.transform.position = position;

        SpriteRenderer spriteRenderer = coinObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetCoinSprite();
        spriteRenderer.sortingOrder = 2;

        SphereCollider coinCollider = coinObject.AddComponent<SphereCollider>();
        coinCollider.isTrigger = true;
        coinCollider.radius = 0.35f;

        return coinObject.AddComponent<Coin>();
    }

    public bool TryCollect()
    {
        if (hasBeenCollected)
        {
            return false;
        }

        hasBeenCollected = true;
        Destroy(gameObject);
        return true;
    }

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Cannot set the coin cleanup boundary because there is no Main Camera.", this);
            leftEdge = float.NegativeInfinity;
            return;
        }

        leftEdge = mainCamera.ScreenToWorldPoint(Vector3.zero).x;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        transform.position += Vector3.left * GameManager.Instance.gameSpeed * Time.deltaTime;

        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }

    private static Sprite GetCoinSprite()
    {
        if (coinSprite != null)
        {
            return coinSprite;
        }

        const int textureSize = 48;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color edgeColor = new Color(0.75f, 0.36f, 0.02f);
        Color faceColor = new Color(1f, 0.78f, 0.08f);
        Color highlightColor = new Color(1f, 0.94f, 0.43f);
        float center = (textureSize - 1) * 0.5f;
        float outerRadius = center - 1f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float offsetX = x - center;
                float offsetY = y - center;
                float distance = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);

                if (distance > outerRadius)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
                else if (distance > outerRadius * 0.78f)
                {
                    texture.SetPixel(x, y, edgeColor);
                }
                else if (offsetX < -outerRadius * 0.28f && offsetY > outerRadius * 0.2f)
                {
                    texture.SetPixel(x, y, highlightColor);
                }
                else
                {
                    texture.SetPixel(x, y, faceColor);
                }
            }
        }

        texture.Apply();
        coinSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            textureSize);
        return coinSprite;
    }
}
