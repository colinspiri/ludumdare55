using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour {
    public float speed;

    void Update() {
        var player = PlayerController.Instance;
        if (player != null)
        {
            Vector2 direction = player.transform.position - transform.position;
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position,
                speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(Vector3.forward * angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject == PlayerController.Instance.gameObject)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
