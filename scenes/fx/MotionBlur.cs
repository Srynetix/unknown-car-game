using Godot;
using ShaderExt;

public class MotionBlur : ColorRect
{
    [Export]
    public float Strength {
        get => this.GetShaderParam<float>("strength");
        set => this.SetShaderParam("strength", value);
    }

    [Export]
    public float AngleDegrees {
        get => this.GetShaderParam<float>("angle_degrees");
        set => this.SetShaderParam("angle_degrees", value);
    }
}
