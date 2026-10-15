using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Minigame4Controller : MonoBehaviour
{
    public static Minigame4Controller Instance { get; private set; }

    [Header("Game Settings")]
    public int targetScore = 5; // จำนวนครั้งที่ต้องชู้ตลงห่วง
    public float timeLimit = 60f; // เวลาที่กำหนด (วินาที)

    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // ตัวหนังสือแสดงเวลา
    public TextMeshProUGUI scoreText; // ตัวหนังสือแสดงคะแนน
    
    [Header("Win/Loss UI")]
    public GameObject winCanvas;
    public GameObject lossCanvas;

    [HideInInspector]
    public bool isGameOver = false;
    private int currentScore = 0;
    private float timeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

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

        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore + " / " + targetScore;
        }
    }

    public void AddScore()
    {
        if (isGameOver) return;

        currentScore++;
        UpdateUI();
        Debug.Log("ชู้ตเข้าห่วง! คะแนนปัจจุบัน: " + currentScore);

        if (currentScore >= targetScore)
        {
            GameWon();
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
