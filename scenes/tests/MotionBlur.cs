using Godot;
using ShaderExt;

public class MotionBlur : ColorRect
{
    [Export]
    public float Strength {
        get => GetStrength();
        set => SetStrength(value);
    }

    [Export]
    public float AngleDegrees {
        get => GetAngleDegrees();
        set => SetAngleDegrees(value);
    }

    private float GetStrength() {
        return this.GetShaderParam<float>("strength");
    }

    private float GetAngleDegrees() {
        return this.GetShaderParam<float>("angle_degrees");
    }

    private void SetStrength(float value) {
        this.SetShaderParam("strength", value);
    }

    private void SetAngleDegrees(float value) {
        this.SetShaderParam("angle_degrees", value);
    }
}
