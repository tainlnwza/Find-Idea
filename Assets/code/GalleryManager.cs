using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class NPCImageMapping
{
    public string npcName;
    public GameObject imageUI;
}

public class GalleryManager : MonoBehaviour
{
    public static GalleryManager Instance { get; private set; }

    [Header("UI Settings")]
    public GameObject galleryPanel; // แผง UI หลักของ Gallery
    
    [Header("Images Mapping")]
    [Tooltip("ระบุชื่อ NPC และรูปภาพที่ต้องการให้ปลดล็อคเมื่อคุยจบ")]
    public List<NPCImageMapping> npcImages;

    private bool isGalleryOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // ซ่อน Gallery Panel ตอนเริ่มเกม
        if (galleryPanel != null) 
        {
            galleryPanel.SetActive(false);
        }

        // ซ่อนทุกรูปไว้ก่อน ยกเว้นรูปที่ถูกปลดล็อคแล้ว
        UpdateImagesVisibility();
    }

    private void Update()
    {
        // ผู้เล่นสามารถกด Q เพื่อเปิด/ปิดหน้า Gallery ได้
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleGallery();
        }
    }

    public void ToggleGallery()
    {
        isGalleryOpen = !isGalleryOpen;

        if (galleryPanel != null)
        {
            galleryPanel.SetActive(isGalleryOpen);
        }

        if (isGalleryOpen)
        {
            // อัปเดตการแสดงผลรูปภาพก่อนเปิดหน้าต่าง
            UpdateImagesVisibility();

            // ปลดล็อคเมาส์ให้สามารถเลือกดูอะไรใน UI ได้
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // ล็อคเมาส์กลับเมื่อปิด
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool IsGalleryOpen()
    {
        return isGalleryOpen;
    }

    public void UnlockImage(string npcName)
    {
        // บันทึกว่าผู้เล่นได้ปลดล็อครูปของ NPC ตัวนี้แล้ว
        if (!GameState.UnlockedNPCImages.Contains(npcName))
        {
            GameState.UnlockedNPCImages.Add(npcName);
            Debug.Log("Unlocked image for NPC: " + npcName);
        }

        // ทำให้รูปที่ตรงกับ NPC ปรากฏขึ้น (เผื่อโชว์ขึ้นมาทันที หรือรอให้ผู้เล่นกด Q เข้าไปดู)
        UpdateImagesVisibility();
        
        // ตรวจสอบว่าปลดล็อคครบหรือยัง
        CheckGameEnd();
    }

    private void CheckGameEnd()
    {
        if (npcImages == null || npcImages.Count == 0) return;

        int unlockedCount = 0;
        foreach (var mapping in npcImages)
        {
            if (GameState.UnlockedNPCImages.Contains(mapping.npcName))
            {
                unlockedCount++;
            }
        }

        if (unlockedCount >= npcImages.Count)
        {
            Debug.Log("ปลดล็อครูปภาพครบทั้งหมดแล้ว เปลี่ยนหน้าไปยัง End scene");
            SceneManager.LoadScene("End scene");
        }
    }

    private void UpdateImagesVisibility()
    {
        foreach (var mapping in npcImages)
        {
            if (mapping.imageUI != null)
            {
                // ถ้ารายชื่ออยู่ใน HashSet ที่เคยปลดล็อคแล้ว ก็จะโชว์รูป
                mapping.imageUI.SetActive(GameState.UnlockedNPCImages.Contains(mapping.npcName));
            }
        }
    }
}
