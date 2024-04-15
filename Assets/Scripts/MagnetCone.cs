using System;
using UnityEngine;

public class MagnetCone : MonoBehaviour {
    public static MagnetCone Instance;
    
    [SerializeField] private SpriteRenderer coneSprite;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;

    [SerializeField] private SpriteRenderer magnetSpriteRenderer;
    [SerializeField] private Sprite positiveSprite;
    [SerializeField] private Sprite negativeSprite;

    private Polarity _currentPolarity;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        UpdateSprite();
    }

    private void Update() {
        if (InputManager.Instance.firePressed) SwitchPolarity();
    }

    private void SwitchPolarity() {
        _currentPolarity = _currentPolarity switch {
            Polarity.Positive => Polarity.Negative,
            Polarity.Negative => Polarity.Positive,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        UpdateSprite();
        AudioManager.Instance.SwitchPolarity(_currentPolarity);
    }

    private void UpdateSprite() {
        coneSprite.color = _currentPolarity switch {
            Polarity.Positive => positiveColor,
            Polarity.Negative => negativeColor,
            _ => throw new ArgumentOutOfRangeException()
        };

        magnetSpriteRenderer.sprite = _currentPolarity switch
        {
            Polarity.Positive => positiveSprite,
            Polarity.Negative => negativeSprite,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        var magneticObject = col.gameObject.GetComponent<MagneticObject>();
        if (magneticObject != null)
        {
            magneticObject.InsideMagnetCone(_currentPolarity);
        }
    }

    public Polarity GetConePolarity()
    {
        return _currentPolarity;
    }
}

public enum Polarity { Positive, Negative }