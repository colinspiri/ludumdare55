using System;
using System.Collections;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {
    [SerializeField] private GameObjectCollection allEnemies;

    private bool _gameWon;
    [SerializeField] private float onWinDelay;
    public UnityEvent onWin;
    
    private void Update() {
        if (allEnemies.Count == 0 && _gameWon == false) {
            _gameWon = true;
            StartCoroutine(OnWinCoroutine());
        }
    }
    
    private IEnumerator OnWinCoroutine() {
        yield return new WaitForSeconds(onWinDelay);

        onWin?.Invoke();
    }
}