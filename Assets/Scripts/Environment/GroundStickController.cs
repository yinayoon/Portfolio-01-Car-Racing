using UnityEngine;

// - 차량이 바닥에 밀착하도록 위치와 회전을 조정함
// + Raycast를 사용하여 바닥 감지
// + 부드러운 위치 조정 및 회전 조정
public class GroundStickController : MonoBehaviour
{
    [Header("- Ground Settings")]
    public LayerMask groundLayer;       // 바닥을 감지할 레이어
    public float groundCheckDistance = 1.5f; // 바닥 확인 거리
    public float heightOffset = 0.1f;   // 바닥과 차량 사이의 보정 높이
    public float groundLerpSpeed = 10f; // 부드럽게 바닥에 붙는 속도

    [Header("- Rigidbody Component")]
    private Rigidbody rb; // Rigidbody 컴포넌트 참조

    private void Awake()
    {
        // Rigidbody 컴포넌트 가져오기
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        StickToGround();
    }

    private void StickToGround()
    {
        // Raycast를 사용한 바닥 감지
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            // - 목표 높이를 설정
            // + Raycast로 감지한 바닥의 높이에 heightOffset을 더해 차량의 목표 위치를 설정
            Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y + heightOffset, transform.position.z);

            // 부드럽게 위치 보정
            rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, groundLerpSpeed * Time.fixedDeltaTime));

            // - 경사에 맞춰 회전
            // + 오브젝트(예: 차량)가 현재의 up 방향을 지면의 법선 벡터(hit.normal)로 회전하도록 조정하기 위한 것
            Quaternion groundRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            // + 부드럽게 회전을 적용
            rb.MoveRotation(Quaternion.Lerp(transform.rotation, groundRotation, groundLerpSpeed * Time.fixedDeltaTime));
        }
    }
}
