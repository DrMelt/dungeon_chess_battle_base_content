using System;
using DungeonChessBattle.Game.Mod.Shared;
using Godot;

namespace DungeonChessBattle.BaseContent.Display;

/// <summary>
/// 矩形范围技能提示，通过着色器展示矩形区域范围。
/// 提示场景根实现 <see cref="IRectRangeHint"/>，由宿主在选位置目标期间驱动；
/// 宿主在挂载后一帧首次调用，Init 执行时子节点 _Ready 已完成。
/// </summary>
public partial class SkillRangeRect_Hint : Node3D, IRectRangeHint {
    /// <summary>导出引用集合节点。</summary>
    public SkillRangeRect_HintInterRefs? InterRefs {
        get; private set;
    }

    /// <summary>范围提示着色器材质。</summary>
    private ShaderMaterial? _shaderMaterial;

    /// <summary>节点就绪：获取引用集合节点。</summary>
    public override void _Ready() {
        InterRefs = GetNode<SkillRangeRect_HintInterRefs>("SkillRangeRect_HintInterRefs");
    }

    /// <inheritdoc/>
    public void Init(Vector3 fromPos, Vector3 toPos, float near, float far, float fromLeft, float toRight) {
        var interRefs = InterRefs ?? throw new InvalidOperationException("InterRefs has not been initialized.");
        var decalRef = interRefs.DecalRef ?? throw new InvalidOperationException("DecalRef is not assigned.");
        _shaderMaterial = (decalRef.MaterialOverride as ShaderMaterial) ?? throw new InvalidOperationException("decalRef.MaterialOverride is not a ShaderMaterial.");

        GlobalPosition = fromPos;
        toPos.Y = fromPos.Y;
        LookAt(toPos, up: Vector3.Up);

        Scale = new Vector3(toRight - fromLeft, 1, far);
        var dPos = decalRef.Position;
        dPos.X = (toRight + fromLeft) * 0.5f;
        decalRef.Position = dPos;

        _shaderMaterial.SetShaderParameter("Near", near);
    }
}
