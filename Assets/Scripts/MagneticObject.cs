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
    private Vector3 _startingPosition;
    private Transform _vibrateTarget;
    private Collider2D _objectCol;
    private Collider2D _coneCol;
    private float _colliderDistance;
    private GameObject _parent;
    private Vector3 _attractFinalPosition;
    private Coroutine _attractCoroutine;
    private Coroutine _repelCoroutine;
    
    private enum MagneticState { None, Attracted, Repelled, OnPlayer, OnWall }
    private MagneticState _state;
    public bool Moving => _state is MagneticState.Attracted or MagneticState.Repelled;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _state = MagneticState.None;
        _rb = GetComponent<Rigidbody2D>();
        _vibrateTarget = GetComponent<Transform>();
        _startingPosition = _vibrateTarget.position;
        _objectCol = GetComponent<Collider2D>();
        _coneCol = MagnetCone.Instance.GetComponent<Collider2D>();
        _parent = transform.parent.gameObject;

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

        if (_colliderDistance < vibrateRange &&  _state == MagneticState.None && MagnetCone.Instance.GetConePolarity() == polarity)
        {
            var point = new Vector3(Random.Range(_startingPosition.x - vibrateAmount, _startingPosition.x + vibrateAmount),
                Random.Range(_startingPosition.y - vibrateAmount, _startingPosition.y + vibrateAmount), _startingPosition.z);
            _vibrateTarget.position = point;
            
            if(!sfx_vibrate.isPlaying) sfx_vibrate.Play();
        }
        else if(sfx_vibrate.isPlaying) sfx_vibrate.Stop();
    }

    private void ChangeState(MagneticState newState)
    {
        if (newState == _state) return;
        _state = newState;
    }

    public void InsideMagnetCone(Polarity conePolarity) {

        if (_state == MagneticState.None || _state == MagneticState.OnWall) {
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
    }

    private void Attract()
    {
        if (_repelCoroutine != null) {
            StopCoroutine(_repelCoroutine);
        }
        _attractCoroutine = StartCoroutine(AttractCoroutine());
    }

    private void Repel()
    {
        if (_attractCoroutine != null) {
            StopCoroutine(_attractCoroutine);
        }
        _repelCoroutine = StartCoroutine(RepelCoroutine());
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
                if (_attractCoroutine != null) StopCoroutine(_attractCoroutine);
                if (_repelCoroutine != null) StopCoroutine(_repelCoroutine);
                HitWall();
            }

            // have object bounce off a bit
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        
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

        var timeGoneBy = 0.0f;
        while (timeGoneBy < attractionTime && _state == MagneticState.Attracted)
        {
            transform.position = Vector3.Lerp(transform.position, PlayerController.Instance.transform.position, timeGoneBy / attractionTime);
            timeGoneBy += Time.deltaTime;
            yield return null;
        }
        
        // collides with player and state becomes OnPlayer
        if (_state == MagneticState.OnPlayer) {
            transform.position = _attractFinalPosition;
        }

        ChangeState(MagneticState.OnPlayer);
    }
    
    private IEnumerator RepelCoroutine()
    {
        ChangeState(MagneticState.Repelled);
        
        sfx_whoosh.Play();

        Physics2D.IgnoreLayerCollision(6, 7, false);

        var point = PlayerController.Instance.transform.position + PlayerController.Instance.transform.right * repelDistance;
        Vector3 targetPosition = new Vector3(point.x, point.y, transform.position.z);
        _startingPosition = targetPosition;
        transform.SetParent(_parent.transform);

        var timeGoneBy = 0.0f;
        while (timeGoneBy < repelTime && _state == MagneticState.Repelled)
        {
            if (Time.timeScale != 0) {
                timeGoneBy += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, targetPosition, timeGoneBy / repelTime);
            }
            yield return null;
        }
        
        Physics2D.IgnoreLayerCollision(6, 7, true);

        ChangeState(MagneticState.None);
    }
}
