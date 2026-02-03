using UnityEngine;

public class EnemySlime : BattleUnit
{
    protected override void Start()
    {

                // 슬라임 스탯
        maxHp = 50f;
        attackRange = 2.2f;
        moveSpeed = 1.5f; // 느림
        attackPower = 5f;
        
        base.Start();
    }

    protected override void Attack()
    {
        if(target != null)
        {
            
            target.TakeDamage(attackPower);
        }
    }
    // 슬라임이 죽을 때 분열하는 기능을 넣고 싶다면?
    // BattleUnit의 Die() 함수도 virtual로 만들고 여기서 override 하면 됨!
}