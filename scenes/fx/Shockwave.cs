using Godot;
using ShaderExt;

public class Shockwave : ColorRect
{
    [Export]
    public float Force {
        get => this.GetShaderParam<float>("force");
        set => this.SetShaderParam("force", value);
    }

    [Export]
    public float Thickness {
        get => this.GetShaderParam<float>("thickness");
        set => this.SetShaderParam("thickness", value);
    }

    [Export]
    public Vector2 Center {
        get => this.GetShaderParam<Vector2>("center");
        set => this.SetShaderParam("center", value);
    }

    private Tween _Tween;

    public override void _Ready()
    {
        _Tween = GetNode<Tween>("Tween");
    }

    public void Start(Vector2 position) {
        Center = position;
        _Tween.StopAll();

        _Tween.InterpolateProperty(Material, "shader_param/size", 0.1f, 1.25f, 2);
        _Tween.InterpolateProperty(Material, "shader_param/force", 0.0f, 0.25f, 2);
        _Tween.InterpolateProperty(Material, "shader_param/thickness", 0.0f, 0.1f, 2);
        _Tween.Start();
    }

    public bool IsRunning() {
        return _Tween.IsActive();
    }

    public override void _ExitTree()
    {
        _Tween.StopAll();
        Thickness = 0;
        Force = 0;
    }
}
