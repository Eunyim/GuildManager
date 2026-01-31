using UnityEngine;

public class Ally_Archer : BattleUnit
{
   [Header("궁수 전용")]
    public GameObject arrowPrefab; // 화살 프리팹 연결 필요!

    protected override void Start()
    {
        base.Start();
        
        // 궁수 전용 스탯
        maxHp = 80f; // 체력 낮음
        currentHp = maxHp;
        attackRange = 6.0f; // 긴 사거리
        moveSpeed = 3.0f;
    }

    protected override void PerformAttack()
    {
        if(target == null || arrowPrefab == null) return;

        Debug.Log($"🏹 {unitName}의 화살 발사!");
        
        GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        
        Vector3 dir = (target.transform.position - transform.position).normalized;
        
        Projectile p = arrow.GetComponent<Projectile>();
        
        // 아군이면 적을, 적이면 아군을 타겟팅
        string targetTag = gameObject.CompareTag("Player") ? "Enemy" : "Player";
        p.Setup(dir, attackPower, targetTag);
    }
}