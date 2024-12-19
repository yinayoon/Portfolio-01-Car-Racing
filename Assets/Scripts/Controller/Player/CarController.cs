using UnityEngine;

// - �÷��̾� ������ �̵�, ȸ��, �帮��Ʈ, �ν��� ���� ������ ó��
// + �̵� �� ȸ�� ����
// + �ν��� �� �帮��Ʈ Ȱ��ȭ
// + �ӵ� �� �극��ũ ó��
public class CarController : MonoBehaviour
{
    [Header("- Movement Settings")]
    public float acceleration = 5f; // ���ӵ�
    public float deceleration = 5f; // ���ӵ�
    public float brakeForce = 10f; // �극��ũ ���ӵ�
    public float maxSpeed = 20f; // �ִ� �ӵ�
    public float turnSpeed = 50f; // ȸ�� �ӵ�
    public float driftTurnSpeedMultiplier = 1.5f; // �帮��Ʈ �� ȸ�� �ӵ� ����
    public Rigidbody rb;

    [Header("- Boost Settings")]
    public float boostMultiplier = 2f; // �ν��� ����
    public float boostDuration = 3f; // �ν��� ���� �ð�
    public float boostCooldown = 5f; // �ν��� ��� �ð�
    public float boostSpeedIncrease = 10f; // �ν��� �ӵ� ������
    public GameObject boostEffect; // �ν��� ����Ʈ ������Ʈ

    [Header("- Wheel Settings")]
    public Transform[] wheels; // ���� Transform �迭
    public float wheelTurnSpeed = 200f; // ���� ȸ�� �ӵ�

    [Header("- Camera Settings")]
    public Transform cameraTransform; // ī�޶� Transform
    public Vector3 cameraFixPosition = new Vector3(0, 2, -4); // ī�޶� ��ġ ������
    public float cameraFollowSpeed = 10f; // ī�޶� ���󰡴� �ӵ�

    [Header("- Effects Settings")]
    public GameObject smokeEffect;
    public GameObject collisionParticlePrefab; // �浹 �� ������ ��ƼŬ ������

    [Header("- Sound Settings")]
    public AudioSource engineSound; // ���� ����
    public AudioSource accelerationSound; // �׼� ����
    public AudioSource driftSound; // �帮��Ʈ ����
    public AudioSource collisionSound; // �浹 ����

    private float currentSpeed = 0f; // ���� �ӵ�
    private bool isDrifting = false; // �帮��Ʈ ����
    private bool isBoosting = false; // �ν��� ����
    private float boostTimeLeft = 0f; // ���� �ν��� �ð�
    private float timeAboveMaxSpeed = 0f; // �ִ� �ӵ� �̻����� �޸� �ð�
    private float currentBoostCooldown = 0f; // ���� ���� �ν��� ��� �ð�

    public static bool IsInputEnabled = true; // �Է� Ȱ��ȭ ���� �÷���

    void Start()
    {
        IsInputEnabled = true; // �÷��̾��� �Է��� Ȱ��ȭ
        smokeEffect.SetActive(true); // ����ũ ����Ʈ�� �ʱ� ���¿��� ��Ȱ��ȭ
        boostEffect.SetActive(false); // �ν��� ����Ʈ�� �ʱ� ���¿��� ��Ȱ��ȭ
        engineSound.loop = true; // ���� �Ҹ��� �ݺ������� ����ǵ��� ����
        engineSound.Play(); // ���� ���� ���
    }

