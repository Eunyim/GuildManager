using UnityEngine;

public class GameManager : MonoBehaviour
{
    // [싱글톤 패턴] 전역에서 접근 가능한 유일한 인스턴스
    public static GameManager Instance;

    [Header("길드 데이터")]
    public string guildName = "용감한 모험가들"; // 길드 이름
    public int gold = 1000;          // 초기 자금
    public int reputation = 0;       // 명성
    public int day = 1;              // 날짜

    private void Awake()
    {
        // 싱글톤 보장 로직
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }

    // 테스트용 함수
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"[재정] 현재 골드: {gold} G");
    }
}