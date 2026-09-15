using DungeonChessBattle.Battle.Mod.Shared;
using DungeonChessBattle.Battle.Config.Shared.Buffs;
using DungeonChessBattle.Battle.Config.Shared.Combat;
using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Battle.Config.Shared.Content;
using DungeonChessBattle.Battle.Shared.Camp;
using DungeonChessBattle.Battle.Shared.ValueObjects;
using DungeonChessBattle.Battle.Config.Shared.Movement;
using DungeonChessBattle.Battle.Config.Shared.Range;

namespace DungeonChessBattle.BaseContent;

using CampRelationEnum = DungeonChessBattle.Battle.Shared.Camp.CampRelation;

/// <summary>
/// 内容注册器：把原本内置于主解决方案 Battle.Config.Registry 的单位/技能/Buff/副本定义整体迁出，
/// 行为实现随定义就地构造并注入，经 IModBootstrapContext 注册。注册次序即覆盖次序。
/// </summary>
public static class ContentRegistrar {
    /// <summary>全部内容注册入口。</summary>
    public static void RegisterAll(IModBootstrapContext context) {
        var buffs = RegisterBuffs(context);
        var skills = RegisterSkills(context, buffs);
        var units = RegisterUnits(context, skills);
        RegisterDungeons(context, units);
    }

