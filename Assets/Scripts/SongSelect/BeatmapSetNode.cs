using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class BeatmapSetNode
{
    public BeatmapSet beatmapSet;
    public int beatmapIndex = -1; // 확장되지 않은 상태에서는 -1
    public int index = 0; // 리스트 내에서의 인덱스
    public BeatmapSetNode prev;
    public BeatmapSetNode next;

    // UI 요소
    public GameObject nodeUI;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI artistText;
    private TextMeshProUGUI versionText;
    private RawImage image;
    private Button button;

    // 클릭 이벤트
    public event Action<BeatmapSetNode> OnClick;

    public BeatmapSetNode(BeatmapSet beatmapSet)
    {
        this.beatmapSet = beatmapSet;
    }

    // 선택된 Beatmap 반환
    public Beatmap GetSelectedBeatmap()
    {
        if (beatmapIndex < 0 || beatmapIndex >= beatmapSet.Count)
            return null;
        return beatmapSet.Get(beatmapIndex);
    }

    // UI 초기화
    public void InitializeUI(GameObject prefab, Transform parent)
    {
        nodeUI = GameObject.Instantiate(prefab, parent);

        // 필요한 UI 컴포넌트 참조 가져오기
        titleText = nodeUI.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        artistText = nodeUI.transform.Find("Artist").GetComponent<TextMeshProUGUI>();
        versionText = nodeUI.transform.Find("Version").GetComponent<TextMeshProUGUI>();
        image = nodeUI.transform.Find("Image").GetComponent<RawImage>();
        button = nodeUI.GetComponent<Button>();

        // Beatmap 정보로 UI 업데이트
        UpdateUI();

        // 클릭 이벤트 등록
        button.onClick.AddListener(() => OnClick?.Invoke(this));
    }

    // UI 업데이트
    public void UpdateUI()
    {
        Beatmap beatmap = GetSelectedBeatmap() ?? beatmapSet.Get(0);

        titleText.text = beatmap.title;
        artistText.text = beatmap.artist;
        versionText.text = beatmap.version;

        if (!string.IsNullOrEmpty(beatmap.imagePath))
        {
            Texture2D imageTexture = GameManager.ResourceCache.GetCachedImage(beatmap.imagePath);
            if (imageTexture != null)
            {
                image.texture = imageTexture;
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

    // 포커스 상태 설정
    public void SetFocus(bool focus)
    {
        // 포커스 상태에 따라 UI 업데이트 (예: 색상 변경)
        if (nodeUI != null)
        {
            Image background = nodeUI.GetComponent<Image>();
            if (focus)
            {
                background.color = Color.cyan;
            }
            else
            {
                background.color = Color.white;
            }
        }
    }

    // 문자열 표현 반환
    public override string ToString()
    {
        if (beatmapIndex == -1)
            return beatmapSet.ToString();
        else
            return beatmapSet.Get(beatmapIndex).ToString();
    }
}
