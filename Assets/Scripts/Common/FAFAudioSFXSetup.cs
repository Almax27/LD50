using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum FAFAudioSFXVarianceMode
{
    Random,
    Ordered
}


[CreateAssetMenu(fileName = "SFX_", menuName = "Audio/SFX", order = 1)]
public class FAFAudioSFXSetup : ScriptableObject {

    public List<AudioClip> Clips = new List<AudioClip>();

    public FAFAudioSFXVarianceMode varianceMode = FAFAudioSFXVarianceMode.Random;

    public AudioMixerGroup MixerGroup = null;
    [Range(0, 1)] public float Volume = 1.0f;
    [Range(0, 1)] public float VolumeVariance = 0.0f;
    [Range(0, 1)] public float Pitch = 1.0f;
    [Range(0, 1)] public float PitchVariance = 0.0f;
    [Range(0, 1)] public float SpatialBlend = 0.0f;

    int playIndex = 0;

    public AudioSource Play(Vector3 _pos, float _volumeScalar = 1.0f, float _pitchScalar = 1.0f, float _pitchOffset = 0.0f)
    {
        AudioSource source = null;
        if (Clips != null && Clips.Count > 0)
        {
            AudioClip clip = null;
            switch (varianceMode)
            {
                default:
                case FAFAudioSFXVarianceMode.Random:
                    clip = Clips[Random.Range(0, Clips.Count)];
                    break;
                case FAFAudioSFXVarianceMode.Ordered:
                    clip = Clips[playIndex % Clips.Count];
                    break;
            }

            float volume = (Volume  * _volumeScalar) + Random.Range(-VolumeVariance, VolumeVariance);
            float pitch = (Pitch * _pitchScalar) + _pitchOffset + Random.Range(-PitchVariance, PitchVariance);
            source = FAFAudio.Instance.Play(Clips[Random.Range(0, Clips.Count)], _pos, volume, pitch, MixerGroup);
            if(source)
            {
                source.spatialBlend = SpatialBlend;
                source.minDistance = 10;
            }

            playIndex++;
        }
        return source;
    }
}
