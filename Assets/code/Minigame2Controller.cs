using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Minigame2Controller : MonoBehaviour
{
    [Header("Game Settings")]
    public int targetCollectAmount = 5; // จำนวนชิ้นที่ต้องเก็บให้ครบ
    public float timeLimit = 60f; // เวลาที่กำหนด (วินาที)

    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // ตัวหนังสือแสดงเวลา
    public TextMeshProUGUI counterText; // ตัวหนังสือแสดงยอดนับของ
    
    [Header("Win/Loss UI")]
    public GameObject winCanvas;
    public GameObject lossCanvas;

    private int currentCollected = 0;
    private float timeRemaining;
    private bool isGameOver = false;

    void Start()
    {
        timeRemaining = timeLimit;
        
        if (winCanvas != null) winCanvas.SetActive(false);
        if (lossCanvas != null) lossCanvas.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (isGameOver) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            GameOver();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString() + "s";
        }

        if (counterText != null)
        {
            counterText.text = "Collected: " + currentCollected + " / " + targetCollectAmount;
        }
    }

    public void CollectItem()
    {
        if (isGameOver) return;

        currentCollected++;
        UpdateUI();
    }

    public void TryFinish()
    {
        if (isGameOver) return;

        if (currentCollected >= targetCollectAmount)
        {
            GameWon();
        }
        else
        {
            Debug.Log("เก็บของยังไม่ครบ! ขาดอีก " + (targetCollectAmount - currentCollected) + " ชิ้น");
        }
    }

    void GameWon()
    {
        isGameOver = true;
        GameState.MinigameWon = true;
        Debug.Log("ชนะ");
        if (winCanvas != null) winCanvas.SetActive(true);
        UnlockCursor();
        DisablePlayerMovement();
    }

    void GameOver()
    {
        isGameOver = true;
        GameState.MinigameWon = false;
        Debug.Log("แพ้ (เวลาหมด)");
        if (lossCanvas != null) lossCanvas.SetActive(true);
        UnlockCursor();
        DisablePlayerMovement();
    }

    private void DisablePlayerMovement()
    {
        ThirdPersonPlayerController player = FindObjectOfType<ThirdPersonPlayerController>();
        if (player != null)
        {
            player.enabled = false;
            if (player.animator != null)
            {
                player.animator.SetFloat("Speed", 0f);
            }
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBackHomeClicked()
    {
        SceneManager.LoadScene("_main map");
    }
}
