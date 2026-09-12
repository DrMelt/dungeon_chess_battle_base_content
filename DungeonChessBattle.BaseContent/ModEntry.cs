using DungeonChessBattle.Battle.Mod.Interface;
using DungeonChessBattle.Battle.Mod.Shared;

namespace DungeonChessBattle.BaseContent;

/// <summary>
/// 基座内容 mod 数据入口：内容与行为实现整体从主解决方案 GameConfig 迁出，
/// 经 <see cref="IModEntry"/> 在引导上下文上注册。
/// </summary>
public sealed class ModEntry : IModEntry {
    /// <summary>注册全部内容定义；注册次序见 <see cref="ContentRegistrar"/>。</summary>
    public void Initialize(IModBootstrapContext context) => ContentRegistrar.RegisterAll(context);
}
