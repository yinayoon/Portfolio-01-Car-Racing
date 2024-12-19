using System.Collections;
using UnityEngine;
using TMPro;

// - ���� ���� ���� ī��Ʈ�ٿ� UI�� ǥ���ϴ� ��ũ��Ʈ
// + ī��Ʈ�ٿ� ���ڿ� ���� �޽��� ���
// + ī��Ʈ�ٿ� ���� �� ���� �� AI Ȱ��ȭ
public class CountdownDisplay : MonoBehaviour
{
    [Header("- Countdown Settings")]
    public TextMeshProUGUI countdownText; // TextMeshProUGUI ������Ʈ
    public float countdownTime = 3f; // ī��Ʈ�ٿ� ���� �ð�

    [Header("- Player Settings")]
    public CarController carController; // �÷��̾� �ڵ��� ��Ʈ�ѷ� ��ũ��Ʈ

    [Header("- AI Settings")]
    public AICarController[] aiCarControllers; // AI �ڵ��� ��Ʈ�ѷ� ��ũ��Ʈ �迭

    [Header("- Audio Settings")]
    public AudioSource countdownAudio; // ī��Ʈ�ٿ� ȿ����
    public AudioSource startAudio; // START ȿ����

    private void Start()
    {
        // �÷��̾� �ڵ��� ��Ȱ��ȭ
        carController.enabled = false;

        // AI �ڵ��� ��Ȱ��ȭ
        foreach (AICarController aiCar in aiCarControllers)
        {
            aiCar.enabled = false;
        }

        // ī��Ʈ�ٿ��� ���۵ǵ��� �ڷ�ƾ�� ȣ��
        StartCoroutine(StartCountdown());
    }

    // <StartCountdown()�� ����>
    // StartCountdown�� �ڷ�ƾ �޼����, ī��Ʈ�ٿ��� ����ǰ� ���� �� ������ ���۵ǵ��� ó��
    private IEnumerator StartCountdown()
    {
        // countdownTime ���� ���� �����Ͽ� currentTime�̶�� ���� ������ ����
        float currentTime = countdownTime;

        // - ī��Ʈ�ٿ� ����
        // + ī��Ʈ�ٿ� �ð��� 0���� Ŭ ���� ����
        while (currentTime > 0)
        {
            countdownText.text = Mathf.Ceil(currentTime).ToString(); // ī��Ʈ�ٿ� ���� �ð��� ȭ�鿡 ���� ���·� ǥ��
            countdownAudio.Play(); // ȿ���� ���

            yield return new WaitForSeconds(1f); // 1�� ����
            currentTime--; // currentTime ���� ���ҽ��� ī��Ʈ�ٿ��� ����ǵ��� ��
        }

        countdownText.text = "START!"; // "START!" ���
        startAudio.Play(); // ȿ���� ���

        yield return new WaitForSeconds(1f);


        countdownText.text = ""; // �ؽ�Ʈ �����
        carController.enabled = true; // CarController ��ũ��Ʈ�� Ȱ��ȭ�Ͽ� �÷��̾ �ڵ����� ������ �� �ְ� ��

        // - AI �ڵ��� Ȱ��ȭ
        // + aiCarControllers �迭�� ���Ե� ��� AI �ڵ����� Ȱ��ȭ�Ͽ� ��θ� ���� �����̵��� ����
        foreach (AICarController aiCar in aiCarControllers)
        {
            aiCar.enabled = true;
        }

        // ElapsedTimeDisplay Ŭ������ StartTiming �޼��带 ȣ���Ͽ� Ÿ�̸Ӹ� �۵�
        ElapsedTimeDisplay.StartTiming();
    }
}
