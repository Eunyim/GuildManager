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

    [Header("아군 스폰 위치 설정")]
    public Transform[] frontSpawnPoints; // 전위 스폰 좌표들 (앞줄)
    public Transform[] backSpawnPoints;  // 후위 스폰 좌표들 (뒷줄)

    [Header("던전 진행")]
    public GameObject chestPrefab; // 아까 만든 보물상자 프리팹을 인스펙터에서 끌어다 넣으세요!
    private bool isRoomCleared = false; // 방 클리어 여부

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

    // ★ 위치 잡기 변수들 (전위와 후위를 따로 셉니다)
    int frontIndex = 0;
    int backIndex = 0;
    int allyCount = 0;

    foreach (Adventurer member in members)
    {
        GameObject prefabToSpawn = null;
        bool isFrontLine = false; // ★ 이 캐릭터가 앞줄에 서야 하는지 판별하는 변수

        // 직업에 맞는 프리팹 선택 및 전위/후위 판별
        switch (member.job)
        {
            case JobType.Warrior: 
                prefabToSpawn = warriorPrefab; 
                isFrontLine = true;  // 전사는 앞줄
                break;
            case JobType.Rogue:   
                prefabToSpawn = roguePrefab; 
                isFrontLine = true;  // 도적은 앞줄
                break;
            case JobType.Archer:  
                prefabToSpawn = archerPrefab; 
                isFrontLine = false; // 궁수는 뒷줄
                break;
            case JobType.Mage:    
                prefabToSpawn = magePrefab; 
                isFrontLine = false; // 법사는 뒷줄
                break;
            case JobType.Healer:  
                prefabToSpawn = healerPrefab; 
                isFrontLine = false; // 힐러는 뒷줄
                break;
            default:              
                prefabToSpawn = warriorPrefab; 
                isFrontLine = true;
                break;
        }

        if (prefabToSpawn == null) continue;

        // 소환!
        GameObject newUnit = Instantiate(prefabToSpawn);
        
        // 이름 및 태그 설정
        newUnit.name = $"Unit_{member.name}";
        newUnit.tag = "Player";

        // ★ 위치 배치 로직
        Transform targetSpawnPoint = null;

        // 앞줄 직업이고, 앞줄 스폰 지점에 자리가 남아있다면
        if (isFrontLine && frontSpawnPoints != null && frontIndex < frontSpawnPoints.Length)
        {
            targetSpawnPoint = frontSpawnPoints[frontIndex];
            frontIndex++;
        }
        // 뒷줄 직업이고, 뒷줄 스폰 지점에 자리가 남아있다면
        else if (!isFrontLine && backSpawnPoints != null && backIndex < backSpawnPoints.Length)
        {
            targetSpawnPoint = backSpawnPoints[backIndex];
            backIndex++;
        }
        else
        {
            // 만약 유니티 에디터에서 스폰 포인트를 덜 만들었을 때를 대비한 예비(Fallback) 지점
            targetSpawnPoint = playerSpawnPoint; 
        }

        // 위치 배치 (캐릭터들이 완전히 겹치지 않게 아주 약간의 랜덤성만 추가)
        float randomX = Random.Range(-0.2f, 0.2f);
        float randomY = Random.Range(-0.2f, 0.2f);
        
        if (targetSpawnPoint != null)
        {
            newUnit.transform.position = targetSpawnPoint.position + new Vector3(randomX, randomY, 0);
        }

        // 스탯 주입 (유저님이 짜두신 기존 로직 완벽 유지)
        BattleUnit unitScript = newUnit.GetComponent<BattleUnit>();
        if (unitScript != null)
        {
            unitScript.Initialize(member); 
        }

        allyCount++;
    }
    
    Debug.Log($"⚔️ 아군 {allyCount}명 소환 완료! (전위: {frontIndex}명, 후위: {backIndex}명)");
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

        ResetAllyPositions();

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
            
            // ★ 핵심 버그 픽스: 적이 모두 죽었고(<=0), 아직 방 클리어 처리가 안 되었다면(!isRoomCleared) 딱 한 번만 실행!
            if (enemyCount <= 0 && !isRoomCleared)
            {
                isRoomCleared = true;   // 자물쇠를 닫아서 광역기로 인한 중복 실행 완벽 차단!
                isBattleActive = false; // 전투 종료 처리
                
                Debug.Log("🎉 적 전멸! 보물상자 소환!");
                
                SpawnTreasureChest(); // ★ 상자 소환 함수 호출!
            }
        }
        
        // ❌ 이전에 있던 Invoke("CheckRoomClear", 0.1f); 부분은 완전히 삭제되었습니다! ❌
    }

    void SpawnTreasureChest()
    {
      // ★ X좌표를 3.0f (또는 4.0f) 정도로 줘서 화면 우측(적 진영)에 상자가 나오게 합니다.
        // Y좌표는 유저님 맵에 맞게 바닥 쪽에 맞춰주시면 더 좋습니다.
        Vector3 chestSpawnPos = new Vector3(4.0f, 0f, 0f); 
        
        GameObject chest = Instantiate(chestPrefab, chestSpawnPos, Quaternion.identity);

        // 살아남은 아군 중 한 명에게 상자를 열러 가라고 명령!
        OrderToLoot(chest.transform);
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

    private void CheckRoomClear()
    {
        if (isRoomCleared) return;

        // 맵에 'Enemy' 태그를 가진 오브젝트가 남아있는지 확인
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (enemies.Length == 0) // 남은 적이 0명이다! (방 클리어!)
        {
            isRoomCleared = true;
            Debug.Log("🎉 적 전멸! 보물상자 소환!");

            // 1. 맵 중앙(또는 적절한 위치)에 보물상자 소환
            Vector3 chestSpawnPos = new Vector3(0, 0, 0); 
            GameObject chest = Instantiate(chestPrefab, chestSpawnPos, Quaternion.identity);

            // 2. 살아남은 아군 중 한 명에게 상자를 열러 가라고 명령!
            OrderToLoot(chest.transform);
        }
    }

    // ★ 상자 열러 갈 사람 지정하는 함수
    private void OrderToLoot(Transform chestTransform)
    {
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Player");
        if (allies.Length > 0)
        {
            // (나중에는 도적을 우선순위로 보내는 로직을 짤 수도 있습니다)
            // 일단은 살아남은 아군 중 첫 번째 캐릭터를 보냅니다.
            BattleUnit looter = allies[0].GetComponent<BattleUnit>();
            if (looter != null)
            {
                looter.CommandLoot(chestTransform);
            }
        }
    }

    // (에러 방지용 빈 함수들)
    public void ShowDamageText(Vector3 pos, float damage, bool crit) { }
    public void ShowHealText(Vector3 pos, float amount) { }

    // ★ 도주 버튼을 눌렀을 때 실행될 함수
    public void OnClickFleeButton()
    {
        Debug.Log("🚨 [긴급 도주] 매니저 권한으로 전투를 강제 종료합니다!");

        // 1. 맵에 살아있는 모든 아군의 행동(AI)을 강제로 정지시킵니다.
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject allyObj in allies)
        {
            BattleUnit unit = allyObj.GetComponent<BattleUnit>();
            if (unit != null)
            {
                // (Phase 3에서 여기에 캐릭터별 남은 체력(HP)을 저장하는 로직이 들어갈 자리입니다)
                unit.enabled = false; // 스크립트를 꺼버려서 멈추게 함
            }
        }

        // 2. 적들도 멈추게 합니다.
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemyObj in enemies)
        {
            MonoBehaviour[] scripts = enemyObj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false; 
            }
        }

        // 3. 1초 뒤에 로비 씬으로 도망칩니다! (코루틴 사용)
        StartCoroutine(EscapeToLobby());
    }

    private System.Collections.IEnumerator EscapeToLobby()
    {
       yield return new WaitForSeconds(1.0f); // 1초 대기 (도망치는 연출)
        
        // ★ [핵심 버그 픽스] 로비로 씬을 넘기기 전에 파티 상태를 'Idle(대기)'로 초기화!
        if (GameManager.Instance != null && GameManager.Instance.currentDispatchParty != null)
        {
            // 1. 해당 파티의 상태를 다시 '대기' 상태로 바꿔줍니다.
            GameManager.Instance.currentDispatchParty.state = PartyState.Idle; 
            
            // 2. 매니저의 머릿속에서 '현재 파견나간 파티'를 지워버립니다.
            GameManager.Instance.currentDispatchParty = null; 
            
            Debug.Log("🏠 파티가 무사히 길드로 귀환하여 대기 상태로 전환되었습니다.");
        }

        // 로비 씬으로 이동! (씬 이름 확인 필수)
        SceneManager.LoadScene("LobbyScene");
    }

    public void OnChestOpened()
    {
        Debug.Log("✅ 상자 파밍 완료! 다음 방으로 이동합니다...");
        
        // ★ 핵심 버그 픽스: 예약되어 있던 잉여 CheckRoomClear 호출을 모두 취소합니다!
        CancelInvoke("CheckRoomClear"); 

        isRoomCleared = false; 
        
        // (유저님의 변수명에 맞게 currentStageIndex를 올려주고 코루틴 실행)
        currentStageIndex++; 
        StartCoroutine(StartStageCoroutine(currentStageIndex)); 
    }

    // ★ 다음 방으로 넘어갈 때 아군 위치를 진형에 맞게 재배치하는 함수
    private void ResetAllyPositions()
    {
        GameObject[] allyObjects = GameObject.FindGameObjectsWithTag("Player");
        List<BattleUnit> survivingAllies = new List<BattleUnit>();

        foreach (GameObject obj in allyObjects)
        {
            BattleUnit unit = obj.GetComponent<BattleUnit>();
            if (unit != null && !unit.isDead)
            {
                survivingAllies.Add(unit);
                unit.ResetState(); // 1단계에서 만든 상태 초기화 실행!
            }
        }

        // ★ 핵심: 체력(maxHp)이 높은 순서대로 내림차순 정렬 (탱커가 무조건 1순위)
        survivingAllies.Sort((a, b) => b.maxHp.CompareTo(a.maxHp));

        int frontIndex = 0;
        int backIndex = 0;

        foreach (BattleUnit unit in survivingAllies)
        {
            Transform targetPoint = null;

            // 전위에 자리가 남아있으면 앞줄로, 다 찼으면 뒷줄로 배정
            if (frontSpawnPoints != null && frontIndex < frontSpawnPoints.Length)
            {
                targetPoint = frontSpawnPoints[frontIndex];
                frontIndex++;
            }
            else if (backSpawnPoints != null && backIndex < backSpawnPoints.Length)
            {
                targetPoint = backSpawnPoints[backIndex];
                backIndex++;
            }

            // 위치로 텔레포트 (살짝 겹치지 않게 랜덤값 추가)
            if (targetPoint != null)
            {
                float randomX = Random.Range(-0.2f, 0.2f);
                float randomY = Random.Range(-0.2f, 0.2f);
                unit.transform.position = targetPoint.position + new Vector3(randomX, randomY, 0);
            }
        }

        Debug.Log("🔄 다음 방 진입! 아군 진형(위치) 재배치 완료!");
    }
}