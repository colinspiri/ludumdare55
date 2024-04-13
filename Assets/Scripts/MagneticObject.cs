using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticObject : MonoBehaviour
{
    public Polarity polarity;
    Vector2 playerPosition;
    [HideInInspector] public bool attractedToPlayer;
    public GameObject player;
    public GameObject cone;
    private Rigidbody2D rb;
    public float attractionSpeed;
    private bool isTouchingPlayer;
    private bool isTouchingCone;
    public float vibrateAmount;
    private Vector3 startingPosition;
    private Transform vibrateTarget;
    public bool vibrateEnabled;
    public float vibrateRange;
    Collider2D objectCol;
    Collider2D coneCol;
    float colliderDistance;
    private Vector3 currentPosition;


    // Start is called before the first frame update
    void Start()
    {
        attractedToPlayer = false;
        isTouchingPlayer = false;
        rb = GetComponent<Rigidbody2D>();
        vibrateTarget = GetComponent<Transform>();
        startingPosition = vibrateTarget.position;
        objectCol = GetComponent<Collider2D>();
        coneCol = cone.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attractedToPlayer && isTouchingPlayer == false)
        {
            // we could dotween to add an ease
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, attractionSpeed * Time.deltaTime);
        }

        colliderDistance = Physics2D.Distance(objectCol, coneCol).distance;

        if (colliderDistance < vibrateRange && !isTouchingCone && !attractedToPlayer && 
            !isTouchingPlayer && cone.GetComponent<MagnetCone>().GetConePolarity() == polarity)
        {
            var point = new Vector3(Random.Range(startingPosition.x - vibrateAmount, startingPosition.x + vibrateAmount),
                Random.Range(startingPosition.y - vibrateAmount, startingPosition.y + vibrateAmount), startingPosition.z);
            vibrateTarget.position = point;
        }
    }

    public void AttractToMagnet()
    {
        //Debug.Log(gameObject.name + " is affected by magnet");

        attractedToPlayer = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && attractedToPlayer)
        {
            isTouchingPlayer = true;

            transform.SetParent(other.transform);

            rb.isKinematic = true;
        }

        if (other.gameObject.CompareTag("Cone"))
        {
            isTouchingCone = true;
        }
    }

/*    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && attractedToPlayer)
        {
            isTouchingPlayer = true;

            transform.SetParent(other.transform);

            rb.isKinematic = true;
        }

        if (other.gameObject.CompareTag("Cone"))
        {
            isTouchingCone = true;
        }

*//*        if (other.gameObject.CompareTag("Cone Vibrate Collider"))
        {
            Debug.Log("collided");
            vibrateEnabled = true;
        }*//*
    }*/

/*    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cone Vibrate Collider"))
        {
            vibrateEnabled = false;
        }

*//*        if (collision.gameObject.CompareTag("Player") && attractedToPlayer)
        {
            isTouchingPlayer = false;

            //transform.SetParent(collision.transform);
            transform.position = transform.localPosition;
            transform.parent = null;

            rb.isKinematic = false;
        }

        if (collision.gameObject.CompareTag("Cone"))
        {
            isTouchingCone = false;
        }*//*
    }*/
}
