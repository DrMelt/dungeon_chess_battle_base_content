using DungeonChessBattle.Battle.Shared.ValueObjects;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 副本展示数据资源形状：dungeons/*.tres 的脚本类，字段在 Godot 编辑器配置。
/// </summary>
[GlobalClass]
public partial class BaseContentDungeonDisplay : Resource {
    /// <summary>副本键，来自领域配置；长度上限由 DungeonKeyId 承担，超限在产出展示数据时抛异常。</summary>
    [Export]
    public string DungeonKey { get; set; } = "";

    /// <summary>副本显示名。</summary>
    [Export(PropertyHint.MultilineText)]
    public string DisplayName { get; set; } = "";

    /// <summary>副本描述。</summary>
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    /// <summary>环境表现场景，随包导出；未配置为 null，宿主不建环境。</summary>
    [Export]
    public PackedScene? EnvScene {
        get; set;
    }

    /// <summary>产出注册用的副本展示数据；未声明字段留空，由注册表沿用被覆盖者。</summary>
    public DungeonDisplay ToDisplay() =>
        new(new DungeonKeyId(DungeonKey), DisplayName, Description, EnvScene);
}