    /// <summary>注册全部 Buff 定义，返回技能装配要引用的两个；其余只注册不入返回。</summary>
    private static (BuffDefinition DotMagic, BuffDefinition Hot) RegisterBuffs(IModBootstrapContext context) {
        var dotMagic = new BuffDefinition {
            BuffTypeId = "buff_dot_magic",
            Duration = 30.0,
            MaxStacks = 1,
            DamageType = DamageType.Magic,
            Effect = new Buffs.DotEffect(damagePerSec: 10.0f),
        };
        var dotPhysical = new BuffDefinition {
            BuffTypeId = "buff_dot_physical",
            Duration = 15.0,
            MaxStacks = 1,
            DamageType = DamageType.Physical,
            Effect = new Buffs.DotEffect(damagePerSec: 100.0f),
        };
        var hot = new BuffDefinition {
            BuffTypeId = "buff_hot",
            Duration = 15.0,
            MaxStacks = 1,
            Effect = new Buffs.HotEffect(healthPerSec: 100.0f),
        };

        context.RegisterBuff(dotMagic);
        context.RegisterBuff(dotPhysical);
        context.RegisterBuff(hot);
        return (dotMagic, hot);
    }
    /// <summary>注册全部技能定义，返回单位配置要引用的具名定义。</summary>
    private static SkillSet RegisterSkills(
        IModBootstrapContext context, (BuffDefinition DotMagic, BuffDefinition Hot) buffs) {
        var magicDamage = new SkillDefinition {
            SkillId = new SkillKeyId("skill_magic_damage"),
            SpellTime = 2.0f,
            CooldownTime = 0.0f,
            Gcd = GcdDefinition.Default,
            NeedUnitTarget = true,
            NeedPosTarget = false,
            TargetPolicy = SkillTargetPolicy.Different,
            CastRange = 10f,
            Effect = new Skills.DamageEffect(damage: 140.0f, damageType: DamageType.Magic),
        };
        var cure = new SkillDefinition {
            SkillId = new SkillKeyId("skill_cure"),
            SpellTime = 0.5f,
            CooldownTime = 0.0f,
            Gcd = GcdDefinition.Default,
            NeedUnitTarget = true,
            NeedPosTarget = false,
            TargetPolicy = SkillTargetPolicy.Same,
            CastRange = 8f,
            Effect = new Skills.HealEffect(curePotency: 500.0f),
        };
        var addDotMagic = new SkillDefinition {
            SkillId = new SkillKeyId("skill_add_dot_magic"),
            SpellTime = 0.0f,
            CooldownTime = 0.0f,
            Gcd = GcdDefinition.Default,
            NeedUnitTarget = true,
            NeedPosTarget = false,
            TargetPolicy = SkillTargetPolicy.Different,
            CastRange = 10f,
            Effect = new Skills.AddBuffEffect(buff: buffs.DotMagic),
        };
        var addHot = new SkillDefinition {
            SkillId = new SkillKeyId("skill_add_hot"),
            SpellTime = 0.0f,
            CooldownTime = 0.0f,
            Gcd = GcdDefinition.Default,
            NeedUnitTarget = true,
            NeedPosTarget = false,
            TargetPolicy = SkillTargetPolicy.Same,
            CastRange = 8f,
            Effect = new Skills.AddBuffEffect(buff: buffs.Hot),
        };
        var rectRangeDamage = new SkillDefinition {
            SkillId = new SkillKeyId("skill_rect_range_damage"),
            SpellTime = 2.0f,
            CooldownTime = 0.0f,
            Gcd = GcdDefinition.Default,
            NeedUnitTarget = false,
            NeedPosTarget = true,
            TargetPolicy = SkillTargetPolicy.Different,
            CastRange = null,
            CastArea = new RectShape { NearClamp = 0f, FarClamp = 5.0f },
            Effect = new Skills.RangeDamageEffect(damage: 200.0f, damageType: DamageType.Physical),
        };
        var taunt = new SkillDefinition {
            SkillId = new SkillKeyId("skill_taunt"),
            SpellTime = 0.0f,
            CooldownTime = 20.0f,
            Gcd = new GcdDefinition { GroupKey = null, Time = 2.5f },
            NeedUnitTarget = true,
            NeedPosTarget = false,
            TargetPolicy = SkillTargetPolicy.Different,
            CastRange = 10f,
            Effect = new Skills.HateSkillEffect(op: HateEffectOp.SetTop, value: 1000.0f),
        };

        foreach (var skill in new[] { magicDamage, cure, addDotMagic, addHot, rectRangeDamage, taunt })
            context.RegisterSkill(skill);

        return new SkillSet(magicDamage, cure, addDotMagic, addHot, rectRangeDamage, taunt);
    }
    /// <summary>注册全部单位配置，返回副本装配要引用的两个敌人单位。</summary>
    private static (UnitConfig Goblin, UnitConfig GoblinBoss) RegisterUnits(
        IModBootstrapContext context, SkillSet skills) {
        // 决策不持状态，三个单位共用同一实例
        var intelligence = new Intelligence.EnemyIntelligence();
        var whiteMage = new UnitConfig {
            ConfigKey = "WhiteMage",
            IsPlayerSelectable = true,
            BaseConfig = new UnitBaseConfig(
                MaxHealth: 1000f, BodyRadius: 0.5f, BaseSpeed: 2.0f,
                PhysicalAttackBase: 1.0f, PhysicalTakePercent: 1.0f,
                MagicAttackBase: 1.0f, MagicTakePercent: 1.0f, CureIntensity: 1.0f),
            Skills = [
                skills.AddHot,
                skills.Cure,
                skills.AddDotMagic,
                skills.MagicDamage,
                skills.RectRangeDamage,
                skills.Taunt,
            ],
            Intelligence = intelligence,
            HateRule = new Hates.DefaultHateRule(),
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
                skills.MagicDamage,
                skills.RectRangeDamage,
            ],
            Intelligence = intelligence,
            HateRule = new Hates.DefaultHateRule(),
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
                skills.AddDotMagic,
                skills.MagicDamage,
                skills.RectRangeDamage,
            ],
            Intelligence = intelligence,
            HateRule = new Hates.DefaultHateRule(),
            HateFactor = 1.0f,
        };

        context.RegisterUnit(whiteMage);
        context.RegisterUnit(goblin);
        context.RegisterUnit(goblinBoss);
        return (goblin, goblinBoss);
    }
    private static void RegisterDungeons(
        IModBootstrapContext context, (UnitConfig Goblin, UnitConfig GoblinBoss) units) {
        UnitConfig goblin = units.Goblin;
        UnitConfig goblinBoss = units.GoblinBoss;
        CampRelationResolver relations = CampRelationsPve;

        context.RegisterDungeon(new DungeonConfig(
            DungeonKey: "goblin_camp",
            PlayerCampOptions: [new PlayerCampOption("a", ["Camp_A"], SpawnBaseX: 0f, SpawnXSpacing: 3f)],
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
            PlayerCampOptions: [new PlayerCampOption("a", ["Camp_A"], SpawnBaseX: 0f, SpawnXSpacing: 3f)],
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

    /// <summary>本内容定义的 Boss 阵营标识：引擎不持阵营取值白名单，取值由内容包自持。</summary>
    private static readonly CampId BossCamp = new("Camp_BOSS");

    /// <summary>PvE 阵营关系：双方均含 Boss 阵营为友；任一方含 Boss 阵营即敌对；存在共同阵营为友；其余返回 Unknown。</summary>
    private static CampRelationEnum CampRelationsPve(
        IReadOnlyList<CampId> sourceCamps, IReadOnlyList<CampId> targetCamps) {
        bool sourceHasBoss = sourceCamps.Contains(BossCamp);
        bool targetHasBoss = targetCamps.Contains(BossCamp);

        if (sourceHasBoss && targetHasBoss)
            return CampRelationEnum.Friendly;
        if (sourceHasBoss || targetHasBoss)
            return CampRelationEnum.Enemy;

        if (sourceCamps.Any(camp => targetCamps.Contains(camp)))
            return CampRelationEnum.Friendly;
        return CampRelationEnum.Unknown;
    }

    /// <summary>本内容的技能装配结果：单位配置直接引用的具名定义。</summary>
    private sealed record SkillSet(
        SkillDefinition MagicDamage,
        SkillDefinition Cure,
        SkillDefinition AddDotMagic,
        SkillDefinition AddHot,
        SkillDefinition RectRangeDamage,
        SkillDefinition Taunt);
}
