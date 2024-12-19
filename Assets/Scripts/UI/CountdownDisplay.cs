using System.Collections;
using UnityEngine;
using TMPro;

// - 게임 시작 전에 카운트다운 UI를 표시하는 스크립트
// + 카운트다운 숫자와 시작 메시지 출력
// + 카운트다운 종료 후 차량 및 AI 활성화
public class CountdownDisplay : MonoBehaviour
{
    [Header("- Countdown Settings")]
    public TextMeshProUGUI countdownText; // TextMeshProUGUI 컴포넌트
    public float countdownTime = 3f; // 카운트다운 시작 시간

    [Header("- Player Settings")]
    public CarController carController; // 플레이어 자동차 컨트롤러 스크립트

    [Header("- AI Settings")]
    public AICarController[] aiCarControllers; // AI 자동차 컨트롤러 스크립트 배열

    [Header("- Audio Settings")]
    public AudioSource countdownAudio; // 카운트다운 효과음
    public AudioSource startAudio; // START 효과음

    private void Start()
    {
        // 플레이어 자동차 비활성화
        carController.enabled = false;

        // AI 자동차 비활성화
        foreach (AICarController aiCar in aiCarControllers)
        {
            aiCar.enabled = false;
        }

        // 카운트다운이 시작되도록 코루틴을 호출
        StartCoroutine(StartCountdown());
    }

    // <StartCountdown()의 역할>
    // StartCountdown은 코루틴 메서드로, 카운트다운이 진행되고 끝난 후 게임이 시작되도록 처리
    private IEnumerator StartCountdown()
    {
        // countdownTime 변수 값을 복사하여 currentTime이라는 지역 변수를 생성
        float currentTime = countdownTime;

        // - 카운트다운 진행
        // + 카운트다운 시간이 0보다 클 동안 실행
        while (currentTime > 0)
        {
            countdownText.text = Mathf.Ceil(currentTime).ToString(); // 카운트다운 남은 시간을 화면에 정수 형태로 표시
            countdownAudio.Play(); // 효과음 재생

            yield return new WaitForSeconds(1f); // 1초 지연
            currentTime--; // currentTime 값을 감소시켜 카운트다운이 진행되도록 함
        }

        countdownText.text = "START!"; // "START!" 출력
        startAudio.Play(); // 효과음 재생

        yield return new WaitForSeconds(1f);


        countdownText.text = ""; // 텍스트 숨기기
        carController.enabled = true; // CarController 스크립트를 활성화하여 플레이어가 자동차를 조작할 수 있게 함

        // - AI 자동차 활성화
        // + aiCarControllers 배열에 포함된 모든 AI 자동차를 활성화하여 경로를 따라 움직이도록 설정
        foreach (AICarController aiCar in aiCarControllers)
        {
            aiCar.enabled = true;
        }

        // ElapsedTimeDisplay 클래스의 StartTiming 메서드를 호출하여 타이머를 작동
        ElapsedTimeDisplay.StartTiming();
    }
}
