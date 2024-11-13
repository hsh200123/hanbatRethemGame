using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System;

// 곡 입력 처리 클래스
public class BeatmapCreator : MonoBehaviour
{
    public TMP_InputField titleInput;
    public TMP_InputField artistInput;
    public TMP_InputField creatorInput;
    public TMP_InputField versionInput;
    public Button uploadMusicButton;
    public Button uploadImageButton;
    public Button uploadBeatmapButton;
    public Button backButton;
    public TextMeshProUGUI debugText;

    private FileUploader fileUploader;

    private string uploadedMusicPath;
    private string uploadedImagePath;


    void Awake()
    {
        fileUploader = new FileUploader(new PCFileBrowser());

    }

    void Start()
    {
        // 버튼 클릭 이벤트 등록
        uploadBeatmapButton.onClick.AddListener(() => OnCreateBeatmap());
        backButton.onClick.AddListener(OnCloseCreateBeatmapCanvas);

        uploadMusicButton.onClick.AddListener(() => StartCoroutine(OnUploadMusic()));
        uploadImageButton.onClick.AddListener(() => StartCoroutine(OnUploadImage()));

    }



    // 음악 파일 업로드
    IEnumerator OnUploadMusic()
    {
        var task = fileUploader.UploadMusicFileAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        uploadedMusicPath = task.Result;
        if (!string.IsNullOrEmpty(uploadedMusicPath))
        {
            debugText.text = "음악 파일이 업로드되었습니다: " + uploadedMusicPath;
        }
        else
        {
            debugText.text = "음악 파일 업로드 실패.";
        }
    }

    // 이미지 파일 업로드
    IEnumerator OnUploadImage()
    {
        var task = fileUploader.UploadImageFileAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }

