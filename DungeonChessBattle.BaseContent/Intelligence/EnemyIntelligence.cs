using System.Numerics;
using DungeonChessBattle.Battle.Shared;
using DungeonChessBattle.Battle.Config.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Camp;
using DungeonChessBattle.Battle.Shared.Intelligence;

namespace DungeonChessBattle.BaseContent.Intelligence;

/// <summary>
/// 默认敌人决策模块：为单个敌方单位生成当帧动作意图。
/// 纯函数式决策，依赖 <see cref="IBattleUnitView"/> 只读接口；阵营关系由调用方按副本运行时注入，不绑定实例；
/// 施法可行性经 <see cref="IBattleSceneView.CanCast"/> 向战斗世界询问，裁定口径唯一在引擎侧；目标选择以仇恨为优先，无仇恨回退最近者；
/// 技能集取自运行时单位的技能键，射程与目标策略按技能键在内容侧技能表解析，不写魔数。
/// </summary>
/// <param name="approachRange">停靠距离：技能都没声明射程时按它逼近，由内容显式给出。</param>
/// <param name="skills">内容侧技能表，按技能键解析规则；只读查表，不是单位的技能来源。</param>
public sealed class EnemyIntelligence(
    float approachRange,
    IReadOnlyDictionary<SkillKeyId, SkillDefinition> skills) : IUnitIntelligence {
    private readonly float _approachRange = approachRange;
    private readonly IReadOnlyDictionary<SkillKeyId, SkillDefinition> _skills = skills;

    /// <inheritdoc />
    public EnemyDecision Decide(IBattleUnitView self, IBattleSceneView scene, CampRelationResolver relations) {
        // 正在读条：原地等待读条完成，避免移动打断自身读条
        if (self.SkillCasting != default)
            return EnemyDecision.Idle();

        var target = SelectTarget(self, scene.Units, relations);
        if (target == null)
            return EnemyDecision.Idle();

        float distance = Vector2.Distance(self.Snapshot.Position, target.Snapshot.Position);
        if (distance > ApproachRange(self))
            return EnemyDecision.MoveTo(target.Snapshot.Position - self.Snapshot.Position);

        // 已进入停靠距离：按运行时技能序找首个可施放技能，锚点恒为已选目标当前位置
        foreach (var skillKey in self.SkillKeys) {
            Vector2 anchor = target.Snapshot.Position;
            if (!scene.CanCast(self.UnitId, skillKey, target.UnitId, anchor))
                continue;
            return EnemyDecision.Cast(skillKey, target.UnitId, anchor);
        }

        return EnemyDecision.Idle();
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

    /// <summary>
    /// 停靠距离：敌方目标技能中最远射程，属 AI 逼近偏好而非施法权威判定。
    /// 单位目标技能取 CastRange，位置目标技能取形状 FarReach，由技能数据配置；
    /// 技能都没声明射程时取构造时内容给出的停靠距离。
    /// </summary>
    private float ApproachRange(IBattleUnitView self) {
        float range = 0f;
        foreach (var skillKey in self.SkillKeys) {
            if (!_skills.TryGetValue(skillKey, out var skill))
                continue;
            if (!skill.TargetPolicy.HasFlag(SkillTargetPolicy.Different))
                continue;

            float? reach = skill.CastArea is { } area ? area.FarReach : skill.CastRange;
            if (reach is { } value && value > range)
                range = value;
        }
        return range > 0f ? range : _approachRange;
    }
}
