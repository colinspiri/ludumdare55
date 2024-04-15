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
    // components
    [Header("Polarity")]
    [SerializeField] private SpriteRenderer polarityIcon;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;
    [Header("Audio")]
    [SerializeField] private AudioSource sfx_vibrate;
    [SerializeField] private AudioSource sfx_whoosh;
    
    // serialized parameters
    [Header("Params")]
    public Polarity polarity;
    [SerializeField] private float vibrateAmount;
    [SerializeField] private float vibrateRange;
    [SerializeField] private float attractionTime;
    [SerializeField] private float repelDistance;
    [SerializeField] private float repelTime;
    [SerializeField] private float cameraShakeMagnitudeOnCollision;

    // state
    private Rigidbody2D _rb;
    private Vector3 _vibrateOrigin;
    private Collider2D _objectCol;
    private Collider2D _coneCol;
    private float _colliderDistance;
    private GameObject _initialParent;
    private Vector3 _attractFinalPosition;
    private Coroutine _attractCoroutine;
    private Coroutine _repelCoroutine;
    private Vector3 _onPlayerOffset;
    
    private enum MagneticState { None, Attracted, Repelled, OnPlayer, OnWall }
    private MagneticState _state;
    public bool Moving => _state is MagneticState.Attracted or MagneticState.Repelled;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _state = MagneticState.None;
        _rb = GetComponent<Rigidbody2D>();
        _vibrateOrigin = transform.position;
        _objectCol = GetComponent<Collider2D>();
        _coneCol = MagnetCone.Instance.GetComponent<Collider2D>();
        _initialParent = transform.parent.gameObject;

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
        CheckVibration();
    }

    private void CheckVibration() {
        _colliderDistance = Physics2D.Distance(_objectCol, _coneCol).distance;

        if (_state == MagneticState.None && _colliderDistance < vibrateRange && MagnetCone.Instance.GetConePolarity() == polarity)
        {
            var point = new Vector3(Random.Range(_vibrateOrigin.x - vibrateAmount, _vibrateOrigin.x + vibrateAmount),
                Random.Range(_vibrateOrigin.y - vibrateAmount, _vibrateOrigin.y + vibrateAmount), _vibrateOrigin.z);
            transform.position = point;
            
            if(!sfx_vibrate.isPlaying) sfx_vibrate.Play();
        }
        else if(sfx_vibrate.isPlaying) sfx_vibrate.Stop();
    }

    private void ChangeState(MagneticState newState)
    {
        if (newState == _state) return;
        var oldState = _state;
        _state = newState;

        if (_state == MagneticState.None) {
            _vibrateOrigin = transform.position;
        }
        else if (_state == MagneticState.OnPlayer) {
            transform.SetParent(PlayerController.Instance.transform);
        }

        if (oldState == MagneticState.OnPlayer) {
            transform.SetParent(_initialParent.transform);
        }

        Debug.Log(gameObject.name + " state = " + _state);
    }

    public void InsideMagnetCone(Polarity conePolarity) {
        if (_state == MagneticState.None) {
            if(polarity == conePolarity) Attract();
            else Repel();
        }
        else if (_state == MagneticState.Attracted) {
            if(polarity != conePolarity) Repel();
        }
        else if (_state == MagneticState.Repelled) {
            if(polarity == conePolarity) Attract();
        }
        else if (_state == MagneticState.OnPlayer) {
            if(polarity != conePolarity) Repel();
        }
        else if (_state == MagneticState.OnWall) {
            if(polarity == conePolarity) Attract();
        }
    }

    private void Attract()
    {
        StopMoving();
        _attractCoroutine = StartCoroutine(AttractCoroutine());
    }

    private void Repel()
    {
        StopMoving();
        _repelCoroutine = StartCoroutine(RepelCoroutine());
    }

    private void StopMoving() {
        if (_attractCoroutine != null) StopCoroutine(_attractCoroutine);
        if (_repelCoroutine != null) StopCoroutine(_repelCoroutine);
        Physics2D.IgnoreLayerCollision(6, 7, true);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && _state == MagneticState.Attracted)
        {
            StopMoving();
            HitPlayer();
        }
        
        if (other.gameObject.CompareTag("Wall") && Moving)
        {
            StopMoving();
            HitWall();
            // have object bounce off a bit
        }
    }

    private void HitPlayer() {
        ChangeState(MagneticState.OnPlayer);

        _rb.constraints = RigidbodyConstraints2D.FreezePosition;
        _attractFinalPosition = transform.position;
        
        CameraShake.Instance.Shake(cameraShakeMagnitudeOnCollision);
        AudioManager.Instance.PlayMetalHitPlayer();
    }

    private void HitWall()
    {
        ChangeState(MagneticState.OnWall);
        AudioManager.Instance.PlayMetalHitWall();
    }

    private IEnumerator AttractCoroutine()
    {
        ChangeState(MagneticState.Attracted);
        
        sfx_whoosh.Play();

        Physics2D.IgnoreLayerCollision(6, 7, false);

        var targetPosition = PlayerController.Instance.transform.position;
        var startingPosition = transform.position;
        var timeGoneBy = 0.0f;
        while (timeGoneBy < attractionTime && _state == MagneticState.Attracted)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, timeGoneBy / attractionTime);
            timeGoneBy += Time.deltaTime;
            yield return null;
        }
        
        // collides with player and state becomes OnPlayer
        transform.position = _attractFinalPosition;
        
        Physics2D.IgnoreLayerCollision(6, 7, true);
    }
    
    private IEnumerator RepelCoroutine()
    {
        ChangeState(MagneticState.Repelled);
        
        sfx_whoosh.Play();

        Physics2D.IgnoreLayerCollision(6, 7, false);

        var targetPosition = PlayerController.Instance.transform.position + PlayerController.Instance.transform.right * repelDistance;
        var startingPosition = transform.position;
        var timeGoneBy = 0.0f;
        while (timeGoneBy < repelTime && _state == MagneticState.Repelled)
        {
            if (Time.timeScale != 0) {
                timeGoneBy += Time.deltaTime;
                transform.position = Vector3.Lerp(startingPosition, targetPosition, timeGoneBy / repelTime);
            }
            yield return null;
        }
        transform.position = targetPosition;

        Physics2D.IgnoreLayerCollision(6, 7, true);

        ChangeState(MagneticState.None);
    }
}
