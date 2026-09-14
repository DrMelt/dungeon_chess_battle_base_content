using DungeonChessBattle.Battle.Shared.Buffs;
using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Events;
using DungeonChessBattle.BaseContent.Combat;

namespace DungeonChessBattle.BaseContent.Buffs;

/// <summary>持续伤害 DOT 效果。</summary>
/// <param name="damagePerSec">每秒伤害基础值。</param>
public sealed class DotEffect(float damagePerSec) : IBuffEffect {
    /// <inheritdoc />
    public IEnumerable<IBattleEvent> Tick(double elapsedSeconds, IBuffView instance, UnitSnapshot target) {
        if (instance.From is not { } from)
            yield break;

        float baseDps = damagePerSec * (float)elapsedSeconds;
        var result = DamageProcessor.Process(from, target, baseDps, instance.DamageType);
        yield return new DamageOccurred(
            instance.SourceUnitId, instance.TargetUnitId, result.AppliedDamage, instance.DamageType);
    }
}