    void Update()
    {
        // - �ν��� ��� �ð� ���� ����
        // + !isBoosting : ���� �ν��Ͱ� Ȱ��ȭ���� �ʾ��� ���� ����
        // + currentBoostCooldown : �ν��͸� �ٽ� ����� �� �ֱ���� ���� ��� �ð��� �ǹ�
        if (!isBoosting && currentBoostCooldown > 0f)
        {
            currentBoostCooldown -= Time.deltaTime; // �� �����Ӹ��� ��� �ð��� ����
            currentBoostCooldown = Mathf.Max(0f, currentBoostCooldown); // �ð��� 0 ���Ϸ� �������� �ʵ��� ����
        }

        // - �ν��� Ȱ��ȭ ����
        // + �÷��̾ Z Ű�� �����ų� �ν��Ͱ� ��� �ð��� �����ų� ������ �ִ� �ӵ��� ������ ���¶��
        if (Input.GetKeyDown(KeyCode.Z) && currentBoostCooldown <= 0f && currentSpeed >= maxSpeed)
        {
            ActivateBoost(); // �ν��� Ȱ��ȭ �Լ� ȣ��
        }

        // - ����Ű���� ���� ���ų� �ν��� �ð��� ������ ����
        // + �÷��̾ W, S �Ǵ� ����Ű�� ������ �ʰų� �ν��� ���� �ð��� ��������
        if (isBoosting && (Input.GetAxisRaw("Vertical") == 0 || boostTimeLeft <= 0f))
        {
            EndBoost(); // �ν��� ��Ȱ��ȭ �Լ� ȣ��
        }

        // - �ν��� �ð� �ʱ�ȭ ����
        // + ���� �ӵ��� �ִ� �ӵ� �̸��̰ų� �ν��Ͱ� Ȱ��ȭ�Ǿ� ���� ������
        if (currentSpeed < maxSpeed && !isBoosting)
        {
            timeAboveMaxSpeed = 0f; // �ν��� ���ǿ� �ʿ��� �ִ� �ӵ� �ʰ� �ð��� �ʱ�ȭ
        }
    }

