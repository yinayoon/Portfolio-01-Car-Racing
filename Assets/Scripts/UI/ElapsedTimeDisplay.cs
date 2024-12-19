using UnityEngine;
using TMPro;

// - 게임이 진행된 시간을 UI로 표시
// + 경과 시간 계산
// + MM:SS 형식으로 시간 출력
public class ElapsedTimeDisplay : MonoBehaviour
{
    [Header("- UI Settings")]
    public TextMeshProUGUI timeText; // TextMeshProUGUI 컴포넌트

    [Header("- Time Management")]
    private float elapsedTime = 0f; // 누적된 경과 시간을 저장
    private static bool isTiming = false; // 타이머가 작동 중인지 여부를 확인 변수

    private void Update()
    {
        if (isTiming)
        {
            // 시간 증가
            elapsedTime += Time.deltaTime;

            // - 시간을 형식화하여 표시 (MM:SS)
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            // 경과 시간을 분과 초 형식으로 문자열로 변환
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public static void StartTiming()
    {
        isTiming = true;
    }
}
