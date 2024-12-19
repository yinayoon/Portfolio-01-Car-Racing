using UnityEngine;
using TMPro;
using UnityEngine.UI;

// - 차량의 속도와 부스터 상태를 시각적으로 표시
// + 속도계 바와 텍스트 업데이트
// + 부스터 대기 시간을 슬라이더로 표시
public class SpeedometerGUI : MonoBehaviour
{
    [Header("- Speedometer UI Settings")]
    public RectTransform speedometerBar; // GUI 바의 RectTransform
    public TextMeshProUGUI speedText; // 속도 표시용 TextMeshPro
    public Slider boostCooldownSlider; // 부스터 대기 시간을 보여줄 Slider

    [Header("- Speed Settings")]
    public float minAngle = 0f; // 최소 각도
    public float maxAngle = 180f; // 최대 각도 (일반 속도 상태)
    public float boostMaxAngle = 220f; // 부스터 상태 최대 각도
    public float boostSpeed = 10f; // 부스터 상태로 증가하는 속도
    public float smoothDecreaseSpeed = 5f; // 부스트 후 자연스럽게 감소하는 속도

    [Header("- Car Controller Settings")]
    public CarController carController; // 자동차 컨트롤러

    private bool isBoosting = false; // 부스터 상태 확인
    private float currentGaugeAngle; // 현재 게이지 각도

    void Update()
    {
        // 자동차 속도 가져오기
        float currentSpeed = carController.GetCurrentSpeed();
        float maxSpeed = carController.maxSpeed;

        // 부스터 상태 확인
        isBoosting = carController.IsBoosting();

        // 각도 계산
        if (isBoosting) // 부스터가 활성화된 경우
        {
            // 현재 각도(currentTargetAngle)를 부스터 상태의 최대 각도(boostMaxAngle)로 점진적으로 증가시킵니다.
            currentGaugeAngle = Mathf.MoveTowards(currentGaugeAngle, boostMaxAngle, boostSpeed * Time.deltaTime);
        }
        else if (!isBoosting) // 부스터가 비활성화된 경우
        {
            // 현재 속도와 최대 속도의 비율로 목표 각도(targetAngle)를 계산
            float speedRatio;
            if (currentSpeed < 0) { speedRatio = 0; }
            else if (currentSpeed > maxSpeed) { speedRatio = 1; }
            else { speedRatio = currentSpeed / maxSpeed; }

            // 최소 각도(minAngle)와 최대 각도(maxAngle) 사이의 각도 계산
            float gaugeAngle = Mathf.Lerp(minAngle, maxAngle, speedRatio);

            // 현재 게이지 각도(currentGaugeAngle)가 목표 게이지 각도(gaugeAngle)보다 클 경우 실행
            // 이 조건은 부스터가 꺼졌거나 속도가 줄어들어 속도계 바늘이 점점 내려가는 상황을 처리
            if (currentGaugeAngle > gaugeAngle) 
            {
                // Mathf.MoveTowards는 특정 값(currentGaugeAngle)이 목표 값(gaugeAngle)으로 점진적으로 감소
                currentGaugeAngle = Mathf.MoveTowards(currentGaugeAngle, gaugeAngle, smoothDecreaseSpeed * Time.deltaTime);
            }
            else // 속도계가 gaugeAngle 값을 유지하고 있거나 도달한 상황에서 실행
            {
                // 부스터가 비활성화된 상태에서 정상적으로 게이지 각도로 설정
                currentGaugeAngle = gaugeAngle;
            }
        }

        // - GUI 바 회전
        // + 속도계 바늘을 회전시키기 위해 로컬 회전을 설정
        // + Z축을 기준으로 -currentGaugeAngle 만큼 회전시킴
        // + 음수 값을 사용해 시계 방향으로 회전
        speedometerBar.localRotation = Quaternion.Euler(0, 0, -currentGaugeAngle);

        // - 속도 텍스트 업데이트
        // + currentGaugeAngle를 minAngle과 boostMaxAngle의 범위로 정규화
        // + Mathf.InverseLerp() : 특정 값이 두 숫자 사이의 위치를 0에서 1 사이의 정규화된 값으로 반환하는 기능을 함
        float normalizedAngle = Mathf.InverseLerp(minAngle, boostMaxAngle, currentGaugeAngle);
        float displaySpeed = Mathf.Lerp(0, 221, normalizedAngle);
        if (displaySpeed >= 220) { displaySpeed = 220; }
        // 최종적으로 계산된 속도를 텍스트로 표시
        speedText.text = Mathf.RoundToInt(displaySpeed).ToString() + " km/h";

        // - 부스터 대기 시간 업데이트
        // + carController.GetBoostCooldown() : 부스터가 다시 사용 가능해질 때까지 남은 시간을 반환
        // + carController.GetBoostCooldownMax() : 부스터 대기 시간의 최대 값을 반환
        float boostCooldown = carController.GetBoostCooldown(); // 남은 부스터 대기 시간
        float boostCooldownMax = carController.GetBoostCooldownMax(); // 부스터 대기 최대 시간

        // 부스터 대기 시간을 슬라이더로 표현
        float normalizedCooldown;

        // 부스터 대기 시간이 없을 때 1로 설정
        if (boostCooldown <= 0) { normalizedCooldown = 1f; } // 슬라이더가 완전히 채워졌음을 나타냄
        // 부스터 대기 시간이 최대일 때 0으로 설정
        else if (boostCooldown >= boostCooldownMax) { normalizedCooldown = 0f; }  // 슬라이더가 완전히 비어 있음을 나타냄
        // 0과 1 사이의 비율 계산
        else { normalizedCooldown = 1f - (boostCooldown / boostCooldownMax); }
        // boostCooldown과 boostCooldownMax의 비율을 사용해 normalizedCooldown을 계산함
        // 0에서 1 사이의 값으로, 슬라이더에 반영됨
        // ex) boostCooldown = 2, boostCooldownMax = 4이면, 1f - (2 / 4) = 0.5임. (슬라이더가 반쯤 차 있는 상태를 나타냄)

        boostCooldownSlider.value = normalizedCooldown;

        // 슬라이더 색상 업데이트 (예: 대기 상태에서 점점 채워지도록)
        Color sliderColor;

        if (isBoosting)
        {
            sliderColor = Color.green; // 부스터 활성화 상태에서는 초록색
        }
        else
        {
            // 부스터 비활성화 상태에서는 빨간색에서 노란색으로 변화
            sliderColor = Color.Lerp(Color.red, Color.yellow, boostCooldownSlider.value);            
        }

        // 컴포넌트에서 채워지는 막대(바)의 컬러에 sliderColor를 적용
        boostCooldownSlider.fillRect.GetComponent<Image>().color = sliderColor;
    }
}
