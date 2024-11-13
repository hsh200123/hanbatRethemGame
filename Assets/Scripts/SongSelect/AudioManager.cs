using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    private AudioSource audioSource;

    public void Init()
    {
        GameObject audioManagerObject = new GameObject { name = "AudioManager" };
        audioSource = audioManagerObject.AddComponent<AudioSource>();

        Object.DontDestroyOnLoad(audioManagerObject);
    }
    public void PlayPreview(Beatmap beatmap)
    {
        // ResourceCache에서 오디오 클립 가져오기
        if (!string.IsNullOrEmpty(beatmap.audioPath))
        {
            AudioClip audioClip = GameManager.ResourceCache.GetCachedAudio(beatmap.audioPath);

            if (audioClip != null)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

                audioSource.clip = audioClip;
                audioSource.time = beatmap.previewTime / 1000f;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("오디오 클립이 캐시에 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("오디오 경로가 비어 있습니다.");
        }
    }
}
