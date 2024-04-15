using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    
    [SerializeField]
    private float spawnInterval = 2.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(spawnEnemy(spawnInterval, enemyPrefab));
    }

    private IEnumerator spawnEnemy(float interval, GameObject enemy)
    {
        yield return new WaitForSeconds(interval);
        
        GameObject newEnemy = Instantiate(enemy, new Vector3(Random.Range(-16f, 16), (6f), 0),
            Quaternion.identity);
        
        GameObject anotherEnemy = Instantiate(enemy, new Vector3(Random.Range(-16f, 16), (-6f), 0),
            Quaternion.identity);

        StartCoroutine(spawnEnemy(interval, enemy));
    }
}