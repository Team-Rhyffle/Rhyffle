/// <summary>
/// 카드 강화 시스템 — 화음 / 공명 (`카드 시스템` v2 2차 spec).
///
/// 규칙:
/// - 화음: 동일 Artist + 동일 Class + **다른** Member 의 카드로 강화
/// - 공명: 동일 Artist + 동일 Class + **동일** Member 의 카드로 강화
/// - 각 10단계까지 (CardInstance.MAX_ENHANCEMENT_LEVEL)
/// - 강화 실패 확률 0 (성공 보장)
/// - Harmony / Resonance level 은 독립적
///
/// Apply 메서드는 CardInstance 의 level 을 mutate. 호출자가 material 카드의
/// 소비/삭제 책임. (이 모듈은 인벤토리 자체 관리하지 않음)
/// </summary>
public static class CardEnhancement {
    public enum EnhanceType {
        Harmony,
        Resonance,
    }

    public enum EnhanceResult {
        Success,
        AtMaxLevel,
        IncompatibleMaterial,
    }

    /// <summary>화음 재료 호환성 검사. 동일 Artist + 동일 Class + 다른 Member.</summary>
    public static EnhanceResult ValidateHarmonyMaterial(CardMetadata target, CardMetadata material) {
        if (target == null || material == null)     return EnhanceResult.IncompatibleMaterial;
        if (target.Artist != material.Artist)       return EnhanceResult.IncompatibleMaterial;
        if (target.Class  != material.Class)        return EnhanceResult.IncompatibleMaterial;
        if (target.Member == material.Member)       return EnhanceResult.IncompatibleMaterial;
        return EnhanceResult.Success;
    }

    /// <summary>공명 재료 호환성 검사. 동일 Artist + 동일 Class + 동일 Member.</summary>
    public static EnhanceResult ValidateResonanceMaterial(CardMetadata target, CardMetadata material) {
        if (target == null || material == null)     return EnhanceResult.IncompatibleMaterial;
        if (target.Artist != material.Artist)       return EnhanceResult.IncompatibleMaterial;
        if (target.Class  != material.Class)        return EnhanceResult.IncompatibleMaterial;
        if (target.Member != material.Member)       return EnhanceResult.IncompatibleMaterial;
        return EnhanceResult.Success;
    }

    /// <summary>
    /// 화음 강화 시도. 호환성 + level cap 검사 통과 시 instance.HarmonyLevel++.
    /// 실패 시 instance 상태 변경 없음.
    /// </summary>
    public static EnhanceResult ApplyHarmony(CardInstance instance, CardMetadata target, CardMetadata material) {
        var v = ValidateHarmonyMaterial(target, material);
        if (v != EnhanceResult.Success) return v;
        if (instance.HarmonyLevel >= CardInstance.MAX_ENHANCEMENT_LEVEL) return EnhanceResult.AtMaxLevel;
        instance.HarmonyLevel++;
        return EnhanceResult.Success;
    }

    /// <summary>
    /// 공명 강화 시도. 호환성 + level cap 검사 통과 시 instance.ResonanceLevel++.
    /// 실패 시 instance 상태 변경 없음.
    /// </summary>
    public static EnhanceResult ApplyResonance(CardInstance instance, CardMetadata target, CardMetadata material) {
        var v = ValidateResonanceMaterial(target, material);
        if (v != EnhanceResult.Success) return v;
        if (instance.ResonanceLevel >= CardInstance.MAX_ENHANCEMENT_LEVEL) return EnhanceResult.AtMaxLevel;
        instance.ResonanceLevel++;
        return EnhanceResult.Success;
    }
}
