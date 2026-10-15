using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Basketball : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForward = 15f; // ความแรงพุ่งไปข้างหน้า (ปรับเป็น 15)
    public float throwUpward = 10f; // ความแรงลอยขึ้น (ปรับเป็น 10)
    public float throwSideways = 5f; // ความแรงออกซ้ายขวา
    public float holdingDistance = 2f; // ระยะห่างจากกล้องตอนที่จับบาสอยู่

    [HideInInspector]
    public bool hasScored = false; // เอาไว้เช็คว่าลูกนี้ได้คะแนนไปแล้วหรือยัง (กันลูกค้างในห่วงแล้วคะแนนพุ่ง)

    private Rigidbody rb;
    private bool isHolding = false;
    private Vector2 mouseDownPos;
    private float mouseDownTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // สร้าง Physics Material โคตรเด้งให้ลูกบาสอัตโนมัติ (สมจริง)
        PhysicsMaterial bouncyMat = new PhysicsMaterial("BasketballBouncy");
        bouncyMat.bounciness = 0.85f; // ความเด้ง (0-1)
        bouncyMat.dynamicFriction = 0.6f;
        bouncyMat.staticFriction = 0.6f;
        bouncyMat.bounceCombine = PhysicsMaterialCombine.Maximum;
        GetComponent<Collider>().material = bouncyMat;
    }

    void Update()
    {
        if (Minigame4Controller.Instance != null && Minigame4Controller.Instance.isGameOver) 
        {
            if (isHolding) ReleaseBall(false);
            return;
        }

        // กดเมาส์ซ้ายค้าง: จับลูกบาส
        if (Input.GetMouseButtonDown(0))
        {
            isHolding = true;
            rb.isKinematic = true; // หยุดฟิสิกส์ชั่วคราวตอนถือ
            hasScored = false; // รีเซ็ตคะแนนเพื่อให้ชู้ตลงห่วงใหม่ได้
            
            mouseDownPos = Input.mousePosition;
            mouseDownTime = Time.time;
        }

        // ตอนถืออยู่: ให้ลูกบาสลอยตามเมาส์
        if (isHolding)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = holdingDistance; 
            transform.position = Camera.main.ScreenToWorldPoint(mousePos);

            // ปล่อยเมาส์: โยนลูกบาส
            if (Input.GetMouseButtonUp(0))
            {
                ReleaseBall(true);
            }
        }
    }

    private void ReleaseBall(bool applyForce)
    {
        isHolding = false;
        rb.isKinematic = false; // เปิดฟิสิกส์กลับมา

        if (applyForce)
        {
            Vector2 mouseUpPos = Input.mousePosition;
            float timeHeld = Time.time - mouseDownTime;
            
            // ป้องกันการหารด้วยศูนย์ หรือเวลาน้อยเกินไปจนแรงพุ่งเป็นอนันต์
            if (timeHeld < 0.05f) timeHeld = 0.05f;

            // คำนวณความเร็วในการลากเมาส์ และทำให้เป็น Normalized (0-1) โดยเทียบกับความสูงจอภาพ
            // เพื่อไม่ให้ค่ากระโดดไปหลักพันเวลาลากเมาส์เร็วๆ
            Vector2 swipeVelocity = (mouseUpPos - mouseDownPos) / timeHeld / Screen.height;

            // เอาเฉพาะแรงที่ลากขึ้น (ถ้าลากลงจะไม่พุ่งไปข้างหน้า)
            float forceY = swipeVelocity.y > 0 ? swipeVelocity.y : 0;
            
            // หาหน้าหันของกล้อง (อ้างอิงจากผู้เล่น) แต่ล็อกแกน Y ไว้ไม่ให้ชู้ตลงพื้น
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0; 
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            // คำนวณแรงผลัก โดยให้ทิศทางอิงตามกล้องเสมอ
            Vector3 throwForce = (camRight * swipeVelocity.x * throwSideways) + 
                                 (Vector3.up * forceY * throwUpward) + 
                                 (camForward * forceY * throwForward);
            
            // ออกแรงผลัก (Impulse = ผลักแบบกระแทกทีเดียว)
            rb.AddForce(throwForce, ForceMode.Impulse);
            
            // เพิ่มการหมุน (Spin) ให้สมจริง (อิงตามมุมกล้องด้วย)
            rb.AddTorque(camRight * forceY * 20f, ForceMode.Impulse);
        }
    }
}
