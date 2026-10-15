using UnityEngine;

public static class GameState
{
    // Flags สำหรับระบบมินิเกม
    public static bool ReturningFromMinigame = false;
    public static bool MinigameWon = false;
    
    // ข้อมูลสำหรับเปิด Dialogue ต่อ
    public static string LastNPCName = "";

    // ข้อมูลสำหรับบันทึกตำแหน่งผู้เล่น
    public static Vector3 PlayerSavedPosition = Vector3.zero;
    public static Quaternion PlayerSavedRotation = Quaternion.identity;

    // ระบบช่องเก็บของสำหรับเควส (Minigame 3)
    public static System.Collections.Generic.List<string> Inventory = new System.Collections.Generic.List<string>();
    public static System.Collections.Generic.List<string> ActiveQuests = new System.Collections.Generic.List<string>();

    // ข้อมูลสำหรับสะสมรูปภาพที่ปลดล็อคจาก NPC
    public static System.Collections.Generic.HashSet<string> UnlockedNPCImages = new System.Collections.Generic.HashSet<string>();
}
