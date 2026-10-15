using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HoopTrigger : MonoBehaviour
{
    private void Start()
    {
        // บังคับให้ Collider นี้เป็น Trigger (ผู้เล่นเดินทะลุได้ แต่จับการชนได้)
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // เช็คว่าวัตถุที่เข้ามาชนคือลูกบาสหรือไม่
        Basketball ball = other.GetComponent<Basketball>();
        
        if (ball != null && !ball.hasScored)
        {
            // เช็คว่าลูกบาสกำลังตกลงมา (ความเร็วแกน Y ติดลบ) 
            // เพื่อป้องกันคนโกงด้วยการโยนยัดเข้าจากข้างใต้ห่วง
            if (ball.GetComponent<Rigidbody>().velocity.y < 0)
            {
                ball.hasScored = true; // ล็อกไว้ไม่ให้ลูกเดิมเด้งไปมาแล้วบวกคะแนนเพิ่ม
                
                if (Minigame4Controller.Instance != null)
                {
                    Minigame4Controller.Instance.AddScore();
                }
            }
        }
    }
}
