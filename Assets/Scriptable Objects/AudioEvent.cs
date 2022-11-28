using UnityEngine;
public abstract class AudioEvent : ScriptableObject
{
    public abstract void Play(AudioSource source, int audioArrayPosition);
    public abstract void Play(AudioSource source, string audioName);
}
