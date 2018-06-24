using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{

	public Sound[] sounds;
	private Sound[] music;
	private static int currentMusic = 0;

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

			s.source.clip = s.clip;
			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
		}
	}

	private void Start()
	{
		music = Array.FindAll(sounds, sound => sound.isMusic);
		if(music.Length > 0)
		{
			var length = music[currentMusic].source.clip.length;
			Play(music[currentMusic].name);
			Invoke("NextMusic", length);
		}
	}

	private void NextMusic()
	{
		currentMusic++;
		if (currentMusic >= music.Length)
			currentMusic = 0;

		var length = music[currentMusic].source.clip.length;
		Play(music[currentMusic].name);
		Invoke("NextMusic", length);
	}

	public void Play(string name)
	{
		var s = Array.Find(sounds, sound => sound.name == name);

		s?.source.Play();
	}
}
