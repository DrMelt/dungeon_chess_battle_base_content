using DungeonChessBattle.Game.Mod.Shared;
using DungeonChessBattle.Game.Shared.Display;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 基座技能展示数据资源形状：skills/*.tres 的脚本类，字段在 Godot 编辑器配置。
/// </summary>
[GlobalClass]
public partial class BaseContentSkillDisplay : Resource {
    /// <summary>技能 ID。</summary>
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

    /// <summary>施放特效场景资源名，见宿主 Game.Shared 的 DisplayAssetIds；空串表示无。</summary>
    [Export]
    public string ApplyEffectSceneId { get; set; } = "";

    /// <summary>范围提示场景资源名，见宿主 Game.Shared 的 DisplayAssetIds；空串表示无。</summary>
    [Export]
    public string RangeHintSceneId { get; set; } = "";

    /// <summary>产出注册用的技能展示数据；两个场景名经宿主注册表解析，未注册为 null。</summary>
    public SkillDisplay ToDisplay(IDisplayRegistry registry) => new(
        SkillId, SkillName, SkillDescription, Icon,
        registry.Scene(ApplyEffectSceneId), registry.Scene(RangeHintSceneId));
}
