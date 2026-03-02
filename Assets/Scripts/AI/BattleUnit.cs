using UnityEngine;
using System.Collections.Generic;

public class BattleUnit : MonoBehaviour
{
    [Header("기본 스탯")]
    public float maxHp = 100;
    public float currentHp;
    public float maxMp = 100;
    public float currentMp = 0;
    
    [Header("전투 설정")]
    public float attackRange = 1.5f; 
    public float moveSpeed = 2.0f;
    public float attackPower = 10f;
    public float attackCooldown = 1.0f;
    public float mpRegenOnHit = 20;

    [Header("UI & 이펙트")]
    public GameObject hpBarPrefab;
    private HPBar myHPBar; 

    [Header("특성 시스템")]
    public List<TraitData> activeTraits = new List<TraitData>();
    private float baseAttackPower; 
    protected bool isCoward = false; 

    protected BattleUnit target; 
    protected Animator anim;     
    protected float lastAttackTime; 

    [Header("AI 설정")]
    public float aggro = 10f; // 어그로 수치 (전사는 프리팹에서 높게 설정)

    [Header("파밍(Looting) 시스템")]
    private Transform chestTarget; // 열어야 할 보물상자 위치
    private bool isLooting = false; // 지금 파밍하러 가는 중인가?

    public void Initialize(Adventurer data)
    {
        maxHp = data.hp;
        currentHp = maxHp;
        attackPower = data.atk;
        currentMp = 0; 
        name = $"Unit_{data.name}";

        if (data.traits != null)
        {
            activeTraits = data.traits;
        }
        baseAttackPower = attackPower;
    }

