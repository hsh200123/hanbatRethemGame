using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpenUI : MonoBehaviour
{
    public Button CreateAccountSetting;
    public Button CreateAccount;
    public Button Login;
    public Button OpenSettingButton;
    public Button CloseSettingButton;
    public TMP_InputField Input_id;
    public TMP_InputField Input_pw;
    public TMP_InputField Input_new_id;
    public TMP_InputField Input_new_pw;
    public GameObject OpenSettingCanvas;
    public GameObject AccountSettingCanvas;
    FBManager fbManager;

    void Start()
    {
        fbManager = new FBManager();
        FBInitailize();
        OpenSettingButton.onClick.AddListener(OnOpenSettingCanvas);
        CloseSettingButton.onClick.AddListener(OnCloseSettingCanvas);
        CreateAccountSetting.onClick.AddListener(OnOpenAccountCanvas);
        CreateAccount.onClick.AddListener(OnCloseAccountCanvas);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("인터넷에 연결되어 있지 않습니다.");
        }
    }
    async void FBInitailize()
    {
        await fbManager.InitializeFirebase();
    }

    void OnOpenSettingCanvas()
    {
        OpenSettingCanvas.SetActive(true);
    }

    void OnCloseSettingCanvas()
    {
        OpenSettingCanvas.SetActive(false);
    }
    void OnOpenAccountCanvas()
    {
        Debug.Log(2);
        AccountSettingCanvas.SetActive(true);
    }
    void OnCloseAccountCanvas()
    {
        AccountSettingCanvas.SetActive(false);
    }

    public async void LogIn()
    {
        if (!fbManager.isauth())
        {
            Debug.LogError("Firebase 인증이 초기화되지 않았습니다.");
            return;
        }
        string player_id = Input_new_id.text;
        string player_pw = Input_new_pw.text;
        await fbManager.LoginWithEmailPassword(player_id, player_pw);
    }

    public async void Create_Account()
    {
        if (!fbManager.isauth())
        {
            Debug.LogError("Firebase 인증이 초기화되지 않았습니다.");
            return;
        }
        string player_id = Input_id.text;
        string player_pw = Input_pw.text;
        await fbManager.SignUpWithEmailPassword(player_id,player_pw);
    }
}
