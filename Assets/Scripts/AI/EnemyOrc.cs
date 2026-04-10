using UnityEngine;
using System.Collections.Generic;

public class EnemyOrc : BattleUnit
{
    [Header("드랍 아이템 설정")]
    public List<DropItem> dropTable; 

    protected override void Start()
    {
        // 오크 스탯: 튼튼하고 강하지만 느림
        maxHp = 250f;
        attackRange = 2.5f; 
        moveSpeed = 1.2f; 
        attackPower = 25f;
        attackCooldown = 2.0f; // 공격 속도 느림

        base.Start();
    }

    protected override void Attack()
    {
        if(target != null)
        {
            target.TakeDamage(attackPower);
            Debug.Log($"🪓 [오크] 육중한 공격! {target.name}에게 {attackPower} 데미지!");
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
                Debug.Log($"💎 오크에게서 [{loot.item.itemName}] 획득!");
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.AddItemToLootBag(loot.item);
                }
            }
        }
    }
}
