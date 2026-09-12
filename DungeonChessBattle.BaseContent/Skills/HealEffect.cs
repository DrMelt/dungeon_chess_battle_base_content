using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Events;
using DungeonChessBattle.BaseContent.Combat;

namespace DungeonChessBattle.BaseContent.Skills;

/// <summary>单体治疗技能效果。</summary>
/// <param name="curePotency">治疗基础值，经施法者治疗强度换算。</param>
public sealed class HealEffect(float curePotency) : ISkillEffect {
    /// <inheritdoc />
    public SkillResolution Resolve(SkillResolveContext ctx) {
        if (ctx.Target is not { } target)
            return SkillResolution.Empty;
        var result = HealProcessor.Process(ctx.Caster.Snapshot, target.Snapshot, curePotency);
        return new SkillResolution([new HealOccurred(ctx.Caster.UnitId, target.UnitId, result.ActualHeal)], []);
    }
}
