using Godot;

public class Shockwave : ColorRect
{
    public float Force {
        get => GetForce();
        set => SetForce(value);
    }

    public float Thickness {
        get => GetThickness();
        set => SetThickness(value);
    }

    public Vector2 Center {
        get => GetCenter();
        set => SetCenter(value);
    }

    private Tween _Tween;

    public override void _Ready()
    {
        _Tween = GetNode<Tween>("Tween");
    }

    public void Start(Vector2 position) {
        SetCenter(position);

        _Tween.InterpolateProperty(Material, "shader_param/size", 0.1f, 1.25f, 2);
        _Tween.InterpolateProperty(Material, "shader_param/force", 0.0f, 0.25f, 2);
        _Tween.InterpolateProperty(Material, "shader_param/thickness", 0.0f, 0.1f, 2);
        _Tween.Start();
    }

    public bool IsRunning() {
        return _Tween.IsActive();
    }

    private void SetCenter(Vector2 value) {
        ((ShaderMaterial)Material).SetShaderParam("center", value);
    }

    private Vector2 GetCenter() {
        return (Vector2)((ShaderMaterial)Material).GetShaderParam("center");
    }

    private void SetForce(float value) {
        ((ShaderMaterial)Material).SetShaderParam("force", value);
    }

    private float GetForce() {
        return (float)((ShaderMaterial)Material).GetShaderParam("force");
    }

    private void SetThickness(float value) {
        ((ShaderMaterial)Material).SetShaderParam("thickness", value);
    }

    private float GetThickness() {
        return (float)((ShaderMaterial)Material).GetShaderParam("thickness");
    }
}
