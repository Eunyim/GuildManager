using UnityEngine;

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
    private HPBar myHPBar; // ★ 생성된 HP바를 기억할 변수

    protected BattleUnit target; 
    protected Animator anim;     
    protected float lastAttackTime; 

    public void Initialize(Adventurer data)
    {
        maxHp = data.hp;
        currentHp = maxHp;
        attackPower = data.atk;
        currentMp = 0; 
        name = $"Unit_{data.name}";
    }

    protected virtual void Start()
    {
        if (currentHp <= 0) currentHp = maxHp;
        anim = GetComponent<Animator>();
        
        // 타겟 찾기
        FindNearestTarget();
        
        // ★ HP바 생성 및 연결
        CreateHPBar();
    }

    protected virtual void Update()
    {
        // 1. UI 갱신 (매 프레임 체력/마나 동기화)
        if (myHPBar != null)
        {
            myHPBar.UpdateBar(currentHp, maxHp, currentMp, maxMp);
        }

        // 2. 타겟이 없거나 죽었으면 다시 찾기
        if (target == null || target.currentHp <= 0)
        {
            FindNearestTarget();
            return; 
        }

        // 3. 거리 계산
        float distance = Vector3.Distance(transform.position, target.transform.position);

        // 4. 이동 vs 공격 결정
        if (distance <= attackRange)
        {
            // 사거리 안이면 멈추고 공격
            StopAndAttack();
        }
        else
        {
            // 사거리 밖이면 이동
            MoveToTarget();
        }
    }

    protected virtual void MoveToTarget()
    {
        // ★ 서로 밀지 않게 하려면 Rigidbody 설정을 건드려야 함 (아래 설명 참조)
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);

        // 방향 전환
        if (target.transform.position.x < transform.position.x) 
            transform.localScale = new Vector3(-1, 1, 1); 
        else 
            transform.localScale = new Vector3(1, 1, 1);  
    }

    protected void StopAndAttack()
    {
        // 공격 쿨타임 체크
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
        // 근접 공격인 경우 여기서 바로 데미지
        // 원거리(투사체)인 경우 자식 클래스(Archer/Mage)에서 override 함
        if (target != null) 
        {
            target.TakeDamage(attackPower);
            Debug.Log($"⚔️ {name}의 공격! -> {target.name} (HP: {target.currentHp})");
        }

        // MP 회복
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
        
        // 데미지 텍스트 (옵션)
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ShowDamageText(transform.position, damage, false);
        }

        if (currentHp <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"💀 {name} 사망!");
        
        // 죽으면 HP바도 같이 삭제
        if (myHPBar != null) Destroy(myHPBar.gameObject);

        if (BattleManager.Instance != null)
        {
            bool isPlayerSide = gameObject.CompareTag("Player");
            BattleManager.Instance.OnUnitDead(isPlayerSide);
        }
        Destroy(gameObject);
    }

    void FindNearestTarget()
    {
        string targetTag = gameObject.CompareTag("Player") ? "Enemy" : "Player";
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);
        
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float d = Vector3.Distance(transform.position, enemy.transform.position);
            if (d < shortestDistance)
            {
                shortestDistance = d;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null) target = nearestEnemy.GetComponent<BattleUnit>();
    }

    void CreateHPBar()
    {
        if (hpBarPrefab == null) return;
        
        // 유닛의 자식으로 생성해서 따라다니게 함
        GameObject barObj = Instantiate(hpBarPrefab, transform); 
        
        // ★ 생성된 스크립트를 가져와서 변수에 저장!
        myHPBar = barObj.GetComponent<HPBar>();
        if (myHPBar != null) myHPBar.Setup(this);
    }
}