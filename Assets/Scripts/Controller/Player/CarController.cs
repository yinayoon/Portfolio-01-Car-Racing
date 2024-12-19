using UnityEngine;

// - 플레이어 차량의 이동, 회전, 드리프트, 부스터 등의 조작을 처리
// + 이동 및 회전 제어
// + 부스터 및 드리프트 활성화
// + 속도 및 브레이크 처리
public class CarController : MonoBehaviour
{
    [Header("- Movement Settings")]
    public float acceleration = 5f; // 가속도
    public float deceleration = 5f; // 감속도
    public float brakeForce = 10f; // 브레이크 감속도
    public float maxSpeed = 20f; // 최대 속도
    public float turnSpeed = 50f; // 회전 속도
    public float driftTurnSpeedMultiplier = 1.5f; // 드리프트 시 회전 속도 배율
    public Rigidbody rb;

    [Header("- Boost Settings")]
    public float boostMultiplier = 2f; // 부스터 배율
    public float boostDuration = 3f; // 부스터 지속 시간
    public float boostCooldown = 5f; // 부스터 대기 시간
    public float boostSpeedIncrease = 10f; // 부스터 속도 증가량
    public GameObject boostEffect; // 부스터 이펙트 오브젝트

    [Header("- Wheel Settings")]
    public Transform[] wheels; // 바퀴 Transform 배열
    public float wheelTurnSpeed = 200f; // 바퀴 회전 속도

    [Header("- Camera Settings")]
    public Transform cameraTransform; // 카메라 Transform
    public Vector3 cameraFixPosition = new Vector3(0, 2, -4); // 카메라 위치 오프셋
    public float cameraFollowSpeed = 10f; // 카메라 따라가는 속도

    [Header("- Effects Settings")]
    public GameObject smokeEffect;
    public GameObject collisionParticlePrefab; // 충돌 시 생성될 파티클 프리팹

    [Header("- Sound Settings")]
    public AudioSource engineSound; // 엔진 사운드
    public AudioSource accelerationSound; // 액셀 사운드
    public AudioSource driftSound; // 드리프트 사운드
    public AudioSource collisionSound; // 충돌 사운드

    private float currentSpeed = 0f; // 현재 속도
    private bool isDrifting = false; // 드리프트 상태
    private bool isBoosting = false; // 부스터 상태
    private float boostTimeLeft = 0f; // 남은 부스터 시간
    private float timeAboveMaxSpeed = 0f; // 최대 속도 이상으로 달린 시간
    private float currentBoostCooldown = 0f; // 현재 남은 부스터 대기 시간

    public static bool IsInputEnabled = true; // 입력 활성화 상태 플래그

    void Start()
    {
        IsInputEnabled = true; // 플레이어의 입력을 활성화
        smokeEffect.SetActive(true); // 스모크 이펙트를 초기 상태에서 비활성화
        boostEffect.SetActive(false); // 부스터 이펙트를 초기 상태에서 비활성화
        engineSound.loop = true; // 엔진 소리가 반복적으로 재생되도록 설정
        engineSound.Play(); // 엔진 사운드 재생
    }

    void Update()
    {
        // - 부스터 대기 시간 감소 로직
        // + !isBoosting : 현재 부스터가 활성화되지 않았을 때만 실행
        // + currentBoostCooldown : 부스터를 다시 사용할 수 있기까지 남은 대기 시간을 의미
        if (!isBoosting && currentBoostCooldown > 0f)
        {
            currentBoostCooldown -= Time.deltaTime; // 매 프레임마다 대기 시간을 줄임
            currentBoostCooldown = Mathf.Max(0f, currentBoostCooldown); // 시간이 0 이하로 내려가지 않도록 보장
        }

        // - 부스터 활성화 로직
        // + 플레이어가 Z 키를 눌렀거나 부스터가 대기 시간이 끝났거나 차량이 최대 속도에 도달한 상태라면
        if (Input.GetKeyDown(KeyCode.Z) && currentBoostCooldown <= 0f && currentSpeed >= maxSpeed)
        {
            ActivateBoost(); // 부스터 활성화 함수 호출
        }

        // - 방향키에서 손을 떼거나 부스터 시간이 끝나면 종료
        // + 플레이어가 W, S 또는 방향키를 누르지 않거나 부스터 지속 시간이 끝났으면
        if (isBoosting && (Input.GetAxisRaw("Vertical") == 0 || boostTimeLeft <= 0f))
        {
            EndBoost(); // 부스터 비활성화 함수 호출
        }

        // - 부스터 시간 초기화 로직
        // + 차량 속도가 최대 속도 미만이거나 부스터가 활성화되어 있지 않으면
        if (currentSpeed < maxSpeed && !isBoosting)
        {
            timeAboveMaxSpeed = 0f; // 부스터 조건에 필요한 최대 속도 초과 시간을 초기화
        }
    }

