using UnityEngine;
using System.Collections.Generic;

public class EnemyGoblin : BattleUnit
{
    [Header("드랍 아이템 설정")]
    public List<DropItem> dropTable; 

    protected override void Start()
    {
        // 고블린 스탯: 빠르고 약함
        maxHp = 60f;
        attackRange = 1.8f; 
        moveSpeed = 3.0f; 
        attackPower = 8f;
        attackCooldown = 0.8f; // 공격 속도 빠름

        base.Start();
    }

    protected override void Attack()
    {
        if(target != null)
        {
            target.TakeDamage(attackPower);
            Debug.Log($"🔪 [고블린] 슥삭! {target.name}에게 {attackPower} 데미지!");
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
                Debug.Log($"💎 고블린에게서 [{loot.item.itemName}] 획득!");
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.AddItemToLootBag(loot.item);
                }
            }
        }
    }
}
