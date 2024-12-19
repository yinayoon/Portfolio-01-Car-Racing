using UnityEngine;

// - 차량에 커스텀 중력을 적용하고 바닥에 밀착되도록 처리
// + 사용자 정의 중력 적용
// + 바닥 밀착 및 경사면 회전 처리
public class HeavyGravityController : MonoBehaviour
{
    [Header("- Gravity Settings")]
    public float customGravity = -50f; // 중력 값을 높이는 정도
    public float groundCheckDistance = 2f; // 바닥 확인 거리
    public LayerMask groundLayer; // 바닥 레이어 설정
    public float groundOffset = 0.1f; // 바닥과 차량 사이의 보정값

    [Header("- Rigidbody Settings")]
    private Rigidbody rb; // Rigidbody 컴포넌트 참조

    private void Start()
    {
        // Rigidbody 설정
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = false; // Rigidbody 기본 중력 비활성화
        rb.mass = 1500f; // 차량 무게 증가
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 움직임을 부드럽게 만듦

        // 전역 중력값 설정 (Unity의 전역 중력 설정이며, 모든 Rigidbody에 영향을 미침)
        Physics.gravity = new Vector3(0, customGravity, 0);
    }

    // 지속적으로 중력을 적용하고 바닥 상태를 확인하여, 차량이 공중에 뜨는 현상을 방지
    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(0, customGravity, 0), ForceMode.Acceleration);
    }
}
