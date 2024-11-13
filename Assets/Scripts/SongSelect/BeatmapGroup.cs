using System;
using System.Collections.Generic;

public enum BeatmapGroup
{
    ALL,
    RECENT,
    FAVORITE
}

public static class BeatmapGroupExtensions
{
    private static BeatmapGroup currentGroup = BeatmapGroup.ALL;

    public static BeatmapGroup Current
    {
        get { return currentGroup; }
        set { currentGroup = value; }
    }

    public static List<BeatmapSetNode> Filter(this BeatmapGroup group, List<BeatmapSetNode> list)
    {
        switch (group)
        {
            case BeatmapGroup.ALL:
                return new List<BeatmapSetNode>(list);
            case BeatmapGroup.RECENT:
                return FilterRecent(list);
            case BeatmapGroup.FAVORITE:
                return FilterFavorite(list);
            default:
                return new List<BeatmapSetNode>(list);
        }
    }

    private static List<BeatmapSetNode> FilterRecent(List<BeatmapSetNode> list)
    {
        int K = 20; // 최근 플레이한 곡 수
        List<BeatmapSetNode> recentList = new List<BeatmapSetNode>();

        list.Sort((x, y) =>
        {
            DateTime lastPlayedX = DateTime.MinValue;
            DateTime lastPlayedY = DateTime.MinValue;

            foreach (Beatmap beatmap in x.beatmapSet)
            {
                DateTime lastPlayed = beatmap.lastPlayed;
                if (lastPlayed > lastPlayedX)
                    lastPlayedX = lastPlayed;
            }

            foreach (Beatmap beatmap in y.beatmapSet)
            {
                DateTime lastPlayed = beatmap.lastPlayed;
                if (lastPlayed > lastPlayedY)
                    lastPlayedY = lastPlayed;
            }

            return lastPlayedY.CompareTo(lastPlayedX); // 내림차순 정렬
        });

        foreach (BeatmapSetNode node in list)
        {
            if (recentList.Count >= K)
                break;

            DateTime lastPlayed = DateTime.MinValue;

            foreach (Beatmap beatmap in node.beatmapSet)
            {
                DateTime beatmapLastPlayed = beatmap.lastPlayed;
                if (beatmapLastPlayed > lastPlayed)
                    lastPlayed = beatmapLastPlayed;
            }

            if (lastPlayed != DateTime.MinValue)
                recentList.Add(node);
        }

        return recentList;
    }

    private static List<BeatmapSetNode> FilterFavorite(List<BeatmapSetNode> list)
    {
        List<BeatmapSetNode> favoriteList = new List<BeatmapSetNode>();

        foreach (BeatmapSetNode node in list)
        {
            if (node.beatmapSet.IsFavorite())
                favoriteList.Add(node);
        }

        return favoriteList;
    }
}
