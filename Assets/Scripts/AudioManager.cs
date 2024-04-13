using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    
    public AudioMixer mixer;
    
    [Header("Music")]
    public AudioSource music;
    
    [Header("SFX")]
    public AudioSource recordScratch;

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
}