    void FixedUpdate()
    {
        // Rigidbody가 null인 경우 FixedUpdate 실행 중단
        if (rb == null)
        {
            Debug.LogError("Rigidbody가 할당되지 않았습니다. Rigidbody를 추가하거나 스크립트를 확인하세요.");
            return;
        }

        // 방향키 입력
        float moveInput = Input.GetAxis("Vertical"); // W/S 또는 ^/v
        float turnInput = Input.GetAxis("Horizontal"); // A/D 또는 </>

        HandleBoost(); // 부스터의 상태를 업데이트하고, 필요한 경우 활성화 또는 종료를 처리
        HandleBrake(moveInput); // 브레이크 입력 여부를 확인하고 감속 로직을 처리
        HandleDrift(turnInput); // 드리프트 입력 여부를 확인하고 상태를 조정

        // - 엔진 사운드 피치 조정
        // + 현재 속도(currentSpeed)와 최대 속도(maxSpeed)의 비율에 따라 엔진 소리의 피치를 조정.
        engineSound.pitch = Mathf.Clamp(1 + (currentSpeed / maxSpeed), 1, 2); // 피치 값이 1에서 2 사이를 벗어나지 않도록 제한

        // - 최대 속도 제한
        float effectiveMaxSpeed;
        // + 부스터가 활성화 상태라면 최대 속도에 부스터 배율을 곱한 값을 사용
        if (isBoosting) { effectiveMaxSpeed = maxSpeed * boostMultiplier; }
        // + 부스터가 비활성화 상태라면 기본 최대 속도를 사용
        else { effectiveMaxSpeed = maxSpeed; }
        // + currentSpeed가 최소값(-effectiveMaxSpeed)과 최대값(effectiveMaxSpeed) 사이에 있도록 제한
        // + 후진이 필요하므로 0이아닌 -effectiveMaxSpeed으로 적용
        currentSpeed = Mathf.Clamp(currentSpeed, -effectiveMaxSpeed, effectiveMaxSpeed);

        // 입력이 비활성화된 상태라면 이동 및 회전을 처리하지 않고 메서드를 종료
        if (!IsInputEnabled)
        {
            return;
        }

        // - 앞으로/뒤로 이동
        // + 현재 이동 속도(currentSpeed)와 프레임 시간(Time.fixedDeltaTime)에 기반한 이동 방향 벡터
        Vector3 moveDirection = transform.forward * currentSpeed * Time.fixedDeltaTime;
        // + Rigidbody의 현재 위치에서 계산된 방향으로 이동
        rb.MovePosition(rb.position + moveDirection);

        // 좌우 회전
        if (currentSpeed != 0) // 움직일 때만 회전
        {
            float adjustedTurnSpeed; // 현재 자동차의 회전 속도를 저장하는 변수
            // - isDrifting 변수가 true라면, 자동차가 드리프트 상태라는 뜻
            // + turnSpeed에 driftTurnSpeedMultiplier 값을 곱하여 회전 속도를 증가
            if (isDrifting) { adjustedTurnSpeed = turnSpeed * driftTurnSpeedMultiplier; }
            // + 기본 회전 속도인 turnSpeed를 그대로 사용
            else { adjustedTurnSpeed = turnSpeed; }

            // - 회전 각도를 기준으로 쿼터니언을 생성
            // + turnInput * adjustedTurnSpeed * Time.fixedDeltaTime : Y축을 기준으로 회전
            Quaternion turnRotation = Quaternion.Euler(0, turnInput * adjustedTurnSpeed * Time.fixedDeltaTime, 0);
            // - 현재 자동차의 Rigidbody에 새로운 회전 값을 적용
            // + rb.rotation * turnRotation : 현재 회전에 새로운 회전을 곱하여 업데이트
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        // 바퀴 회전 함수 호출
        RotateWheels(moveInput);

        // 카메라가 차량을 따라가게 하는 함수 호출
        FollowCamera();
    }

    // <HandleBoost()의 역할>
    // 부스터 기능 처리
    // 차량의 현재 속도가 maxSpeed 이상인 경우, 최대 속도 초과 시간(timeAboveMaxSpeed)을 증가시킴
    // 속도가 maxSpeed 미만으로 떨어지면 timeAboveMaxSpeed를 0으로 초기화함
    // 부스터가 발동되기 전에 일정 시간 동안 maxSpeed 이상을 유지해야 하도록 설계함
    void HandleBoost()
    {
        // 현재 속도가 최대 속도를 초과했는지 확인
        if (currentSpeed >= maxSpeed)
        {
            // 최대 속도 초과 시간 누적
            timeAboveMaxSpeed += Time.fixedDeltaTime;
        }
        else
        {
            // 속도가 최대 속도 미만이면 누적 시간 초기화
            timeAboveMaxSpeed = 0f;
        }

        // - 부스터 활성화 조건 확인
        // + Z 키를 눌렀거나(누른 순간 감지), 자동차가 최대 속도(maxSpeed)를 초과한 상태로 일정 시간 이상(boostCooldown) 유지했거나
        // 이전 부스터 사용으로 인한 지속 시간이 모두 소모되었는지 확인하여 조건이 만족하면
        if (Input.GetKeyDown(KeyCode.Z) && timeAboveMaxSpeed >= boostCooldown && boostTimeLeft <= 0f)
        {
            ActivateBoost(); // 부스터 활성화
        }

        // 부스터가 활성화 상태일 경우
        if (isBoosting)
        {
            // 부스터 남은 시간 감소
            boostTimeLeft -= Time.fixedDeltaTime;

            // 부스터 시간이 다되면 비활성화
            if (boostTimeLeft <= 0f) { EndBoost(); }
        }
    }

    // <ActivateBoost()의 역할>
    // 부스터 기능을 활성화하는 역할을 수행
    void ActivateBoost()
    {
        isBoosting = true; // 부스터 상태 활성화        
        boostTimeLeft = boostDuration; // 부스터 지속 시간 설정
        currentBoostCooldown = boostCooldown; // 부스터 대기 시간을 boostCooldown으로 초기화
        currentSpeed += boostSpeedIncrease; // 부스터로 인한 속도 증가

        // 부스터 이펙트 활성화
        boostEffect.SetActive(true); 

        // 가속 사운드 재생 이미 재생 중이 아닐 경우
        if (!accelerationSound.isPlaying)
        {
            accelerationSound.Play(); // 가속 사운드 재생
        }
    }

    // <EndBoost()의 역할>
    // 부스터(Boost) 상태가 종료될 때 관련 속성과 효과를 초기 상태로 복구함
    void EndBoost()
    {
        isBoosting = false; // 부스터 상태를 비활성화하여 차량이 부스터 상태가 아님을 명시
        timeAboveMaxSpeed = 0f; // 부스터 사용 도중 누적된 최대 속도 초과 시간을 초기화

        boostEffect.SetActive(false); // 부스터 이펙트 비활성화

        // 가속 사운드 재생 이미 재생 중일 경우
        if (accelerationSound.isPlaying)
        {
            accelerationSound.Stop(); // 가속 사운드 중지
        }
    }

    // <HandleBrake()의 역할>
    // 플레이어가 차량의 속도를 제어하거나 멈출 수 있도록 브레이크와 가속을 처리하는 기능 함
    void HandleBrake(float moveInput)
    {
        // 플레이어가 브레이크(Shift 키)를 눌렀는지 확인
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // 현재 속도(currentSpeed)를 0으로 부드럽게 줄임
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, brakeForce * Time.fixedDeltaTime);
        }
        else // 브레이크를 누르지 않은 경우
        {
            // - 속도 계산
            // + 플레이어가 전진(W) 또는 후진(S) 입력을 감지하고, 부스터가 활성화되지 않았을 때만 속도를 변경
            if (moveInput != 0 && !isBoosting)
            {
                // 가속도를 변수로 저장
                float effectiveAcceleration = acceleration;

                // 전진(W) 또는 후진(S) 입력에 따라 차량의 속도를 증가하거나 감소
                currentSpeed += moveInput * effectiveAcceleration * Time.fixedDeltaTime;
            }
            else if (!isBoosting) // 부스터가 비활성화 상태라면 감속 처리
            {
                // 차량 속도를 부드럽게 0으로 줄임
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
            }
        }
    }

    // <HandleDrift()의 역할>
    // 드리프트를 활성화하거나 비활성화하는 함수
    void HandleDrift(float turnInput)
    {
        // LeftControl 키가 눌렸는지 확인
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // 드리프트 활성화
            isDrifting = true;

            // 드리프트 사운드가 재생 중이 아닐 경우
            if (!driftSound.isPlaying)
            {
                driftSound.Play(); // 드리프트 사운드 재생
            }
        }
        else
        {
            // 드리프트 비활성화
            isDrifting = false;

            // 드리프트 사운드가 재생 중일 경우 중지
            if (driftSound.isPlaying)
            {
                driftSound.Stop(); // 드리프트 사운드 중지
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 특정 태그에 대해 예외 처리
        if (collision.gameObject.CompareTag("Ground"))
        {
            return; // 태그가 "Ground"인 경우 충돌 효과를 적용하지 않음
        }

        // 충돌 소리 재생
        collisionSound.Play();

        // - 충돌 파티클 생성
        // + 충돌 위치에 파티클 생성
        GameObject collisionParticle = Instantiate(collisionParticlePrefab, collision.contacts[0].point, Quaternion.identity);
        // + 일정 시간 후 파티클 제거
        Destroy(collisionParticle, 2f);
    }

    // <RotateWheels()의 역할>
    // 차량이 전진하거나 후진할 때 바퀴를 자연스럽게 회전
    void RotateWheels(float moveInput)
    {
        // 입력된 이동 값이 0이 아니라면 바퀴를 회전시킴
        if (moveInput != 0)
        {
            // 모든 바퀴 배열(wheels)을 순회
            foreach (Transform wheel in wheels)
            {
                // 바퀴가 null이 아닌 경우에만 회전
                if (wheel != null)
                {
                    // 입력된 값(moveInput)에 바퀴 회전 속도(wheelTurnSpeed)를 곱하고,
                    // 경과 시간(Time.fixedDeltaTime)을 곱하여 바퀴를 회전
                    wheel.Rotate(Vector3.right * moveInput * wheelTurnSpeed * Time.fixedDeltaTime);
                }
            }
        }
    }

    // <FollowCamera()의 역할>
    // 차량이 이동할 때 카메라가 자연스럽게 따라오며, 차량의 방향을 항상 바라보게 함
    void FollowCamera()
    {
        // - 차량의 위치를 기준으로 카메라의 목표 위치를 계산
        // + transform.TransformDirection(cameraOffset) : 로컬 좌표계에서 cameraFixPosition 값을 차량의 월드 좌표계로 변환
        // + 차량의 위치와 cameraFixPosition을 더해 카메라의 목표 위치를 계산
        Vector3 targetPosition = transform.position + transform.TransformDirection(cameraFixPosition);

        // - 카메라를 목표 위치로 부드럽게 이동
        // + Lerp 함수는 현재 위치와 목표 위치 사이의 중간 위치를 반환, 자연스러움을 위해 추가
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.fixedDeltaTime);
        // 카메라가 차량을 바라보도록 회전
        cameraTransform.LookAt(transform.position);
    }

    // <GetCurrentSpeed()의 역할>
    // 현재 차량의 속도(currentSpeed)를 반환
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    // <IsBoosting()의 역할>
    // 부스터가 활성화되어 있는지 여부를 반환
    public bool IsBoosting()
    {
        return isBoosting;
    }

    // <GetBoostCooldown()의 역할>
    // 부스터가 다시 활성화될 때까지 남은 시간을 반환
    public float GetBoostCooldown()
    {
        return currentBoostCooldown;
    }

    // <GetBoostCooldownMax()의 역할>
    // 부스터의 최대 대기 시간을 반환
    public float GetBoostCooldownMax()
    {
        return boostCooldown;
    }
}
