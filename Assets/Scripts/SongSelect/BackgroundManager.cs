using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager
{
    private RawImage backgroundImage;

    // Init 메서드에서 배경 이미지를 초기화하지 않음 (유연성 제공)
    public void SetBackgroundImage(Beatmap beatmap)
    {
        if (backgroundImage == null)
        {
            GameObject bgObject = GameObject.Find("BackgroundImage");
            if (bgObject != null)
            {
                backgroundImage = bgObject.GetComponent<RawImage>();
            }
            else
            {
                Debug.LogError("BackgroundImage 오브젝트가 현재 씬에 존재하지 않습니다.");
                return;
            }
        }

        // ResourceCache에서 이미지 텍스처 가져오기
        if (!string.IsNullOrEmpty(beatmap.imagePath))
        {
            Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(beatmap.imagePath);
            if (imageTexture != null)
            {
                backgroundImage.texture = imageTexture;
            }
            else
            {
                Debug.LogError("이미지 텍스처가 캐시에 존재하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("이미지 경로가 비어 있습니다.");
        }
    }
}
