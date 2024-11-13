using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseBeatmapSelectController : MonoBehaviour
{
    public Button createButton;
    public GameObject createMusicCanvas;
    void Start()
    {
        createButton.onClick.AddListener(OnOpenCreateMusicCanvas);
    }


    void OnOpenCreateMusicCanvas()
    {
        createMusicCanvas.SetActive(true);
    }

}
