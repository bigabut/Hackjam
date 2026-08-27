using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        // Menyambungkan slider ke fungsi otomatis saat nilai berubah & sync dengan PlayerPrefs
        if (_musicSlider != null)
        {
            _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (_sfxSlider != null)
        {
            _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnDestroy()
    {
        // Membersihkan listener saat objek dihancurkan untuk menghindari error (Memory Leak)
        if (_musicSlider != null)
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
        
        if (_sfxSlider != null)
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
    }

    #region Volume Controls

    public void OnMusicVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null!");
        }
    }

    public void OnSFXVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null!");
        }
    }

    #endregion
}
