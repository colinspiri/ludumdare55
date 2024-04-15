using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MagneticObjectSpawner : MonoBehaviour
{
    [SerializeField] private MagneticObject magneticObject;
    [SerializeField] private float spawnDelay;
    [SerializeField] private SpriteRenderer spawnerIcon;
    [SerializeField] private AnimationCurve fadeInCurve;
    [SerializeField] private float iconFullOpacity;

    private MagneticObject _spawnedObject;

    // Start is called before the first frame update
    void Start()
    {
        SpawnNewObject();
    }

    private void SpawnNewObject() {
        spawnerIcon.DOFade(0, 0.1f);
        
        var rand = Random.Range(0, 2);
        var newObject = Instantiate(magneticObject);
        if (rand == 0) magneticObject.polarity = Polarity.Positive;
        if (rand == 1) magneticObject.polarity = Polarity.Negative;
        
        newObject.transform.position = transform.position;
        _spawnedObject = newObject.GetComponent<MagneticObject>();

        _spawnedObject.onDeath += SpawnOnDeath;
    }

    private void SpawnOnDeath() {
        StartCoroutine(SpawnObjectAfterDelay());

        _spawnedObject.onDeath -= SpawnOnDeath;
    }

    private IEnumerator SpawnObjectAfterDelay() {
        spawnerIcon.DOFade(iconFullOpacity, spawnDelay).SetEase(fadeInCurve);
        yield return new WaitForSeconds( 0.1f + spawnDelay);
        
        SpawnNewObject();
    }
}
