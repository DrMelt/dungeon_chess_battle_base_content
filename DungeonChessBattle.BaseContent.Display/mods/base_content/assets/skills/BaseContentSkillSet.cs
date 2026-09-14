using System.Collections.Generic;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 技能展示资源集合：<c>skills/skill_set.tres</c> 的脚本类，条目在 Godot 编辑器拖拽维护。
/// 集合由总集合 <c>assets/display_set.tres</c> 汇总；新增技能只加 .tres 并拖入集合，不必改 DisplayEntry。
/// </summary>
[GlobalClass]
public partial class BaseContentSkillSet : Resource {
    /// <summary>本 mod 的全部技能展示资源。</summary>
    [Export]
    public Godot.Collections.Array<BaseContentSkillDisplay> SkillResources { get; set; } = [];

    /// <summary>把本集合条目转为注册用展示数据，跳过编辑器留空的槽位。</summary>
    public IEnumerable<SkillDisplay> ToSkillDisplays() {
        foreach (var data in SkillResources) {
            if (data is null)
                continue;
            yield return data.ToDisplay();
        }
    }
}
