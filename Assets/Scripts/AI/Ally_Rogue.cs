using UnityEngine;

public class Ally_Rogue : BattleUnit
{
    // 부모가 protected이므로 여기도 protected
    protected override void Start()
    {
        maxHp = 70;
        attackRange = 2.3f;
        moveSpeed = 4.5f;   
        attackCooldown = 0.5f; 
        mpRegenOnHit = 15;

        base.Start(); 
    }

    // 부모가 protected이므로 여기도 protected
    protected override void Attack()
    {
        bool isCritical = Random.value < 0.3f; 
        float finalDamage = isCritical ? attackPower * 2.0f : attackPower;

        if (target != null)
        {
            target.TakeDamage(finalDamage);
        }

        currentMp += mpRegenOnHit;
        if (currentMp > maxMp) currentMp = maxMp;
    }

    // ★ [수정] 부모(BattleUnit)가 public이므로 여기도 public이어야 함!
    protected override void UseSkill()
    {
        Debug.Log($"🗡️ {name}의 암살 스킬!");
        if (target != null)
        {
            target.TakeDamage(attackPower * 3.0f);
        }
        currentMp = 0;
    }
}