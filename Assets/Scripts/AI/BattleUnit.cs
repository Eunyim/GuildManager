using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [Header("기본 스탯 (자식에서 덮어씌움)")]
    public float maxHp = 100;
    public float currentHp;
    public float attackRange = 1.5f; 
    public float moveSpeed = 2.0f;
    public float attackPower = 10f;
    public float attackCooldown = 1.0f;

    protected BattleUnit target; 
    protected Animator anim;     
    protected float lastAttackTime; 

    [Header("UI 설정")]
    public GameObject hpBarPrefab;

    // ★ [중요] 이 함수가 있어야 BattleManager가 데이터를 꽂아줍니다.
    public void Initialize(Adventurer data)
    {
        maxHp = data.hp;
        currentHp = maxHp;
        attackPower = data.atk;
        
        // 이름 변경 (게임 오브젝트 이름도 바꾸기)
        name = $"Unit_{data.name}";

        // 만약 HP바가 이미 있다면 갱신 로직 필요 (여기선 생략)
    }

    protected virtual void Start()
    {
        if (currentHp <= 0) currentHp = maxHp;
        anim = GetComponent<Animator>();
        FindNearestTarget();
        CreateHPBar();
    }

    protected virtual void Update()
    {
        if (target == null || target.currentHp <= 0)
        {
            FindNearestTarget();
            return; 
        }

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
            Attack(); 
            lastAttackTime = Time.time;
        }
    }

    protected virtual void Attack()
    {
        Debug.Log($"{name}의 기본 공격! (데미지: {attackPower})");
        if (target != null) target.TakeDamage(attackPower);
    }

    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"{name}가 {damage} 피해를 입음! 남은 체력: {currentHp}");

        if (currentHp <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"{name} 사망!");
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
        GameObject barObj = Instantiate(hpBarPrefab);
        HPBar hpScript = barObj.GetComponent<HPBar>();
        if (hpScript != null) hpScript.Setup(this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}