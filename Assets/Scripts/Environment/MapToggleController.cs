using UnityEngine;

// - ���� Ȱ��ȭ/��Ȱ��ȭ�� ó��
// + �� ������Ʈ ���� ��ȯ
public class MapToggleController : MonoBehaviour
{
    public GameObject[] mapObject; // �� ������Ʈ�� �Ҵ��� ���� (������ ǥ���� ���� ������Ʈ �迭)
    private bool isMapActive = false; // �� ���� (Ȱ��ȭ/��Ȱ��ȭ)

    private void Start()
    {
        // ������ ���۵Ǹ� ��� �� ������Ʈ�� ��Ȱ��ȭ
        for (int i = 0; i < mapObject.Length; i++)
            mapObject[i].SetActive(false);
    }

    private void Update()
    {
        // M Ű �Է� ����
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap(); // �� ���¸� ��ȯ
        }
    }

    private void ToggleMap()
    {
        // �� ���� ����
        isMapActive = !isMapActive;

        // �� ������Ʈ Ȱ��ȭ/��Ȱ��ȭ (isMapActive ���� �°� ����)
        if (mapObject != null)
        {
            for (int i = 0; i < mapObject.Length; i++)
                mapObject[i].SetActive(isMapActive);
        }
    }
}
