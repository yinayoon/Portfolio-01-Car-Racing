using UnityEngine;

// - 특정 트리거에 들어왔을 때 오브젝트를 활성화하거나 비활성화함
// + 트리거 이벤트를 감지하여 오브젝트 상태 전환
public class TriggerActivateObject : MonoBehaviour
{
    public string playerTag = "Player"; // 플레이어 자동차에 할당된 태그
    public GameObject targetObject; // 충돌 시 활성화할 게임오브젝트

    private void Start()
    {
        targetObject.SetActive(false); // 초기에는 비활성화
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            targetObject.SetActive(true); // 게임오브젝트 활성화
        }
    }
}