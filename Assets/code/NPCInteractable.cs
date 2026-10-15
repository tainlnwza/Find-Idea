using System.Collections;
using UnityEngine;
using TMPro;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("NPC Data")]
    public string npcName = "Name"; // เปลี่ยนชื่อได้อิสระ เช่น Mom, Dad
    [TextArea(3, 5)]
    public string[] dialogueLines; // ใส่กล่องข้อความได้ไม่จำกัด
    [Header("Scene Transition (Optional)")]
    public string targetSceneName = ""; // ปล่อยว่างไว้ถ้าไม่ต้องการเปลี่ยนซีน

    [Header("Choice Settings")]
    [Tooltip("ระบุลำดับหน้าของ Dialogue ที่ต้องการให้ขึ้น Choice (เริ่มนับจาก 0) ถ้าใส่ -1 จะไม่มี")]
    public int choiceLineIndex = -1;

    [Header("Fetch Quest Settings (Minigame 3 & 5)")]
    public bool isFetchQuestNPC = false; // ติ๊กถ้า NPC ตัวนี้แจกเควส
    public string requiredItemID = "QuestItem1"; // รหัสของที่ต้องการ
    [Tooltip("จำนวนของที่ต้องการ (มินิเกม 5 ให้ตั้งค่ามากกว่า 1)")]
    public int requiredItemAmount = 1; 
    [TextArea(2, 3)]
    public string waitingDialogue = "หาของมาให้ฉันหรือยัง?"; // ประโยคตอนรอผู้เล่นไปหาของ

    [Header("Animation")]
    public Animator npcAnimator; // ลาก Animator ของ NPC มาใส่เพื่อเล่นท่าทาง

    [Header("Interaction UI")]
    public TextMeshPro promptText; // ลาก TextMeshPro แบบ 3D มาใส่ (ตัวที่เขียน E to talk)
    public float fadeSpeed = 10f; // ความเร็วในการจางเข้า/ออก

    private bool isPlayerLooking = false;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (promptText != null)
        {
            // ซ่อนข้อความตั้งแต่เริ่มเกม
            Color color = promptText.color;
            color.a = 0f;
            promptText.color = color;
        }

        // ถ้าระบบจำได้ว่าเพิ่งชนะมินิเกมของ NPC ตัวนี้ ให้คุยต่ออัตโนมัติ
        if (GameState.ReturningFromMinigame && GameState.MinigameWon && GameState.LastNPCName == npcName)
        {
            StartCoroutine(ResumeDialogueCoroutine());
        }
    }

    private IEnumerator ResumeDialogueCoroutine()
    {
        yield return new WaitForSeconds(0.1f); // หน่วงเวลานิดนึงให้ Scene ตั้งตัวทัน

        int nextIndex = choiceLineIndex + 1;
        if (nextIndex < dialogueLines.Length && DialogueManager.Instance != null)
        {
            SetTalkingState(true);
            // ส่ง -1 ไปเพื่อให้ไม่โชว์ Choice UI ซ้ำอีกรอบ
            DialogueManager.Instance.StartDialogue(npcName, dialogueLines, targetSceneName, this, -1, nextIndex);
        }
    }

    private void Update()
    {
        // ทำให้ข้อความ "E to talk" หันหน้าเข้าหากล้องเสมอ (Billboard effect)
        if (promptText != null && Camera.main != null)
        {
            promptText.transform.rotation = Quaternion.LookRotation(promptText.transform.position - Camera.main.transform.position);
        }
    }

    // เรียกตอนผู้เล่นหันมามอง
    public void OnLookStart()
    {
        isPlayerLooking = true;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePrompt(1f)); // Fade In
    }

    // เรียกตอนผู้เล่นหันหน้าหนีหรือเดินออกนอกระยะ
    public void OnLookEnd()
    {
        isPlayerLooking = false;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePrompt(0f)); // Fade Out
    }

    // เรียกตอนกด E
    public void Interact()
    {
        if (DialogueManager.Instance != null && !DialogueManager.Instance.IsDialogueActive())
        {
            SetTalkingState(true);
            
            if (isFetchQuestNPC)
            {
                // นับจำนวนไอเทมรหัสนี้ในกระเป๋าว่ามีกี่ชิ้น
                int currentAmount = GameState.Inventory.FindAll(id => id == requiredItemID).Count;

                if (currentAmount >= requiredItemAmount)
                {
                    // มีของครบแล้ว จบเควสเลย
                    for(int i = 0; i < requiredItemAmount; i++)
                    {
                        GameState.Inventory.Remove(requiredItemID);
                    }
                    GameState.ActiveQuests.Remove(requiredItemID);
                    
                    // เปิดประโยคหลัง choice ทันที
                    DialogueManager.Instance.StartDialogue(npcName, dialogueLines, targetSceneName, this, -1, choiceLineIndex + 1);
                }
                else if (GameState.ActiveQuests.Contains(requiredItemID))
                {
                    // รับเควสไปแล้วแต่ยังไม่มีของ หรือ มีของยังไม่ครบ
                    string waitMsg = requiredItemAmount > 1 
                        ? waitingDialogue + "\n(มีแล้ว " + currentAmount + "/" + requiredItemAmount + " ชิ้น)" 
                        : waitingDialogue;
                    DialogueManager.Instance.StartDialogue(npcName, new string[] { waitMsg }, "", this, -1, 0);
                }
                else
                {
                    // ยังไม่เคยรับเควส ให้โชว์เนื้อเรื่องและโชว์ปุ่ม Choice มารับเควส
                    DialogueManager.Instance.StartDialogue(npcName, dialogueLines, targetSceneName, this, choiceLineIndex, 0);
                }
            }
            else
            {
                // คุยปกติ
                DialogueManager.Instance.StartDialogue(npcName, dialogueLines, targetSceneName, this, choiceLineIndex, 0);
            }
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void SetTalkingState(bool isTalking)
    {
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("isTalking", isTalking);
        }
    }

    private IEnumerator FadePrompt(float targetAlpha)
    {
        if (promptText == null) yield break;

        Color currentColor = promptText.color;

        // ค่อยๆ เปลี่ยนค่า Alpha ให้โปร่งแสงหรือชัดขึ้น
        while (Mathf.Abs(currentColor.a - targetAlpha) > 0.01f)
        {
            currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
            promptText.color = currentColor;
            yield return null;
        }

        currentColor.a = targetAlpha;
        promptText.color = currentColor;
    }
}