    protected virtual void Start()
    {
        if (currentHp <= 0) currentHp = maxHp;
        anim = GetComponent<Animator>();
        
        // ★ 구형 AI 대신 새로운 통합 타겟 찾기 실행
        SearchForTarget();
        
        CreateHPBar();
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // ★ [추가된 파밍 AI] 상자를 향해 걸어가는 상태라면?
        if (isLooting && chestTarget != null)
        {
            // 상자 쪽으로 뚜벅뚜벅 걷기
            transform.position = Vector3.MoveTowards(transform.position, chestTarget.position, moveSpeed * Time.deltaTime);
            
            // 방향 전환 (상자 쪽 바라보기)
            if (chestTarget.position.x < transform.position.x) 
                transform.localScale = new Vector3(-1, 1, 1); 
            else 
                transform.localScale = new Vector3(1, 1, 1);
            
            return; // 파밍 중일 때는 아래의 전투 로직(CheckTraits, FindTarget 등)을 아예 무시합니다!
        }

        CheckTraits(); // 실시간 특성 체크

        // 1. UI 갱신
        if (myHPBar != null)
        {
            myHPBar.UpdateBar(currentHp, maxHp, currentMp, maxMp);
        }

        // 2. 타겟이 없거나 죽었으면 다시 찾기 (새로운 타겟팅 함수 호출)
        if (target == null || target.currentHp <= 0)
        {
            SearchForTarget();
            return; 
        }

        // 3. 거리 계산 및 이동/공격 판별
        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance <= attackRange)
        {
            StopAndAttack();
        }
        else
        {
            MoveToTarget();
        }
    }

    // ★ [추가] 배틀 매니저에서 적 리스트를 가져와 새로운 FindTarget에 넘겨주는 연결 함수
    protected void SearchForTarget()
    {
        // 내가 플레이어면 "Enemy" 태그를 찾고, 적이면 "Player" 태그를 찾음
        string targetTag = gameObject.CompareTag("Player") ? "Enemy" : "Player";
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);

        List<BattleUnit> targetList = new List<BattleUnit>();
        foreach (GameObject obj in targetObjects)
        {
            BattleUnit unit = obj.GetComponent<BattleUnit>();
            if (unit != null) targetList.Add(unit);
        }

        FindTarget(targetList);
    }

    // ★ 통합된 새로운 타겟 탐색 함수 (오직 이 녀석 하나만 존재해야 합니다!)
    protected virtual void FindTarget(List<BattleUnit> enemyList)
    {
        // 1. [특성] 내가 '겁쟁이'라면? 적을 찾지 않고 튼튼한 아군 뒤로 숨는다!
        if (isCoward && BattleManager.Instance != null)
        {
            target = FindStrongestAlly();
            return; 
        }

        // 2. [기본 AI] 적 탐색 (어그로 시스템 적용)
        if (enemyList == null || enemyList.Count == 0) return;

        float bestScore = Mathf.Infinity;
        BattleUnit bestTarget = null;

        foreach (BattleUnit enemy in enemyList)
        {
            if (enemy == null || enemy.currentHp <= 0) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            float score = distance - enemy.aggro; // 어그로 적용

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemy;
            }
        }
        target = bestTarget;
    }

    // -------------------------------------------------------------
    // 아래는 이동, 공격, 특성 등 기능 함수들입니다.
    // -------------------------------------------------------------

    private void CheckTraits()
    {
        attackPower = baseAttackPower;
        isCoward = false;

        foreach (TraitData trait in activeTraits)
        {
            if (trait == null) continue;

            bool conditionMet = false;

            switch (trait.conditionType)
            {
                case TraitConditionType.Always:
                    conditionMet = true; 
                    break;
                case TraitConditionType.NoAllyNearby:
                    conditionMet = CheckNoAllyNearby(trait.conditionValue);
                    break;
            }

            if (conditionMet)
            {
                ApplyTraitEffect(trait);
            }
        }
    }

    private bool CheckNoAllyNearby(float radius)
    {
        string myTag = gameObject.tag; // 나와 같은 편 태그
        GameObject[] allyObjects = GameObject.FindGameObjectsWithTag(myTag);

        foreach (GameObject obj in allyObjects) 
        {
            BattleUnit ally = obj.GetComponent<BattleUnit>();
            if (ally == null || ally.isDead || ally == this) continue; 

            float dist = Vector2.Distance(transform.position, ally.transform.position);
            if (dist <= radius) return false; // 반경 내에 아군 발견!
        }
        return true;
    }

    private void ApplyTraitEffect(TraitData trait)
    {
        switch (trait.effectType)
        {
            case TraitEffectType.ModifyAttackPower:
                attackPower = baseAttackPower * trait.effectValue; 
                break;
            case TraitEffectType.ChangeAI_HideBehindAlly:
                isCoward = true; 
                break;
        }
    }

    private BattleUnit FindStrongestAlly()
    {
        BattleUnit strongest = null;
        float maxHealth = -1;

        string myTag = gameObject.tag; // 나와 같은 편 태그
        GameObject[] allyObjects = GameObject.FindGameObjectsWithTag(myTag);

        foreach (GameObject obj in allyObjects)
        {
            BattleUnit ally = obj.GetComponent<BattleUnit>();
            if (ally == null || ally.isDead || ally == this) continue;

            if (ally.maxHp > maxHealth)
            {
                maxHealth = ally.maxHp;
                strongest = ally;
            }
        }
        return strongest != null ? strongest : this;
    }

    protected virtual void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);

        if (target.transform.position.x < transform.position.x) 
            transform.localScale = new Vector3(-1, 1, 1); 
        else 
            transform.localScale = new Vector3(1, 1, 1);  
    }

    protected void StopAndAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (currentMp >= maxMp)
            {
                UseSkill();
            }
            else
            {
                Attack();
            }
            lastAttackTime = Time.time;
        }
    }

    protected virtual void Attack()
    {
        if (target != null) 
        {
            target.TakeDamage(attackPower);
            Debug.Log($"⚔️ {name}의 공격! -> {target.name} (HP: {target.currentHp})");
        }

        currentMp += mpRegenOnHit;
        if (currentMp > maxMp) currentMp = maxMp;
    }

    protected virtual void UseSkill()
    {
        Debug.Log($"{name}의 스킬!");
        currentMp = 0; 
    }

    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ShowDamageText(transform.position, damage, false);
        }

        if (currentHp <= 0) Die();
    }

    // 체력이 0이 되었을 때의 처리 (isDead 프로퍼티 역할 수행)
    public bool isDead { get { return currentHp <= 0; } }

    protected virtual void Die()
    {
        Debug.Log($"💀 {name} 사망!");
        
        if (myHPBar != null) Destroy(myHPBar.gameObject);

        if (BattleManager.Instance != null)
        {
            bool isPlayerSide = gameObject.CompareTag("Player");
            BattleManager.Instance.OnUnitDead(isPlayerSide);
        }
        Destroy(gameObject);
    }

    void CreateHPBar()
    {
        if (hpBarPrefab == null) return;
        
        GameObject barObj = Instantiate(hpBarPrefab, transform); 
        myHPBar = barObj.GetComponent<HPBar>();
        if (myHPBar != null) myHPBar.Setup(this);
    }

    // 매니저가 "상자 열러 가!" 하고 명령할 때 호출되는 함수
    public void CommandLoot(Transform chest)
    {
        isLooting = true;
        chestTarget = chest;
        target = null; // 전투 타겟 초기화 (싸움 끝!)
        Debug.Log($"🏃 {name} : 내가 상자 열러 갈게!");
    }
}