using UnityEngine;

// - AI 차량이 골인지점에 도달했을 때 동작을 처리하는 스크립트
// + 골인지점 도달 시 일정 시간 동안 이동 후 정지
// + AI의 웨이포인트 초기화 및 이동 종료
public class AIFinishController : MonoBehaviour
{
    [Header("- Movement Settings")]
    public float moveSpeed = 30f;    // 이동 속도 (골인 후)
    public float delayTime = 4f;    // 정지 전 대기 시간

    [Header("- AI Components")]
    public AICarController aiCarController;

    private bool hasFinished = false; // 골인지점 도달 여부
    private float stopDelay;          // 멈추기까지의 시간

    private void Start()
    {
        // 기존 AICarController 가져오기
        aiCarController = GetComponent<AICarController>();
    }

    private void Update()
    {
        // AI 자동차가 골인지점에 도달했는지 여부 확인
        if (hasFinished)
        {
            stopDelay -= Time.deltaTime;

            if (stopDelay <= 0f)
            {
                // 이동 멈춤
                moveSpeed = 0f;
                enabled = false; // 스크립트 비활성화
                aiCarController.WheelRotationClear();
            }
            else
            {
                ContinueForward(); // ContinueForward를 통해 설정된 방향으로 일정 시간 동안만 이동
                aiCarController.WaypointsClear(); // 웨이포인트 초기화(WaypointsClear)로 인해 경로를 따라가지 않음
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 골인지점 태그 확인
        if (other.CompareTag("Gameover") && !hasFinished)
        {
            hasFinished = true; // 골인 상태
            stopDelay = delayTime; // 딜레이 설정
        }
    }

    private void ContinueForward()
    {
        // 계속 전진 (transform.Translate 사용)
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}
