using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticObject : MonoBehaviour {
    public Polarity polarity;

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AttractToMagnet() {
        Debug.Log(gameObject.name + " is affected by magnet");
    }
}
