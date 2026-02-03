using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동용
using TMPro; // 텍스트 제어용
using System.Collections.Generic;
using System.Collections;


public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance; // 싱글톤

    [Header("아군 설정")]
    public GameObject unitPrefab;    // 아군 프리팹 (Prefab_BattleUnit)
    public Transform spawnPointTeam; // 아군 스폰 위치 (Ally_Spawn)

    [Header("적군 설정")]
    public GameObject defaultEnemyPrefab;
    public Transform spawnPointEnemy;

    [Header("UI 연결")]
    public GameObject resultPanel;      // 결과창 패널 (Panel_Result)
    public TextMeshProUGUI resultText;  // 결과 텍스트 (Victory/Defeat)

    [Header("UI 패널 연결")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("아군 프리팹 연결")]
    public GameObject warriorPrefab; // ★ 전사 프리팹 넣을 곳
    public GameObject archerPrefab;  // ★ 궁수 프리팹 넣을 곳
    // public GameObject magePrefab; // (나중에 마법사도 있다면)
    // public GameObject roguePrefab;
    // public GameObject healerPrefab;

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

        SpawnEnemyFromQuest(); // 적군 소환

        // 3. 최종 집계 로그 출력
        Debug.Log($"⚔️ 전투 시작! 아군: {allyCount}명 vs 적군: {enemyCount}명");
    }

    void SpawnEnemyFromQuest() //퀘스트 정보로 적 소환
    {
       GameObject prefabToSpawn = null;

        // 1. 매니저한테 퀘스트 정보가 있는지 물어봄
        if (GameManager.Instance != null && GameManager.Instance.currentQuest != null)
        {
            prefabToSpawn = GameManager.Instance.currentQuest.enemyPrefab;
        }
        
        // 2. 정보가 없으면? (그냥 던전 씬 바로 실행했을 때) -> 테스트용 프리팹 사용
        if (prefabToSpawn == null)
        {
            prefabToSpawn = defaultEnemyPrefab;
            Debug.Log("⚠️ 퀘스트 정보가 없어 테스트용 적을 소환합니다.");
        }

        if (prefabToSpawn == null) return; // 그래도 없으면 포기

        // 3. 진짜 소환
        GameObject enemyObj = Instantiate(prefabToSpawn);
        enemyObj.transform.position = spawnPointEnemy.position;
        enemyObj.transform.localScale = new Vector3(-1, 1, 1); // 왼쪽 보게 뒤집기

        // 4. 이름과 태그 설정
        enemyObj.name = prefabToSpawn.name;
        enemyObj.tag = "Enemy"; 
        
        // 5. 카운트 증가
        enemyCount++;
        // (만약 적을 여러 마리 소환하고 싶으면 여기서 반복문 돌리면 됩니다)
    }

   

    // --- 1. 아군 소환 로직 ---
    void SetupAllyParty()
    {
      if (GameManager.Instance == null || GameManager.Instance.currentDispatchParty == null) return;

        List<Adventurer> members = GameManager.Instance.currentDispatchParty.members;

        // 위치 잡기 변수들
        int index = 0;
        Vector3 startPos = spawnPointTeam.position;
        float spacing = 2.0f; 

        foreach (Adventurer member in members)
        {
            GameObject prefabToSpawn = null;

            // ★ [수정됨] Adventurer.job.Warrior -> JobType.Warrior
            switch (member.job)
            {
                case JobType.Warrior:
                    prefabToSpawn = warriorPrefab;
                    break;
                case JobType.Archer:
                    prefabToSpawn = archerPrefab;
                    break;
                // case JobType.Mage: prefabToSpawn = magePrefab; break;
                default:
                    prefabToSpawn = warriorPrefab; // 기본값
                    break;
            }

            if (prefabToSpawn == null)
            {
                Debug.LogError($"직업({member.job})에 맞는 프리팹이 연결되지 않았습니다!");
                continue;
            }

            // 소환!
            GameObject newUnit = Instantiate(prefabToSpawn);
            
            // 이름 변경
            newUnit.name = $"Unit_{member.name}";
            newUnit.tag = "Player";

            // 위치 배치
            // 1. X축: 뒤로 갈수록 조금씩 뒤에 서기
            float randomX = Random.Range(0f, 0.5f); // 0~0.5만큼 랜덤하게 흔들림
            float xPos = -index * spacing - randomX;

            // 2. Y축: 위아래로 랜덤하게 퍼지기
            float randomY = Random.Range(-1.5f, 1.5f); // 위아래로 1.5만큼 랜덤

            // 최종 위치 적용
            newUnit.transform.position = startPos + new Vector3(xPos, randomY, 0);

            // 스탯 주입 (BattleUnit 컴포넌트가 있다면)
            BattleUnit unitScript = newUnit.GetComponent<BattleUnit>();
            if (unitScript != null)
            {
                // unitScript.InitializeData(member); // 나중에 구현
            }

            index++;
            allyCount++;
        }
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

   public void GameOver(bool isWin)
    {
        // 시간을 멈추거나 유닛을 멈춤 (선택사항)
        // Time.timeScale = 0; 
        
        if (isWin)
        {
            Debug.Log("🎉 승리!");
            // 승리 UI 패널 켜기 (여기서 버튼을 누르게 유도)
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }
        else
        {
            Debug.Log("💀 패배...");
            // 패배 UI 패널 켜기
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }
    }

    // 2. 귀환 버튼: 플레이어가 UI의 [로비로] 버튼을 눌렀을 때 호출
    public void OnClick_ReturnToLobby()
    {
        // ★ 아까 작성하신 "보상 및 이동 로직"은 전부 여기에 있어야 합니다!
        
        if (GameManager.Instance != null)
        {
            // A. 보상 지급
            int reward = 100;
            if (GameManager.Instance.currentQuest != null)
                reward = GameManager.Instance.currentQuest.rewardGold;
            
            GameManager.Instance.AddGold(reward);

            // B. 파티 상태 해제
            if (GameManager.Instance.currentDispatchParty != null)
                GameManager.Instance.currentDispatchParty.state = PartyState.Idle;

            // C. 데이터 초기화
            GameManager.Instance.currentQuest = null;
            GameManager.Instance.currentDispatchParty = null;
        }

        // D. 씬 이동
        SceneManager.LoadScene("LobbyScene");
        
        // (참고) 시간을 멈췄었다면 다시 풀어줘야 함
        // Time.timeScale = 1;
    }

   
}