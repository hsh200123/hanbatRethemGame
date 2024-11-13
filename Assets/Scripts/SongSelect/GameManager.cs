using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager s_instance;
    public static GameManager Instance { get { Init(); return s_instance; } }


    public static FBManager FBManager { get { return Instance.fbManager; } }
    public static BeatmapSetList BeatmapSetList { get { return Instance.beatmapSetList; } }
    public static AudioManager AudioManager { get { return Instance.audioManager; } }
    public static BackgroundManager BackgroundManager { get { return Instance.backgroundManager; } }
    public static ResourceCache ResourceCache { get {return Instance.resourceCache; } }

    private FBManager fbManager = new FBManager();
    private BeatmapSetList beatmapSetList = new BeatmapSetList();
    private AudioManager audioManager = new AudioManager();
    private BackgroundManager backgroundManager = new BackgroundManager();
    private ResourceCache resourceCache = new ResourceCache();
    private void Start()
    {
        Init();
    }

    private static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                s_instance = go.AddComponent<GameManager>();
            }

            DontDestroyOnLoad(go);
            s_instance.InitializeManagers();
        }
    }

    private async void InitializeManagers()
    {
        audioManager.Init();
        await fbManager.InitializeFirebase();
    }
}