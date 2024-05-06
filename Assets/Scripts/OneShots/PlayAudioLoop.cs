using MarkusSecundus.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayAudioLoop : MonoBehaviour
{
    [SerializeField] AudioClip Begin;
    [SerializeField] AudioClip Loop;
    [SerializeField] bool StartOnBegin;
    AudioSource _audio;
    
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        if (StartOnBegin) StartPlaying();
    }
    public void StartPlaying()
    {
        _audio.Stop();
        _audio.clip = Begin;
        _audio.loop = false;
        _audio.Play();
        this.InvokeWithDelay(() =>
        {
            _audio.clip = Loop;
            _audio.loop = true;
            _audio.Play();
        }, new WaitWhile(() => _audio.isPlaying));
    }
}
