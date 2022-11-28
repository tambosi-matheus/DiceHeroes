using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio")]
public class MainAudio : AudioEvent
{
    public Audio[] clips;

    [System.Serializable]
    public class Audio
    {
        public string name;
        public AudioClip audio;

        [Range(0.1f, 2)]
        public float volume;

        [Range(0.8f, 2)]
        public float pitch;
    }
    

    public override void Play(AudioSource source, int position)
    {
        if (clips.Length == 0) return;

        var diceAudio = clips[position];        
        source.clip = diceAudio.audio;
        source.volume = diceAudio.volume;
        source.pitch = diceAudio.pitch;
        source.PlayOneShot(diceAudio.audio);
    }

    public override void Play(AudioSource source, string name)
    {
        if (clips.Length == 0) return;

        var diceAudio = Array.Find(clips, a => a.name.Equals(name));
        source.clip = diceAudio.audio;
        source.volume = diceAudio.volume;
        source.pitch = diceAudio.pitch;
        source.PlayOneShot(diceAudio.audio);
    }
}