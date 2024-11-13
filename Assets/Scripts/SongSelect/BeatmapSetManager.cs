using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeatmapSetManager : MonoBehaviour
{
    public GameObject beatmapSetNodePrefab;
    public Transform scrollViewContent;
    public TextMeshProUGUI messageText;


    private List<BeatmapSetNode> beatmapSetNodes;
    private BeatmapParser beatmapParser;

    private int currentFocusIndex = 0;

    private void Start()
    {
        StartCoroutine(InitializeBeatmapSets());
    }

    // 곡 로드
    private IEnumerator InitializeBeatmapSets()
    {
        // BeatmapParser를 통해 Beatmap을 로드하고, BeatmapSetList에 추가
        beatmapParser = new BeatmapParser();
        var task = beatmapParser.ParserAllBeatmapsAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.Exception == null)
        {
            List<Beatmap> beatmaps = task.Result;

            if (beatmaps.Count == 0)
            {
                messageText.text = "현재 추가된 곡이 없습니다.";
                yield break;
            }
            else
            {
                messageText.text = "";
            }
            Debug.Log($"beatmaps count : {beatmaps.Count}");
            // BeatmapSetList에 Beatmap 추가
            // 여기서는 단순히 모든 Beatmap을 하나의 BeatmapSet으로 추가하지만,
            // 실제로는 아티스트와 제목을 기준으로 그룹화해야 합니다.
            //GameManager.BeatmapSetList.AddSongGroup(beatmaps);

            // Beatmap들을 아티스트와 제목을 기준으로 그룹화
            Dictionary<string, List<Beatmap>> beatmapGroups = new Dictionary<string, List<Beatmap>>();

            foreach (Beatmap beatmap in beatmaps)
            {
                string key = $"{beatmap.artist}-{beatmap.title}";
                if (!beatmapGroups.ContainsKey(key))
                {
                    beatmapGroups[key] = new List<Beatmap>();
                }
                beatmapGroups[key].Add(beatmap);
            }

            // BeatmapSetList에 그룹화된 BeatmapSet 추가
            foreach (var group in beatmapGroups)
            {
                GameManager.BeatmapSetList.AddSongGroup(group.Value);
            }

            // 리스트를 재구성하기 위해 Reset 호출
             GameManager.BeatmapSetList.Reset();

            // 정렬 및 초기화
            GameManager.BeatmapSetList.Init();

            // 노드 목록 가져오기
            beatmapSetNodes = new List<BeatmapSetNode>();
            Debug.Log($"GameManager.BeatmapSetList.Size() : {GameManager.BeatmapSetList.Size()}");

            for (int i = 0; i < GameManager.BeatmapSetList.Size(); i++)
            {
                BeatmapSetNode node = GameManager.BeatmapSetList.GetBaseNode(i);
                node.InitializeUI(beatmapSetNodePrefab, scrollViewContent);
                beatmapSetNodes.Add(node);

                // 클릭 이벤트 등록
                node.OnClick += OnBeatmapSetNodeClick;
            }

            // 첫 번째 노드에 포커스 설정
            SetFocusNode(0);
        }
        else
        {
            Debug.LogError("곡 로딩 중 오류 발생: " + task.Exception.Message);
        }
    }

    // 포커스된 노드 설정
    public void SetFocusNode(int index)
    {
        if (index < 0 || index >= beatmapSetNodes.Count)
            return;
        Debug.Log($"SetFocusNode index : {index}, currentFocusIndex : {currentFocusIndex}");
        // 이전 포커스된 노드의 포커스 해제
        beatmapSetNodes[currentFocusIndex].SetFocus(false);

        // 새로운 노드에 포커스 설정
        currentFocusIndex = index;
        beatmapSetNodes[currentFocusIndex].SetFocus(true);
    }

    // BeatmapSetNode 클릭 시 처리
    private void OnBeatmapSetNodeClick(BeatmapSetNode node)
    {
        Debug.Log("OnBeatmapSetNodeClick");
        // 포커스된 노드 설정
        int index = beatmapSetNodes.IndexOf(node);
        SetFocusNode(index);

        // 노드 확장
        GameManager.BeatmapSetList.Expand(index);

        // UI 갱신
        RefreshUI();

        // 오디오 및 배경 이미지 설정
        Beatmap selectedBeatmap = node.GetSelectedBeatmap();
        if (selectedBeatmap != null)
        {
            GameManager.AudioManager.PlayPreview(selectedBeatmap);
            GameManager.BackgroundManager.SetBackgroundImage(selectedBeatmap);
        }
    }

    // UI 갱신 메서드 (확장/축소 시 호출)
    private void RefreshUI()
    {
        // 기존 UI 제거
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // 노드 목록 갱신
        beatmapSetNodes = new List<BeatmapSetNode>();
        Debug.Log($" GameManager.BeatmapSetList.Size() : { GameManager.BeatmapSetList.Size()}");
        for (int i = 0; i < GameManager.BeatmapSetList.Size(); i++)
        {
            BeatmapSetNode node = GameManager.BeatmapSetList.GetBaseNode(i) ?? GameManager.BeatmapSetList.GetNode(i);
            Debug.Log("노트 생성");
            node.InitializeUI(beatmapSetNodePrefab, scrollViewContent);
            beatmapSetNodes.Add(node);

            // 클릭 이벤트 등록
            node.OnClick += OnBeatmapSetNodeClick;
        }
    }
}
