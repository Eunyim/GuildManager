using UnityEngine;
using System.Collections.Generic;

public class EnemyDarkMage : BattleUnit
{
    [Header("다크메이지 전용")]
    public GameObject fireballPrefab; // 파이어볼 프리팹 (Fireball 컴포넌트 필요)

    [Header("드랍 아이템 설정")]
    public List<DropItem> dropTable; 

    protected override void Start()
    {
        // 다크메이지 스탯: 원거리, 체력은 낮지만 데미지 높음
        maxHp = 100f;
        attackRange = 8.0f; // 긴 사거리
        moveSpeed = 1.8f; 
        attackPower = 20f;
        attackCooldown = 2.5f; // 긴 캐스팅 시간

        base.Start();
    }

    protected override void Attack()
    {
        if (target == null) return;
        
        Debug.Log($"🔥 [다크메이지] 암흑 화염구 발사!");

        if (fireballPrefab != null)
        {
            GameObject fbObj = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            Fireball fb = fbObj.GetComponent<Fireball>();
            if (fb != null)
            {
                fb.Setup(target.transform.position, attackPower, target.tag);
            }
        }
        else
        {
            // 프리팹이 없을 경우를 대비한 직접 데미지 (안전 장치)
            target.TakeDamage(attackPower);
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
                Debug.Log($"💎 다크메이지에게서 [{loot.item.itemName}] 획득!");
                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.AddItemToLootBag(loot.item);
                }
            }
        }
    }
}
