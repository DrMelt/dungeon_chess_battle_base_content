using DungeonChessBattle.Battle.Mod.Interface;
using DungeonChessBattle.Battle.Mod.Shared;

namespace DungeonChessBattle.BaseContent;

/// <summary>
/// 内容 mod 数据入口：本内容包的单位、技能、Buff 与副本定义经 <see cref="IModEntry"/>
/// 在引导上下文上整体注册。
/// </summary>
public sealed class ModEntry : IModEntry {
    /// <summary>注册全部内容定义；注册次序见 <see cref="ContentRegistrar"/>。</summary>
    public void Initialize(IModBootstrapContext context) => ContentRegistrar.RegisterAll(context);
}
