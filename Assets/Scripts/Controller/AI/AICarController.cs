using UnityEngine;

// AI 차량의 이동, 부스터 활성화, 회피 로직을 처리하는 핵심 스크립트
// - 주요 기능
// + AI 차량의 웨이포인트 기반 이동
// + 장애물 감지 및 회피
// + 부스터 활성화 및 종료 처리
public class AICarController : MonoBehaviour
{
    [Header("- Waypoints Configuration")]
    public Transform[] waypoints; // 웨이포인트 배열
    private int currentWaypointIndex = 0; // 현재 웨이포인트 인덱스

    [Header("- Car Settings")]
    public float speed = 15f; // 기본 이동 속도
    public float boostedSpeed = 30f; // 부스터 사용 시 속도
    private float currentSpeed; // 현재 속도
    public float rotationSpeed = 5f; // 회전 속도

    [Header("- Ground Detection")]
    public float groundCheckDistance = 1.0f; // 바닥 확인 거리
    public LayerMask groundLayer; // 바닥 레이어
    public float heightOffset = 0.1f; // 높이 보정 값

    [Header("- Path Randomization")]
    public float randomOffsetRange = 2f; // 경로에서 랜덤 이탈 범위
    private Vector3 randomOffset; // 현재 랜덤 오프셋

    [Header("- Booster Settings")]
    private bool isBoosting = false; // 부스터 사용 여부
    public float boostChance = 0.3f; // 부스터를 사용할 확률 (0 ~ 1)
    public float boostDuration = 2f; // 부스터 지속 시간
    private float boostTimer = 0f; // 부스터 타이머
    public GameObject boostEffect; // 부스터 효과 이펙트 게임 오브젝트

    [Header("- Sound Settings")]
    public AudioSource engineSound; // 엔진 소리
    public AudioSource boosterSound; // 부스터 소리
    public AudioSource collisionAudioSource; // 충돌 소리 재생용 오디오 소스
    public float enginePitchMin = 0.8f; // 엔진 최소 피치
    public float enginePitchMax = 1.5f; // 엔진 최대 피치

    [Header("- Collision Settings")]
    public GameObject sparkParticle; // 자식 파티클 오브젝트
    public float particleLifetime = 0.5f; // 파티클 활성화 지속 시간
    private bool isParticleActive = false; // 파티클 활성화 상태

    [Header("- Wheel Settings")]
    public Transform[] wheelTransforms; // 바퀴 휠 오브젝트
    public float wheelRotationSpeed = 1000f; // 휠 회전 속도

