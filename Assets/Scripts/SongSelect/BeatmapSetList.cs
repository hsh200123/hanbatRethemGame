using System;
using System.Collections.Generic;

public class BeatmapSetList
{
    // 전체 BeatmapSetNode 목록
    public List<BeatmapSetNode> parsedNodes { get; private set; }

    // 현재 그룹에 따른 노드 목록
    private List<BeatmapSetNode> groupNodes;

    // 검색 결과에 따른 노드 목록
    private List<BeatmapSetNode> nodes;

    // 전체 Beatmap 수
    private int mapCount = 0;

    // 현재 확장된 노드의 인덱스 (-1이면 확장 없음)
    private int expandedIndex = -1;

    // 확장된 노드의 시작과 끝
    private BeatmapSetNode expandedStartNode;
    private BeatmapSetNode expandedEndNode;

    // 마지막 검색어
    private string lastQuery;


    public BeatmapSetList()
    {
        parsedNodes = new List<BeatmapSetNode>();
        Reset();
    }

    // 리스트 초기화
    public void Reset()
    {
        groupNodes = BeatmapGroupExtensions.Current.Filter(parsedNodes);
        nodes = new List<BeatmapSetNode>(groupNodes);
        expandedIndex = -1;
        expandedStartNode = null;
        expandedEndNode = null;
        lastQuery = null;
    }

    // 노드 추가
    public void AddSongGroup(List<Beatmap> beatmaps)
    {
        BeatmapSet beatmapSet = new BeatmapSet(beatmaps);
        BeatmapSetNode node = new BeatmapSetNode(beatmapSet);
        parsedNodes.Add(node);
        mapCount += beatmaps.Count;
    }

    // 노드 개수 반환
    public int Size()
    {
        return nodes.Count;
    }

    // 정렬 초기화
    public void Init()
    {
        if (Size() < 1)
            return;

        // 정렬
        nodes.Sort(BeatmapSortOrderExtensions.Current.GetComparator());
        expandedIndex = -1;
        expandedStartNode = null;
        expandedEndNode = null;

        // 링크드 리스트 구성
        BeatmapSetNode lastNode = nodes[0];
        lastNode.index = 0;
        lastNode.prev = null;
        for (int i = 1; i < Size(); i++)
        {
            BeatmapSetNode node = nodes[i];
            lastNode.next = node;
            node.index = i;
            node.prev = lastNode;

            lastNode = node;
        }
        lastNode.next = null;
    }

    // 노드 확장
    public void Expand(int index)
    {
        Unexpand();

        BeatmapSetNode node = GetBaseNode(index);
        if (node == null)
            return;

        expandedStartNode = null;
        expandedEndNode = null;

        BeatmapSet beatmapSet = node.beatmapSet;
        BeatmapSetNode prevNode = node.prev;
        BeatmapSetNode nextNode = node.next;

        for (int i = 0; i < beatmapSet.Count; i++)
        {
            BeatmapSetNode newNode = new BeatmapSetNode(beatmapSet)
            {
                index = index,
                beatmapIndex = i,
                prev = i == 0 ? prevNode : nodes[nodes.Count - 1]
            };

            if (i == 0)
            {
                expandedStartNode = newNode;
                if (prevNode != null)
                    prevNode.next = newNode;
            }
            else
            {
                nodes[nodes.Count - 1].next = newNode;
            }

            nodes.Insert(index + i, newNode);
        }

        if (nextNode != null)
        {
            nodes[nodes.Count - 1].next = nextNode;
            nextNode.prev = nodes[nodes.Count - 1];
        }

        expandedEndNode = nodes[nodes.Count - 1];
        expandedIndex = index;
    }

    // 노드 축소
    public void Unexpand()
    {
        if (expandedIndex < 0 || expandedIndex >= nodes.Count)
            return;

        int startIndex = expandedIndex;
        int count = expandedEndNode.index - expandedStartNode.index + 1;

        // 링크드 리스트 재구성
        BeatmapSetNode prevNode = expandedStartNode.prev;
        BeatmapSetNode nextNode = expandedEndNode.next;

        if (prevNode != null)
            prevNode.next = nextNode;
        if (nextNode != null)
            nextNode.prev = prevNode;

        nodes.RemoveRange(startIndex, count);

        expandedIndex = -1;
        expandedStartNode = null;
        expandedEndNode = null;
    }

    // 기본 노드 반환 (확장 고려 안 함)
    public BeatmapSetNode GetBaseNode(int index)
    {
        if (index < 0 || index >= nodes.Count)
            return null;

        BeatmapSetNode node = nodes[index];
        // 확장된 노드가 아닌 기본 노드를 반환
        return node.beatmapIndex == -1 ? node : null;
    }

    // 검색 기능
    public void Search(string query)
    {
        if (query == null)
            return;

        query = query.Trim().ToLower();
        if (lastQuery != null && query.Equals(lastQuery))
            return;

        lastQuery = query;
        List<string> terms = new List<string>(query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

        if (query.Length == 0 || terms.Count == 0)
        {
            nodes = new List<BeatmapSetNode>(groupNodes);
            return;
        }

        nodes = new List<BeatmapSetNode>();

        foreach (BeatmapSetNode node in groupNodes)
        {
            bool matches = true;
            foreach (string term in terms)
            {
                if (!node.beatmapSet.Matches(term))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
                nodes.Add(node);
        }
    } 
   
    public BeatmapSetNode GetNode(int index)
    {
        if (index < 0 || index >= nodes.Count)
            return null;
        return nodes[index];
    }
}
