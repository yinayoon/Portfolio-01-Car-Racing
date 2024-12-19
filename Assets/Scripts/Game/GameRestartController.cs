using UnityEngine;
using UnityEngine.SceneManagement;

// - 특정 키 입력을 통해 게임을 재시작할 수 있는 스크립트
// + 스페이스바 입력으로 게임 재시작
public class GameRestartController : MonoBehaviour
{
    private void Update()
    {
        // 스페이스바 입력 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        // 현재 활성화된 씬 이름 가져오기
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 현재 씬 재시작
        SceneManager.LoadScene(currentSceneName);
    }
}
