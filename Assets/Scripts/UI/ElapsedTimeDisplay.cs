using UnityEngine;
using TMPro;

// - ������ ����� �ð��� UI�� ǥ��
// + ��� �ð� ���
// + MM:SS �������� �ð� ���
public class ElapsedTimeDisplay : MonoBehaviour
{
    [Header("- UI Settings")]
    public TextMeshProUGUI timeText; // TextMeshProUGUI ������Ʈ

    [Header("- Time Management")]
    private float elapsedTime = 0f; // ������ ��� �ð��� ����
    private static bool isTiming = false; // Ÿ�̸Ӱ� �۵� ������ ���θ� Ȯ�� ����

    private void Update()
    {
        if (isTiming)
        {
            // �ð� ����
            elapsedTime += Time.deltaTime;

            // - �ð��� ����ȭ�Ͽ� ǥ�� (MM:SS)
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            // ��� �ð��� �а� �� �������� ���ڿ��� ��ȯ
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public static void StartTiming()
    {
        isTiming = true;
    }
}
