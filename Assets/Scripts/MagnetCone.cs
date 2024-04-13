using System;
using UnityEngine;

public class MagnetCone : MonoBehaviour {
    [SerializeField] private SpriteRenderer coneSprite;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;
    
    private Polarity _currentPolarity;

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
    }

    private void UpdateSprite() {
        coneSprite.color = _currentPolarity switch {
            Polarity.Positive => positiveColor,
            Polarity.Negative => negativeColor,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        var magneticObject = col.gameObject.GetComponent<MagneticObject>();
        if (magneticObject != null)
        {
            if (magneticObject.polarity == _currentPolarity) magneticObject.AttractToMagnet();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var magneticObject = collision.gameObject.GetComponent<MagneticObject>();
        if (magneticObject != null)
        {
            magneticObject.attractedToPlayer = false;
        }
    }

    public Polarity GetConePolarity()
    {
        return _currentPolarity;
    }
}

public enum Polarity { Positive, Negative }