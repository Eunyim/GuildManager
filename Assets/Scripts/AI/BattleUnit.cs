using UnityEngine;

// 상태 목록
public enum UnitState 
{ 
    Idle, 
    Move, 
    Attack, 
    Dead 
}

// 성격 목록 (나중을 위해 미리 준비)
/*public enum TraitType 
{ 
    Normal, 
    Brave, 
    Coward, 
    Berserker 
}*/

public class BattleUnit : MonoBehaviour
{
    [Header("기본 스탯 (상속 가능)")]
    protected float maxHp = 100f;
    protected float currentHp;
    protected float attackPower = 10f;
    protected float attackRange = 2.0f;
    protected float moveSpeed = 3.0f;
    protected float attackSpeed = 1.0f; // 공격 속도

    [Header("상태 정보")]
    public UnitState currentState = UnitState.Idle;
    public TraitType myTrait = TraitType.Normal;
    public BattleUnit target;

    protected Rigidbody2D rb;
    protected float lastAttackTime;
    protected HPBar myHPBar; // HP바 연결
    protected string unitName;

    // 자식이 덮어쓸 수 있게 virtual로 선언
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHp = maxHp;
        unitName = gameObject.name;
        myHPBar = GetComponentInChildren<HPBar>(); // HP바 찾기

        FindNearestTarget();
    }

    protected virtual void Update()
    {
        if (currentState == UnitState.Dead) return;

        // FSM (상태 머신)
        switch (currentState)
        {
            case UnitState.Idle:
                if (target == null || target.currentState == UnitState.Dead) 
                    FindNearestTarget();
                else 
                    currentState = UnitState.Move;
                break;

            case UnitState.Move:
                MoveToTarget();
                break;

            case UnitState.Attack:
                rb.linearVelocity = Vector2.zero; // 멈춤
                
                if (target == null || target.currentState == UnitState.Dead)
                {
                    currentState = UnitState.Idle;
                    target = null;
                }
                else
                {
                    // 공격 쿨타임 체크
                    if (Time.time > lastAttackTime + (1f / attackSpeed))
                    {
                        PerformAttack(); // ★ 자식이 정의한 공격 실행
                    }
                }
                break;
        }
    }

    // ★ 자식이 덮어써야 하는 공격 함수 (기본은 빈 껍데기)
    protected virtual void PerformAttack()
    {
        // 자식 클래스(UnitWarrior 등)에서 이 부분을 구현함
    }

    // 공통 이동 로직
    protected void MoveToTarget()
    {
        if (target == null)
        {
            currentState = UnitState.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist <= attackRange)
        {
            currentState = UnitState.Attack;
        }
        else
        {
            Vector2 dir = (target.transform.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
            
            // 방향 전환 (좌우 반전)
            if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1);
            else transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // 공통 타겟 찾기 로직
    protected void FindNearestTarget()
    {
        string targetTag = "";
        
        if (gameObject.CompareTag("Player")) targetTag = "Enemy";
        else if (gameObject.CompareTag("Enemy")) targetTag = "Player";
        else return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);
        
        float closestDist = Mathf.Infinity;
        BattleUnit closestUnit = null;

        foreach (GameObject enemyObj in enemies)
        {
            BattleUnit unit = enemyObj.GetComponent<BattleUnit>();
            if (unit == null || unit.currentState == UnitState.Dead) continue;

            float dist = Vector3.Distance(transform.position, enemyObj.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestUnit = unit;
            }
        }

        if (closestUnit != null)
        {
            target = closestUnit;
        }
    }

    // 공통 데미지 처리
    public void TakeDamage(float damage)
    {
        if (currentState == UnitState.Dead) return;

        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;

        if (myHPBar != null) myHPBar.UpdateHP(currentHp, maxHp);
        Debug.Log($"🩸 {unitName} 피격! 남은 체력: {currentHp}/{maxHp}");

        if (currentHp == 0) Die();
        else StartCoroutine(FlashRed());
    }

    // 공통 사망 처리 (virtual로 만들어서 자식이 바꿀 수 있게 함)
    protected virtual void Die()
    {
        if (currentState == UnitState.Dead) return;

        currentState = UnitState.Dead;
        Debug.Log($"💀 {unitName} 사망.");

        if (BattleManager.Instance != null)
        {
            bool isPlayer = gameObject.CompareTag("Player");
            BattleManager.Instance.OnUnitDead(isPlayer);
        }

        GetComponent<Collider2D>().enabled = false;
        
        SpriteRenderer spr = GetComponent<SpriteRenderer>();
        if(spr != null) spr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        Destroy(gameObject, 3.0f);
    }

    protected System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer spr = GetComponent<SpriteRenderer>();
        if (spr != null)
        {
            Color original = spr.color;
            spr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spr.color = original; 
        }
    }
}