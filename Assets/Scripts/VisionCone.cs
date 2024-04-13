using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
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