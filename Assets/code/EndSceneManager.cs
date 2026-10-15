using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;

[System.Serializable]
public class AnswerEndingMapping
{
    public Image answerImage;
    public VideoClip endingVideo;
}

public class EndSceneManager : MonoBehaviour
{
    [Header("Answers Settings")]
    [Tooltip("กำหนดปุ่มคำตอบแต่ละอันให้คู่กับไฟล์วิดีโอ (ถ้าตอนจบเหมือนกันก็ใช้ไฟล์เดียวกันได้)")]
    public List<AnswerEndingMapping> answerMappings;

    [Header("Positions")]
    [Tooltip("ตำแหน่งว่างๆ ตรงกลางซ้าย ที่อยากให้ปุ่มเด้งไปหา")]
    public RectTransform selectedPosition; 

    [Header("UI Controls")]
    [Tooltip("ปุ่มกดยืนยันเพื่อดูตอนจบ")]
    public Button confirmButton; 
    [Tooltip("หน้าจอ UI ตัวหนังสือ Press Enter to Skip")]
    public GameObject skipUI; 
    [Tooltip("UI อื่นๆ ในฉากที่ต้องการให้ซ่อนตอนวิดีโอเล่น (เช่น พื้นหลัง, ตัวหนังสืออื่นๆ)")]
    public GameObject[] otherUIToHide;

    [Header("Video Settings")]
    [Tooltip("ตัวเล่นวิดีโอ")]
    public VideoPlayer videoPlayer;

    private Image currentSelectedImage;
    private Dictionary<Image, Vector3> originalPositions = new Dictionary<Image, Vector3>();
    private bool isVideoPlaying = false;

    void Start()
    {
        // ปลดเมาส์เผื่อตอนจบมาแล้วเมาส์ล็อคอยู่
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // บันทึกตำแหน่งเดิมของแต่ละรูป และตั้งค่าให้เวลากดแล้วเด้งไปที่ๆ กำหนด
        foreach (var mapping in answerMappings)
        {
            if (mapping.answerImage != null)
            {
                originalPositions[mapping.answerImage] = mapping.answerImage.transform.position;
                
                // ตรวจสอบว่า Image นี้มี Button Component หรือยัง ถ้ายังไม่มีให้แอบเพิ่มเข้าไปอัตโนมัติเพื่อให้คลิกได้
                Button btn = mapping.answerImage.GetComponent<Button>();
                if (btn == null)
                {
                    btn = mapping.answerImage.gameObject.AddComponent<Button>();
                }
                
                // จำเป็นต้องสร้างตัวแปรรับค่าใน Loop เพื่อป้องกันบั๊ก Closure ใน C#
                Image img = mapping.answerImage;
                btn.onClick.AddListener(() => SelectAnswer(img));
            }
        }

        // ตั้งค่าปุ่มยืนยัน
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(PlayEnding);
        }

        // ซ่อน UI ของการข้ามวิดีโอไว้ก่อน
        if (skipUI != null) skipUI.SetActive(false);

        // ดักจับเมื่อวิดีโอเล่นจนจบตามธรรมชาติ
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += EndVideo;
        }
    }

    void Update()
    {
        // ถ้าวิดีโอกำลังเล่นอยู่ และกด Enter ให้ทำการ Skip
        if (isVideoPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Debug.Log("Skipped Ending Video!");
                EndVideo(videoPlayer);
            }
        }
    }

    private void SelectAnswer(Image selectedImg)
    {
        // หากมีรูปเก่าถูกเลือกอยู่ ให้ส่งกลับที่เดิม
        if (currentSelectedImage != null && originalPositions.ContainsKey(currentSelectedImage))
        {
            currentSelectedImage.transform.position = originalPositions[currentSelectedImage];
        }

        // กำหนดรูปใหม่ที่ถูกเลือก และย้ายพิกัด
        currentSelectedImage = selectedImg;
        if (selectedPosition != null)
        {
            currentSelectedImage.transform.position = selectedPosition.position;
        }
    }

    private void PlayEnding()
    {
        if (currentSelectedImage == null)
        {
            Debug.LogWarning("กรุณาเลือกคำตอบก่อนกดยืนยัน!");
            return;
        }

        // หาว่ารูปที่เลือก ผูกไว้กับวิดีโอไฟล์ไหน
        VideoClip clipToPlay = null;
        foreach (var mapping in answerMappings)
        {
            if (mapping.answerImage == currentSelectedImage)
            {
                clipToPlay = mapping.endingVideo;
                break;
            }
        }

        // หากมีไฟล์วิดีโอให้เล่น
        if (clipToPlay != null && videoPlayer != null)
        {
            // ซ่อนปุ่มคำตอบและปุ่มยืนยันเพื่อให้วิดีโอเด่นขึ้น (ทางเลือก)
            SetMainUIVisible(false);

            // โชว์ป้าย Press Enter to skip
            if (skipUI != null) skipUI.SetActive(true);

            videoPlayer.clip = clipToPlay;
            videoPlayer.Play();
            isVideoPlaying = true;
        }
        else
        {
            Debug.LogError("ไม่พบไฟล์วิดีโอ หรือไม่ได้ใส่ VideoPlayer ไว้ในสคริปต์");
        }
    }

    private void EndVideo(VideoPlayer vp)
    {
        // หยุดวิดีโอและซ่อนป้าย
        if (videoPlayer != null) videoPlayer.Stop();
        if (skipUI != null) skipUI.SetActive(false);
        isVideoPlaying = false;

        // คืนค่ารูปคำตอบที่เลือกกลับไปที่เดิม เพื่อให้คนเล่นลองเลือกอันอื่นได้
        if (currentSelectedImage != null && originalPositions.ContainsKey(currentSelectedImage))
        {
            currentSelectedImage.transform.position = originalPositions[currentSelectedImage];
            currentSelectedImage = null;
        }

        // โชว์หน้า UI หลักกลับมา
        SetMainUIVisible(true);
    }

    private void SetMainUIVisible(bool visible)
    {
        // เปิด/ปิดรูปคำตอบทั้งหมด
        foreach (var mapping in answerMappings)
        {
            if (mapping.answerImage != null)
            {
                mapping.answerImage.gameObject.SetActive(visible);
            }
        }

        // เปิด/ปิดปุ่มยืนยัน
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(visible);
        }

        // เปิด/ปิด UI อื่นๆ ที่ผู้เล่นตั้งค่าไว้
        if (otherUIToHide != null)
        {
            foreach (var ui in otherUIToHide)
            {
                if (ui != null) ui.SetActive(visible);
            }
        }
    }
}
