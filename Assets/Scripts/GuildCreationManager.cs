using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro 사용 시 필수

public class GuildCreationManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TMP_InputField nameInput; // 입력창 연결

    public void OnClickSign() // 버튼에 연결할 함수
    {
        // 1. 유효성 검사: 이름이 비어있으면 안 됨
        if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.Log("길드 이름을 입력해주세요!");
            return;
        }

        // 2. 데이터 저장 (싱글톤 GameManager 활용)
        // (CS 전공자시니 null 체크 습관은 좋지만, GameManager는 보통 확실히 있으니 바로 접근합니다)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.guildName = nameInput.text; // 이름 저장
            GameManager.Instance.gold = 1000; // 초기 자금 지급
            GameManager.Instance.day = 1;     // 1일차 시작
        }

        // 3. 로비로 이동
        SceneManager.LoadScene("LobbyScene");
    }
}