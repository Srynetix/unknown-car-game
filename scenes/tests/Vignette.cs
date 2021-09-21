using Godot;

public class Vignette : ColorRect
{
    public float Ratio {
        get => GetRatio();
        set => SetRatio(value);
    }

    private float GetRatio() {
        return (float)((ShaderMaterial)Material).GetShaderParam("ratio");
    }

    private void SetRatio(float value) {
        ((ShaderMaterial)Material).SetShaderParam("ratio", value);
    }
}
