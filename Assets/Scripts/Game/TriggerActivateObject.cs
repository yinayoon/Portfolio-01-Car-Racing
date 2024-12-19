using UnityEngine;

// - Ư�� Ʈ���ſ� ������ �� ������Ʈ�� Ȱ��ȭ�ϰų� ��Ȱ��ȭ��
// + Ʈ���� �̺�Ʈ�� �����Ͽ� ������Ʈ ���� ��ȯ
public class TriggerActivateObject : MonoBehaviour
{
    public string playerTag = "Player"; // �÷��̾� �ڵ����� �Ҵ�� �±�
    public GameObject targetObject; // �浹 �� Ȱ��ȭ�� ���ӿ�����Ʈ

    private void Start()
    {
        targetObject.SetActive(false); // �ʱ⿡�� ��Ȱ��ȭ
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetObject.SetActive(true); // ���ӿ�����Ʈ Ȱ��ȭ
        }
    }
}