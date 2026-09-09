using System.Collections.Generic;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 基座 Buff 展示资源集合：<c>buffs/buff_set.tres</c> 的脚本类，条目在 Godot 编辑器拖拽维护。
/// 集合由总集合 <c>assets/display_set.tres</c> 汇总；新增 Buff 只加 .tres 并拖入集合，不必改 DisplayEntry。
/// </summary>
[GlobalClass]
public partial class BaseContentBuffSet : Resource {
    /// <summary>本 mod 的全部 Buff 展示资源。</summary>
    [Export]
    public Godot.Collections.Array<BaseContentBuffDisplay> BuffResources { get; set; } = [];

    /// <summary>把本集合条目转为注册用展示数据，跳过编辑器留空的槽位。</summary>
    public IEnumerable<BuffDisplay> ToBuffDisplays() {
        foreach (var data in BuffResources) {
            if (data is null)
                continue;
            yield return data.ToDisplay();
        }
    }
}
