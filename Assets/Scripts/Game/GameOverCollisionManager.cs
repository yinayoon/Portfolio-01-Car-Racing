using UnityEngine;

// - ������ �浹�� ���� ����
// + �浹 ����
// + �÷��̾� ���� ���
public class GameOverCollisionManager : MonoBehaviour
{
    private int rank; // �浹�� ���� ������ ������ ������ ����� ����
    public static int PlayerRank; // �÷��̾��� ������ �����ϴ� ���� ����

    private void OnTriggerEnter(Collider other)
    {     
        if (other.tag == "AI") // �浹�� ��ü�� AI���� Ȯ��
        {
            rank++;
        }
        else if (other.tag == "Player") // �浹�� ��ü�� �÷��̾����� Ȯ��
        {
            rank++;
            PlayerRank = rank;
        }
    }
}
