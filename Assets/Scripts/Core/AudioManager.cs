using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string soundName;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Centang jika audio ini harus di-loop (misal suara jalan)")]
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    // Membuat AudioManager menjadi Singleton agar bisa dipanggil dari mana saja
    public static AudioManager Instance;

    // Variabel global pengali volume
    private float _globalSFXVolume = 1f;
    private float _globalMusicVolume = 1f;

    [Header("Audio Sources")]
    [Tooltip("AudioSource untuk memutar efek suara (SFX)")]
    public AudioSource sfxSource;
    [Tooltip("AudioSource untuk memutar musik (Music/BGM)")]
    public AudioSource musicSource;

    // AudioSource khusus untuk SFX yang di-loop (misal langkah kaki) agar tidak memotong SFX lain
    private AudioSource _loopSfxSource;

    [Header("Audio Lists")]
    [Tooltip("Daftar audio clip untuk SFX")]
    public List<SoundData> sfxList = new List<SoundData>();
    [Tooltip("Daftar audio clip untuk Musik")]
    public List<SoundData> musicList = new List<SoundData>();
    [Tooltip("Daftar suara announcer combo (Anjay, Brainrot, Cooked, Sigma, Skibidi)")]
    public List<SoundData> comboList = new List<SoundData>();

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
            // Agar AudioManager tidak hancur saat pindah scene:
            DontDestroyOnLoad(gameObject);

            // Load volume dari memori (PlayerPrefs)
            _globalSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            _globalMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            
            if (sfxSource != null) sfxSource.volume = _globalSFXVolume;
            if (musicSource != null) musicSource.volume = _globalMusicVolume;

            // Buat komponen AudioSource baru secara otomatis khusus untuk SFX yang loop
            _loopSfxSource = gameObject.AddComponent<AudioSource>();
            _loopSfxSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Memutar efek suara (SFX).
    /// </summary>
    public void PlaySFX(string name)
    {
        SoundData s = sfxList.Find(x => x.soundName == name);
        if (s != null)
        {
            if (s.loop)
            {
                // Jika audio diset loop, jadikan clip utama di _loopSfxSource
                _loopSfxSource.clip = s.clip;
                _loopSfxSource.volume = s.volume * _globalSFXVolume;
                _loopSfxSource.loop = true;
                
                // Mencegah audio me-restart dari awal jika sudah sedang memutar clip yang sama
                if (!_loopSfxSource.isPlaying || _loopSfxSource.clip != s.clip)
                {
                    _loopSfxSource.Play();
                }
            }
            else
            {
                // Jika tidak loop, putar menumpuk (OneShot)
                sfxSource.PlayOneShot(s.clip, s.volume * _globalSFXVolume);
            }
        }
        else
        {
            Debug.LogWarning($"[AudioManager] SFX '{name}' tidak ditemukan!");
        }
    }

    /// <summary>
    /// Menghentikan SFX yang sedang looping (misal saat player berhenti jalan).
    /// </summary>
    public void StopSFX()
    {
        if (_loopSfxSource != null)
        {
            _loopSfxSource.Stop();
            _loopSfxSource.loop = false;
        }
    }

    /// <summary>
    /// Menghentikan SEMUA efek suara (termasuk yang one-shot dan loop).
    /// Berguna saat Game Over atau Victory.
    /// </summary>
    public void StopAllSFX()
    {
        if (sfxSource != null) sfxSource.Stop();
        if (_loopSfxSource != null) _loopSfxSource.Stop();
    }

    /// <summary>
    /// Memutar suara Combo Announcer.
    /// </summary>
    public void PlayComboSFX(string name)
    {
        SoundData s = comboList.Find(x => x.soundName == name);
        if (s != null)
        {
            sfxSource.PlayOneShot(s.clip, s.volume * _globalSFXVolume);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Combo SFX '{name}' tidak ditemukan!");
        }
    }

    /// <summary>
    /// Memutar musik latar (BGM).
    /// </summary>
    public void PlayMusic(string name)
    {
        SoundData s = musicList.Find(x => x.soundName == name);
        if (s != null)
        {
            musicSource.clip = s.clip;
            musicSource.volume = s.volume * _globalMusicVolume;
            musicSource.loop = true; // Musik biasanya di-loop
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Music '{name}' tidak ditemukan!");
        }
    }

    /// <summary>
    /// Menghentikan musik yang sedang berputar.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Mengatur volume SFX lewat Slider.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        _globalSFXVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        if (sfxSource != null) sfxSource.volume = volume;
        if (_loopSfxSource != null) _loopSfxSource.volume = volume;
    }

    /// <summary>
    /// Mengatur volume Music lewat Slider.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        _globalMusicVolume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        if (musicSource != null) musicSource.volume = volume;
    }
}
