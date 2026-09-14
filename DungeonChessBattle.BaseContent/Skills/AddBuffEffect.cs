using DungeonChessBattle.Battle.Config.Shared.Buffs;
using DungeonChessBattle.Battle.Shared.Combat;

namespace DungeonChessBattle.BaseContent.Skills;

/// <summary>施加 Buff 的技能效果。</summary>
/// <param name="buff">释放时施加给目标的 Buff 定义。</param>
public sealed class AddBuffEffect(BuffDefinition buff) : ISkillEffect {
    /// <inheritdoc />
    public SkillResolution Resolve(SkillResolveContext ctx) {
        if (ctx.Target is not { } target)
            return SkillResolution.Empty;
        return new SkillResolution([], [new BuffToApply(buff, target.UnitId, ctx.Caster.Snapshot, ctx.Caster.UnitId)]);
    }
}
