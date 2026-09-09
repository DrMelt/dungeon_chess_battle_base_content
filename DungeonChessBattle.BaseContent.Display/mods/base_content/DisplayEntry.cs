using DungeonChessBattle.Game.Mod.Interface;
using DungeonChessBattle.Game.Mod.Shared;
using Godot;

namespace DungeonChessBattle.BaseContent.Display.mods.base_content;

/// <summary>
/// 基座 mod 展示入口：展示数据（名称/描述/图标/场景资源名）全部落在本工程
/// <c>mods/base_content/assets</c> 的 .tres 资源文件，经 Godot 编辑器配置。
/// 装载清单是总集合 <c>assets/display_set.tres</c>，本入口只装载它、取它转好的注册用展示数据逐条注册，
/// 字段映射与场景名解析都在资源类一侧，不在此处。
/// 总集合缺失即本 mod 不注册任何条目，占位补全由装配侧兜底；某类集合缺失只丢该类条目。
/// </summary>
public sealed class DisplayEntry : IModDisplayEntry {
    private const string DisplaySetPath = "assets/display_set.tres";

    public void Initialize(IModDisplayRuntime runtime, ModDisplayContext context) {
        if (LoadData<BaseContentDisplaySet>(context, DisplaySetPath) is not { } sets)
            return;

        foreach (var display in sets.ToSkillDisplays(context.Registry))
            runtime.RegisterSkill(display);
        foreach (var display in sets.ToBuffDisplays())
            runtime.RegisterBuff(display);
        foreach (var display in sets.ToUnitDisplays())
            runtime.RegisterUnit(display);
        foreach (var display in sets.ToDungeonDisplays(context.Registry))
            runtime.RegisterDungeon(display);
    }

    private static T? LoadData<T>(ModDisplayContext context, string relativePath) where T : Resource =>
        context.Resources.LoadResource<T>(new ModAssetKey(context.ModId, relativePath));
}
