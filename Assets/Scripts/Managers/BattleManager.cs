using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동용
using TMPro; // 텍스트 제어용
using System.Collections.Generic;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    
    public static BattleManager Instance; // 싱글톤

    [Header("던전 구성")]
    public List<StageData> dungeonStages; // 스테이지 데이터 리스트
    private int currentStageIndex = 0;
    
    [Header("스폰 위치")]
    public Transform playerSpawnPoint; // 아군 시작 위치 (왼쪽)
    public Transform enemySpawnPoint;  // 적군 시작 위치 (오른쪽)

    [Header("상태 정보")]
    public bool isBattleActive = false;
    public int allyCount = 0;  // 현재 생존 아군 수
    public int enemyCount = 0; // 현재 생존 적군 수

    [Header("UI 패널 연결")]
    public GameObject victoryPanel; // 승리 패널
    public GameObject defeatPanel;  // 패배 패널

    [Header("아군 프리팹 연결 (직업별)")]
    public GameObject warriorPrefab; 
    public GameObject archerPrefab;  
    public GameObject magePrefab;    
    public GameObject roguePrefab;
    public GameObject healerPrefab;

    [Header("던전 전리품")]
    public List<MonsterDropData> earnedItems = new List<MonsterDropData>(); // ★ 던전에서 주운 아이템들

    [Header("던전 오브젝트")]
    public GameObject treasureChestPrefab; // ★ 추가: 보물상자 프리팹

    // (에러 방지용: 안 써도 둠)
    public GameObject damageTextPrefab; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. 아군 소환 (직업별 프리팹 사용)
        SetupAllyParty();

        // 2. 던전 스테이지 시작 (0번부터)
        if (dungeonStages != null && dungeonStages.Count > 0)
        {
            StartCoroutine(StartStageCoroutine(0));
        }
        else
        {
            Debug.LogError("🚨 던전 스테이지 데이터(StageData)가 비어있습니다!");
        }
    }

    // --- 1. 아군 소환 로직 (GameManager 연동) ---
    void SetupAllyParty()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentDispatchParty == null) 
        {
            Debug.LogWarning("게임 매니저나 파티 정보가 없습니다. 테스트 모드일 수 있습니다.");
            return;
        }

        List<Adventurer> members = GameManager.Instance.currentDispatchParty.members;

        // 위치 잡기 변수들
        int index = 0;
        float spacing = 2.0f; 

        foreach (Adventurer member in members)
        {
            GameObject prefabToSpawn = null;

            // 직업에 맞는 프리팹 선택
            switch (member.job)
            {
                case JobType.Warrior: prefabToSpawn = warriorPrefab; break;
                case JobType.Archer:  prefabToSpawn = archerPrefab; break;
                case JobType.Mage:    prefabToSpawn = magePrefab; break;
                case JobType.Rogue:   prefabToSpawn = roguePrefab; break;
                case JobType.Healer:  prefabToSpawn = healerPrefab; break;
                default:              prefabToSpawn = warriorPrefab; break;
            }

            if (prefabToSpawn == null) continue;

            // 소환!
            GameObject newUnit = Instantiate(prefabToSpawn);
            
            // 이름 및 태그 설정
            newUnit.name = $"Unit_{member.name}";
            newUnit.tag = "Player";

            // 위치 배치 (약간의 랜덤성 추가)
            float randomX = Random.Range(0f, 0.5f);
            float randomY = Random.Range(-1.5f, 1.5f);
            float xPos = -index * spacing - randomX;
            
            newUnit.transform.position = playerSpawnPoint.position + new Vector3(xPos, randomY, 0);

            // 스탯 주입
            BattleUnit unitScript = newUnit.GetComponent<BattleUnit>();
            if (unitScript != null)
            {
                unitScript.Initialize(member); 
            }

            index++;
            allyCount++;
        }
        
        Debug.Log($"⚔️ 아군 {allyCount}명 소환 완료!");
    }

    // --- 2. 스테이지 진행 코루틴 (적 소환) ---
    IEnumerator StartStageCoroutine(int stageIndex)
    {
        currentStageIndex = stageIndex;
        
        // 더 이상 스테이지가 없으면 -> 던전 완전 클리어!
        if (currentStageIndex >= dungeonStages.Count)
        {
            Debug.Log("🎉 던전 클리어! 모든 스테이지 정복 완료!");
            GameOver(true); // 승리 처리
            yield break;
        }

        Debug.Log($"⚔️ 스테이지 {stageIndex + 1} 시작: {dungeonStages[stageIndex].stageName}");
        
        // 플레이어가 숨 돌릴 시간 (2초 대기)
        yield return new WaitForSeconds(2.0f);

        // 적 소환
        SpawnEnemies(dungeonStages[stageIndex]);
        isBattleActive = true;
    }

    void SpawnEnemies(StageData stageData)
    {
        if (stageData == null) return;

        List<GameObject> enemies = stageData.enemyPrefabs;
        float spacing = 1.5f;
        enemyCount = enemies.Count; // 적 숫자 갱신

        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject prefab = enemies[i];
            if (prefab == null) continue;

            // 적은 오른쪽에서 생성
            Vector3 pos = enemySpawnPoint.position + new Vector3(i * spacing, 0, 0);
            
            GameObject newEnemy = Instantiate(prefab, pos, Quaternion.identity);
            newEnemy.tag = "Enemy";
            newEnemy.transform.localScale = new Vector3(-1, 1, 1); // 왼쪽 보게 반전
        }
        
        Debug.Log($"😈 적 {enemyCount}명 등장!");
    }

    // --- 3. 유닛 사망 처리 (심판) ---
    public void OnUnitDead(bool isPlayerSide)
    {
        // 이미 전투가 끝났으면 무시
        if (!isBattleActive && (allyCount <= 0 || currentStageIndex >= dungeonStages.Count)) return;

        if (isPlayerSide)
        {
            allyCount--;
            if (allyCount <= 0)
            {
                Debug.Log("💀 아군 전멸... 패배했습니다.");
                isBattleActive = false;
                GameOver(false); // 패배 처리
            }
        }
        else // 적군 사망
        {
            enemyCount--;
            if (enemyCount <= 0)
            {
                Debug.Log("✅ 스테이지 클리어! 잠시 후 다음 방으로 이동합니다...");
                isBattleActive = false;
                
                SpawnTreasureChest(); // ★ 바로 넘어가지 말고 상자 소환!
            }
        }
    }

    void SpawnTreasureChest()
    {
        if (treasureChestPrefab != null)
        {
            // 화면 중앙쯤에 상자 생성
            Instantiate(treasureChestPrefab, new Vector3(0, -1, 0), Quaternion.identity);
        }
        else
        {
            // 프리팹 안 넣었으면 그냥 바로 다음 스테이지로
            GoToNextStage(); 
        }
    }

    // ★ 상자를 열었을 때 호출될 함수 (기존 StartStageCoroutine 부르는 역할)
    // 상자를 열었을 때 호출될 함수
    public void GoToNextStage()
    {
        currentStageIndex++; // 다음 방 번호로 올림
        
        // ★ [핵심 수정] QuestData에서 찾지 말고, 일단 "3스테이지"라고 횟수를 못 박아버립니다!
        // (만약 BattleManager 안에 stages 리스트가 있다면 stages.Count로 적으셔도 됩니다)
        if (currentStageIndex < 3) 
        {
            // 다음 방으로 이동! (괄호 안에 currentStageIndex 넣기)
            StartCoroutine(StartStageCoroutine(currentStageIndex)); 
        }
        else
        {
            Debug.Log("🎉 던전 완전 클리어! 최종 보상 정산 화면으로!");
            // (나중에 여기에 결과창 UI 띄우는 코드 추가)
        }
    }

    // --- 4. 게임 종료 및 UI ---
    public void GameOver(bool isWin)
    {
        if (isWin)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }
        else
        {
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }
    }

    // --- 5. 로비로 돌아가기 (버튼 연결용) ---
    public void OnClick_ReturnToLobby()
    {
        if (GameManager.Instance != null)
        {
            // 승리했을 때만 보상을 준다면 조건을 추가할 수 있습니다.
            // 여기서는 일단 귀환하면 보상을 주는 로직 유지 (퀘스트 완료 처리)
            int reward = 0;
            if (GameManager.Instance.currentQuest != null)
            {
                reward = GameManager.Instance.currentQuest.rewardGold;
            }
            
            // (던전을 클리어해야 돈을 준다면 bool 플래그가 필요하지만, 일단 기존 로직 존중)
            GameManager.Instance.AddGold(reward);

            // 파티 상태 해제 (중요!)
            if (GameManager.Instance.currentDispatchParty != null)
                GameManager.Instance.currentDispatchParty.state = PartyState.Idle;

            // 데이터 초기화
            GameManager.Instance.currentQuest = null;
            GameManager.Instance.currentDispatchParty = null;
        }

        // 씬 이동
        SceneManager.LoadScene("LobbyScene");
    }

    public void AddItemToLootBag(MonsterDropData item)
    {
        earnedItems.Add(item);
        Debug.Log($"🎒 가방에 챙김: {item.itemName} (현재 가방에 총 {earnedItems.Count}개)");
    }

    // (에러 방지용 빈 함수들)
    public void ShowDamageText(Vector3 pos, float damage, bool crit) { }
    public void ShowHealText(Vector3 pos, float amount) { }
}