using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ResourceCache
{
    private Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();

    // 썸네일과 곡 미리 로드 메서드
    public async Task PreloadBeatmapResourcesAsync(List<string> audioPaths, List<string> imagePaths)
    {
        foreach (string audioPath in audioPaths)
        {
            if (!audioCache.ContainsKey(audioPath))
            {
                AudioClip clip = await GetAudioClipAsync(audioPath);
                if (clip != null)
                {
                    audioCache[audioPath] = clip;
                }
            }
        }

        foreach (string imagePath in imagePaths)
        {
            if (!imageCache.ContainsKey(imagePath))
            {
                Texture2D texture = await GetImageTextureAsync(imagePath);
                if (texture != null)
                {
                    imageCache[imagePath] = texture;
                }
            }
        }
    }


    // 미리 로드된 오디오 클립을 반환하는 메서드
    public AudioClip GetCachedAudio(string audioPath)
    {
        audioCache.TryGetValue(audioPath, out AudioClip cachedClip);
        return cachedClip;
    }

    // 미리 로드한 이미지 텍스처를 반환하는 메서드
    public Texture2D GetCachedImage(string imagePath)
    {
        imageCache.TryGetValue(imagePath, out Texture2D cachedTexture);
        return cachedTexture;
    }

    // 오디오 파일 로드
    public async Task<AudioClip> GetAudioClipAsync(string audioPath)
    {
        if (audioCache.TryGetValue(audioPath, out AudioClip cachedClip))
        {
            return cachedClip; // 캐시에 존재하면 반환
        }

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.MPEG))
        {
            await www.SendWebRequestAsync();
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                audioCache[audioPath] = clip; // 캐시에 저장
                return clip;
            }
            else
            {
                Debug.LogError("오디오 로드 실패: " + www.error);
                return null;
            }
        }
    }

    // 이미지 파일 로드
    public async Task<Texture2D> GetImageTextureAsync(string imagePath)
    {
        if (imageCache.TryGetValue(imagePath, out Texture2D cachedTexture))
        {
            return cachedTexture; // 캐시에 존재하면 반환
        }

        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file://" + imagePath))
        {
            await uwr.SendWebRequestAsync();
            if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                imageCache[imagePath] = texture; // 캐시에 저장
                return texture;
            }
            else
            {
                Debug.LogError("이미지 로드 실패: " + uwr.error);
                return null;
            }
        }
    }
}
