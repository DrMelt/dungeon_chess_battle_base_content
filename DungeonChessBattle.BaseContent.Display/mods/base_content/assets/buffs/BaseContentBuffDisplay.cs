using DungeonChessBattle.Battle.Shared.ValueObjects;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 基座 Buff 展示数据资源形状：buffs/*.tres 的脚本类，字段在 Godot 编辑器配置。
/// </summary>
[GlobalClass]
public partial class BaseContentBuffDisplay : Resource {
    /// <summary>Buff 键，与数据面 BuffDefinition.BuffTypeId 逐字相同；长度上限由 BuffTypeId 承担，超限在产出展示数据时抛异常。</summary>
    [Export]
    public string BuffTypeId {
        get; set;
    } = "";

    /// <summary>Buff 图标。</summary>
    [Export]
    public Texture2D? Icon {
        get; set;
    }

    /// <summary>Buff 名称。</summary>
    [Export(PropertyHint.MultilineText)]
    public string BuffName { get; set; } = "";

    /// <summary>Buff 描述。</summary>
    [Export(PropertyHint.MultilineText)]
    public string BuffDescription { get; set; } = "";

    /// <summary>产出注册用的 Buff 展示数据；未声明字段留空，由注册表沿用被覆盖者。</summary>
    public BuffDisplay ToDisplay() => new(new BuffTypeId(BuffTypeId), BuffName, BuffDescription, Icon);
}
