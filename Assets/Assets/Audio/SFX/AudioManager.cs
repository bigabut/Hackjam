using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string soundName;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Centang jika audio ini harus di-loop (misal suara jalan)")]
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static AudioManager Instance;

    // =========================================================
    // VOLUME
    // =========================================================

    private float _globalSFXVolume = 1f;
    private float _globalMusicVolume = 1f;

    // =========================================================
    // AUDIO SOURCES
    // =========================================================

    [Header("Audio Sources")]

    [Tooltip("AudioSource untuk memutar efek suara (SFX)")]
    public AudioSource sfxSource;

    [Tooltip("AudioSource untuk memutar musik (Music/BGM)")]
    public AudioSource musicSource;

    // AudioSource khusus untuk SFX looping
    private AudioSource _loopSfxSource;

    // =========================================================
    // AUDIO LISTS
    // =========================================================

    [Header("Audio Lists")]

    [Tooltip("Daftar audio clip untuk SFX")]
    public List<SoundData> sfxList =
        new List<SoundData>();

    [Tooltip("Daftar audio clip untuk Musik")]
    public List<SoundData> musicList =
        new List<SoundData>();

    // =========================================================
    // CURRENT MUSIC
    // =========================================================

    private string _currentMusicName;

    public string CurrentMusicName =>
        _currentMusicName;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // =====================================================
        // SINGLETON
        // =====================================================

        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

            // =================================================
            // LOAD VOLUME
            // =================================================

            _globalSFXVolume =
                PlayerPrefs.GetFloat(
                    "SFXVolume",
                    1f
                );

            _globalMusicVolume =
                PlayerPrefs.GetFloat(
                    "MusicVolume",
                    1f
                );

            if (sfxSource != null)
            {
                sfxSource.volume =
                    _globalSFXVolume;
            }

            if (musicSource != null)
            {
                musicSource.volume =
                    _globalMusicVolume;
            }

            // =================================================
            // LOOP SFX SOURCE
            // =================================================

            _loopSfxSource =
                gameObject.AddComponent<AudioSource>();

            _loopSfxSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================
    // PLAY SFX
    // =========================================================

    public void PlaySFX(string name)
    {
        Debug.Log(
            $"PlaySFX dipanggil: {name}"
        );

        SoundData s =
            sfxList.Find(
                x => x.soundName == name
            );

        if (s != null)
        {
            Debug.Log(
                $"SFX ditemukan: {s.soundName}"
            );

            // =================================================
            // LOOPING SFX
            // =================================================

            if (s.loop)
            {
                _loopSfxSource.clip = s.clip;

                _loopSfxSource.volume =
                    s.volume *
                    _globalSFXVolume;

                _loopSfxSource.loop = true;

                if (!_loopSfxSource.isPlaying ||
                    _loopSfxSource.clip != s.clip)
                {
                    _loopSfxSource.Play();
                }
            }

            // =================================================
            // ONE SHOT SFX
            // =================================================

            else
            {
                sfxSource.PlayOneShot(
                    s.clip,
                    s.volume *
                    _globalSFXVolume
                );
            }
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] SFX '{name}' tidak ditemukan!"
            );
        }
    }

    // =========================================================
    // STOP LOOPING SFX
    // =========================================================

    public void StopSFX()
    {
        if (_loopSfxSource != null)
        {
            _loopSfxSource.Stop();
            _loopSfxSource.loop = false;
        }
    }

    // =========================================================
    // STOP ALL SFX
    // =========================================================

    public void StopAllSFX()
    {
        if (sfxSource != null)
            sfxSource.Stop();

        if (_loopSfxSource != null)
            _loopSfxSource.Stop();
    }

    // =========================================================
    // PLAY MUSIC
    // =========================================================

    public void PlayMusic(string name)
    {
        // =====================================================
        // AUDIO MANAGER CHECK
        // =====================================================

        if (musicSource == null)
        {
            Debug.LogWarning(
                "[AudioManager] Music Source belum di-assign!"
            );

            return;
        }

        // =====================================================
        // CARI MUSIC
        // =====================================================

        SoundData s =
            musicList.Find(
                x => x.soundName == name
            );

        if (s == null)
        {
            Debug.LogWarning(
                $"[AudioManager] Music '{name}' tidak ditemukan!"
            );

            return;
        }

        // =====================================================
        // MUSIC SUDAH SAMA
        // =====================================================

        if (_currentMusicName == name &&
            musicSource.clip == s.clip &&
            musicSource.isPlaying)
        {
            // Jangan restart music
            return;
        }

        // =====================================================
        // GANTI MUSIC
        // =====================================================

        _currentMusicName = name;

        musicSource.clip = s.clip;

        musicSource.volume =
            s.volume *
            _globalMusicVolume;

        musicSource.loop = true;

        musicSource.Play();

        Debug.Log(
            $"[AudioManager] Playing Music: {name}"
        );
    }

    // =========================================================
    // STOP MUSIC
    // =========================================================

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        _currentMusicName = "";
    }

    // =========================================================
    // SFX VOLUME
    // =========================================================

    public void SetSFXVolume(float volume)
    {
        _globalSFXVolume = volume;

        PlayerPrefs.SetFloat(
            "SFXVolume",
            volume
        );

        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }

        if (_loopSfxSource != null)
        {
            _loopSfxSource.volume = volume;
        }
    }

    // =========================================================
    // MUSIC VOLUME
    // =========================================================

    public void SetMusicVolume(float volume)
    {
        _globalMusicVolume = volume;

        PlayerPrefs.SetFloat(
            "MusicVolume",
            volume
        );

        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
}