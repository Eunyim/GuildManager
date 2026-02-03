using UnityEngine;

// BattleUnit을 상속받음 (: BattleUnit)
public class Ally_Warrior : BattleUnit
{
    protected override void Start()
    {

                // 전사 전용 스탯 설정
        maxHp = 150f; 
        currentHp = maxHp;
        attackRange = 2.2f; // 짧은 사거리
        moveSpeed = 3.5f;
        
        base.Start(); // 부모의 Start 실행 (HP바 연결 등)
        
    }

    protected override void Attack()
    {
        if(target != null)
        {
            Debug.Log($"⚔️ {name}의 베기 공격!");
            target.TakeDamage(attackPower);
        }
    }
}