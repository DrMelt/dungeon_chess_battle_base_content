using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Events;

namespace DungeonChessBattle.BaseContent.Skills;

/// <summary>单体仇恨技能效果。</summary>
/// <param name="op">仇恨操作类型。</param>
/// <param name="value">仇恨操作数值。</param>
public sealed class HateSkillEffect(HateEffectOp op, float value) : ISkillEffect {
    /// <inheritdoc />
    public SkillResolution Resolve(SkillResolveContext ctx) {
        if (ctx.Target is not { } target)
            return SkillResolution.Empty;
        return new SkillResolution([new HateRequested(target.UnitId, ctx.Caster.UnitId, op, value)], []);
    }
}
