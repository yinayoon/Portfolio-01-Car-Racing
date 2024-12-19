using UnityEngine;
using UnityEngine.SceneManagement;

// - Ư�� Ű �Է��� ���� ������ ������� �� �ִ� ��ũ��Ʈ
// + �����̽��� �Է����� ���� �����
public class GameRestartController : MonoBehaviour
{
    private void Update()
    {
        // �����̽��� �Է� ����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        // ���� Ȱ��ȭ�� �� �̸� ��������
        string currentSceneName = SceneManager.GetActiveScene().name;

        // ���� �� �����
        SceneManager.LoadScene(currentSceneName);
    }
}
