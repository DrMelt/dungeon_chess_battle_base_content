using DungeonChessBattle.Game.Mod.Shared;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 基座副本展示数据资源形状：dungeons/*.tres 的脚本类，字段在 Godot 编辑器配置。
/// </summary>
[GlobalClass]
public partial class BaseContentDungeonDisplay : Resource {
    /// <summary>副本键，来自领域配置。</summary>
    [Export]
    public string DungeonKey { get; set; } = "";

    /// <summary>副本显示名。</summary>
    [Export(PropertyHint.MultilineText)]
    public string DisplayName { get; set; } = "";

    /// <summary>副本描述。</summary>
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    /// <summary>环境表现场景资源名，见宿主 Game.Shared 的 DisplayAssetIds；空串由消费方回退默认副本场景。</summary>
    [Export]
    public string EnvSceneId { get; set; } = "";

    /// <summary>产出注册用的副本展示数据；环境场景名经宿主注册表解析，空串或未注册为 null。</summary>
    public DungeonDisplay ToDisplay(IDisplayRegistry registry) =>
        new(DungeonKey, DisplayName, Description, registry.Scene(EnvSceneId));
}
