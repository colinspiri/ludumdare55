using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public class MagneticObject : MonoBehaviour
{
    public Polarity polarity;
    [SerializeField] private SpriteRenderer polarityIcon;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;
    Vector2 playerPosition;
    [HideInInspector] public bool attractedToPlayer;
    private Rigidbody2D rb;
    public float attractionSpeed;
    public float attractionTime;
    private bool isTouchingPlayer;
    public bool isTouchingCone;
    public float vibrateAmount;
    public float repelDistance;
    public float repelTime;
    private Vector3 startingPosition;
    private Transform vibrateTarget;
    public float vibrateRange;
    public float cameraShakeMagnitudeOnCollision;
    Collider2D objectCol;
    Collider2D coneCol;
    float colliderDistance;
    private GameObject parent;
    private Vector3 positionOnCollision;

    Sequence attractSequence;
    Sequence repelSequence;


    // Start is called before the first frame update
    void Start()
    {
        attractedToPlayer = false;
        isTouchingPlayer = false;
        rb = GetComponent<Rigidbody2D>();
        vibrateTarget = GetComponent<Transform>();
        startingPosition = vibrateTarget.position;
        objectCol = GetComponent<Collider2D>();
        coneCol = MagnetCone.Instance.GetComponent<Collider2D>();
        parent = transform.parent.gameObject;
        attractSequence = DOTween.Sequence();
        repelSequence = DOTween.Sequence();

        UpdateSprite();
    }
    
    private void UpdateSprite() {
        polarityIcon.color = polarity switch {
            Polarity.Positive => positiveColor,
            Polarity.Negative => negativeColor,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    // Update is called once per frame
    void Update()
    {
/*        if (attractedToPlayer)
        {
            // we could dotween to add an ease
            transform.position = Vector2.MoveTowards(transform.position, PlayerController.Instance.transform.position, attractionSpeed * Time.deltaTime);
        }*/

        colliderDistance = Physics2D.Distance(objectCol, coneCol).distance;

        if (colliderDistance < vibrateRange && !isTouchingCone && !attractedToPlayer && 
            !isTouchingPlayer && MagnetCone.Instance.GetConePolarity() == polarity)
        {
            var point = new Vector3(Random.Range(startingPosition.x - vibrateAmount, startingPosition.x + vibrateAmount),
                Random.Range(startingPosition.y - vibrateAmount, startingPosition.y + vibrateAmount), startingPosition.z);
            vibrateTarget.position = point;
        }

        if ((isTouchingCone) && MagnetCone.Instance.GetConePolarity() != polarity)
        {
            RepelObject();
        }
    }

    public void AttractToMagnet()
    {
        //Debug.Log(transform.position);
        //Debug.Log(gameObject.name + " is affected by magnet");

        attractedToPlayer = true;

        if (repelSequence.IsPlaying()) repelSequence.Kill();

        StartCoroutine(AttractCoroutine());

/*        Tweener attractionTween = transform.DOMove(PlayerController.Instance.transform.position, attractionTime);

        attractionTween.OnUpdate(() => {
            if (!isTouchingPlayer)
            {
                //Debug.Log("is updating");
                attractionTween.ChangeEndValue(PlayerController.Instance.transform.position, attractionTween.Duration() - attractionTween.Elapsed());
                //Debug.Log(PlayerController.Instance.transform.position);
            }
        });*/
        //attractSequence.Append(attractionTween);
    }

    private void RepelObject()
    {
        //Debug.Log("time to repel");
        if (attractSequence.IsPlaying()) attractSequence.Kill();
        var point = PlayerController.Instance.transform.position + PlayerController.Instance.transform.right * repelDistance;

        Vector3 vec = new Vector3(point.x, point.y, transform.position.z);

        //transform.position = Vector2.MoveTowards(transform.position, point, attractionSpeed * Time.deltaTime);

        isTouchingCone = false;
        isTouchingPlayer = false;

        transform.SetParent(parent.transform);

        repelSequence.Append(transform.DOMove(vec, repelTime));

        startingPosition = vec;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && attractedToPlayer)
        {
            Debug.Log("colliding");
            transform.SetParent(other.transform);

            isTouchingPlayer = true;
            
            CameraShake.Instance.Shake(cameraShakeMagnitudeOnCollision);


            rb.constraints = RigidbodyConstraints2D.FreezePosition;

            //rb.isKinematic = true;

            attractedToPlayer = false;

            positionOnCollision = transform.position;
        }
    }

    private IEnumerator AttractCoroutine()
    {
        float timeGoneBy = 0.0f;

        while (timeGoneBy < attractionTime && !isTouchingPlayer)
        {
            transform.position = Vector3.Lerp(transform.position, PlayerController.Instance.transform.position, timeGoneBy / attractionTime);
            timeGoneBy += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        transform.position = positionOnCollision;
    }
}
