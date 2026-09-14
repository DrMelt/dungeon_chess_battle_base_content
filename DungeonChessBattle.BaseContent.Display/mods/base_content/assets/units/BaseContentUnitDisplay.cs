using DungeonChessBattle.Battle.Shared.ValueObjects;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 单位展示数据资源形状：units/*.tres 的脚本类，字段在 Godot 编辑器配置。
/// </summary>
[GlobalClass]
public partial class BaseContentUnitDisplay : Resource {
    /// <summary>单位配置键，与内容注册表里的单位身份逐字相同；长度上限由 UnitConfigKey 承担，超限在产出展示数据时抛异常。</summary>
    [Export]
    public string ConfigKey { get; set; } = "";

    /// <summary>单位显示名。</summary>
    [Export(PropertyHint.MultilineText)]
    public string DisplayName { get; set; } = "";

    /// <summary>单位描述。</summary>
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    /// <summary>单位图标。</summary>
    [Export]
    public Texture2D? Icon {
        get; set;
    }

    /// <summary>单位模型场景，随包导出的 .tscn；未配置为 null，回落宿主共享模板，外观取场景自带材质。</summary>
    [Export]
    public PackedScene? ModelScene {
        get; set;
    }

    /// <summary>产出注册用的单位展示数据；未声明字段留空，由注册表沿用被覆盖者。</summary>
    public UnitDisplay ToDisplay() => new(new UnitConfigKey(ConfigKey), DisplayName, Description, Icon, ModelScene, null);
}
