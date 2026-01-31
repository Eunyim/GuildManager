using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동용
using TMPro; // 텍스트 제어용
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // 싱글톤

    [Header("아군 설정")]
    public GameObject unitPrefab;    // 아군 프리팹 (Prefab_BattleUnit)
    public Transform spawnPointTeam; // 아군 스폰 위치 (Ally_Spawn)

    [Header("적군 설정")]
    public GameObject enemyPrefab;
    public Transform spawnPointEnemy;

    [Header("UI 연결")]
    public GameObject resultPanel;      // 결과창 패널 (Panel_Result)
    public TextMeshProUGUI resultText;  // 결과 텍스트 (Victory/Defeat)

    // 내부 카운트 변수
    private int allyCount = 0;
    private int enemyCount = 0;
    private bool isBattleEnded = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. 아군 소환 및 카운트
        SetupAllyParty();

        // 2. 적군 카운트
        SpawnEnemy();

        // 3. 최종 집계 로그 출력
        Debug.Log($"⚔️ 전투 시작! 아군: {allyCount}명 vs 적군: {enemyCount}명");
    }

    void SpawnEnemy()
    {
        // 안전장치
        if (enemyPrefab == null || spawnPointEnemy == null)
        {
            Debug.LogError("🚨 적 프리팹이나 스폰 위치가 연결되지 않았습니다!");
            return;
        }

        // 1. 적 생성
        GameObject enemyObj = Instantiate(enemyPrefab);
        
        // 2. 위치 설정 (Enemy_Spawn 위치로)
        enemyObj.transform.position = spawnPointEnemy.position;

        // 3. (중요) 방향 뒤집기! (적은 왼쪽을 봐야 하니까)
        // 슬라임 그림이 오른쪽을 보고 있다면, X축을 -1로 뒤집어줍니다.
        enemyObj.transform.localScale = new Vector3(-1, 1, 1);

        // 4. 이름 및 태그 설정
        enemyObj.name = "Enemy_Slime";
        enemyObj.tag = "Enemy"; // 태그 확실하게 붙이기!

        // 5. 카운트 증가
        enemyCount++; 
        
        // (만약 적을 여러 마리 소환하고 싶으면 여기서 반복문 돌리면 됩니다)
    }

   

    // --- 1. 아군 소환 로직 ---
    void SetupAllyParty()
    {
        Party targetParty = null;

        // GameManager에서 정보 가져오기 (없으면 테스트용 생성)
        if (GameManager.Instance != null && GameManager.Instance.currentDispatchParty != null)
        {
            targetParty = GameManager.Instance.currentDispatchParty;
        }
        else
        {
            // 테스트용 임시 데이터
            targetParty = new Party("테스트 팀");
            Adventurer t1 = AdventurerGenerator.Generate(); t1.name = "테스트 전사";
            Adventurer t2 = AdventurerGenerator.Generate(); t2.name = "테스트 궁수";
            targetParty.members.Add(t1);
            targetParty.members.Add(t2);
        }

        // 실제 소환 실행
        SpawnMyParty(targetParty);
    }

    

    void SpawnMyParty(Party party)
    {
        if (unitPrefab == null || spawnPointTeam == null)
        {
            Debug.LogError("🚨 프리팹이나 스폰 위치가 연결되지 않았습니다!");
            return;
        }

        // ★ 여기서 아군 숫자를 확정합니다!
        allyCount = party.members.Count;

        for (int i = 0; i < party.members.Count; i++)
        {
            Adventurer member = party.members[i];
            GameObject unitObj = Instantiate(unitPrefab);

            // 위치 잡기
            Vector3 offset = new Vector3(-1 * (i % 2), i * 1.5f - 2f, 0);
            unitObj.transform.position = spawnPointTeam.position + offset;

            // 이름 및 태그 설정
            unitObj.name = $"Unit_{member.name}";
            unitObj.tag = "Player"; // ★ 중요: 태그를 코드로 강제 설정

            // (선택) 아군 색상 파란색
            unitObj.GetComponent<SpriteRenderer>().color = Color.blue; 
        }
    }

    // --- 3. 유닛 사망 시 호출 (심판) ---
    public void OnUnitDead(bool isPlayerTeam)
    {
        if (isBattleEnded) return;

        if (isPlayerTeam) allyCount--;
        else enemyCount--;

        Debug.Log($"💀 사망 보고! 남은 아군: {allyCount} / 적군: {enemyCount}");

        CheckGameResult();
    }

    void CheckGameResult()
    {
        if (allyCount <= 0)
        {
            GameOver(false); // 패배
        }
        else if (enemyCount <= 0)
        {
            GameOver(true); // 승리
        }
    }

    void GameOver(bool isWin)
    {
        isBattleEnded = true;
        resultPanel.SetActive(true); // 결과창 켜기

        if (isWin)
        {
            resultText.text = "<color=yellow>VICTORY!</color>";
            if (GameManager.Instance != null) GameManager.Instance.AddGold(500);
        }
        else
        {
            resultText.text = "<color=red>DEFEAT...</color>";
        }
    }

    public void OnClickReturnLobby()
    {
        SceneManager.LoadScene("LobbyScene"); //로비씬으로
    }
}