using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Components (ลาก UI มาใส่ให้ครบ)")]
    public GameObject dialoguePanel; // แผง UI ทั้งหมด (Background Image)
    public TextMeshProUGUI nameText; // ตัวหนังสือชื่อตัวละคร
    public TextMeshProUGUI dialogueText; // ตัวหนังสือเนื้อเรื่อง
    public GameObject continuePrompt; // Text "Spacebar to continue"

    [Header("Choice UI (แผงให้เลือกว่าจะไปต่อไหม)")]
    public GameObject choicePanel; // แผงที่มีปุ่ม Yes / No
    public Button yesButton;
    public Button noButton;

    [Header("Settings")]
    public float typeSpeed = 0.05f; // ความเร็วในการพิมพ์ตัวอักษร

    private string[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private int frameOpened;
    private string currentTargetScene;
    private NPCInteractable currentNPC;
    private int currentChoiceLineIndex = -1;

    private void Awake()
    {
        // Singleton Pattern ทำให้เข้าถึง DialogueManager ได้จากทุกสคริปต์
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // กำหนด Event ให้กับปุ่ม Yes / No
        if (yesButton != null) yesButton.onClick.AddListener(OnYesClicked);
        if (noButton != null) noButton.onClick.AddListener(OnNoClicked);

        // ซ่อนหน้าต่างและข้อความตอนเริ่มเกม
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (nameText != null) nameText.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (continuePrompt != null) continuePrompt.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        isDialogueActive = false;
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        // ป้องกันการกด E ในเฟรมเดียวกับที่เปิด (เพื่อไม่ให้เปิดปุ๊บปิดปั๊บ)
        if (Time.frameCount == frameOpened) return;

        // กด E เพื่อปิดหน้าต่างบทสนทนาได้ตลอดเวลา
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndDialogue();
            return;
        }

        // กด Spacebar เพื่ออ่านบรรทัดต่อไป
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ถ้าหน้าต่างเลือกว่าจะไป Scene อื่นเปิดอยู่ ให้บังคับให้คลิกปุ่มแทน (ห้ามข้าม)
            if (choicePanel != null && choicePanel.activeSelf) return;

            if (isTyping)
            {
                // ถ้าตัวหนังสือยังโผล่ไม่หมด ให้แสดงทั้งหมดทันที
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentLines[currentLineIndex];
                isTyping = false;
                
                // ถ้านี่คือบรรทัดสุดท้ายและมี Scene ปลายทาง ให้แสดงตัวเลือกแทนที่จะแสดง Continue
                if (currentLineIndex == currentLines.Length - 1 && !string.IsNullOrEmpty(currentTargetScene))
                {
                    if (choicePanel != null) choicePanel.SetActive(true);
                }
                else
                {
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                }
            }
            else
            {
                // ไปยังข้อความหน้าถัดไป
                NextLine();
            }
        }
    }

    public void StartDialogue(string npcName, string[] lines, string targetScene = "", NPCInteractable npc = null, int choiceLineIndex = -1, int startIndex = 0)
    {
        if (isDialogueActive) return;

        currentNPC = npc;
        currentTargetScene = targetScene;
        currentChoiceLineIndex = choiceLineIndex;
        currentLines = lines;
        currentLineIndex = startIndex;
        isDialogueActive = true;
        frameOpened = Time.frameCount;

        if (nameText != null) 
        {
            nameText.text = npcName;
            nameText.gameObject.SetActive(true);
        }
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        // ปลดล็อคเมาส์เพื่อให้คลิกปุ่ม UI ได้
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowLine();
    }

    private void ShowLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        if (continuePrompt != null) continuePrompt.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentLineIndex]));

        // ถ้าเป็นประโยคสุดท้ายของตัวละครนั้นๆ ให้ทำการปลดล็อครูปภาพ (ถ้ามี)
        if (currentLineIndex == currentLines.Length - 1 && currentNPC != null)
        {
            if (GalleryManager.Instance != null)
            {
                GalleryManager.Instance.UnlockImage(currentNPC.npcName);
            }
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        // ทยอยพิมพ์ทีละตัวอักษรจากซ้ายไปขวา
        foreach (char c in line.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        
        // เช็คเงื่อนไขการโชว์ Choice UI
        bool showChoice = false;
        if (currentChoiceLineIndex != -1 && currentLineIndex == currentChoiceLineIndex)
        {
            showChoice = true;
        }

        if (showChoice)
        {
            if (choicePanel != null) choicePanel.SetActive(true);
        }
        else
        {
            if (continuePrompt != null) continuePrompt.SetActive(true);
        }
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < currentLines.Length)
        {
            ShowLine();
        }
        else
        {
            // จบบทสนทนา
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (nameText != null) nameText.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        if (continuePrompt != null) continuePrompt.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);

        if (currentNPC != null)
        {
            currentNPC.SetTalkingState(false);
            currentNPC = null;
        }

        // ล็อคเมาส์กลับไปเมื่อปิดหน้าต่างบทสนทนา
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // หน่วงเวลา 1 เฟรมก่อนตั้งค่า isDialogueActive เป็น false 
        // เพื่อป้องกันไม่ให้ PlayerInteractor เปิดบทสนทนาขึ้นมาใหม่ในเฟรมเดียวกัน
        StartCoroutine(ResetDialogueActiveState());
    }

    private IEnumerator ResetDialogueActiveState()
    {
        yield return new WaitForEndOfFrame();
        isDialogueActive = false;
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    private void OnYesClicked()
    {
        if (!string.IsNullOrEmpty(currentTargetScene))
        {
            // บันทึกตำแหน่งและสถานะก่อนย้าย Scene
            GameState.ReturningFromMinigame = true;
            GameState.MinigameWon = false; // รอให้เล่นให้ชนะก่อน
            GameState.LastNPCName = nameText.text;

            ThirdPersonPlayerController player = FindObjectOfType<ThirdPersonPlayerController>();
            if (player != null)
            {
                GameState.PlayerSavedPosition = player.transform.position;
                GameState.PlayerSavedRotation = player.transform.rotation;
                Debug.Log("บันทึกตำแหน่งผู้เล่นสำเร็จ: " + GameState.PlayerSavedPosition);
            }
            else
            {
                Debug.LogWarning("หาผู้เล่นไม่เจอ เลยไม่ได้บันทึกตำแหน่ง");
            }

            SceneManager.LoadScene(currentTargetScene);
        }
        else
        {
            // เป็นเควสแบบเดินหาของในฉาก (ไม่มี Scene ให้โหลด)
            if (currentNPC != null && currentNPC.isFetchQuestNPC)
            {
                if (!GameState.ActiveQuests.Contains(currentNPC.requiredItemID))
                {
                    GameState.ActiveQuests.Add(currentNPC.requiredItemID);
                    Debug.Log("รับเควสค้นหา: " + currentNPC.requiredItemID);
                }
            }
            EndDialogue();
        }
    }

    private void OnNoClicked()
    {
        EndDialogue();
    }
}
