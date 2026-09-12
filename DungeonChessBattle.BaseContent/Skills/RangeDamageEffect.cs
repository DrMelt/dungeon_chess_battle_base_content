using System.Numerics;
using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Events;
using DungeonChessBattle.BaseContent.Combat;

namespace DungeonChessBattle.BaseContent.Skills;

/// <summary>范围伤害技能效果：遍历战斗世界预过滤的可作用目标，按定义的形状过滤后产伤害事件。</summary>
/// <param name="damage">伤害基础值。</param>
/// <param name="damageType">伤害类型。</param>
public sealed class RangeDamageEffect(float damage, DamageType damageType) : ISkillEffect {
    /// <inheritdoc />
    public SkillResolution Resolve(SkillResolveContext ctx) {
        if (ctx.Skill.CastArea is not { } area)
            return SkillResolution.Empty;

        var aim = (ctx.TargetPos ?? Vector2.Zero) - ctx.Caster.Snapshot.Position;
        var events = new List<IBattleEvent>();
        foreach (var unit in ctx.Targets) {
            if (!area.Contains(unit.Snapshot.Position, ctx.Caster.Snapshot.Position, aim, unit.Snapshot.BodyRadius))
                continue;
            var result = DamageProcessor.Process(ctx.Caster.Snapshot, unit.Snapshot, damage, damageType);
            events.Add(new DamageOccurred(ctx.Caster.UnitId, unit.UnitId, result.AppliedDamage, damageType));
        }
        return new SkillResolution(events, []);
    }
}
