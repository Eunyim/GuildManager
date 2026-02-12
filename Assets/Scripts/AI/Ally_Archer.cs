using UnityEngine;

public class Ally_Archer : BattleUnit
{
   [Header("궁수 전용")]
    public GameObject arrowPrefab; // 화살 프리팹 연결 필요!

    protected override void Start()
    {

        // 궁수 전용 스탯
        maxHp = 80f; // 체력 낮음
        currentHp = maxHp;
        attackRange = 6.0f; // 긴 사거리
        moveSpeed = 3.0f;
        
        base.Start();
        
    }

    protected override void Attack()
    {
        if (target == null) return;
        
        // 화살 생성
        GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        Arrow proj = arrow.GetComponent<Arrow>();
        
        if (proj != null)
        {
            // 투사체 세팅 (위치, 데미지, 타겟 태그)
            proj.Setup(target.transform.position, attackPower, target.tag);
        }
        
        // MP 회복
        currentMp += mpRegenOnHit;
    }
}