    void FixedUpdate()
    {
        // Rigidbody�� null�� ��� FixedUpdate ���� �ߴ�
        if (rb == null)
        {
            Debug.LogError("Rigidbody�� �Ҵ���� �ʾҽ��ϴ�. Rigidbody�� �߰��ϰų� ��ũ��Ʈ�� Ȯ���ϼ���.");
            return;
        }

        // ����Ű �Է�
        float moveInput = Input.GetAxis("Vertical"); // W/S �Ǵ� ^/v
        float turnInput = Input.GetAxis("Horizontal"); // A/D �Ǵ� </>

        HandleBoost(); // �ν����� ���¸� ������Ʈ�ϰ�, �ʿ��� ��� Ȱ��ȭ �Ǵ� ���Ḧ ó��
        HandleBrake(moveInput); // �극��ũ �Է� ���θ� Ȯ���ϰ� ���� ������ ó��
        HandleDrift(turnInput); // �帮��Ʈ �Է� ���θ� Ȯ���ϰ� ���¸� ����

        // - ���� ���� ��ġ ����
        // + ���� �ӵ�(currentSpeed)�� �ִ� �ӵ�(maxSpeed)�� ������ ���� ���� �Ҹ��� ��ġ�� ����.
        engineSound.pitch = Mathf.Clamp(1 + (currentSpeed / maxSpeed), 1, 2); // ��ġ ���� 1���� 2 ���̸� ����� �ʵ��� ����

        // - �ִ� �ӵ� ����
        float effectiveMaxSpeed;
        // + �ν��Ͱ� Ȱ��ȭ ���¶�� �ִ� �ӵ��� �ν��� ������ ���� ���� ���
        if (isBoosting) { effectiveMaxSpeed = maxSpeed * boostMultiplier; }
        // + �ν��Ͱ� ��Ȱ��ȭ ���¶�� �⺻ �ִ� �ӵ��� ���
        else { effectiveMaxSpeed = maxSpeed; }
        // + currentSpeed�� �ּҰ�(-effectiveMaxSpeed)�� �ִ밪(effectiveMaxSpeed) ���̿� �ֵ��� ����
        // + ������ �ʿ��ϹǷ� 0�̾ƴ� -effectiveMaxSpeed���� ����
        currentSpeed = Mathf.Clamp(currentSpeed, -effectiveMaxSpeed, effectiveMaxSpeed);

        // �Է��� ��Ȱ��ȭ�� ���¶�� �̵� �� ȸ���� ó������ �ʰ� �޼��带 ����
        if (!IsInputEnabled)
        {
            return;
        }

        // - ������/�ڷ� �̵�
        // + ���� �̵� �ӵ�(currentSpeed)�� ������ �ð�(Time.fixedDeltaTime)�� ����� �̵� ���� ����
        Vector3 moveDirection = transform.forward * currentSpeed * Time.fixedDeltaTime;
        // + Rigidbody�� ���� ��ġ���� ���� �������� �̵�
        rb.MovePosition(rb.position + moveDirection);

        // �¿� ȸ��
        if (currentSpeed != 0) // ������ ���� ȸ��
        {
            float adjustedTurnSpeed; // ���� �ڵ����� ȸ�� �ӵ��� �����ϴ� ����
            // - isDrifting ������ true���, �ڵ����� �帮��Ʈ ���¶�� ��
            // + turnSpeed�� driftTurnSpeedMultiplier ���� ���Ͽ� ȸ�� �ӵ��� ����
            if (isDrifting) { adjustedTurnSpeed = turnSpeed * driftTurnSpeedMultiplier; }
            // + �⺻ ȸ�� �ӵ��� turnSpeed�� �״�� ���
            else { adjustedTurnSpeed = turnSpeed; }

            // - ȸ�� ������ �������� ���ʹϾ��� ����
            // + turnInput * adjustedTurnSpeed * Time.fixedDeltaTime : Y���� �������� ȸ��
            Quaternion turnRotation = Quaternion.Euler(0, turnInput * adjustedTurnSpeed * Time.fixedDeltaTime, 0);
            // - ���� �ڵ����� Rigidbody�� ���ο� ȸ�� ���� ����
            // + rb.rotation * turnRotation : ���� ȸ���� ���ο� ȸ���� ���Ͽ� ������Ʈ
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        // ���� ȸ�� �Լ� ȣ��
        RotateWheels(moveInput);

        // ī�޶� ������ ���󰡰� �ϴ� �Լ� ȣ��
        FollowCamera();
    }

    // <HandleBoost()�� ����>
    // �ν��� ��� ó��
    // ������ ���� �ӵ��� maxSpeed �̻��� ���, �ִ� �ӵ� �ʰ� �ð�(timeAboveMaxSpeed)�� ������Ŵ
    // �ӵ��� maxSpeed �̸����� �������� timeAboveMaxSpeed�� 0���� �ʱ�ȭ��
    // �ν��Ͱ� �ߵ��Ǳ� ���� ���� �ð� ���� maxSpeed �̻��� �����ؾ� �ϵ��� ������
    void HandleBoost()
    {
        // ���� �ӵ��� �ִ� �ӵ��� �ʰ��ߴ��� Ȯ��
        if (currentSpeed >= maxSpeed)
        {
            // �ִ� �ӵ� �ʰ� �ð� ����
            timeAboveMaxSpeed += Time.fixedDeltaTime;
        }
        else
        {
            // �ӵ��� �ִ� �ӵ� �̸��̸� ���� �ð� �ʱ�ȭ
            timeAboveMaxSpeed = 0f;
        }

        // - �ν��� Ȱ��ȭ ���� Ȯ��
        // + Z Ű�� �����ų�(���� ���� ����), �ڵ����� �ִ� �ӵ�(maxSpeed)�� �ʰ��� ���·� ���� �ð� �̻�(boostCooldown) �����߰ų�
        // ���� �ν��� ������� ���� ���� �ð��� ��� �Ҹ�Ǿ����� Ȯ���Ͽ� ������ �����ϸ�
        if (Input.GetKeyDown(KeyCode.Z) && timeAboveMaxSpeed >= boostCooldown && boostTimeLeft <= 0f)
        {
            ActivateBoost(); // �ν��� Ȱ��ȭ
        }

        // �ν��Ͱ� Ȱ��ȭ ������ ���
        if (isBoosting)
        {
            // �ν��� ���� �ð� ����
            boostTimeLeft -= Time.fixedDeltaTime;

            // �ν��� �ð��� �ٵǸ� ��Ȱ��ȭ
            if (boostTimeLeft <= 0f) { EndBoost(); }
        }
    }

    // <ActivateBoost()�� ����>
    // �ν��� ����� Ȱ��ȭ�ϴ� ������ ����
    void ActivateBoost()
    {
        isBoosting = true; // �ν��� ���� Ȱ��ȭ        
        boostTimeLeft = boostDuration; // �ν��� ���� �ð� ����
        currentBoostCooldown = boostCooldown; // �ν��� ��� �ð��� boostCooldown���� �ʱ�ȭ
        currentSpeed += boostSpeedIncrease; // �ν��ͷ� ���� �ӵ� ����

        // �ν��� ����Ʈ Ȱ��ȭ
        boostEffect.SetActive(true); 

        // ���� ���� ��� �̹� ��� ���� �ƴ� ���
        if (!accelerationSound.isPlaying)
        {
            accelerationSound.Play(); // ���� ���� ���
        }
    }

    // <EndBoost()�� ����>
    // �ν���(Boost) ���°� ����� �� ���� �Ӽ��� ȿ���� �ʱ� ���·� ������
    void EndBoost()
    {
        isBoosting = false; // �ν��� ���¸� ��Ȱ��ȭ�Ͽ� ������ �ν��� ���°� �ƴ��� ���
        timeAboveMaxSpeed = 0f; // �ν��� ��� ���� ������ �ִ� �ӵ� �ʰ� �ð��� �ʱ�ȭ

        boostEffect.SetActive(false); // �ν��� ����Ʈ ��Ȱ��ȭ

        // ���� ���� ��� �̹� ��� ���� ���
        if (accelerationSound.isPlaying)
        {
            accelerationSound.Stop(); // ���� ���� ����
        }
    }

    // <HandleBrake()�� ����>
    // �÷��̾ ������ �ӵ��� �����ϰų� ���� �� �ֵ��� �극��ũ�� ������ ó���ϴ� ��� ��
    void HandleBrake(float moveInput)
    {
        // �÷��̾ �극��ũ(Shift Ű)�� �������� Ȯ��
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // ���� �ӵ�(currentSpeed)�� 0���� �ε巴�� ����
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, brakeForce * Time.fixedDeltaTime);
        }
        else // �극��ũ�� ������ ���� ���
        {
            // - �ӵ� ���
            // + �÷��̾ ����(W) �Ǵ� ����(S) �Է��� �����ϰ�, �ν��Ͱ� Ȱ��ȭ���� �ʾ��� ���� �ӵ��� ����
            if (moveInput != 0 && !isBoosting)
            {
                // ���ӵ��� ������ ����
                float effectiveAcceleration = acceleration;

                // ����(W) �Ǵ� ����(S) �Է¿� ���� ������ �ӵ��� �����ϰų� ����
                currentSpeed += moveInput * effectiveAcceleration * Time.fixedDeltaTime;
            }
            else if (!isBoosting) // �ν��Ͱ� ��Ȱ��ȭ ���¶�� ���� ó��
            {
                // ���� �ӵ��� �ε巴�� 0���� ����
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
            }
        }
    }

    // <HandleDrift()�� ����>
    // �帮��Ʈ�� Ȱ��ȭ�ϰų� ��Ȱ��ȭ�ϴ� �Լ�
    void HandleDrift(float turnInput)
    {
        // LeftControl Ű�� ���ȴ��� Ȯ��
        if (Input.GetKey(KeyCode.LeftControl))
        {
            // �帮��Ʈ Ȱ��ȭ
            isDrifting = true;

            // �帮��Ʈ ���尡 ��� ���� �ƴ� ���
            if (!driftSound.isPlaying)
            {
                driftSound.Play(); // �帮��Ʈ ���� ���
            }
        }
        else
        {
            // �帮��Ʈ ��Ȱ��ȭ
            isDrifting = false;

            // �帮��Ʈ ���尡 ��� ���� ��� ����
            if (driftSound.isPlaying)
            {
                driftSound.Stop(); // �帮��Ʈ ���� ����
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ư�� �±׿� ���� ���� ó��
        if (collision.gameObject.CompareTag("Ground"))
        {
            return; // �±װ� "Ground"�� ��� �浹 ȿ���� �������� ����
        }

        // �浹 �Ҹ� ���
        collisionSound.Play();

        // - �浹 ��ƼŬ ����
        // + �浹 ��ġ�� ��ƼŬ ����
        GameObject collisionParticle = Instantiate(collisionParticlePrefab, collision.contacts[0].point, Quaternion.identity);
        // + ���� �ð� �� ��ƼŬ ����
        Destroy(collisionParticle, 2f);
    }

    // <RotateWheels()�� ����>
    // ������ �����ϰų� ������ �� ������ �ڿ������� ȸ��
    void RotateWheels(float moveInput)
    {
        // �Էµ� �̵� ���� 0�� �ƴ϶�� ������ ȸ����Ŵ
        if (moveInput != 0)
        {
            // ��� ���� �迭(wheels)�� ��ȸ
            foreach (Transform wheel in wheels)
            {
                // ������ null�� �ƴ� ��쿡�� ȸ��
                if (wheel != null)
                {
                    // �Էµ� ��(moveInput)�� ���� ȸ�� �ӵ�(wheelTurnSpeed)�� ���ϰ�,
                    // ��� �ð�(Time.fixedDeltaTime)�� ���Ͽ� ������ ȸ��
                    wheel.Rotate(Vector3.right * moveInput * wheelTurnSpeed * Time.fixedDeltaTime);
                }
            }
        }
    }

    // <FollowCamera()�� ����>
    // ������ �̵��� �� ī�޶� �ڿ������� �������, ������ ������ �׻� �ٶ󺸰� ��
    void FollowCamera()
    {
        // - ������ ��ġ�� �������� ī�޶��� ��ǥ ��ġ�� ���
        // + transform.TransformDirection(cameraOffset) : ���� ��ǥ�迡�� cameraFixPosition ���� ������ ���� ��ǥ��� ��ȯ
        // + ������ ��ġ�� cameraFixPosition�� ���� ī�޶��� ��ǥ ��ġ�� ���
        Vector3 targetPosition = transform.position + transform.TransformDirection(cameraFixPosition);

        // - ī�޶� ��ǥ ��ġ�� �ε巴�� �̵�
        // + Lerp �Լ��� ���� ��ġ�� ��ǥ ��ġ ������ �߰� ��ġ�� ��ȯ, �ڿ��������� ���� �߰�
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.fixedDeltaTime);
        // ī�޶� ������ �ٶ󺸵��� ȸ��
        cameraTransform.LookAt(transform.position);
    }

    // <GetCurrentSpeed()�� ����>
    // ���� ������ �ӵ�(currentSpeed)�� ��ȯ
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    // <IsBoosting()�� ����>
    // �ν��Ͱ� Ȱ��ȭ�Ǿ� �ִ��� ���θ� ��ȯ
    public bool IsBoosting()
    {
        return isBoosting;
    }

    // <GetBoostCooldown()�� ����>
    // �ν��Ͱ� �ٽ� Ȱ��ȭ�� ������ ���� �ð��� ��ȯ
    public float GetBoostCooldown()
    {
        return currentBoostCooldown;
    }

    // <GetBoostCooldownMax()�� ����>
    // �ν����� �ִ� ��� �ð��� ��ȯ
    public float GetBoostCooldownMax()
    {
        return boostCooldown;
    }
}
