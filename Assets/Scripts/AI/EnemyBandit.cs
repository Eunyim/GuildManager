using UnityEngine;
using System.Collections.Generic;

public class EnemyBandit : BattleUnit
{
    [Header("드랍 아이템 설정")]
    public List<DropItem> dropTable; 

    protected override void Start()
    {
        // 도적단 스탯: 재빠르고 높은 데미지
        maxHp = 120f;
        attackRange = 1.5f; 
        moveSpeed = 3.5f; 
        attackPower = 15f;
        attackCooldown = 0.9f; 

        base.Start();
    }

    protected override void Attack()
    {
        if(target != null)
        {
            target.TakeDamage(attackPower);
            Debug.Log($"🗡️ [도적단] 기습! {target.name}에게 {attackPower} 데미지!");
        }
        else
        {
             Debug.LogWarning("⚠️ [도적단] 타겟이 없어 공격할 수 없습니다.");
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
                Debug.Log($"💎 도적단에게서 [{loot.item.itemName}] 획득!");
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.AddItemToLootBag(loot.item);
                }
            }
        }
    }
}
