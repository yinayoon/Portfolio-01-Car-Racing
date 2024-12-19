using UnityEngine;
using TMPro;
using UnityEngine.UI;

// - ������ �ӵ��� �ν��� ���¸� �ð������� ǥ��
// + �ӵ��� �ٿ� �ؽ�Ʈ ������Ʈ
// + �ν��� ��� �ð��� �����̴��� ǥ��
public class SpeedometerGUI : MonoBehaviour
{
    [Header("- Speedometer UI Settings")]
    public RectTransform speedometerBar; // GUI ���� RectTransform
    public TextMeshProUGUI speedText; // �ӵ� ǥ�ÿ� TextMeshPro
    public Slider boostCooldownSlider; // �ν��� ��� �ð��� ������ Slider

    [Header("- Speed Settings")]
    public float minAngle = 0f; // �ּ� ����
    public float maxAngle = 180f; // �ִ� ���� (�Ϲ� �ӵ� ����)
    public float boostMaxAngle = 220f; // �ν��� ���� �ִ� ����
    public float boostSpeed = 10f; // �ν��� ���·� �����ϴ� �ӵ�
    public float smoothDecreaseSpeed = 5f; // �ν�Ʈ �� �ڿ������� �����ϴ� �ӵ�

    [Header("- Car Controller Settings")]
    public CarController carController; // �ڵ��� ��Ʈ�ѷ�

    private bool isBoosting = false; // �ν��� ���� Ȯ��
    private float currentGaugeAngle; // ���� ������ ����

    void Update()
    {
        // �ڵ��� �ӵ� ��������
        float currentSpeed = carController.GetCurrentSpeed();
        float maxSpeed = carController.maxSpeed;

        // �ν��� ���� Ȯ��
        isBoosting = carController.IsBoosting();

        // ���� ���
        if (isBoosting) // �ν��Ͱ� Ȱ��ȭ�� ���
        {
            // ���� ����(currentTargetAngle)�� �ν��� ������ �ִ� ����(boostMaxAngle)�� ���������� ������ŵ�ϴ�.
            currentGaugeAngle = Mathf.MoveTowards(currentGaugeAngle, boostMaxAngle, boostSpeed * Time.deltaTime);
        }
        else if (!isBoosting) // �ν��Ͱ� ��Ȱ��ȭ�� ���
        {
            // ���� �ӵ��� �ִ� �ӵ��� ������ ��ǥ ����(targetAngle)�� ���
            float speedRatio;
            if (currentSpeed < 0) { speedRatio = 0; }
            else if (currentSpeed > maxSpeed) { speedRatio = 1; }
            else { speedRatio = currentSpeed / maxSpeed; }

            // �ּ� ����(minAngle)�� �ִ� ����(maxAngle) ������ ���� ���
            float gaugeAngle = Mathf.Lerp(minAngle, maxAngle, speedRatio);

            // ���� ������ ����(currentGaugeAngle)�� ��ǥ ������ ����(gaugeAngle)���� Ŭ ��� ����
            // �� ������ �ν��Ͱ� �����ų� �ӵ��� �پ��� �ӵ��� �ٴ��� ���� �������� ��Ȳ�� ó��
            if (currentGaugeAngle > gaugeAngle) 
            {
                // Mathf.MoveTowards�� Ư�� ��(currentGaugeAngle)�� ��ǥ ��(gaugeAngle)���� ���������� ����
                currentGaugeAngle = Mathf.MoveTowards(currentGaugeAngle, gaugeAngle, smoothDecreaseSpeed * Time.deltaTime);
            }
            else // �ӵ��谡 gaugeAngle ���� �����ϰ� �ְų� ������ ��Ȳ���� ����
            {
                // �ν��Ͱ� ��Ȱ��ȭ�� ���¿��� ���������� ������ ������ ����
                currentGaugeAngle = gaugeAngle;
            }
        }

        // - GUI �� ȸ��
        // + �ӵ��� �ٴ��� ȸ����Ű�� ���� ���� ȸ���� ����
        // + Z���� �������� -currentGaugeAngle ��ŭ ȸ����Ŵ
        // + ���� ���� ����� �ð� �������� ȸ��
        speedometerBar.localRotation = Quaternion.Euler(0, 0, -currentGaugeAngle);

        // - �ӵ� �ؽ�Ʈ ������Ʈ
        // + currentGaugeAngle�� minAngle�� boostMaxAngle�� ������ ����ȭ
        // + Mathf.InverseLerp() : Ư�� ���� �� ���� ������ ��ġ�� 0���� 1 ������ ����ȭ�� ������ ��ȯ�ϴ� ����� ��
        float normalizedAngle = Mathf.InverseLerp(minAngle, boostMaxAngle, currentGaugeAngle);
        float displaySpeed = Mathf.Lerp(0, 221, normalizedAngle);
        if (displaySpeed >= 220) { displaySpeed = 220; }
        // ���������� ���� �ӵ��� �ؽ�Ʈ�� ǥ��
        speedText.text = Mathf.RoundToInt(displaySpeed).ToString() + " km/h";

        // - �ν��� ��� �ð� ������Ʈ
        // + carController.GetBoostCooldown() : �ν��Ͱ� �ٽ� ��� �������� ������ ���� �ð��� ��ȯ
        // + carController.GetBoostCooldownMax() : �ν��� ��� �ð��� �ִ� ���� ��ȯ
        float boostCooldown = carController.GetBoostCooldown(); // ���� �ν��� ��� �ð�
        float boostCooldownMax = carController.GetBoostCooldownMax(); // �ν��� ��� �ִ� �ð�

        // �ν��� ��� �ð��� �����̴��� ǥ��
        float normalizedCooldown;

        // �ν��� ��� �ð��� ���� �� 1�� ����
        if (boostCooldown <= 0) { normalizedCooldown = 1f; } // �����̴��� ������ ä�������� ��Ÿ��
        // �ν��� ��� �ð��� �ִ��� �� 0���� ����
        else if (boostCooldown >= boostCooldownMax) { normalizedCooldown = 0f; }  // �����̴��� ������ ��� ������ ��Ÿ��
        // 0�� 1 ������ ���� ���
        else { normalizedCooldown = 1f - (boostCooldown / boostCooldownMax); }
        // boostCooldown�� boostCooldownMax�� ������ ����� normalizedCooldown�� �����
        // 0���� 1 ������ ������, �����̴��� �ݿ���
        // ex) boostCooldown = 2, boostCooldownMax = 4�̸�, 1f - (2 / 4) = 0.5��. (�����̴��� ���� �� �ִ� ���¸� ��Ÿ��)

        boostCooldownSlider.value = normalizedCooldown;

        // �����̴� ���� ������Ʈ (��: ��� ���¿��� ���� ä��������)
        Color sliderColor;

        if (isBoosting)
        {
            sliderColor = Color.green; // �ν��� Ȱ��ȭ ���¿����� �ʷϻ�
        }
        else
        {
            // �ν��� ��Ȱ��ȭ ���¿����� ���������� ��������� ��ȭ
            sliderColor = Color.Lerp(Color.red, Color.yellow, boostCooldownSlider.value);            
        }

        // ������Ʈ���� ä������ ����(��)�� �÷��� sliderColor�� ����
        boostCooldownSlider.fillRect.GetComponent<Image>().color = sliderColor;
    }
}
