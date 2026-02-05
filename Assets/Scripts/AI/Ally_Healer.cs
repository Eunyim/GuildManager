using UnityEngine;

public class Ally_Healer : BattleUnit
{
    // protected (부모와 맞춤)
    protected override void Start()
    {
        // 힐러 스탯
        maxHp = 80;
        attackRange = 4.0f; // 원거리 평타
        moveSpeed = 2.5f;
        attackPower = 10;   // 딜은 약함
        mpRegenOnHit = 25;  // 4대 때리면 힐 가능

        base.Start();
    }

    // protected (부모와 맞춤)
    protected override void Attack()
    {
        // 기본 공격은 적을 때림 (원거리 마법탄 느낌)
        base.Attack(); 
    }

    // ★ public (부모가 public이므로)
    public override void UseSkill()
    {
        Debug.Log($"✨ {name}의 치유 스킬 시전!");

        // 1. 가장 아픈 아군 찾기
        BattleUnit targetAlly = FindLowestHpAlly();

        if (targetAlly != null)
        {
            // 2. 힐량 계산 (공격력의 3배)
            float healAmount = attackPower * 3.0f;
            
            // 3. 체력 회복
            targetAlly.currentHp += healAmount;
            if (targetAlly.currentHp > targetAlly.maxHp) 
                targetAlly.currentHp = targetAlly.maxHp;

           

            Debug.Log($"{targetAlly.name}를 {healAmount}만큼 치유!");
        }

        // MP 소모
        currentMp = 0;
    }

    // [보조 함수] 가장 체력 비율이 낮은 아군 찾기
    BattleUnit FindLowestHpAlly()
    {
        // 나와 같은 편(Tag)을 모두 찾음
        GameObject[] allies = GameObject.FindGameObjectsWithTag(gameObject.tag);
        
        BattleUnit lowestUnit = null;
        float lowestRatio = 1.0f; // 100%

        foreach (GameObject allyObj in allies)
        {
            BattleUnit unit = allyObj.GetComponent<BattleUnit>();
            // 살아있는 유닛 중에서
            if (unit != null && unit.currentHp > 0)
            {
                float ratio = unit.currentHp / unit.maxHp;
                // 비율이 더 낮으면 갱신
                if (ratio < lowestRatio)
                {
                    lowestRatio = ratio;
                    lowestUnit = unit;
                }
            }
        }
        return lowestUnit;
    }
}