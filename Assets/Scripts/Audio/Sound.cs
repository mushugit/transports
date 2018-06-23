using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {

	public string name;

	public AudioClip clip;

	public bool isMusic = false;

	[Range(0f,1f)]
	public float volume = 0.5f;
	[Range(.1f,3f)]
	public float pitch = 1f;

	public bool Loop = false;

	[HideInInspector]
	public AudioSource source;
}
