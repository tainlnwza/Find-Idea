using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f; // รัศมีการตรวจจับรอบตัว
    [Range(0, 360)]
    public float viewAngle = 90f; // องศาการมองเห็น (มุมด้านหน้า)
    
    [Tooltip("นำโมเดลผู้เล่นมาใส่ที่ช่องนี้ เพื่อให้จุดศูนย์กลางวงกลมอยู่ตรงผู้เล่น (ถ้าปล่อยว่าง จะยึดจุดที่ติดสคริปต์นี้)")]
    public Transform playerCenter;

    private IInteractable currentInteractable;

    void Update()
    {
        // ถ้าระบบ Dialogue เปิดอยู่ จะปิดระบบตรวจจับและกด E (เพื่อให้ Player โฟกัสกับ UI)
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive())
            return;

        CheckInteraction();

        // ตรวจสอบการกด E เพื่อคุยกับ/หรือเก็บของ ที่อยู่ใกล้ที่สุด
        if (Input.GetKeyDown(KeyCode.E) && IsInteractableValid(currentInteractable))
        {
            currentInteractable.Interact();
        }
    }

    private bool IsInteractableValid(IInteractable interactable)
    {
        // เช็คว่า Object ยังมีชีวิตอยู่ ไม่ได้ถูกลบทิ้ง (Destroy) ไปแล้ว
        return interactable != null && (interactable as MonoBehaviour) != null;
    }

    void CheckInteraction()
    {
        // คำนวณจุดศูนย์กลางของทรงกลม (ขยับขึ้นมาช่วงกลางลำตัว)
        Vector3 origin = playerCenter != null ? playerCenter.position : transform.position;
        origin += Vector3.up * 1f;

        // ใช้ OverlapSphere ค้นหาทุก Collider ที่อยู่ในรัศมี
        Collider[] colliders = Physics.OverlapSphere(origin, interactRange);
        
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        // วนลูปหา Interactable ที่อยู่ "ใกล้ผู้เล่นที่สุด" ในวงกลม
        Vector3 forward = playerCenter != null ? playerCenter.forward : transform.forward;
        forward.y = 0; // ไม่คิดแกน Y ตอนคำนวณมุม

        foreach (Collider col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            
            if (interactable != null)
            {
                // เช็คว่า Object อยู่ในระยะมุมมองการมองเห็นด้านหน้าหรือไม่
                Vector3 dirToObj = (interactable.GetTransform().position - origin).normalized;
                dirToObj.y = 0; // ไม่คิดแกน Y 

                if (Vector3.Angle(forward, dirToObj) <= viewAngle / 2f)
                {
                    // หาระยะห่างระหว่างผู้เล่นกับ Object
                    float distance = Vector3.Distance(origin, interactable.GetTransform().position);
                    
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }
        }

        // ถ้ามี Object ในรัศมีอย่างน้อย 1 ตัว
        if (IsInteractableValid(closestInteractable))
        {
            // ถ้าเป็นตัวใหม่ (ไม่ใช่ตัวที่กำลังมองอยู่เดิม) หรือตัวเดิมถูกลบไปแล้ว
            if (currentInteractable != closestInteractable || !IsInteractableValid(currentInteractable))
            {
                if (IsInteractableValid(currentInteractable)) currentInteractable.OnLookEnd(); // ตัวเก่า Fade Out หายไป
                currentInteractable = closestInteractable;
                currentInteractable.OnLookStart(); // ตัวใหม่ Fade In โผล่ขึ้นมา
            }
        }
        else
        {
            // ไม่เจอใดๆ เลยในวงกลม
            ClearInteractable();
        }
    }

    void ClearInteractable()
    {
        if (IsInteractableValid(currentInteractable))
        {
            currentInteractable.OnLookEnd();
        }
        currentInteractable = null;
    }

    // ------------------------------------------------------------------
    // ฟังก์ชัน OnDrawGizmos จะช่วยวาดโครงเส้นทรงกลมให้เห็นในหน้าต่าง Scene
    // ทำให้คุณเห็นอาณาเขตรอบตัวละครได้อย่างชัดเจน
    // ------------------------------------------------------------------
    void OnDrawGizmos()
    {
        Vector3 origin = playerCenter != null ? playerCenter.position : transform.position;
        origin += Vector3.up * 1f;
        Vector3 forward = playerCenter != null ? playerCenter.forward : transform.forward;

        // วาดเส้นทรงกลม (ระยะรัศมีทั้งหมด) เป็นสีเขียวจางๆ
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(origin, interactRange);

        // วาดเส้นขอบเขตการมองเห็น (มุมซ้ายและมุมขวา) เป็นสีเหลือง
        Gizmos.color = Color.yellow;
        
        // คำนวณเส้นมุมซ้ายและขวา
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * forward;

        // วาดเส้นจากจุดศูนย์กลางออกไปตามระยะ
        Gizmos.DrawLine(origin, origin + leftBoundary * interactRange);
        Gizmos.DrawLine(origin, origin + rightBoundary * interactRange);
    }
}
