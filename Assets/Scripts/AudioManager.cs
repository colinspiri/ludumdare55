using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    
    [SerializeField] private AudioMixer mixer;
    
    [Header("Music")]
    [SerializeField] private AudioSource music;
    
    [Header("SFX")]
    [SerializeField] private AudioSource polarity_positive;
    [SerializeField] private AudioSource polarity_negative;
    [SerializeField] private AudioSource splat;
    [SerializeField] private AudioSource metal_hit_player;
    [SerializeField] private AudioSource metal_hit_wall;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.parent = null;
        DontDestroyOnLoad(this);
    }

    private void Start() {
        SetVolume("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", 0.4f));
        SetVolume("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetVolume("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 1f));

        SceneManager.activeSceneChanged += (oldscene, newscene) => {
            music.Stop();
            music.Play();
        };
    }

    public void SetVolume(string mixerParameter, float value) {
        mixer.SetFloat(mixerParameter, Mathf.Log10(value) * 20f);
        PlayerPrefs.SetFloat(mixerParameter, value);
    }

    public void PauseAudio() {
    }
    public void ResumeAudio() {
        
    }

    public void SwitchPolarity(Polarity polarity) {
        return;
        if (polarity == Polarity.Positive) {
            polarity_positive.Play();
        }
        else if (polarity == Polarity.Negative) {
            polarity_negative.Play();
        }
    }
    
    public void PlaySplat() {
        splat.Play();
    }

    public void PlayMetalHitPlayer() {
        metal_hit_player.Play();
    }
    public void PlayMetalHitWall() {
        metal_hit_wall.Play();
    }
}