    [Header("- Obstacle Avoidance")]
    public float obstacleAvoidanceDistance = 5f; // 장애물 감지 거리
    public LayerMask obstacleLayer; // 장애물 레이어
    public float slowDownFactor = 0.5f; // 감속 비율

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("Waypoints array is null or empty. Please assign waypoints in the inspector.");
            return;
        }

        // 초기 속도를 기본 속도로 설정
        currentSpeed = speed;

        // 랜덤 오프셋을 생성하여 경로를 약간 벗어나게 설정
        GenerateRandomOffset();
    }

    private void Update()
    {        
        // 웨이포인트로 이동
        MoveToNextWaypoint();

        // 차량이 바닥에 고정되도록 위치를 조정
        AdjustPositionToGround();

        // 부스터 상태를 처리
        HandleBooster();

        // 엔진 소리 업데이트
        UpdateEngineSound();

        // 바퀴 회전 업데이트
        RotateWheels();

        // 파티클 타이머 관리
        if (isParticleActive && sparkParticle != null)
        {
            particleLifetime -= Time.deltaTime;
            if (particleLifetime <= 0f)
            {
                // 파티클 비활성화 및 상태 초기화
                sparkParticle.SetActive(false);
                isParticleActive = false;
                particleLifetime = 0.5f;
            }
        }
    }

    // <MoveToNextWaypoint()의 역할>
    // AI 자동차가 웨이포인트 경로를 따라 움직이도록 처리하는 핵심 이동 로직임
    // AI 자동차가 현재 웨이포인트를 향해 이동하며 장애물을 회피하거나 속도를 조정하고,
    // 웨이포인트에 도달하면 다음 웨이포인트로 전환하는 작업을 수행함
    private void MoveToNextWaypoint()
    {
        // - 웨이포인트가 없으면 함수 종료
        if (waypoints.Length == 0) return;

        // - 현재 웨이포인트로의 목표 위치 설정
        // + currentWaypointIndex에 해당하는 웨이포인트의 위치를 가져옴
        // + 여기에 randomOffset을 더해 약간의 불규칙적인 움직임을 추가함
        // + y축 좌표는 차량의 현재 높이를 유지하도록 설정함
        Vector3 targetPosition = waypoints[currentWaypointIndex].position + randomOffset;
        targetPosition.y = transform.position.y; // 높이 고정

        // - 장애물 감지 및 회피 처리
        // + 차량 앞에 장애물이 있는지 Physics.Raycast로 확인
        if (Physics.Raycast(transform.position, transform.forward, obstacleAvoidanceDistance, obstacleLayer))
        {
            // - 감속 처리
            // + 장애물이 감지되면 속도를 slowDownFactor로 줄임
            currentSpeed = Mathf.Lerp(currentSpeed, speed * slowDownFactor, Time.deltaTime * 2f);

            // - 회피 방향 결정
            Vector3 avoidanceDirection = Vector3.zero;
            // + 오른쪽이 비어 있으면 오른쪽으로 회피
            if (!Physics.Raycast(transform.position, transform.right, obstacleAvoidanceDistance, obstacleLayer))
            {
                avoidanceDirection = transform.right; // 오른쪽으로 회피
            }
            // + 왼쪽이 비어 있으면 왼쪽으로 회피
            else if (!Physics.Raycast(transform.position, -transform.right, obstacleAvoidanceDistance, obstacleLayer))
            {
                avoidanceDirection = -transform.right; // 왼쪽으로 회피
            }

            // - 회피 경로 설정
            // + 회피 방향으로 목표 위치를 수정
            if (avoidanceDirection != Vector3.zero)
            {
                targetPosition = transform.position + avoidanceDirection * 2.0f;
            }
        }
        else
        {
            // 장애물이 없으면 속도를 원래대로 복원
            currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 2f);
        }

        // - 목표 위치로 이동
        // + MoveTowards를 사용하여 차량이 목표 위치로 부드럽게 이동하도록 함
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

        // - 목표 방향으로 회전
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            // + 차량이 목표 위치를 바라보도록 회전
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // + Lerp를 사용하여 회전을 부드럽게 처리
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // - 차량이 현재 웨이포인트에 도달했는지 확인
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            // + 다음 웨이포인트로 이동
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            // + 새로운 랜덤 오프셋 생성
            GenerateRandomOffset();

            // + 부스터 활성화 시도
            TryActivateBooster();
        }
    }

    private void AdjustPositionToGround()
    {
        // 바닥 감지 Raycast 실행
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            // 바닥 높이에 맞게 위치 조정
            Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y + heightOffset, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

            // 경사면에 맞게 회전 조정
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, slopeRotation, Time.deltaTime * 5f);
        }
    }

    // AI가 웨이포인트 경로를 따라 이동할 때 약간의 불규칙적인 움직임을 추가하기 위해 랜덤한 오프셋(offset)을 생성
    // 이를 통해 AI 차량의 움직임이 더 자연스럽고 다이내믹하게 보이도록 함
    private void GenerateRandomOffset()
    {
        // 웨이포인트 경로에서의 랜덤 오프셋 생성
        randomOffset = new Vector3(
            Random.Range(-randomOffsetRange, randomOffsetRange), // X축에서의 랜덤 오프셋
            0, // Y축 오프셋은 고정 (바닥에 붙어 있어야 하므로)
            Random.Range(-randomOffsetRange, randomOffsetRange) // Z축에서의 랜덤 오프셋
        );
    }

    // AI 자동차가 일정 확률로 부스터를 활성화할 수 있도록 처리하는 기능을 담당함
    private void TryActivateBooster()
    {
        // - 부스터를 활성화할 확률 체크
        // + !isBoosting : 현재 부스터가 활성화되어 있지 않은 상태인지 확인
        // + Random.value < boostChance : 0부터 1 사이의 무작위 값을 생성하여 boostChance와 비교
        if (!isBoosting && Random.value < boostChance)
        {
            isBoosting = true; // 부스터가 활성화되었음을 표시
            currentSpeed = boostedSpeed; // 부스터 속도(boostedSpeed)로 변경
            boostTimer = boostDuration; // 부스터 지속 시간 설정

            boostEffect.SetActive(true); // 부스터 이펙트 활성화            
            boosterSound.Play(); // 부스터 소리 재생
        }
    }

    // <HandleBooster()의 역할>
    // 부스터가 활성화된 상태에서의 동작을 관리하고, 부스터가 종료되면 관련 설정을 초기 상태로 복구
    private void HandleBooster()
    {
        if (isBoosting) // 부스터가 활성화된 상태인지 확인
        {
            // 부스터 타이머 감소
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f) // boostTimer가 0 이하가 되면 부스터가 종료
            {                
                isBoosting = false; // 부스터 비활성화
                currentSpeed = speed; // 속도를 기본 속도로 복원

                boostEffect.SetActive(false); // 부스터 이펙트 비활성화
                boosterSound.Stop(); // 부스터 사운드 정지
            }
        }
    }

    // <UpdateEngineSound()의 역할>
    // AI 자동차의 속도에 따라 엔진 소리의 피치(pitch)를 동적으로 조정하여 차량 움직임에 맞는 음향 효과를 제공
    private void UpdateEngineSound()
    {
        // - 엔진 소리의 피치를 속도에 따라 조정
        // + Mathf.Lerp를 사용해 지정된 두 값(enginePitchMin과 enginePitchMax) 사이에서 비율(currentSpeed / boostedSpeed)에 따라 중간 값을 계산
        float pitch = Mathf.Lerp(enginePitchMin, enginePitchMax, currentSpeed / boostedSpeed);
        engineSound.pitch = pitch; // 계산된 피치를 엔진 소리의 pitch 속성에 적용
    }

    // <RotateWheels()의 역할>
    // AI 자동차의 바퀴 휠이 차량의 현재 속도에 맞춰 자연스럽게 회전하도록 처리하는 역할을 함
    private void RotateWheels()
    {
        // 바퀴 배열 유효성 검사
        if (wheelTransforms == null || wheelTransforms.Length == 0) return;

        // 바퀴 회전 계산
        float rotationAmount = currentSpeed * wheelRotationSpeed * Time.deltaTime;

        // 바퀴 회전 적용
        foreach (Transform wheel in wheelTransforms)
        {
            if (wheel != null)
            {
                wheel.Rotate(Vector3.right, rotationAmount, Space.Self); // Space.Self : 로컬 축
            }
        }
    }

    // 해당 "OnCollisionEnter"는 AI 차량이 다른 오브젝트와 충돌할 때 실행되며,
    // 특정 충돌 조건에 따라 스파크 파티클을 활성화하거나, 충돌 소리를 재생하는 역할을 함
    private void OnCollisionEnter(Collision collision)
    {
        // 특정 태그의 오브젝트와 충돌 시 스파크를 생성하지 않음
        if (collision.gameObject.CompareTag("Ground")) return;

        // - 충돌 지점으로 스파크 파티클 이동 및 활성화
        // + collision.contacts[0].point : 충돌 지점의 좌표
        sparkParticle.transform.position = collision.contacts[0].point; // 스파크 파티클의 위치를 충돌 지점으로 이동
        isParticleActive = true;
        
        collisionAudioSource.Play(); // 충돌 소리 재생
    }

    // <OnDrawGizmos()의 역할>
    // Unity 에디터의 Scene 뷰에서 디버깅과 시각화를 위해 Gizmos를 그리는 함수
    private void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            // 웨이포인트 위치와 경로 시각화
            Gizmos.color = Color.green; // 웨이포인트와 경로를 그릴 때 사용할 색상을 초록색으로 설정

            // 모든 웨이포인트를 순회하며 구체와 경로를 그림
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.5f); // 웨이포인트의 위치를 구체(Sphere)로 표시
                if (i < waypoints.Length - 1) // 마지막 웨이포인트는 다음 웨이포인트가 없으므로 선(Line)을 그리지 않음
                    // 현재 웨이포인트와 다음 웨이포인트 사이에 선(Line)을 그려 경로를 시각화
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // 장애물 감지 거리 시각화
        Gizmos.color = Color.red; // 장애물 감지 거리 시각화를 위한 색상을 빨간색으로 설정
        // transform.position에서 transform.forward)으로 장애물 감지 거리를 선(Line)으로 표시
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * obstacleAvoidanceDistance);
    }

    public void WaypointsClear()
    {
        // 웨이포인트 배열 초기화
        waypoints = new Transform[0];
    }

    public void WheelRotationClear()
    {
        // 바퀴 회전 속도 초기화
        wheelRotationSpeed = 0;
    }
}
