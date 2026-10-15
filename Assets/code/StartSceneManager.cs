using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("ตัวเล่นวิดีโอ")]
    public VideoPlayer videoPlayer;

    [Header("UI Controls")]
    [Tooltip("หน้าจอ UI ตัวหนังสือ Press Enter to Skip")]
    public GameObject skipUI; 

    [Header("Scene Transition")]
    [Tooltip("ชื่อของฉากที่ต้องการให้ไปต่อหลังจากดูวิดีโอจบ (เช่น _main map)")]
    public string nextSceneName = "_main map";

    private bool isTransitioning = false;

    void Start()
    {
        // โชว์ป้าย Press Enter to skip
        if (skipUI != null) skipUI.SetActive(true);

        if (videoPlayer != null)
        {
            // สั่งให้เล่นทันทีเมื่อเปิดเกม
            videoPlayer.Play();
            // ดักจับเมื่อวิดีโอเล่นจนจบตามธรรมชาติ
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("ไม่พบ VideoPlayer ในสคริปต์ StartSceneManager");
        }
    }

    void Update()
    {
        // ถ้าระบบกำลังย้ายฉากอยู่ ให้หยุดทำงานทันที (กันผู้เล่นกดรัวๆ)
        if (isTransitioning) return;

        // ถ้ากด Enter ให้ทำการ Skip วิดีโอ
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log("Skipped Intro Video!");
            TransitionToNextScene();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (isTransitioning) return;
        Debug.Log("Intro Video Finished!");
        TransitionToNextScene();
    }

    private void TransitionToNextScene()
    {
        isTransitioning = true;
        
        // หยุดวิดีโอและซ่อนป้าย
        if (videoPlayer != null) videoPlayer.Stop();
        if (skipUI != null) skipUI.SetActive(false);

        // รีเซ็ตสถานะเกมก่อนเริ่มเล่น (เพื่อความแน่ใจว่าค่าต่างๆ เช่น ตำแหน่งเดิม ถูกล้างแล้ว)
        GameState.ReturningFromMinigame = false;

        // โหลดเข้าฉากหลัก
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("ยังไม่ได้ใส่ชื่อฉากต่อไปใน Next Scene Name!");
        }
    }
}
