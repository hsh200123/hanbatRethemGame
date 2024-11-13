using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� ������ Ŭ����
[System.Serializable]
public class Beatmap
{
    public string title;        
    public string artist;
    public string creator;
    public string version;      //���̵�
    public string audioFilename;//����� �̸�
    public string imageFilename;//�̹��� �̸�
    public string audioPath;    // ����� �ּ�
    public string imagePath;    // �̹��� �ּ�
    public int previewTime;
    public string tags;
    public string folderName;

 
    public bool favorite;

    // �߰��� �Ӽ���
    public int bpm;
    public int endTime;
    public DateTime dateAdded;
    public int playCount;
    public DateTime lastPlayed;
    public double starRating = -1;

    // ������
    public Beatmap()
    {
        dateAdded = DateTime.Now;
        playCount = 0;
        lastPlayed = DateTime.MinValue;
    }

    public void Initialize()
    {

    }
    // �÷��� ī���� ����
    public void IncrementPlayCounter()
    {
        playCount++;
        lastPlayed = DateTime.Now;
    }

    // ���ã�� ���
    public void ToggleFavorite()
    {
        favorite = !favorite;
    }
}

//   public AudioClip audioClip;     //����
//public Texture2D imageTexture;  //�̹���