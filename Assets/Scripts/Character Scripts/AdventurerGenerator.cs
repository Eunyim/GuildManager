using UnityEngine;

public static class AdventurerGenerator
{
    // 뽑기 등급별 랭크 확률표 (D, C, B, A, S 순서)
    private static float[] probBasic   = { 60f, 30f, 9f,  1f,  0f };
    private static float[] probNormal  = { 30f, 40f, 20f, 9f,  1f };
    private static float[] probPremium = { 10f, 20f, 40f, 25f, 5f };

    // ─── 이름 생성 ────────────────────────────────────────────────────

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
        return GenerateName();
    }

    private static string GenerateName()
    {
        string front = NameFront[Random.Range(0, NameFront.Length)];
        string back  = NameBack[Random.Range(0, NameBack.Length)];

        if (Random.value < 0.3f)
        {
            string mid = NameMid[Random.Range(0, NameMid.Length)];
            return front + mid + back;
        }
        return front + back;
    }

    // ─── 메인 생성 함수 ───────────────────────────────────────────────

    public static Adventurer Generate(SummonTier tier)
    {
        Adventurer adv = new Adventurer();

        adv.name  = GenerateUniqueName();
        adv.job   = (JobType)Random.Range(0, System.Enum.GetValues(typeof(JobType)).Length);
        adv.trait = (TraitType)Random.Range(0, System.Enum.GetValues(typeof(TraitType)).Length);
        adv.rank  = DetermineRank(tier);

        // 랭크에 따른 시작 레벨 결정
        adv.level = DetermineStartingLevel(adv.rank);

        // Lv.1 기준 스탯 계산
        ApplyBaseStats(adv);

        // Lv.2 이상이면 레벨업 스탯 추가 적용
        for (int lv = 2; lv <= adv.level; lv++)
            ApplyLevelUpStats(adv);

        // 경험치 초기화 (현재 레벨 기준)
        adv.exp           = 0;
        adv.expToNextLevel = adv.level * 100;

        // 체력 완전 회복
        adv.currentHp = adv.hp;

        // 고랭크 모험가는 시작 특성 보유 가능
        TryGrantStartingTrait(adv);

        return adv;
    }

    // ─── 랭크 결정 ───────────────────────────────────────────────────

    private static RankType DetermineRank(SummonTier tier)
    {
        float[] probs = probBasic;
        if (tier == SummonTier.Normal)   probs = probNormal;
        else if (tier == SummonTier.Premium) probs = probPremium;

        float total = 0;
        foreach (float p in probs) total += p;

        float roll = Random.Range(0, total);
        float sum  = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            sum += probs[i];
            if (roll <= sum) return (RankType)i;
        }
        return RankType.D;
    }

    // ─── 시작 레벨 (랭크별) ──────────────────────────────────────────

    private static int DetermineStartingLevel(RankType rank)
    {
        switch (rank)
        {
            case RankType.D: return 1;
            case RankType.C: return 1;
            case RankType.B: return Random.Range(2, 4);  // 2~3
            case RankType.A: return Random.Range(4, 6);  // 4~5
            case RankType.S: return Random.Range(6, 9);  // 6~8
            default:         return 1;
        }
    }

    // ─── Lv.1 기준 스탯 ──────────────────────────────────────────────

    private static void ApplyBaseStats(Adventurer adv)
    {
        float baseHp, baseAtk, baseSpeed;

        switch (adv.job)
        {
            case JobType.Warrior: baseHp = 150f; baseAtk = 12f; baseSpeed = 2.0f; break;
            case JobType.Archer:  baseHp = 80f;  baseAtk = 18f; baseSpeed = 2.5f; break;
            case JobType.Mage:    baseHp = 70f;  baseAtk = 25f; baseSpeed = 2.0f; break;
            case JobType.Rogue:   baseHp = 60f;  baseAtk = 20f; baseSpeed = 3.0f; break;
            case JobType.Healer:  baseHp = 90f;  baseAtk = 8f;  baseSpeed = 2.2f; break;
            default:              baseHp = 100f; baseAtk = 10f; baseSpeed = 2.0f; break;
        }

        float rankMult  = 1.0f + (int)adv.rank * 0.5f;
        float randomVar = Random.Range(0.9f, 1.1f);

        adv.hp    = Mathf.RoundToInt(baseHp  * rankMult * randomVar);
        adv.atk   = Mathf.RoundToInt(baseAtk * rankMult * randomVar);
        adv.speed = baseSpeed;
    }

    // ─── 레벨당 스탯 성장 (GameManager.LevelUp과 동일 공식) ──────────

    private static void ApplyLevelUpStats(Adventurer adv)
    {
        adv.hp  += GetHpGain(adv.job, adv.rank);
        adv.atk += GetAtkGain(adv.job, adv.rank);
    }

    private static int GetHpGain(JobType job, RankType rank)
    {
        float base_;
        switch (job)
        {
            case JobType.Warrior: base_ = 15f; break;
            case JobType.Healer:  base_ = 9f;  break;
            case JobType.Archer:  base_ = 8f;  break;
            case JobType.Mage:    base_ = 7f;  break;
            case JobType.Rogue:   base_ = 6f;  break;
            default:              base_ = 10f; break;
        }
        return Mathf.RoundToInt(base_ * (1f + (int)rank * 0.2f));
    }

    private static int GetAtkGain(JobType job, RankType rank)
    {
        float base_;
        switch (job)
        {
            case JobType.Mage:    base_ = 5f; break;
            case JobType.Archer:  base_ = 4f; break;
            case JobType.Rogue:   base_ = 4f; break;
            case JobType.Warrior: base_ = 3f; break;
            case JobType.Healer:  base_ = 2f; break;
            default:              base_ = 3f; break;
        }
        return Mathf.RoundToInt(base_ * (1f + (int)rank * 0.2f));
    }

    // ─── 고랭크 시작 특성 ─────────────────────────────────────────────

    // 랭크별 긍정 특성 시작 확률: B=30%, A=60%, S=100%
    private static void TryGrantStartingTrait(Adventurer adv)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.positiveTraitPool == null ||
            GameManager.Instance.positiveTraitPool.Count == 0) return;

        float chance;
        switch (adv.rank)
        {
            case RankType.B: chance = 0.30f; break;
            case RankType.A: chance = 0.60f; break;
            case RankType.S: chance = 1.00f; break;
            default: return; // D, C는 시작 특성 없음
        }

        if (Random.value < chance)
        {
            var pool = GameManager.Instance.positiveTraitPool;
            TraitData trait = pool[Random.Range(0, pool.Count)];
            if (!adv.traits.Contains(trait))
            {
                adv.traits.Add(trait);
                Debug.Log($"✨ [{adv.name}] 시작 특성 '{trait.traitName}' 보유 (Rank {adv.rank})");
            }
        }
    }
}
