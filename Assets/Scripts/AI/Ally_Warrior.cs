using UnityEngine;

// BattleUnit을 상속받음 (: BattleUnit)
public class Ally_Warrior : BattleUnit
{
    protected override void Start()
    {
        base.Start(); // 부모의 Start 실행 (HP바 연결 등)
        
        // 전사 전용 스탯 설정
        maxHp = 150f; 
        currentHp = maxHp;
        attackRange = 1.5f; // 짧은 사거리
        moveSpeed = 3.5f;
    }

    protected override void PerformAttack()
    {
        if(target != null)
        {
            Debug.Log($"⚔️ {unitName}의 베기 공격!");
            target.TakeDamage(attackPower);
        }
    }
}