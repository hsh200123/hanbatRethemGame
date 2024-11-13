using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System;

// 파일 선택 인터페이스
public interface IFileBrowser
{
    Task<string> OpenFilePanelAsync(string title, string fileTypes);
}

// PC용 파일 선택기 구현
public class PCFileBrowser : IFileBrowser
{
    public Task<string> OpenFilePanelAsync(string title, string fileTypes)
    {
        // 지원하는 파일 확장자 필터 생성
        string[] extensions = fileTypes.Split(',');
        string filters = string.Join(",", extensions);
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters(title, "", new string[] { "Files", filters });

        return Task.FromResult(string.IsNullOrEmpty(path) ? null : path);
    }
}


// 파일 업로드 클래스
public class FileUploader
{
    private IFileBrowser fileBrowser;

    public FileUploader(IFileBrowser fileBrowser)
    {
        this.fileBrowser = fileBrowser;
    }

    // 음악 파일 업로드
    public async Task<string> UploadMusicFileAsync()
    {
        try
        {
            // mp3 파일 선택
            string path = await fileBrowser.OpenFilePanelAsync("Select MP3 file", "mp3");

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.Log("음악 파일이 성공적으로 선택되었습니다: " + path);
                return path;
            }
            else
            {
                Debug.Log("음악 파일을 선택하지 않았거나 파일이 존재하지 않습니다.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("음악 파일 업로드 중 오류 발생: " + ex.Message);
            return null;
        }
    }

    // 이미지 파일 업로드 (PNG, JPG 지원)
    public async Task<string> UploadImageFileAsync()
    {
        try
        {
            // 이미지 파일 선택
            string path = await fileBrowser.OpenFilePanelAsync("Select Image file", "png,jpg,jpeg");

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                Debug.Log("이미지 파일이 성공적으로 선택되었습니다: " + path);
                return path;
            }
            else
            {
                Debug.Log("이미지 파일을 선택하지 않았거나 파일이 존재하지 않습니다.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("이미지 파일 업로드 중 오류 발생: " + ex.Message);
            return null;
        }
    }
}
