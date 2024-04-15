using System.Collections;
using UnityEngine;

public class Hitstop : MonoBehaviour
{
    public static Hitstop Instance;

    [SerializeField] private float defaultHitstopTime = 1f;
    [SerializeField] private float hitstopShakeStrength = 0.4f;

    private void Awake()
    {
        Instance = this;
    }

    public void DoHitstop(float hitstopTime = 0f)
    {
        CameraShake.Instance.Shake(hitstopShakeStrength);
        StartCoroutine(HitstopCoroutine(hitstopTime));
    }

    private IEnumerator HitstopCoroutine(float hitstopTime = 0f)
    {
        if (hitstopTime == 0) hitstopTime = defaultHitstopTime;
        
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(hitstopTime);

        Time.timeScale = 1;
    }
}