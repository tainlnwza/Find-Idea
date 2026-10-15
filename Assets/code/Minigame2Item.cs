using UnityEngine;
using TMPro;
using System.Collections;

public class Minigame2Item : MonoBehaviour, IInteractable
{
    public enum ItemType { Collectible, Goal }

    [Header("Item Settings")]
    public ItemType itemType = ItemType.Collectible;
    
    [Header("Interaction UI")]
    public TextMeshPro promptText; // ป้าย "E to Collect" หรือ "E to Finish" แบบ 3D
    public float fadeSpeed = 10f;

    private Minigame2Controller controller;
    private Coroutine fadeCoroutine;

    void Start()
    {
        controller = FindObjectOfType<Minigame2Controller>();

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
        // ทำให้ข้อความหันหน้าเข้าหากล้องเสมอ (Billboard effect)
        if (promptText != null && Camera.main != null)
        {
            promptText.transform.rotation = Quaternion.LookRotation(promptText.transform.position - Camera.main.transform.position);
        }
    }

    public void OnLookStart()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePrompt(1f)); // Fade In
    }

    public void OnLookEnd()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadePrompt(0f)); // Fade Out
    }

    public void Interact()
    {
        if (controller == null) return;

        if (itemType == ItemType.Collectible)
        {
            controller.CollectItem();
            Destroy(gameObject); // เก็บแล้วให้ลบวัตถุทิ้งไปเลย
        }
        else if (itemType == ItemType.Goal)
        {
            controller.TryFinish(); // ถ้านี่คือจุดส่งเควส ให้ไปเช็คว่าเก็บครบหรือยัง
        }
    }

    public Transform GetTransform()
    {
        return transform;
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
