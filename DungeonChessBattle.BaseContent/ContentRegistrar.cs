using DungeonChessBattle.Battle.Mod.Shared;
using DungeonChessBattle.Battle.Shared;
using DungeonChessBattle.Battle.Shared.Buffs;
using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Combat.Hates;
using DungeonChessBattle.Battle.Shared.Content;
using DungeonChessBattle.Battle.Shared.Enums;
using DungeonChessBattle.Battle.Shared.Movement;
using DungeonChessBattle.Battle.Shared.Range;

namespace DungeonChessBattle.BaseContent;

using CampRelationEnum = DungeonChessBattle.Battle.Shared.Enums.CampRelation;

/// <summary>
/// 基座内容注册器：把原本内置于主解决方案 Battle.GameConfig 的单位/技能/Buff/副本定义
/// 与行为实现整体迁出，经 IModBootstrapContext 注册。注册次序即覆盖次序。
/// </summary>
public static class ContentRegistrar {
    /// <summary>全部内容注册入口。</summary>
    public static void RegisterAll(IModBootstrapContext context) {
        RegisterBehaviors(context);
        Dictionary<string, BuffDefinition> buffs = RegisterBuffs(context);
        Dictionary<string, SkillDefinition> skills = RegisterSkills(context, buffs);
        Dictionary<string, UnitConfig> units = RegisterUnits(context, skills);
        RegisterDungeons(context, units);
        context.SetDefaultDungeonKey("goblin_camp");
    }

    private static void RegisterBehaviors(IModBootstrapContext context) {
        context.RegisterSkillEffect(BehaviorIds.SkillEffect.Damage, static () => new Skills.DamageEffect());
        context.RegisterSkillEffect(BehaviorIds.SkillEffect.Heal, static () => new Skills.HealEffect());
        context.RegisterSkillEffect(BehaviorIds.SkillEffect.AddBuff, static () => new Skills.AddBuffEffect());
        context.RegisterSkillEffect(BehaviorIds.SkillEffect.Hate, static () => new Skills.HateSkillEffect());
        context.RegisterSkillEffect(BehaviorIds.SkillEffect.RangeDamage, static () => new Skills.RangeDamageEffect());

        context.RegisterBuffEffect(BehaviorIds.BuffEffect.Dot, static () => new Buffs.DotEffect());
        context.RegisterBuffEffect(BehaviorIds.BuffEffect.Hot, static () => new Buffs.HotEffect());

        context.RegisterIntelligence(BehaviorIds.Intelligence.EnemyBasic, static () => new Intelligence.EnemyIntelligence());

        context.RegisterHateRule(BehaviorIds.HateRule.Default, static () => new DefaultHateRule());

        context.RegisterCampRelation(BehaviorIds.CampRelation.PveBoss, CampRelationsPve);
    }

