using UnityEngine;

// - 맵의 활성화/비활성화를 처리
// + 맵 오브젝트 상태 전환
public class MapToggleController : MonoBehaviour
{
    public GameObject[] mapObject; // 맵 오브젝트를 할당할 변수 (맵으로 표시할 게임 오브젝트 배열)
    private bool isMapActive = false; // 맵 상태 (활성화/비활성화)

    private void Start()
    {
        // 게임이 시작되면 모든 맵 오브젝트를 비활성화
        for (int i = 0; i < mapObject.Length; i++)
            mapObject[i].SetActive(false);
    }

    private void Update()
    {
        // M 키 입력 감지
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap(); // 맵 상태를 전환
        }
    }

    private void ToggleMap()
    {
        // 맵 상태 반전
        isMapActive = !isMapActive;

        // 맵 오브젝트 활성화/비활성화 (isMapActive 값에 맞게 설정)
        if (mapObject != null)
        {
            for (int i = 0; i < mapObject.Length; i++)
                mapObject[i].SetActive(isMapActive);
        }
    }
}
