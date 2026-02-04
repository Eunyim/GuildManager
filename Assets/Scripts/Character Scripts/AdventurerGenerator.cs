using UnityEngine;

public static class AdventurerGenerator
{
    // 확률표 (D, C, B, A, S 순서)
    private static float[] probBasic   = { 60f, 30f, 9f,  1f,  0f }; 
    private static float[] probNormal  = { 30f, 40f, 20f, 9f,  1f }; 
    private static float[] probPremium = { 10f, 20f, 40f, 25f, 5f }; 

      private static string[] names = { 

        "아서", "란슬롯", "가웨인", "멀린", "헤라클레스", 

        "지크", "잔느", "쿠훌린", "길가메쉬", "이스칸달",

        "토르", "로키", "오딘", "프레이야", "펜릴"

    };

    // ★ 여기에 tier 매개변수가 확실히 있어야 함!
    public static Adventurer Generate(SummonTier tier)
    {
        Adventurer newAdv = new Adventurer();

        newAdv.name = names[Random.Range(0, names.Length)];
        newAdv.job = (JobType)Random.Range(0, System.Enum.GetValues(typeof(JobType)).Length);
        newAdv.trait = (TraitType)Random.Range(0, System.Enum.GetValues(typeof(TraitType)).Length);

        // 등급 결정
        newAdv.rank = DetermineRank(tier);

        // 스탯 계산
        CalculateStats(newAdv);

        return newAdv;
    }

    private static RankType DetermineRank(SummonTier tier)
    {
        float[] probs = probBasic; 
        if (tier == SummonTier.Normal) probs = probNormal;
        else if (tier == SummonTier.Premium) probs = probPremium;

        float total = 0;
        foreach (float p in probs) total += p;

        float randomPoint = Random.Range(0, total);
        float currentSum = 0;

        for (int i = 0; i < probs.Length; i++)
        {
            currentSum += probs[i];
            if (randomPoint <= currentSum)
            {
                // 0=D, 1=C, 2=B, 3=A, 4=S
                return (RankType)i; 
            }
        }
        return RankType.D;
    }

    private static void CalculateStats(Adventurer adv)
    {
        float baseHp = 100;
        float baseAtk = 10;
        // speed가 Adventurer에 없다면 주석 처리
        // float baseSpeed = 2.0f;

        switch (adv.job)
        {
            case JobType.Warrior: baseHp = 150; baseAtk = 12; break;
            case JobType.Archer:  baseHp = 80;  baseAtk = 18; break;
            case JobType.Mage:    baseHp = 70;  baseAtk = 25; break;
            case JobType.Rogue:   baseHp = 60;  baseAtk = 20; break;
            case JobType.Healer:  baseHp = 90;  baseAtk = 8;  break;
        }

        int rankIndex = (int)adv.rank; 
        float rankMultiplier = 1.0f + (rankIndex * 0.5f); 
        float randomVar = Random.Range(0.9f, 1.1f);

        adv.hp = Mathf.RoundToInt(baseHp * rankMultiplier * randomVar);
        adv.atk = Mathf.RoundToInt(baseAtk * rankMultiplier * randomVar);
    }
}