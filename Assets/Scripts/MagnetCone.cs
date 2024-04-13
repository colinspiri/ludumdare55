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
        Debug.Log("switching polarity");
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

    private void OnTriggerEnter2D(Collider2D col) {
        var enemy = col.gameObject.GetComponent<Enemy>();
        if (enemy != null) {
            // enemy.SetInVision(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy != null) {
            // enemy.SetInVision(false);
        }
    }
}

public enum Polarity { Positive, Negative }