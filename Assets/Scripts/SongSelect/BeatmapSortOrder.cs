using System;
using System.Collections.Generic;

public enum BeatmapSortOrder
{
    TITLE,
    ARTIST,
    CREATOR,
    BPM,
    LENGTH,
    DATE,
    PLAYS
}

public static class BeatmapSortOrderExtensions
{
    private static BeatmapSortOrder currentSort = BeatmapSortOrder.TITLE;

    public static BeatmapSortOrder Current
    {
        get { return currentSort; }
        set { currentSort = value; }
    }

    public static IComparer<BeatmapSetNode> GetComparator(this BeatmapSortOrder sortOrder)
    {
        switch (sortOrder)
        {
            case BeatmapSortOrder.TITLE:
                return new TitleComparer();
            case BeatmapSortOrder.ARTIST:
                return new ArtistComparer();
            case BeatmapSortOrder.CREATOR:
                return new CreatorComparer();
            case BeatmapSortOrder.BPM:
                return new BPMComparer();
            case BeatmapSortOrder.LENGTH:
                return new LengthComparer();
            case BeatmapSortOrder.DATE:
                return new DateComparer();
            case BeatmapSortOrder.PLAYS:
                return new PlayCountComparer();
            default:
                return new TitleComparer();
        }
    }

    // 각 비교자 구현
    private class TitleComparer : IComparer<BeatmapSetNode>
    {
        public int Compare(BeatmapSetNode x, BeatmapSetNode y)
        {
            return string.Compare(x.beatmapSet.Get(0).title, y.beatmapSet.Get(0).title, StringComparison.OrdinalIgnoreCase);
        }
    }

    private class ArtistComparer : IComparer<BeatmapSetNode>
    {
        public int Compare(BeatmapSetNode x, BeatmapSetNode y)
        {
            return string.Compare(x.beatmapSet.Get(0).artist, y.beatmapSet.Get(0).artist, StringComparison.OrdinalIgnoreCase);
        }
    }

    private class CreatorComparer : IComparer<BeatmapSetNode>
    {
        public int Compare(BeatmapSetNode x, BeatmapSetNode y)
        {
            return string.Compare(x.beatmapSet.Get(0).creator, y.beatmapSet.Get(0).creator, StringComparison.OrdinalIgnoreCase);
        }
    }

    private class BPMComparer : IComparer<BeatmapSetNode>
    {
        public int Compare(BeatmapSetNode x, BeatmapSetNode y)
        {
            int bpmX = x.beatmapSet.Get(0).bpm;
            int bpmY = y.beatmapSet.Get(0).bpm;
            return bpmX.CompareTo(bpmY);
        }
    }

    private class LengthComparer : IComparer<BeatmapSetNode>
    {
        public int Compare(BeatmapSetNode x, BeatmapSetNode y)
        {
            int lengthX = 0;
            int lengthY = 0;

            foreach (Beatmap beatmap in x.beatmapSet)
            {
                int endTime = beatmap.endTime;
                if (endTime > lengthX)
                    lengthX = endTime;
            }

            foreach (Beatmap beatmap in y.beatmapSet)
            {
                int endTime = beatmap.endTime;
                if (endTime > lengthY)
                    lengthY = endTime;
            }

            return lengthX.CompareTo(lengthY);
        }
    }

    private class DateComparer : IComparer<BeatmapSetNode>
    {
        public int Compare(BeatmapSetNode x, BeatmapSetNode y)
        {
            DateTime dateX = x.beatmapSet.Get(0).dateAdded;
            DateTime dateY = y.beatmapSet.Get(0).dateAdded;
            return dateX.CompareTo(dateY);
        }
    }

    private class PlayCountComparer : IComparer<BeatmapSetNode>
    {
        public int Compare(BeatmapSetNode x, BeatmapSetNode y)
        {
            int playCountX = 0;
            int playCountY = 0;

            foreach (Beatmap beatmap in x.beatmapSet)
            {
                int playCount = beatmap.playCount;
                playCountX += playCount;
            }

            foreach (Beatmap beatmap in y.beatmapSet)
            {
                int playCount = beatmap.playCount;
                playCountY += playCount;
            }

            return playCountX.CompareTo(playCountY);
        }
    }
}
