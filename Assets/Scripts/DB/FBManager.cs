using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine;

//Beatmap.cs            비트맵의 정보를 보유한 클래스 
//FBManager.cs          파이어베이스 관리
//DBManager.cs          로컬 db 관리
//GameManager.cs        각종 Manager를 Singletone을 통해 통합 관리
//BeatmapCreator.cs     비트맵을 생성하고 비트맵을 txt화한 파일, 곡, 이미지를 로컬에 저장하는 클래스. (사용자 입력 정보 : 비트맵 제목, 비트맵 아티스트, 비트맵 제작자, 비트맵 곡(mp3), 비트맵 이미지)
//BeatmapUploader.cs    BeatmapCreator에서 생성한 로컬에 저장되어 있는 비트맵을 txt화한 파일, 곡, 이미지를 파이어베이스에 업로드하는 클래스
//BeatmapParser.cs      로컬에 저장된 비트맵 정보를 읽어들이는 클래스.비트맵을 txt화한 파일들을 beatmap클래스로 파싱하고, 이미지와 곡 정보를 불러옴
//클래스이름못정함.cs   파이어베이스에 업로드 된 비트맵을 ScrollView로 보여주는 UI 클래스(가장 최근에 파이어베이스에 업로드 된 비트맵을 기준으로 최대 30개 묶음으로. 그 이상은 화살표 버튼을 통해 로드)
//클래스이름못정함.cs   로컬에 저장된 비트맵을 ScrollView로 보여주는 UI 클래스

//(서버에 업로드 된 비트맵을 ScrollView로 보여주는 UI 클래스에서 비트맵 클릭 시 로컬 db에 비트맵을 다운받는 방식) - 이건 어디에 작성하는게 좋을까
//(내부 저장소에 저장되어 있는 파일들을 로컬 db에 저장?해주는 클래스. 향후 다시 게임을 시작했을 때 바로 로딩이 되게 끔 하기 위함.))
public class FBManager
{
    public string FBurl = "https://rethemgame-default-rtdb.firebaseio.com/";  // 여기를 Firebase Console의 Database URL로 변경
    public string StorageBucketUrl = "gs://rethemgame.firebasestorage.app"; // 여기를 Firebase Console의 Storage Bucket URL로 변경

    private DatabaseReference databaseRef;
    private FirebaseStorage storage;
    private bool isOnline = true;

    // Firebase 초기화
    public async Task InitializeFirebase()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            // Firebase 앱 초기화
            FirebaseApp app = FirebaseApp.DefaultInstance;

            // Database URL과 Storage Bucket URL을 명시적으로 설정
            app.Options.DatabaseUrl = new Uri(FBurl);
            app.Options.StorageBucket = StorageBucketUrl;

            // Firebase Database와 Storage 인스턴스 초기화
            databaseRef = FirebaseDatabase.GetInstance(app, FBurl).RootReference;
            storage = FirebaseStorage.GetInstance(app, StorageBucketUrl);

