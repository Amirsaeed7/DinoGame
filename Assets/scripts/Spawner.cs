using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnableObject
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance;
    }

    public SpawnableObject[] objects;

    public float minSpawnRte = 1f;
    public float maxSpawnRate = 2f;

    [Range(0f, 1f)] public float coinSpawnChance = 0.15f;
    public float coinHeightOffset = 1f;

    private void OnEnable()
    {
        Invoke(nameof(Spawn), Random.Range(minSpawnRte, maxSpawnRate));
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Spawn()
    {
        if (Random.value < coinSpawnChance)
        {
            Coin.Create(transform.position + Vector3.up * coinHeightOffset);
            Invoke(nameof(Spawn), Random.Range(minSpawnRte, maxSpawnRate));
            return;
        }

        float spawnChance = Random.value;

        foreach (var obj in objects)
        {
            if (spawnChance <= obj.spawnChance)
            {
                GameObject obstacle =  Instantiate(obj.prefab);
                obstacle.transform.position += transform.position;
                break;
            }

            spawnChance -= obj.spawnChance;
        }

        Invoke(nameof(Spawn), Random.Range(minSpawnRte, maxSpawnRate));
    }
}
