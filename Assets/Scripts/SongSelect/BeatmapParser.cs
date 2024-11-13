using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class BeatmapParser
{
    public async Task<List<Beatmap>> ParserAllBeatmapsAsync()
    {
        List<Beatmap> beatmaps = new List<Beatmap>();
        List<string> audioPaths = new List<string>();
        List<string> imagePaths = new List<string>();

        try
        {
            Debug.Log("너냐1");
            string songsDirectory = Path.Combine(Application.persistentDataPath, "Songs");
            Debug.Log("너냐2");
            if (!Directory.Exists(songsDirectory))
            {
                Debug.LogWarning("Songs 디렉토리가 존재하지 않습니다.");
                return beatmaps;
            }
            Debug.Log(songsDirectory);
            DirectoryInfo dirInfo = new DirectoryInfo(songsDirectory);
            DirectoryInfo[] songFolders = dirInfo.GetDirectories();

            foreach (DirectoryInfo songFolder in songFolders)
            {
                FileInfo[] txtFiles = songFolder.GetFiles("*.txt");

                foreach (FileInfo txtFile in txtFiles)
                {
                    Beatmap beatmap = await ParseBeatmapFileAsync(txtFile.FullName);

                    if (beatmap != null)
                    {
                        beatmap.audioPath = Path.Combine(songFolder.FullName, $"{beatmap.audioFilename}");
                        beatmap.imagePath = Path.Combine(songFolder.FullName, $"{beatmap.imageFilename}");

                        audioPaths.Add(beatmap.audioPath);
                        imagePaths.Add(beatmap.imagePath);

                        beatmaps.Add(beatmap);
                    }
                }
            }

            await GameManager.ResourceCache.PreloadBeatmapResourcesAsync(audioPaths, imagePaths);

        }
        catch (Exception ex)
        {
            Debug.LogError("곡 로딩 중 오류 발생: " + ex.Message);
            throw;
        }

        return beatmaps;
    }

    private async Task<Beatmap> ParseBeatmapFileAsync(string filePath)
    {
        try
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            Beatmap beatmap = new Beatmap();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                // 첫 번째 콜론 위치 찾기
                int colonIndex = line.IndexOf(':');
                if (colonIndex == -1) continue;  // 콜론이 없으면 건너뜀

                string key = line.Substring(0, colonIndex).Trim();
                string value = line.Substring(colonIndex + 1).Trim();

                switch (key)
                {
                    case "Title":
                        beatmap.title = value;
                        break;
                    case "Artist":
                        beatmap.artist = value;
                        break;
                    case "Creator":
                        beatmap.creator = value;
                        break;
                    case "Version":
                        beatmap.version = value;
                        break;
                    case "AudioFilename":
                        beatmap.audioFilename = value;
                        break;
                    case "ImageFilename":
                        beatmap.imageFilename = value;
                        break;
                    case "PreviewTime":
                        beatmap.previewTime = int.Parse(value);
                        break;
                    case "DateAdded":
                        try
                        {
                            beatmap.dateAdded = DateTime.ParseExact(
                                value,
                                "yyyy-MM-dd HH:mm:ss",
                                null
                            );
                        }
                        catch (FormatException)
                        {
                            Debug.LogWarning($"DateAdded 형식이 올바르지 않음: {value}");
                        }
                        break;
                    case "FolderName":
                        beatmap.folderName = value;
                        break;
                    // 필요한 경우 추가적인 키 처리 (예: BPM, EndTime 등)
                    default:
                        break;
                }
            }

            return beatmap;
        }
        catch (Exception ex)
        {
            Debug.LogError("곡 파싱 중 오류 발생: " + ex.Message);
            return null;
        }
    }

}
