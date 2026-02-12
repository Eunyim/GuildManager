using UnityEngine;

public class Ally_Mage : BattleUnit
{
    [Header("마법사 전용")]
    public GameObject fireballPrefab; 

    protected override void Start()
    {
        maxHp = 60; 
        maxMp = 80;  
        attackRange = 5.5f; 
        moveSpeed = 2.0f;
        attackPower = 10f; 
        mpRegenOnHit = 20; 

        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
    }

    protected override void UseSkill()
    {
        Debug.Log($"🔥 {name}의 메테오 스트라이크!");

        if (target == null || fireballPrefab == null) return;

        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        
        Fireball proj = fireball.GetComponent<Fireball>();
        if (proj != null)
        {
            // ★ [수정됨] 3번째 재료인 target.tag를 넣어줘서 에러 해결!
            proj.Setup(target.transform.position, attackPower * 2.5f, target.tag);
        }

        currentMp = 0;
    }
}