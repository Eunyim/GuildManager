using UnityEngine;

public static class AdventurerGenerator
{
    // 확률표 (D, C, B, A, S 순서)
    private static float[] probBasic   = { 60f, 30f, 9f,  1f,  0f }; 
    private static float[] probNormal  = { 30f, 40f, 20f, 9f,  1f }; 
    private static float[] probPremium = { 10f, 20f, 40f, 25f, 5f }; 

    // 음절 조합 이름 생성 (앞 × 뒤 = 180가지, 3음절 포함 시 1440가지+)
    private static readonly string[] NameFront = {
        "아", "카", "레", "미", "세", "리", "나", "에", "소", "타",
        "가", "로", "엘", "투", "라", "케", "발", "시", "오", "드"
    };
    private static readonly string[] NameMid = {
        "르", "미", "나", "레", "아", "에", "리", "드", "로", "스"
    };
    private static readonly string[] NameBack = {
        "린", "론", "셀", "엔", "드", "나", "온", "딘", "크", "벨",
        "인", "안", "스", "르", "브"
    };

    // 기존 모험가와 겹치지 않는 이름 생성 (최대 10회 재시도)
    private static string GenerateUniqueName()
    {
        for (int i = 0; i < 10; i++)
        {
            string candidate = GenerateName();
            bool duplicate = false;

            if (GameManager.Instance != null)
                foreach (Adventurer adv in GameManager.Instance.adventurers)
                    if (adv.name == candidate) { duplicate = true; break; }

            if (!duplicate) return candidate;
        }
        // 10회 시도 후에도 겹치면 그냥 반환 (사실상 발생 불가)
        return GenerateName();
    }

    private static string GenerateName()
    {
        string front = NameFront[Random.Range(0, NameFront.Length)];
        string back  = NameBack[Random.Range(0, NameBack.Length)];

        // 30% 확률로 중간 음절 추가 (3음절 이름)
        if (Random.value < 0.3f)
        {
            string mid = NameMid[Random.Range(0, NameMid.Length)];
            return front + mid + back;
        }

        return front + back;
    }

    // ★ 여기에 tier 매개변수가 확실히 있어야 함!
    public static Adventurer Generate(SummonTier tier)
    {
        Adventurer newAdv = new Adventurer();

        newAdv.name = GenerateUniqueName();
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