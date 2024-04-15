using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding;
using ScriptableObjectArchitecture;

public class Enemy : MonoBehaviour {
    [SerializeField] private GameObjectCollection allEnemies;
    public float speed;
    public ParticleSystem deathParticles;
    private AIPath path;
    public float pathFindingDistance;

    private void Awake() {
        allEnemies.Add(gameObject);
    }

    private void Start()
    {
        path = GetComponent<AIPath>();
    }

    void Update()
    {
        var player = PlayerController.Instance;
        if (player != null && Vector2.Distance(gameObject.transform.position, player.gameObject.transform.position) < pathFindingDistance)
        {
/*            Vector2 direction = player.transform.position - transform.position;
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position,
                speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(Vector3.forward * angle);*/

            path.maxSpeed = speed;
            path.destination = player.transform.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            var magneticObject = other.gameObject.GetComponent<MagneticObject>();

            if (magneticObject != null)
            {
                if (magneticObject.Moving) {
                    Die();
                    Hitstop.Instance.DoHitstop();
                }
            }
        }
    }

    private void Die() {
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySplat();
        Destroy(gameObject);
    }

    private void OnDestroy() {
        allEnemies.Remove(gameObject);
    }
}