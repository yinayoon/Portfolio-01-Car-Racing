using UnityEngine;

// - 차량이 충돌한 순서 관리
// + 충돌 감지
// + 플레이어 순위 계산
public class GameOverCollisionManager : MonoBehaviour
{
    private int rank; // 충돌에 의해 감지된 차량의 순서를 기록할 변수
    public static int PlayerRank; // 플레이어의 순위를 저장하는 정적 변수

    private void OnTriggerEnter(Collider other)
    {     
        if (other.tag == "AI") // 충돌한 객체가 AI인지 확인
        {
            rank++;
        }
        else if (other.tag == "Player") // 충돌한 객체가 플레이어인지 확인
        {
            rank++;
            PlayerRank = rank;
        }
    }
}
