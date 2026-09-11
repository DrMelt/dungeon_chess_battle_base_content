using System.Collections.Generic;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 基座展示资源总集合：<c>assets/display_set.tres</c> 的脚本类，汇总四类分类集合，
/// 并对外提供把全部条目转为注册用展示数据的 <c>To*Displays</c>。
/// 每类都是数组，同一类可拆成多个集合文件而不必改 DisplayEntry；
/// DisplayEntry 只装载本集合并逐条注册，不触碰分类集合与条目字段。
/// </summary>
[GlobalClass]
public partial class BaseContentDisplaySet : Resource {
    /// <summary>本 mod 的全部技能展示资源集合。</summary>
    [Export]
    public Godot.Collections.Array<BaseContentSkillSet> SkillSets { get; set; } = [];

    /// <summary>本 mod 的全部 Buff 展示资源集合。</summary>
    [Export]
    public Godot.Collections.Array<BaseContentBuffSet> BuffSets { get; set; } = [];

    /// <summary>本 mod 的全部单位展示资源集合。</summary>
    [Export]
    public Godot.Collections.Array<BaseContentUnitSet> UnitSets { get; set; } = [];

    /// <summary>本 mod 的全部副本展示资源集合。</summary>
    [Export]
    public Godot.Collections.Array<BaseContentDungeonSet> DungeonSets { get; set; } = [];

    /// <summary>汇总各技能集合的注册用展示数据，跳过编辑器留空的集合。</summary>
    public IEnumerable<SkillDisplay> ToSkillDisplays() {
        foreach (var set in SkillSets) {
            if (set is null)
                continue;
            foreach (var display in set.ToSkillDisplays())
                yield return display;
        }
    }

    /// <summary>汇总各 Buff 集合的注册用展示数据，跳过编辑器留空的集合。</summary>
    public IEnumerable<BuffDisplay> ToBuffDisplays() {
        foreach (var set in BuffSets) {
            if (set is null)
                continue;
            foreach (var display in set.ToBuffDisplays())
                yield return display;
        }
    }

    /// <summary>汇总各单位集合的注册用展示数据，跳过编辑器留空的集合。</summary>
    public IEnumerable<UnitDisplay> ToUnitDisplays() {
        foreach (var set in UnitSets) {
            if (set is null)
                continue;
            foreach (var display in set.ToUnitDisplays())
                yield return display;
        }
    }

    /// <summary>汇总各副本集合的注册用展示数据，跳过编辑器留空的集合。</summary>
    public IEnumerable<DungeonDisplay> ToDungeonDisplays() {
        foreach (var set in DungeonSets) {
            if (set is null)
                continue;
            foreach (var display in set.ToDungeonDisplays())
                yield return display;
        }
    }
}
