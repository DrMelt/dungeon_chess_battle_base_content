using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Events;
using DungeonChessBattle.BaseContent.Combat;

namespace DungeonChessBattle.BaseContent.Skills;

/// <summary>单体伤害技能效果。</summary>
/// <param name="damage">伤害基础值，经施法者攻击系数换算。</param>
/// <param name="damageType">伤害类型。</param>
public sealed class DamageEffect(float damage, DamageType damageType) : ISkillEffect {
    /// <inheritdoc />
    public SkillResolution Resolve(SkillResolveContext ctx) {
        if (ctx.Target is not { } target)
            return SkillResolution.Empty;
        var result = DamageProcessor.Process(ctx.Caster.Snapshot, target.Snapshot, damage, damageType);
        return new SkillResolution(
            [new DamageOccurred(ctx.Caster.UnitId, target.UnitId, result.AppliedDamage, damageType)], []);
    }
}