    private static Dictionary<string, BuffDefinition> RegisterBuffs(IModBootstrapContext context) {
        var dotMagic = new DamageOverTimeBuff {
            BuffTypeId = "buff_dot_magic",
            Duration = 30.0,
            MaxStacks = 1,
            DamageType = DamageType.Magic,
            DamagePerSec = 10.0f,
            Effect = context.BuffEffect(BehaviorIds.BuffEffect.Dot),
        };
        var dotPhysical = new DamageOverTimeBuff {
            BuffTypeId = "buff_dot_physical",
            Duration = 15.0,
            MaxStacks = 1,
            DamageType = DamageType.Physical,
            DamagePerSec = 100.0f,
            Effect = context.BuffEffect(BehaviorIds.BuffEffect.Dot),
        };
        var hot = new HealOverTimeBuff {
            BuffTypeId = "buff_hot",
            Duration = 15.0,
            MaxStacks = 1,
            HealthPerSec = 100.0f,
            Effect = context.BuffEffect(BehaviorIds.BuffEffect.Hot),
        };

        context.RegisterBuff(dotMagic);
        context.RegisterBuff(dotPhysical);
        context.RegisterBuff(hot);
        return new Dictionary<string, BuffDefinition>(StringComparer.Ordinal) {
            ["buff_dot_magic"] = dotMagic,
            ["buff_dot_physical"] = dotPhysical,
            ["buff_hot"] = hot,
        };
    }
    private static Dictionary<string, SkillDefinition> RegisterSkills(
        IModBootstrapContext context, Dictionary<string, BuffDefinition> buffs) {
        var skills = new Dictionary<string, SkillDefinition>(StringComparer.Ordinal) {
            ["skill_magic_damage"] = new DamageSkillDefinition {
                SkillId = new SkillKeyId("skill_magic_damage"),
                SpellTime = 2.0f,
                CooldownTime = 0.0f,
                Gcd = GcdDefinition.Default,
                NeedUnitTarget = true,
                NeedPosTarget = false,
                TargetPolicy = SkillTargetPolicy.Different,
                CastRange = 10f,
                Damage = 140.0f,
                DamageType = DamageType.Magic,
                Effect = context.SkillEffect(BehaviorIds.SkillEffect.Damage),
            },
            ["skill_cure"] = new HealSkillDefinition {
                SkillId = new SkillKeyId("skill_cure"),
                SpellTime = 0.5f,
                CooldownTime = 0.0f,
                Gcd = GcdDefinition.Default,
                NeedUnitTarget = true,
                NeedPosTarget = false,
                TargetPolicy = SkillTargetPolicy.Same,
                CastRange = 8f,
                CurePotency = 500.0f,
                Effect = context.SkillEffect(BehaviorIds.SkillEffect.Heal),
            },
            ["skill_add_dot_magic"] = new AddBuffSkillDefinition {
                SkillId = new SkillKeyId("skill_add_dot_magic"),
                SpellTime = 0.0f,
                CooldownTime = 0.0f,
                Gcd = GcdDefinition.Default,
                NeedUnitTarget = true,
                NeedPosTarget = false,
                TargetPolicy = SkillTargetPolicy.Different,
                CastRange = 10f,
                Buff = buffs["buff_dot_magic"],
                Effect = context.SkillEffect(BehaviorIds.SkillEffect.AddBuff),
            },
            ["skill_add_hot"] = new AddBuffSkillDefinition {
                SkillId = new SkillKeyId("skill_add_hot"),
                SpellTime = 0.0f,
                CooldownTime = 0.0f,
                Gcd = GcdDefinition.Default,
                NeedUnitTarget = true,
                NeedPosTarget = false,
                TargetPolicy = SkillTargetPolicy.Same,
                CastRange = 8f,
                Buff = buffs["buff_hot"],
                Effect = context.SkillEffect(BehaviorIds.SkillEffect.AddBuff),
            },
            ["skill_rect_range_damage"] = new RangeDamageSkillDefinition {
                SkillId = new SkillKeyId("skill_rect_range_damage"),
                SpellTime = 2.0f,
                CooldownTime = 0.0f,
                Gcd = GcdDefinition.Default,
                NeedUnitTarget = false,
                NeedPosTarget = true,
                TargetPolicy = SkillTargetPolicy.Different,
                CastArea = new RectShape { NearClamp = 0f, FarClamp = 5.0f },
                Damage = 200.0f,
                DamageType = DamageType.Physical,
                Effect = context.SkillEffect(BehaviorIds.SkillEffect.RangeDamage),
            },
            ["skill_taunt"] = new HateSkillDefinition {
                SkillId = new SkillKeyId("skill_taunt"),
                SpellTime = 0.0f,
                CooldownTime = 20.0f,
                Gcd = new GcdDefinition { GroupKey = null, Time = 2.5f },
                NeedUnitTarget = true,
                NeedPosTarget = false,
                TargetPolicy = SkillTargetPolicy.Different,
                CastRange = 10f,
                Op = HateEffectOp.SetTop,
                Value = 1000.0f,
                Effect = context.SkillEffect(BehaviorIds.SkillEffect.Hate),
            }
        };

        foreach (SkillDefinition skill in skills.Values)
            context.RegisterSkill(skill);
        return skills;
    }
    private static Dictionary<string, UnitConfig> RegisterUnits(
        IModBootstrapContext context, Dictionary<string, SkillDefinition> skills) {
        var whiteMage = new UnitConfig {
            ConfigKey = "WhiteMage",
            IsPlayerSelectable = true,
            BaseConfig = new UnitBaseConfig(
                MaxHealth: 1000f, BodyRadius: 0.5f, BaseSpeed: 2.0f,
                PhysicalAttackBase: 1.0f, PhysicalTakePercent: 1.0f,
                MagicAttackBase: 1.0f, MagicTakePercent: 1.0f, CureIntensity: 1.0f),
            Skills = [
                skills["skill_add_hot"],
                skills["skill_cure"],
                skills["skill_add_dot_magic"],
                skills["skill_magic_damage"],
                skills["skill_rect_range_damage"],
                skills["skill_taunt"],
            ],
            Intelligence = context.Intelligence(BehaviorIds.Intelligence.EnemyBasic),
            HateRule = context.HateRule(BehaviorIds.HateRule.Default),
            HateFactor = 0.8f,
        };
        var goblin = new UnitConfig {
            ConfigKey = "Goblin",
            IsPlayerSelectable = false,
            BaseConfig = new UnitBaseConfig(
                MaxHealth: 800f, BodyRadius: 0.5f, BaseSpeed: 2.2f,
                PhysicalAttackBase: 1.2f, PhysicalTakePercent: 1.0f,
                MagicAttackBase: 1.0f, MagicTakePercent: 1.0f, CureIntensity: 1.0f),
            Skills = [
                skills["skill_magic_damage"],
                skills["skill_rect_range_damage"],
            ],
            Intelligence = context.Intelligence(BehaviorIds.Intelligence.EnemyBasic),
            HateRule = context.HateRule(BehaviorIds.HateRule.Default),
            HateFactor = 1.0f,
        };
        var goblinBoss = new UnitConfig {
            ConfigKey = "GoblinBoss",
            IsPlayerSelectable = false,
            BaseConfig = new UnitBaseConfig(
                MaxHealth: 2000f, BodyRadius: 0.8f, BaseSpeed: 1.8f,
                PhysicalAttackBase: 1.5f, PhysicalTakePercent: 0.8f,
                MagicAttackBase: 1.3f, MagicTakePercent: 0.8f, CureIntensity: 1.0f),
            Skills = [
                skills["skill_add_dot_magic"],
                skills["skill_magic_damage"],
                skills["skill_rect_range_damage"],
            ],
            Intelligence = context.Intelligence(BehaviorIds.Intelligence.EnemyBasic),
            HateRule = context.HateRule(BehaviorIds.HateRule.Default),
            HateFactor = 1.0f,
        };

        context.RegisterUnit(whiteMage);
        context.RegisterUnit(goblin);
        context.RegisterUnit(goblinBoss);
        return new Dictionary<string, UnitConfig>(StringComparer.Ordinal) {
            [whiteMage.ConfigKey.Value] = whiteMage,
            [goblin.ConfigKey.Value] = goblin,
            [goblinBoss.ConfigKey.Value] = goblinBoss,
        };
    }
    private static void RegisterDungeons(
        IModBootstrapContext context, Dictionary<string, UnitConfig> units) {
        UnitConfig goblin = units["Goblin"];
        UnitConfig goblinBoss = units["GoblinBoss"];
        CampRelationResolver relations = context.CampRelation(BehaviorIds.CampRelation.PveBoss);

        context.RegisterDungeon(new DungeonConfig(
            DungeonKey: "goblin_camp",
            PlayerCampOptions: [new PlayerCampOption("a", ["Camp_A"])],
            Enemies: [
                new EnemySpawnConfig(goblin, Count: 3, SpawnBaseX: 30f, SpawnXSpacing: 3f),
                new EnemySpawnConfig(goblinBoss, Count: 1, SpawnBaseX: 42f, SpawnXSpacing: 0f),
            ],
            RelationsResolver: relations,
            EnemyCamps: ["Camp_BOSS"],
            Layout: new BattlefieldLayout(
                50f, 30f, [new ObstacleRect(14f, 9f, 18f, 11f)])));

        context.RegisterDungeon(new DungeonConfig(
            DungeonKey: "deep_cave",
            PlayerCampOptions: [new PlayerCampOption("a", ["Camp_A"])],
            Enemies: [
                new EnemySpawnConfig(goblin, Count: 5, SpawnBaseX: 28f, SpawnXSpacing: 2.5f),
                new EnemySpawnConfig(goblinBoss, Count: 1, SpawnBaseX: 44f, SpawnXSpacing: 0f),
            ],
            RelationsResolver: relations,
            EnemyCamps: ["Camp_BOSS"],
            Layout: new BattlefieldLayout(
                50f, 30f,
                [
                    new ObstacleRect(8f, -14f, 12f, -12f),
                    new ObstacleRect(18f, 8f, 21f, 14f),
                ])));
    }

    /// <summary>PvE 阵营关系：双方均含 Boss 阵营为友；任一方含 Boss 阵营即敌对；存在共同阵营为友；其余返回 Unknown。</summary>
    private static CampRelationEnum CampRelationsPve(
        IReadOnlyList<string> sourceCamps, IReadOnlyList<string> targetCamps) {
        bool sourceHasBoss = sourceCamps.Contains(CampConstants.CampBoss);
        bool targetHasBoss = targetCamps.Contains(CampConstants.CampBoss);

        if (sourceHasBoss && targetHasBoss)
            return CampRelationEnum.Friendly;
        if (sourceHasBoss || targetHasBoss)
            return CampRelationEnum.Enemy;

        if (sourceCamps.Any(camp => targetCamps.Contains(camp)))
            return CampRelationEnum.Friendly;
        return CampRelationEnum.Unknown;
    }
}
