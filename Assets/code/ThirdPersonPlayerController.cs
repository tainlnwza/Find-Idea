using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;

    // [Header("Look Settings")]
    // public float mouseSensitivity = 150f;
    // public Transform cameraTarget; 
    // public float minViewAngle = -40f;
    // public float maxViewAngle = 60f;

    [Header("Look Settings")]
    public float mouseSensitivity = 150f;

    [Header("Invert Controls (แก้ปัญหาสลับทิศ)")]
    public bool invertMouseX = true; // ติ๊กเพื่อสลับเมาส์ซ้าย-ขวา
    public bool invertWASD = true; // ติ๊กเพื่อสลับ W A S D

    [Header("Animation")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        // ตรวจสอบว่ากลับมาจากมินิเกมหรือไม่ และต้องอยู่ในฉาก _main map เท่านั้น
        if (GameState.ReturningFromMinigame && SceneManager.GetActiveScene().name == "_main map")
        {
            controller.enabled = false; // ต้องปิดก่อนเพื่อย้ายตำแหน่ง
            transform.position = GameState.PlayerSavedPosition;
            transform.rotation = GameState.PlayerSavedRotation;
            controller.enabled = true;
        }
    }

    void Start()
    {
        // รีเซ็ตสถานะกลับมาจากมินิเกมในเฟรมถัดไป เฉพาะในฉากหลักเท่านั้น
        if (SceneManager.GetActiveScene().name == "_main map")
        {
            StartCoroutine(ResetMinigameFlag());
        }

        // ล็อกเมาส์ไว้ตรงกลางหน้าจอและซ่อนเมาส์
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private System.Collections.IEnumerator ResetMinigameFlag()
    {
        yield return new WaitForEndOfFrame();
        GameState.ReturningFromMinigame = false;
    }

    void Update()
    {
        // ถ้าระบบ Dialogue เปิดอยู่ ให้หยุดการเดินและการหมุนกล้อง
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
        {
            if (animator != null) animator.SetFloat("Speed", 0f); // ให้ยืนนิ่ง
            return;
        }

        // ถ้าระบบ Gallery เปิดอยู่ ให้หยุดการเดินและการหมุนกล้องเช่นเดียวกัน
        if (GalleryManager.Instance != null && GalleryManager.Instance.IsGalleryOpen())
        {
            if (animator != null) animator.SetFloat("Speed", 0f); // ให้ยืนนิ่ง
            return;
        }

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        // รับค่าการขยับเมาส์ (แกน X อย่างเดียวเพื่อหันซ้าย-ขวา)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        if (invertMouseX) mouseX = -mouseX;

        // หันหน้าตัวละครซ้าย-ขวา ตามแกน X ของเมาส์
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // รีเซ็ตความเร็วตกเมื่ออยู่บนพื้น
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // รับค่าปุ่ม W A S D
        float x = Input.GetAxis("Horizontal"); 
        float z = Input.GetAxis("Vertical");   

        if (invertWASD)
        {
            x = -x;
            z = -z;
        }

        // ตรวจสอบการกด Shift เพื่อวิ่ง
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // คำนวณทิศทางการเดินอ้างอิงจากหน้าของตัวละคร
        Vector3 move = transform.right * x + transform.forward * z;

        // สั่งให้ตัวละครเคลื่อนที่
        controller.Move(move * currentSpeed * Time.deltaTime);

        // จัดการ Animation (0 = ยืน, 1 = เดิน, 2 = วิ่ง)
        if (animator != null)
        {
            float targetSpeed = 0f;
            if (new Vector2(x, z).magnitude > 0.1f)
            {
                targetSpeed = isSprinting ? 2f : 1f;
            }
            
            // ทำให้การเปลี่ยนแอนิเมชันค่อยเป็นค่อยไป (นุ่มนวลขึ้น)
            float currentAnimSpeed = animator.GetFloat("Speed");
            animator.SetFloat("Speed", Mathf.Lerp(currentAnimSpeed, targetSpeed, Time.deltaTime * 10f));
        }

        // คำนวณแรงโน้มถ่วง
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
