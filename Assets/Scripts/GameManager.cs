using System;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObjectCollection allEnemies;

    private bool _gameWon;
    public UnityEvent onWin;
    
    private void Update() {
        if (allEnemies.Count == 0 && _gameWon == false) {
            _gameWon = true;
            onWin?.Invoke();
        }
    }
}