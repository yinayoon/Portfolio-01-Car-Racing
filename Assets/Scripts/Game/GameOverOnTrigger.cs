using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// - 플레이어가 특정 영역에 도달했을 때 게임 종료 상태를 처리
// + 게임 종료 메시지와 플레이어 순위 출력
// + 오디오 그룹 및 타이머 비활성화
public class GameOverOnTrigger : MonoBehaviour
{
    [Header("- Player Settings")]
    public string playerTag = "Player"; // 플레이어 자동차에 할당된 태그
    public CarController carController; // CarController 연결

    [Header("- UI Settings")]
    public TextMeshProUGUI gameOverText; // 게임 종료 메시지 TextMeshPro
    public TextMeshProUGUI playerRankText; // 플레이어 순위 표시 TextMeshPro
    public Image block; // 게임 종료 화면을 표시할 이미지

    [Header("- Audio Settings")]
    public GameObject audioGroup; // 게임 종료 시 비활성화할 오디오 그룹

    [Header("- Timer Settings")]
    public ElapsedTimeDisplay elapsedTimeDisplay;

    private bool isGameOver = false; // 게임 종료 상태

    private void Start()
    {
        block.enabled = false;
        gameOverText.text = ""; // 초기에는 텍스트 비움
        playerRankText.text = ""; // 초기에는 텍스트 비움
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isGameOver) // 충돌한 오브젝트가 플레이어인지 확인
        {
            isGameOver = true; // 게임이 종료 상태임을 표시

            // 게임 종료 텍스트 표시
            block.enabled = true;
            gameOverText.text = "Game Over!";
            StartCoroutine("FinishCo");// 순위 표시 및 추가 처리를 수행

            audioGroup.SetActive(false); // AudioGroup 비활성화
            elapsedTimeDisplay.enabled = false; // ElapsedTimeDisplay 멈춤
        }
    }

    // <FinishCo()의 역할>
    // 게임 종료 후 추가 처리를 수행
    IEnumerator FinishCo()
    {
        // 0.2초 대기한 뒤, 플레이어의 순위를 표시
        yield return new WaitForSeconds(0.2f);
        playerRankText.text = $"Player Rank: {GameOverCollisionManager.PlayerRank}";

        // 5초 대기 후 입력 비활성화
        yield return new WaitForSeconds(5f);
        CarController.IsInputEnabled = false;
    }
}