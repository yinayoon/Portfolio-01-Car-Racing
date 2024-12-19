using UnityEngine;

// - ������ Ŀ���� �߷��� �����ϰ� �ٴڿ� �����ǵ��� ó��
// + ����� ���� �߷� ����
// + �ٴ� ���� �� ���� ȸ�� ó��
public class HeavyGravityController : MonoBehaviour
{
    [Header("- Gravity Settings")]
    public float customGravity = -50f; // �߷� ���� ���̴� ����
    public float groundCheckDistance = 2f; // �ٴ� Ȯ�� �Ÿ�
    public LayerMask groundLayer; // �ٴ� ���̾� ����
    public float groundOffset = 0.1f; // �ٴڰ� ���� ������ ������

    [Header("- Rigidbody Settings")]
    private Rigidbody rb; // Rigidbody ������Ʈ ����

    private void Start()
    {
        // Rigidbody ����
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = false; // Rigidbody �⺻ �߷� ��Ȱ��ȭ
        rb.mass = 1500f; // ���� ���� ����
        rb.interpolation = RigidbodyInterpolation.Interpolate; // �������� �ε巴�� ����

        // ���� �߷°� ���� (Unity�� ���� �߷� �����̸�, ��� Rigidbody�� ������ ��ħ)
        Physics.gravity = new Vector3(0, customGravity, 0);
    }

    // ���������� �߷��� �����ϰ� �ٴ� ���¸� Ȯ���Ͽ�, ������ ���߿� �ߴ� ������ ����
    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(0, customGravity, 0), ForceMode.Acceleration);
    }
}
