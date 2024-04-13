using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticObject : MonoBehaviour
{
    public Polarity polarity;
    Vector2 playerPosition;
    [HideInInspector] public bool attractedToPlayer;
    public GameObject player;
    private Rigidbody2D rb;
    public float attractionSpeed;
    private bool isTouchingPlayer;

    // Start is called before the first frame update
    void Start()
    {
        attractedToPlayer = false;
        isTouchingPlayer = false;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attractedToPlayer && isTouchingPlayer == false)
        {
            // we could dotween to add an ease
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, attractionSpeed * Time.deltaTime);
            //Debug.Log("AAAAAAHHAHHAHAHHA");
        }
    }

    public void AttractToMagnet()
    {
        //Debug.Log(gameObject.name + " is affected by magnet");

        attractedToPlayer = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = true;

            transform.SetParent(other.transform);

            rb.isKinematic = true;

        }
    }
}
