using DungeonChessBattle.Battle.Shared.Combat;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 基座技能展示数据资源形状：skills/*.tres 的脚本类，字段在 Godot 编辑器配置。
/// </summary>
[GlobalClass]
public partial class BaseContentSkillDisplay : Resource {
    /// <summary>技能键，与数据面技能身份逐字相同；长度上限由 SkillKeyId 承担，超限在产出展示数据时抛异常。</summary>
    [Export]
    public string SkillId { get; set; } = "";

    /// <summary>技能图标。</summary>
    [Export]
    public Texture2D? Icon {
        get; set;
    }

    /// <summary>技能名称。</summary>
    [Export(PropertyHint.MultilineText)]
    public string SkillName { get; set; } = "";

    /// <summary>技能描述。</summary>
    [Export(PropertyHint.MultilineText)]
    public string SkillDescription { get; set; } = "";

    /// <summary>范围提示场景，随包导出；未配置为 null，宿主不显示范围预览。</summary>
    [Export]
    public PackedScene? RangeHintScene {
        get; set;
    }

    /// <summary>产出注册用的技能展示数据；未声明字段留空，由注册表沿用被覆盖者。</summary>
    public SkillDisplay ToDisplay() =>
        new(new SkillKeyId(SkillId), SkillName, SkillDescription, Icon, RangeHintScene);
}
