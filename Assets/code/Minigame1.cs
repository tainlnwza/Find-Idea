using UnityEngine;
using UnityEngine.SceneManagement;

public class Minigame1 : MonoBehaviour
{
    [Header("UI Elements (ลาก UI มาใส่ให้ครบ)")]
    public RectTransform backgroundPanel; // Panel ที่เป็นพื้นหลัง
    public RectTransform movingImage; // Image ที่เลื่อนไปมา
    public RectTransform targetImage; // Image ตัวชี้เป้าตรงกลาง

    [Header("Target Object")]
    public Renderer targetObjectRenderer; // วัตถุ 3D ที่ต้องการให้เปลี่ยนสี
    [Tooltip("ชื่อ Property สีของ Material (ถ้าเป็น URP ส่วนใหญ่คือ _BaseColor ถ้าระบบปกติคือ _Color)")]
    public string colorPropertyName = "_BaseColor";

    [Header("Win/Loss UI")]
    public GameObject winCanvas; // หน้าต่างตอนชนะ
    public GameObject lossCanvas; // หน้าต่างตอนแพ้

    [Header("Game Settings")]
    public float moveSpeed = 500f; // ความเร็วของ Image ที่เลื่อน
    public float shrinkAmount = 20f; // ขนาดที่จะลดลงเมื่อกดโดนเป้า
    public int maxMistakes = 5; // จำนวนครั้งที่กดพลาดได้ก่อนแพ้ (จนกว่าจะกลายเป็นสีดำสนิท)
    public float cooldownTime = 0.5f; // กันการกด Spacebar รัวๆ

    private int currentMistakes = 0;
    private float nextPressTime = 0f;
    private bool isGameOver = false;

    // ตัวแปรสำหรับการเลื่อน
    private int direction = 1;
    private float leftBound;
    private float rightBound;
    private Material objectMaterial;

    void Start()
    {
        if (targetObjectRenderer != null)
        {
            // สร้าง Instance ของ Material เพื่อไม่ให้กระทบกับ Object อื่นที่ใช้ Material เดียวกัน
            objectMaterial = targetObjectRenderer.material;
            UpdateObjectColor();
        }

        if (winCanvas != null) winCanvas.SetActive(false);
        if (lossCanvas != null) lossCanvas.SetActive(false);

        CalculateBounds();
    }

    void Update()
    {
        if (isGameOver) return;

        MoveImage();

        // เช็คการกด Spacebar และระบบ Cooldown เพื่อกันการกดรัวๆ
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextPressTime)
        {
            nextPressTime = Time.time + cooldownTime;
            CheckHit();
        }
    }

    void CalculateBounds()
    {
        if (backgroundPanel == null || movingImage == null) return;

        // คำนวณขอบเขตซ้ายขวาของ Panel โดยอิงจากความกว้าง
        float panelWidth = backgroundPanel.rect.width;
        float movingWidth = movingImage.rect.width;

        // สูตรนี้สมมติว่าจุดศูนย์กลาง (Pivot) ของ Background Panel และ Moving Image อยู่ตรงกลาง (0.5, 0.5)
        leftBound = -(panelWidth / 2f) + (movingWidth / 2f);
        rightBound = (panelWidth / 2f) - (movingWidth / 2f);
    }

    void MoveImage()
    {
        if (movingImage == null) return;

        Vector2 pos = movingImage.anchoredPosition;
        pos.x += moveSpeed * direction * Time.deltaTime;

        // เช็คการชนขอบซ้ายขวา และสลับทิศทาง
        if (pos.x >= rightBound)
        {
            pos.x = rightBound;
            direction = -1;
        }
        else if (pos.x <= leftBound)
        {
            pos.x = leftBound;
            direction = 1;
        }

        movingImage.anchoredPosition = pos;
    }

    void CheckHit()
    {
        if (movingImage == null || targetImage == null) return;

        // คำนวณขอบเขตของแกน X ว่าภาพทั้งสองทับซ้อนกันหรือไม่
        float movingMinX = movingImage.anchoredPosition.x - (movingImage.rect.width * movingImage.pivot.x);
        float movingMaxX = movingImage.anchoredPosition.x + (movingImage.rect.width * (1 - movingImage.pivot.x));
        
        float targetMinX = targetImage.anchoredPosition.x - (targetImage.rect.width * targetImage.pivot.x);
        float targetMaxX = targetImage.anchoredPosition.x + (targetImage.rect.width * (1 - targetImage.pivot.x));

        // ถ้าทับซ้อนกัน (Hit)
        bool isHit = (movingMinX <= targetMaxX && movingMaxX >= targetMinX);

        if (isHit)
        {
            // หดขนาดความกว้างของหลอดสีแดง
            float newWidth = movingImage.rect.width - shrinkAmount;
            
            if (newWidth <= 0)
            {
                newWidth = 0;
                movingImage.sizeDelta = new Vector2(newWidth, movingImage.sizeDelta.y);
                GameWon();
            }
            else
            {
                movingImage.sizeDelta = new Vector2(newWidth, movingImage.sizeDelta.y);
                CalculateBounds(); // คำนวณขอบเขตใหม่เพราะหลอดมันสั้นลงแล้ว
            }
        }
        else
        {
            // กดพลาด
            currentMistakes++;
            UpdateObjectColor();

            if (currentMistakes >= maxMistakes)
            {
                GameOver();
            }
        }
    }

    void UpdateObjectColor()
    {
        if (objectMaterial != null)
        {
            // คำนวณสัดส่วนความผิดพลาด (0 = สีขาวสุด, 1 = สีดำสุด)
            float t = (float)currentMistakes / maxMistakes;
            Color newColor = Color.Lerp(Color.white, Color.black, t);
            
            // เปลี่ยนสี Material
            if (objectMaterial.HasProperty(colorPropertyName))
            {
                objectMaterial.SetColor(colorPropertyName, newColor);
            }
            else if (objectMaterial.HasProperty("_Color"))
            {
                objectMaterial.SetColor("_Color", newColor);
            }
            else
            {
                // สำรองกรณีฉุกเฉิน
                objectMaterial.color = newColor; 
            }
        }
    }

    void GameWon()
    {
        isGameOver = true;
        GameState.MinigameWon = true; // บันทึกสถานะว่าชนะ
        Debug.Log("ชนะ");
        if (winCanvas != null) winCanvas.SetActive(true);
        UnlockCursor();
    }

    void GameOver()
    {
        isGameOver = true;
        GameState.MinigameWon = false; // บันทึกสถานะว่าแพ้
        Debug.Log("แพ้");
        if (lossCanvas != null) lossCanvas.SetActive(true);
        UnlockCursor();
    }

    private void UnlockCursor()
    {
        // ปลดล็อคเมาส์เพื่อให้ผู้เล่นสามารถคลิกปุ่ม UI ได้
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnRestartClicked()
    {
        // โหลด Scene ปัจจุบันขึ้นมาใหม่เพื่อเริ่มเล่นใหม่
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBackHomeClicked()
    {
        // โหลดกลับไปที่หน้า _main map
        SceneManager.LoadScene("_main map");
    }
}
