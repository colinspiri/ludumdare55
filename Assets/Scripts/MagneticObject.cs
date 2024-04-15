using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine.UIElements;
using Sequence = DG.Tweening.Sequence;

public class MagneticObject : MonoBehaviour
{
    public Polarity polarity;
    [SerializeField] private SpriteRenderer polarityIcon;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;
    Vector2 playerPosition;
    // [HideInInspector] public bool attractedToPlayer;
    private Rigidbody2D rb;
    public float attractionTime;
    // private bool isTouchingPlayer;
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

    [SerializeField] private AudioSource sfx_vibrate;
    [SerializeField] private AudioSource sfx_whoosh;

    Sequence repelSequence;

    Coroutine attractCoroutine;
    Coroutine repelCoroutine;
    
    private enum MagneticState { OnPlayer, Attracted, Repelled, None }
    private MagneticState _state;
    public bool Moving => _state is MagneticState.Attracted or MagneticState.Repelled;


    // Start is called before the first frame update
    void Start()
    {
        // attractedToPlayer = false;
        // isTouchingPlayer = false;
        _state = MagneticState.None;
        rb = GetComponent<Rigidbody2D>();
        vibrateTarget = GetComponent<Transform>();
        startingPosition = vibrateTarget.position;
        objectCol = GetComponent<Collider2D>();
        coneCol = MagnetCone.Instance.GetComponent<Collider2D>();
        parent = transform.parent.gameObject;
        repelSequence = DOTween.Sequence();

        Physics2D.IgnoreLayerCollision(6, 7, true);

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

        if (colliderDistance < vibrateRange && !isTouchingCone && _state == MagneticState.None && MagnetCone.Instance.GetConePolarity() == polarity)
        {
            var point = new Vector3(Random.Range(startingPosition.x - vibrateAmount, startingPosition.x + vibrateAmount),
                Random.Range(startingPosition.y - vibrateAmount, startingPosition.y + vibrateAmount), startingPosition.z);
            vibrateTarget.position = point;
            
            if(!sfx_vibrate.isPlaying) sfx_vibrate.Play();
        }
        else if(sfx_vibrate.isPlaying) sfx_vibrate.Stop();

        if ((isTouchingCone) && MagnetCone.Instance.GetConePolarity() != polarity)
        {
            RepelObject();
        }
    }

    private void ChangeState(MagneticState newState)
    {
        if (newState == _state) return;
        _state = newState;
    }

    public void AttractToMagnet()
    {
        // attractedToPlayer = true;
        if (repelSequence.IsPlaying()) repelSequence.Kill();
        
        ChangeState(MagneticState.Attracted);

        Physics2D.IgnoreLayerCollision(6, 7, false);

        attractCoroutine = StartCoroutine(AttractCoroutine());
        
        sfx_whoosh.Play();
    }

    private void RepelObject()
    {
        Physics2D.IgnoreLayerCollision(6, 7, false);
        repelCoroutine = StartCoroutine(RepelCoroutine());
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && _state == MagneticState.Attracted)
        {
            transform.SetParent(other.transform);
            HitPlayer();
        }

        if (other.gameObject.CompareTag("Wall"))
        {
            if (Moving)
            {
                // stop coroutine
                if (attractCoroutine != null) StopCoroutine(attractCoroutine);

                if (repelCoroutine != null) StopCoroutine(repelCoroutine);
                HitWall();
            }

            // have object bounce off a bit
        }
    }

    private void HitPlayer() {
        ChangeState(MagneticState.OnPlayer);
        // isTouchingPlayer = true;
        // attractedToPlayer = false;

        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        //rb.isKinematic = true;
        positionOnCollision = transform.position;
        
        CameraShake.Instance.Shake(cameraShakeMagnitudeOnCollision);
        AudioManager.Instance.PlayHitMagnet();
    }

    private void HitWall()
    {
        ChangeState(MagneticState.None);
        AudioManager.Instance.PlayHitMagnet();
    }

    private IEnumerator AttractCoroutine()
    {
        float timeGoneBy = 0.0f;

        while (timeGoneBy < attractionTime && _state == MagneticState.Attracted)
        {
            transform.position = Vector3.Lerp(transform.position, PlayerController.Instance.transform.position, timeGoneBy / attractionTime);
            timeGoneBy += Time.deltaTime;
            yield return null;
        }

        transform.position = positionOnCollision;
        ChangeState(MagneticState.OnPlayer);
    }
    
    private IEnumerator RepelCoroutine()
    {
        var point = PlayerController.Instance.transform.position + PlayerController.Instance.transform.right * repelDistance;
        Vector3 targetPosition = new Vector3(point.x, point.y, transform.position.z);
        startingPosition = targetPosition;
        
        isTouchingCone = false;
        ChangeState(MagneticState.Repelled);

        transform.SetParent(parent.transform);
        
        sfx_whoosh.Play();
        
        float timeGoneBy = 0.0f;

        while (timeGoneBy < repelTime && _state == MagneticState.Repelled)
        {
            if (Time.timeScale != 0) {
                timeGoneBy += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPosition, timeGoneBy / repelTime);
            }
            yield return null;
        }
        
        ChangeState(MagneticState.None);
        Physics2D.IgnoreLayerCollision(6, 7, true);

        //rb.isKinematic = false;
    }
}
