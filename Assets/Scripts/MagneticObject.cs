using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Events;

public class MagneticObject : MonoBehaviour
{
    // components
    private Collider2D _collider;
    public ParticleSystem clashParticles;
    private TrailRenderer _trailRenderer;

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

    [Header("Attraction")] 
    [SerializeField] private AnimationCurve attractionCurve;
    [SerializeField] private float attractionTime;
    [SerializeField] private AnimationCurve repelCurve;
    [SerializeField] private float repelTime;
    [SerializeField] private float repelDistance;
    [SerializeField] private float cameraShakeMagnitudeOnCollision;

    // state
    private Rigidbody2D _rb;
    private Vector3 _vibrateOrigin;
    private Collider2D _coneCol;
    private float _colliderDistance;
    private Vector3 _attractFinalPosition;
    private Coroutine _attractCoroutine;
    private Coroutine _repelCoroutine;
    private Vector3 _onPlayerOffset;

    private enum MagneticState { None, Attracted, Repelled, OnPlayer, OnWall }
    private MagneticState _state;
    public bool Moving => _state is MagneticState.Attracted or MagneticState.Repelled;

    public delegate void OnDeath();
    public OnDeath onDeath;

    // Start is called before the first frame update
    void Start()
    {
        _state = MagneticState.None;
        _rb = GetComponent<Rigidbody2D>();
        _vibrateOrigin = transform.position;
        _collider = GetComponent<Collider2D>();
        _coneCol = MagnetCone.Instance.GetComponent<Collider2D>();
        _trailRenderer = GetComponent<TrailRenderer>();

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

        if (_state == MagneticState.OnPlayer) {
            transform.position = PlayerController.Instance.transform.TransformPoint(_onPlayerOffset);
        }
        if (_state == MagneticState.OnPlayer && MagnetCone.Instance.GetConePolarity() != polarity) {
            Repel();
        }
    }

    private void CheckVibration()
    {
        _colliderDistance = Physics2D.Distance(_collider, _coneCol).distance;

        if (_state == MagneticState.None && _colliderDistance < vibrateRange && MagnetCone.Instance.GetConePolarity() == polarity)
        {
            _trailRenderer.enabled = false;

            var point = new Vector3(Random.Range(_vibrateOrigin.x - vibrateAmount, _vibrateOrigin.x + vibrateAmount),
                Random.Range(_vibrateOrigin.y - vibrateAmount, _vibrateOrigin.y + vibrateAmount), _vibrateOrigin.z);
            transform.position = point;

            if (!sfx_vibrate.isPlaying) sfx_vibrate.Play();
        }
        else if (sfx_vibrate.isPlaying)
        {
            sfx_vibrate.Stop();
            _trailRenderer.enabled = true;
        }

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
            // transform.SetParent(PlayerController.Instance.transform);
            _onPlayerOffset = PlayerController.Instance.transform.InverseTransformPoint(transform.position);
        }

        if (oldState == MagneticState.OnPlayer) {
            // transform.SetParent(_initialParent.transform);
        }

        // Debug.Log(gameObject.name + " state = " + _state);
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

    private void Attract() {
        if (_state == MagneticState.Attracted) return;
        StopMoving();
        _attractCoroutine = StartCoroutine(AttractCoroutine());
    }

    private void Repel() {
        if (_state == MagneticState.Repelled) return;
        StopMoving();
        _repelCoroutine = StartCoroutine(RepelCoroutine());
    }

    private void StopMoving() {
        if (_attractCoroutine != null) StopCoroutine(_attractCoroutine);
        if (_repelCoroutine != null) StopCoroutine(_repelCoroutine);
        Physics2D.IgnoreLayerCollision(6, 7, true);
        _trailRenderer.enabled = true;
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

        if (other.gameObject.CompareTag("Magnetic Object"))
        {
            Debug.Log("collision between " + gameObject.name + " and " + other.gameObject.name);
            AudioManager.Instance.PlayMetalHitMetal();
            Instantiate(clashParticles, transform.position, Quaternion.identity);

            var otherMagneticObject = other.gameObject.GetComponent<MagneticObject>();
            otherMagneticObject.Die();
            Die();
        }
    }

    private void HitPlayer() {
        ChangeState(MagneticState.OnPlayer);
        _trailRenderer.enabled = false;

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

        var startingPosition = transform.position;
        var t = 0f;
        // var curve = AnimationCurve.EaseInOut(0f, 0f, attractionTime, 1f);
        while (t < attractionTime && _state == MagneticState.Attracted)
        {
            t += Time.deltaTime;
            float f = attractionCurve.Evaluate(t / attractionTime);
            transform.position = Vector3.Lerp(startingPosition, PlayerController.Instance.transform.position, f);
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

        var startingPosition = transform.position;
        var t = 0.0f;
        while (t < repelTime && _state == MagneticState.Repelled)
        {
            if (Time.timeScale != 0) {
                t += Time.deltaTime;
                float f = repelCurve.Evaluate(t / repelTime);
                transform.position = Vector3.Lerp(startingPosition, PlayerController.Instance.transform.position + PlayerController.Instance.transform.right * repelDistance, f);
            }
            yield return null;
        }
        transform.position = PlayerController.Instance.transform.position + PlayerController.Instance.transform.right * repelDistance;

        Physics2D.IgnoreLayerCollision(6, 7, true);

        ChangeState(MagneticState.None);
    }

    private void Die() {
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}