            Debug.Log("Firebase가 성공적으로 초기화되었습니다.");
        }
        else
        {
            Debug.LogError("Firebase 의존성 확인 실패: " + dependencyStatus);
            isOnline = false; // Firebase 초기화 실패 시 오프라인 상태로 설정
        }
    }


    // 오프라인 상태 확인
    public bool IsOnline()
    {
        return isOnline;
    }

    // 비트맵 업로드
    public async Task UploadBeatmapAsync(string localFolderPath)
    {
        if (!IsOnline())
        {
            Debug.LogWarning("오프라인 상태입니다. Firebase에 업로드할 수 없습니다.");
            return;
        }

        // Firebase에서 고유 ID 가져오기
        string uniqueId = await GetNextBeatmapIdAsync();

        // 로컬 폴더 이름을 고유 ID로 변경
        DirectoryInfo dirInfo = new DirectoryInfo(localFolderPath);



        // 기존 ID 제거: 폴더 이름에서 처음 만나는 공백 뒤의 텍스트만 사용
        string existingFolderName = dirInfo.Name;
        string folderNameWithoutId = existingFolderName.Contains(" ") ? existingFolderName.Substring(existingFolderName.IndexOf(" ") + 1) : existingFolderName;

        // 새로운 ID와 기존 이름에서 ID를 제거한 폴더 이름 결합
        string newFolderName = $"{uniqueId} {folderNameWithoutId}";
        string newFolderPath = Path.Combine(dirInfo.Parent.FullName, newFolderName);

        try
        {
            Directory.Move(localFolderPath, newFolderPath);
            Debug.Log("폴더 이름 변경 완료.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to move folder: {ex.Message}");
        }


        // 새 폴더 경로에 대한 DirectoryInfo 재설정
        dirInfo = new DirectoryInfo(newFolderPath);
        FileInfo[] txtFiles = dirInfo.GetFiles("*.txt");

        string audioFilename = null;
        string imageFilename = null;
        bool uploadSuccess = true; // 업로드 성공 여부 확인을 위한 변수

        try
        {
            // 텍스트 파일에서 AudioFilename, ImageFilename 추출 및 Id 업데이트
            foreach (FileInfo txtFile in txtFiles)
            {
                // 텍스트 파일 내용 수정 및 업로드
                var updateResult = await UpdateIdInTextFile(txtFile.FullName, uniqueId);
                Debug.Log($"텍스트 파일 {txtFile.Name}의 Id가 업데이트되었습니다.");

                audioFilename = updateResult.audioFilename;
                imageFilename = updateResult.imageFilename;


                // txt 파일 Firebase Storage에 업로드
                string firebaseStoragePath = $"Songs/{newFolderName}/{txtFile.Name}";
                await UploadFileToFirebaseStorage(txtFile.FullName, firebaseStoragePath);
                Debug.Log($"난이도 파일 {txtFile.Name}이 Firebase Storage에 업로드되었습니다.");
            }

            // AudioFilename과 ImageFilename에 해당하는 파일만 업로드
            foreach (FileInfo file in dirInfo.GetFiles())
            {
                if (file.Name == audioFilename || file.Name == imageFilename)
                {
                    string firebaseStoragePath = $"Songs/{newFolderName}/{file.Name}";
                    await UploadFileToFirebaseStorage(file.FullName, firebaseStoragePath);
                    Debug.Log($"{file.Name} 파일이 Firebase Storage에 업로드되었습니다.");
                }
            }
        }
        catch (Exception ex)
        {
            uploadSuccess = false;
            Debug.LogError($"파일 업로드 중 오류 발생: {ex.Message}");
        }

        // 업로드 성공 시에만 ID 증가
        if (uploadSuccess)
        {
            await IncrementBeatmapIdAsync(uniqueId);
        }
        else
        {
            Debug.LogWarning($"업로드가 실패하여 ID 증가가 중단되었습니다: {uniqueId}");
        }
    }
    // 텍스트 파일에서 Id 값을 업데이트하고 나머지 정보를 추출하는 메서드
    private async Task<(string audioFilename, string imageFilename)> UpdateIdInTextFile(string filePath, string newId)
    {
        var updatedLines = new List<string>();
        string audioFilename = null;
        string imageFilename = null;

        string[] lines = await File.ReadAllLinesAsync(filePath);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
            {
                updatedLines.Add(line);
                continue;
            }

            int colonIndex = line.IndexOf(':');
            if (colonIndex == -1)
            {
                updatedLines.Add(line);
                continue;
            }

            string key = line.Substring(0, colonIndex).Trim();
            string value = line.Substring(colonIndex + 1).Trim();

            switch (key)
            {
                case "Id":
                    updatedLines.Add($"Id:{newId}");
                    break;
                case "AudioFilename":
                    audioFilename = value;
                    updatedLines.Add(line);
                    break;
                case "ImageFilename":
                    imageFilename = value;
                    updatedLines.Add(line);
                    break;
                default:
                    updatedLines.Add(line);
                    break;
            }
        }

        // 파일 업데이트
        await File.WriteAllLinesAsync(filePath, updatedLines);

        return (audioFilename, imageFilename);
    }
    // Firebase Storage에 파일 업로드 메서드
    private async Task UploadFileToFirebaseStorage(string localFilePath, string firebaseStoragePath)
    {
        // Firebase Storage의 특정 URL 참조 가져오기
        var storageReference = storage.GetReferenceFromUrl(StorageBucketUrl).Child(firebaseStoragePath);
        var uploadTask = storageReference.PutFileAsync(localFilePath);

        await uploadTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"파일 업로드 실패: {localFilePath}");
            }
            else
            {
                Debug.Log($"파일 업로드 성공: {localFilePath}");
            }
        });
    }

    // Firebase에서 다음 고유 ID 가져오기
    private async Task<string> GetNextBeatmapIdAsync()
    {
        var idSnapshot = await databaseRef.Child("NextBeatmapId").GetValueAsync();
        int currentId = idSnapshot.Exists ? int.Parse(idSnapshot.Value.ToString()) : -1;
        Debug.Log($"currentid: {currentId}");
        return currentId.ToString();
    }

    // 업로드 성공 시 고유 ID를 증가시키는 메서드
    private async Task IncrementBeatmapIdAsync(string currentId)
    {
        int nextId = int.Parse(currentId) + 1;
        await databaseRef.Child("NextBeatmapId").SetValueAsync(nextId);
        Debug.Log($"NextBeatmapId id증가 {nextId}");
    }
}