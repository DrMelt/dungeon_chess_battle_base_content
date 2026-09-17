using System.Numerics;
using DungeonChessBattle.Battle.Shared;
using DungeonChessBattle.Battle.Shared.Camp;
using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Control;

namespace DungeonChessBattle.BaseContent.Decisions;

/// <summary>
/// 默认敌人决策算法：为单个敌方单位产出当帧意图。
/// 决策只读 <see cref="IBattleUnitView"/> 接口；阵营关系由调用方按副本运行时注入，不绑定实例；
/// 施法可行性经 <see cref="IBattleSceneView.CanCast"/> 向战斗世界询问，裁定口径唯一在引擎侧；目标选择以仇恨为优先，无仇恨回退最近者；
/// 技能序取自运行时单位，逼近与否由可施放性与冷却状态派生，不重算射程。决策不持状态，实例可被多单位与多房间共享。
/// 死亡与读条中的单位由意图源拦下，本算法只处理可自由行动的时刻。
/// </summary>
public sealed class EnemyIntelligence : IUnitDecision {
    /// <inheritdoc />
    public UnitIntent Decide(IBattleUnitView self, IBattleSceneView scene, CampRelationResolver relations) {
        var target = SelectTarget(self, scene.Units, relations);
        if (target == null)
            return UnitIntent.Idle;

        // 按运行时技能序找首个可施放技能，锚点恒为已选目标当前位置
        Vector2 anchor = target.Snapshot.Position;
        bool anySkillStateReady = false;
        foreach (var skillKey in self.SkillKeys) {
            if (scene.CanCast(self.UnitId, skillKey, target.UnitId, anchor))
                return UnitIntent.CastSkill(skillKey, target.UnitId, anchor);

            // 冷却未就绪的技能靠近也无用；状态就绪却不可施放，缺口只剩目标或距离，构成逼近理由
            if (self.GetTotalCooldownRemaining(skillKey) <= 0f)
                anySkillStateReady = true;
        }

        return anySkillStateReady
            ? UnitIntent.MoveTo(anchor - self.Snapshot.Position)
            : UnitIntent.Idle;
    }

    /// <summary>选目标：存活敌对单位中仇恨最高者优先，全零仇恨回退距自身最近者。</summary>
    private static IBattleUnitView? SelectTarget(IBattleUnitView self, IReadOnlyList<IBattleUnitView> units,
        CampRelationResolver relations) {
        var selfPos = self.Snapshot.Position;
        IBattleUnitView? topTarget = null;
        float topHate = 0f;
        IBattleUnitView? nearest = null;
        float nearestDistanceSq = float.MaxValue;

        foreach (var candidate in units) {
            if (candidate == self || candidate.IsDead)
                continue;
            if (relations.Invoke(self.Camps, candidate.Camps) != CampRelation.Enemy)
                continue;

            float hate = self.HateOf(candidate.UnitId);
            if (hate > topHate) {
                topHate = hate;
                topTarget = candidate;
            }

            float distanceSq = Vector2.DistanceSquared(selfPos, candidate.Snapshot.Position);
            if (distanceSq < nearestDistanceSq) {
                nearestDistanceSq = distanceSq;
                nearest = candidate;
            }
        }

        return topTarget ?? nearest;
    }
}
