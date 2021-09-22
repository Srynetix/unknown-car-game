using Godot;
using ShaderExt;

public class Vignette : ColorRect
{
    [Export]
    public float Ratio {
        get => this.GetShaderParam<float>("ratio");
        set => this.SetShaderParam("ratio", value);
    }
}
