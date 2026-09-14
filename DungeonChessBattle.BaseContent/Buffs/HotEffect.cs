using DungeonChessBattle.Battle.Shared.Buffs;
using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Events;
using DungeonChessBattle.BaseContent.Combat;

namespace DungeonChessBattle.BaseContent.Buffs;

/// <summary>持续治疗 HOT 效果。</summary>
/// <param name="healthPerSec">每秒治疗基础值。</param>
public sealed class HotEffect(float healthPerSec) : IBuffEffect {
    /// <inheritdoc />
    public IEnumerable<IBattleEvent> Tick(double elapsedSeconds, IBuffView instance, UnitSnapshot target) {
        if (instance.From is not { } from)
            yield break;

        float baseHps = healthPerSec * (float)elapsedSeconds;
        var result = HealProcessor.Process(from, target, baseHps);
        yield return new HealOccurred(instance.SourceUnitId, instance.TargetUnitId, result.ActualHeal);
    }
}
