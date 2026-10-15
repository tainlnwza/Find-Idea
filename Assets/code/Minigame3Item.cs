using UnityEngine;
using TMPro;
using System.Collections;

public class Minigame3Item : MonoBehaviour, IInteractable
{
    [Header("Quest Item Settings")]
    [Tooltip("รหัสของไอเทม ต้องตั้งให้ตรงกับช่อง Required Item ID ใน NPC")]
    public string itemID = "QuestItem1"; 
    
    [Header("Interaction UI")]
    public TextMeshPro promptText; // ป้าย "E to Collect" แบบ 3D
    public float fadeSpeed = 10f;

    private Coroutine fadeCoroutine;
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool isVisible = false;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        // ซ่อนไอเทมตั้งแต่เริ่มเกม (จะโชว์ก็ต่อเมื่อรับเควสแล้ว)
        SetVisibility(false);

        if (promptText != null)
        {
            // ซ่อนข้อความตั้งแต่เริ่มเกม
            Color color = promptText.color;
            color.a = 0f;
            promptText.color = color;
        }
    }

    void Update()
    {
        // ตรวจสอบว่าผู้เล่นรับเควสมาหรือยัง ถ้าเพิ่งรับเควส ให้โชว์ไอเทมขึ้นมา
        if (!isVisible && GameState.ActiveQuests.Contains(itemID))
        {
            SetVisibility(true);
        }

        // ทำให้ข้อความหันหน้าเข้าหากล้องเสมอ (Billboard effect)
        if (promptText != null && Camera.main != null)
        {
            promptText.transform.rotation = Quaternion.LookRotation(promptText.transform.position - Camera.main.transform.position);
        }
    }

    public void OnLookStart()
    {
        if (!isVisible) return; // ถ้ายังไม่โชว์ ห้าม Fade ข้อความ
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePrompt(1f)); // Fade In
    }

    public void OnLookEnd()
    {
        if (!isVisible) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePrompt(0f)); // Fade Out
    }

    public void Interact()
    {
        // เพื่อความเป็นอิสระ ผู้เล่นสามารถเก็บไอเทมเควสได้แม้จะยังไม่ได้คุยกับ NPC ก็ตาม (จะได้ไม่ต้องวิ่งกลับไปกลับมา)
        // เก็บเข้ากระเป๋า
        GameState.Inventory.Add(itemID);
        Debug.Log("เก็บไอเทมเควสสำเร็จ: " + itemID);

        // ลบทิ้งออกจากฉาก
        Destroy(gameObject);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void SetVisibility(bool visible)
    {
        isVisible = visible;
        
        foreach (Renderer r in renderers) { r.enabled = visible; }
        foreach (Collider c in colliders) { c.enabled = visible; }

        if (!visible && promptText != null)
        {
            Color color = promptText.color;
            color.a = 0f;
            promptText.color = color;
        }
    }

    private IEnumerator FadePrompt(float targetAlpha)
    {
        if (promptText == null) yield break;

        Color currentColor = promptText.color;

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