        uploadedImagePath = task.Result;
        if (!string.IsNullOrEmpty(uploadedImagePath))
        {
            debugText.text = "이미지 파일이 업로드되었습니다: " + uploadedImagePath;
        }
        else
        {
            debugText.text = "이미지 파일 업로드 실패.";
        }
    }

    // 곡 생성
    private async void OnCreateBeatmap()
    {
        string title = titleInput.text;
        string artist = artistInput.text;
        string creator = creatorInput.text;
        string level = versionInput.text;

        // 입력값 검증
        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(creator) || string.IsNullOrEmpty(level))
        {
            debugText.text = "모든 필드를 채워주세요.";
            return;
        }

        if (string.IsNullOrEmpty(uploadedMusicPath) || string.IsNullOrEmpty(uploadedImagePath))
        {
            debugText.text = "음악과 이미지를 업로드해주세요.";
            return;
        }

        // 중복되지 않는 큰 값의 랜덤 ID 생성
        string uniqueId = GenerateUniqueID();

        var createTask = CreateBeatmapFolderAsync(uniqueId, title, artist, creator, level, uploadedMusicPath, uploadedImagePath);
        await createTask;

        if (createTask.Exception == null)
        {
            // 곡 생성 성공 시 Firebase 업로드
            await UploadBeatmapToFirebase(uniqueId);
            gameObject.SetActive(false);
        }
        else
        {
            debugText.text = "곡 생성 중 오류 발생: " + createTask.Exception.Message;
        }
    }

    // Firebase에 비트맵 업로드
    private async Task UploadBeatmapToFirebase(string uniqueId)
    {
        string localFolderPath = Path.Combine(Application.persistentDataPath, "Songs", $"{uniqueId} {artistInput.text} - {titleInput.text}").Replace("/", "\\");
        try
        {
            await GameManager.FBManager.UploadBeatmapAsync(localFolderPath);
            debugText.text = "곡이 Firebase에 성공적으로 업로드되었습니다!";
        }
        catch (Exception ex)
        {
            debugText.text = "Firebase 업로드 중 오류 발생: " + ex.Message;
        }
    }

    public async Task CreateBeatmapFolderAsync(string uniqueId, string title, string artist, string creator, string level, string mp3Path, string imagePath)
    {
        try
        {
            // 파일 이름에 사용할 수 없는 문자 제거
            string sanitizedTitle = SanitizeFileName(title);
            string sanitizedArtist = SanitizeFileName(artist);
            string sanitizedCreater = SanitizeFileName(creator);

            // 곡 폴더 이름 생성
            string folderName = $"{uniqueId} {sanitizedArtist} - {sanitizedTitle}";
            string folderPath = Path.Combine(Application.persistentDataPath, "Songs", folderName);
            Debug.Log($"folderPath : {folderPath}");
            // 폴더 생성 여부 확인
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Log($"폴더가 생성되었습니다: {folderPath}");
            }

            // 동일한 레벨 파일 존재 여부 확인
            string levelFileName = $"{sanitizedArtist} {sanitizedTitle} ({sanitizedCreater}) [{level}].txt";
            string levelFilePath = Path.Combine(folderPath, levelFileName);

            if (File.Exists(levelFilePath))
            {
                Debug.LogError("같은 레벨의 파일이 이미 존재합니다.");
                return;
            }

            // 음악 파일 복사
            string audioExtension = Path.GetExtension(mp3Path);
            string audioName = $"song{audioExtension}";
            string audioPath = Path.Combine(folderPath, audioName);
            await CopyFileAsync(mp3Path, audioPath);
            Debug.Log($"음악 파일이 song{audioExtension}로 저장되었습니다: {audioPath}");

            // 이미지 파일 복사
            string imageExtension = Path.GetExtension(imagePath);
            string imageName = $"image{imageExtension}";
            string imageDestinationPath = Path.Combine(folderPath, imageName);
            await CopyFileAsync(imagePath, imageDestinationPath);
            Debug.Log($"이미지 파일이 image{imageExtension}로 저장되었습니다: {imageDestinationPath}");

            // 레벨 파일 생성
            await CreateLevelFileAsync(uniqueId, folderPath, sanitizedTitle, sanitizedArtist, sanitizedCreater, level, audioPath, audioName, imageName);

        }
        catch (Exception ex)
        {
            Debug.LogError("비트맵 생성 중 오류 발생: " + ex.Message);
        }
    }

    // 파일 복사 비동기 처리
    private async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        try
        {
            using (FileStream sourceStream = File.Open(sourcePath, FileMode.Open))
            using (FileStream destinationStream = File.Create(destinationPath))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("파일 복사 중 오류 발생: " + ex.Message);
            throw;
        }
    }

    // 레벨 파일 생성
    private async Task CreateLevelFileAsync(string uniqueId, string folderPath, string title, string artist, string creator, string version, string audioPath, string audioName, string imageName)
    {
        try
        {
            string levelFileName = $"{artist} - {title} ({creator}) {version}.txt";
            string levelFilePath = Path.Combine(folderPath, levelFileName);

            string mp3FileName = Path.GetFileName(audioPath);

            // 오디오 길이 가져오기
            int audioLength = await GetAudioLengthAsync(audioPath);
            int previewTime = UnityEngine.Random.Range(0, audioLength);

            // 현재 시간 (dateAdded)
            DateTime dateAdded = DateTime.Now;

            // 파일 내용 작성
            string fileContent =
                $@"Id:{uniqueId }
Title:{title}
Artist:{artist}
Creator:{creator}
Version:{version}
AudioFilename:{audioName}
ImageFilename:{imageName}
PreviewTime:{previewTime}
DateAdded:{dateAdded.ToString("yyyy/MM/dd HH:mm:ss")}"
;

            await File.WriteAllTextAsync(levelFilePath, fileContent);
            Debug.Log($"레벨 파일이 저장되었습니다: {levelFilePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError("레벨 파일 생성 중 오류 발생: " + ex.Message);
            throw;
        }
    }

    // 오디오 길이 가져오기
    private async Task<int> GetAudioLengthAsync(string audioPath)
    {
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + audioPath, AudioType.MPEG))
        {
            await www.SendWebRequestAsync();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                return (int)(clip.length * 1000); // 밀리초 단위로 변환
            }
            else
            {
                Debug.LogError("오디오 길이 가져오기 실패: " + www.error);
                return 120000; // 기본값으로 2분 설정
            }
        }
    }

    // 고유 ID 생성
    private string GenerateUniqueID()
    {
        return Guid.NewGuid().ToString("N");
    }

    // 파일 이름 정규화 (특수 문자 제거)
    private string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
    // 곡 생성 화면 닫기
    void OnCloseCreateBeatmapCanvas()
    {
        gameObject.SetActive(false);
    }

}
