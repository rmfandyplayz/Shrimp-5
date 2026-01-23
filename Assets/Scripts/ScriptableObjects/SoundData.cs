using UnityEngine;

[CreateAssetMenu(menuName = "Shrimp/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("ID used in code")]
    public string id; // e.g. "ui.click" or "bgm.battle"

    [Header("Audio Config")]
    public AudioClip mainClip;
    [Tooltip("optional: play this once before looping main clip")] 
    public AudioClip introClip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;

    [Header("For Credits Menu")]
    public string title;
    public string body;
}