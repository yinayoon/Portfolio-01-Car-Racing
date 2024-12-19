using UnityEngine;

// - ������ �ٴڿ� �����ϵ��� ��ġ�� ȸ���� ������
// + Raycast�� ����Ͽ� �ٴ� ����
// + �ε巯�� ��ġ ���� �� ȸ�� ����
public class GroundStickController : MonoBehaviour
{
    [Header("- Ground Settings")]
    public LayerMask groundLayer;       // �ٴ��� ������ ���̾�
    public float groundCheckDistance = 1.5f; // �ٴ� Ȯ�� �Ÿ�
    public float heightOffset = 0.1f;   // �ٴڰ� ���� ������ ���� ����
    public float groundLerpSpeed = 10f; // �ε巴�� �ٴڿ� �ٴ� �ӵ�

    [Header("- Rigidbody Component")]
    private Rigidbody rb; // Rigidbody ������Ʈ ����

    private void Awake()
    {
        // Rigidbody ������Ʈ ��������
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        StickToGround();
    }

    private void StickToGround()
    {
        // Raycast�� ����� �ٴ� ����
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            // - ��ǥ ���̸� ����
            // + Raycast�� ������ �ٴ��� ���̿� heightOffset�� ���� ������ ��ǥ ��ġ�� ����
            Vector3 targetPosition = new Vector3(transform.position.x, hit.point.y + heightOffset, transform.position.z);

            // �ε巴�� ��ġ ����
            rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, groundLerpSpeed * Time.fixedDeltaTime));

            // - ��翡 ���� ȸ��
            // + ������Ʈ(��: ����)�� ������ up ������ ������ ���� ����(hit.normal)�� ȸ���ϵ��� �����ϱ� ���� ��
            Quaternion groundRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            // + �ε巴�� ȸ���� ����
            rb.MoveRotation(Quaternion.Lerp(transform.rotation, groundRotation, groundLerpSpeed * Time.fixedDeltaTime));
        }
    }
}
