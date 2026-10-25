using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private float leftEdge;
    private bool hasRegisteredHit;

    public bool TryRegisterHit()
    {
        if (hasRegisteredHit)
        {
            return false;
        }

        hasRegisteredHit = true;
        return true;
    }

    private void Start()
    {
        leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
    }

    private void Update()
    {
        transform.position += Vector3.left * GameManager.Instance.gameSpeed * Time.deltaTime;

        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }
}
