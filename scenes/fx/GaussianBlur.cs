using Godot;
using ShaderExt;

public class GaussianBlur : Control
{
    [Export]
    public float Strength {
        get => this.GetShaderParam<float>("strength");
        set => this.SetShaderParam("strength", value);
    }
}
