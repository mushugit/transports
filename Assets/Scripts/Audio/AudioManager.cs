using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    private Sound[] _music;
    private static int _currentMusic;

    public static AudioManager Player { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Player == null)
            Player = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.playOnAwake = false;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    private void Start()
    {
        _music = Array.FindAll(sounds, sound => sound.isMusic);
        if (_music.Length > 0)
        {
            var length = _music[_currentMusic].source.clip.length;
            var name = _music[_currentMusic].name;
            Play(name);
            MusicInfo.DisplayName(name);
            Invoke("NextMusic", length);
        }
    }

    private void NextMusic()
    {
        _currentMusic++;
        if (_currentMusic >= _music.Length)
            _currentMusic = 0;

        var length = _music[_currentMusic].source.clip.length;
        var name = _music[_currentMusic].name;
        Play(name);
        MusicInfo.DisplayName(name);
        Invoke("NextMusic", length);
    }

    public void Play(string name)
    {
        var s = Array.Find(sounds, sound => sound.name == name);

        s?.source.Play();
    }
}
