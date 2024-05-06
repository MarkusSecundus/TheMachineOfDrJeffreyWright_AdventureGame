using DG.Tweening;
using MarkusSecundus.Utils;
using MarkusSecundus.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] AudioSource musicBackgroundPlayer;
    [SerializeField] float musicBackgroundFadeinSeconds = 2f;
    
    [SerializeField] AudioSource musicTrackPlayer;

    [SerializeField] AudioClip[] musicClips;
    [SerializeField] float musicTrackTransitionFadeoutSeconds = 3f;
    [SerializeField] float musicTrackTransitionFadeinSeconds = 0.3f;


    private void Start()
    {
        StartBackground();
        StartMusic();
    }

    private void StartBackground()
    {
        musicBackgroundPlayer.loop = true;
        var originalVolume = musicBackgroundPlayer.volume;
        musicBackgroundPlayer.volume = 0f;
        musicBackgroundPlayer.DOFade(originalVolume, musicBackgroundFadeinSeconds);
    }

    private void StartMusic()
    {
        musicTrackPlayer.loop = true;
        SwitchTrack(musicClips[0]);
    }

    AudioClip _currentTransitionTarget = null;
    private void SwitchTrack(AudioClip track)
    {
        if (Op.post_assign(ref _currentTransitionTarget, track)) return;

        if (musicTrackPlayer.clip == track) return;
        musicTrackPlayer.DOFade(0f, musicTrackTransitionFadeoutSeconds).OnComplete(() =>
        {
            if (_currentTransitionTarget)
            {
                musicTrackPlayer.clip = _currentTransitionTarget;
                musicTrackPlayer.Play();
                musicTrackPlayer.DOFade(1f, musicTrackTransitionFadeinSeconds);
            }
            _currentTransitionTarget = null;
        });
    }

    public void IncreaseTension()
    {
        SwitchTrack(System.Array.IndexOf(musicClips, musicTrackPlayer.clip) + 1);
    }
    public void SwitchTrack(int index)
    {
        if (index < 0 || index >= musicClips.Length) 
            Debug.LogWarning($"Invalid track index {index}", this);
        SwitchTrack(musicClips[Mathf.Clamp(index, 0, musicClips.Length-1)]);
    }
}
