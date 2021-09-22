using Godot;

public class GaussianBlur : Control
{
    [Export]
    public float Strength {
        get => ShaderExt.GetShaderParam<float>(this, "strength");
        set => ShaderExt.SetShaderParam(this, "strength", value);
    }
}
