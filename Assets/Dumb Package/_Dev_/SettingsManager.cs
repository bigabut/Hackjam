using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _volumeSettingsPanel;
    [SerializeField] private GameObject _controlSettingsPanel;

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

    #region Panel Navigation

    public void ShowVolumeSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI Button");
        
        if (_volumeSettingsPanel != null) _volumeSettingsPanel.SetActive(true);
        if (_controlSettingsPanel != null) _controlSettingsPanel.SetActive(false);
    }

    public void ShowControlSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI Button");
        
        if (_volumeSettingsPanel != null) _volumeSettingsPanel.SetActive(false);
        if (_controlSettingsPanel != null) _controlSettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("UI Button");
        gameObject.SetActive(false);
    }

    #endregion

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
