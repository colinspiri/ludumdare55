using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticObjectSpawner : MonoBehaviour
{
    [SerializeField] private MagneticObject magneticObject;
    [HideInInspector] public bool hasSpawned;
    [SerializeField] private GameObject objectParent;
    private int rand;

    // Start is called before the first frame update
    void Start()
    {
        hasSpawned = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasSpawned)
        {
            rand = Random.Range(0, 2);
            var newObject = Instantiate(magneticObject);
            if (rand == 0) magneticObject.polarity = Polarity.Positive;
            if (rand == 1) magneticObject.polarity = Polarity.Negative;
            newObject.transform.position = transform.position;
            newObject.transform.SetParent(objectParent.transform);
            newObject.spawner = this;
            hasSpawned = true;
        }
    }
}
