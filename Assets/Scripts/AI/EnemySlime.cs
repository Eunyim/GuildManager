using UnityEngine;

public class EnemySlime : BattleUnit
{
    protected override void Start()
    {
        base.Start();

        // 슬라임 스탯
        maxHp = 50f;
        attackRange = 1.5f;
        moveSpeed = 1.5f; // 느림
        attackPower = 5f;
    }

    protected override void PerformAttack()
    {
        if(target != null)
        {
            Debug.Log($"💧 {unitName}의 몸통 박치기!");
            target.TakeDamage(attackPower);
        }
    }
    // 슬라임이 죽을 때 분열하는 기능을 넣고 싶다면?
    // BattleUnit의 Die() 함수도 virtual로 만들고 여기서 override 하면 됨!
}