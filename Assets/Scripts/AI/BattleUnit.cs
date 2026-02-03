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

    protected virtual void Start()
    {
        currentHp = maxHp;
        anim = GetComponent<Animator>();
        FindNearestTarget();
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
            Attack(); // 여기서 자식의 Attack()이 실행됩니다.
            lastAttackTime = Time.time;
        }
    }

    // ★ [수정] 자식이 override 할 수 있도록 virtual 추가!
    protected virtual void Attack()
    {
        Debug.Log($"{name}의 기본 공격! (데미지: {attackPower})");
        
        if (target != null)
        {
            target.TakeDamage(attackPower);
        }
    }

    // ★ [추가] 데미지 받는 함수도 필요합니다.
    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"{name}가 {damage} 피해를 입음! 남은 체력: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{name} 사망!");
        
        // BattleManager에게 사망 소식 알리기
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

        if (nearestEnemy != null)
        {
            target = nearestEnemy.GetComponent<BattleUnit>();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}