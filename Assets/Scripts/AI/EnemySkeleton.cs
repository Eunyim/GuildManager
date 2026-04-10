using UnityEngine;
using System.Collections.Generic;

public class EnemySkeleton : BattleUnit
{
    [Header("드랍 아이템 설정")]
    public List<DropItem> dropTable; 

    protected override void Start()
    {
        // 스켈레톤 스탯: 평균적이지만 죽지 않는 느낌
        maxHp = 120f;
        attackRange = 2.0f; 
        moveSpeed = 1.5f; 
        attackPower = 12f;
        attackCooldown = 1.5f;

        base.Start();
    }

    protected override void Attack()
    {
        if(target != null)
        {
            target.TakeDamage(attackPower);
            Debug.Log($"💀 [스켈레톤] 뼈를 깎는 공격! {target.name}에게 {attackPower} 데미지!");
        }
    }

    protected override void Die()
    {
        CheckDrop();
        base.Die();
    }

    void CheckDrop()
    {
        if (dropTable == null || dropTable.Count == 0) return; 

        foreach (DropItem loot in dropTable)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= loot.dropRate)
            {
                Debug.Log($"💎 스켈레톤에게서 [{loot.item.itemName}] 획득!");
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.AddItemToLootBag(loot.item);
                }
            }
        }
    }
}
