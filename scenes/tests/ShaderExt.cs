using Godot;

namespace ShaderExt {
    public static class ShaderExt {
        public static T GetShaderParam<T>(this CanvasItem canvasItem, string name) {
            return (T)((ShaderMaterial)canvasItem.Material).GetShaderParam(name);
        }

        public static void SetShaderParam(this CanvasItem canvasItem, string name, object value) {
            ((ShaderMaterial)canvasItem.Material).SetShaderParam(name, value);
        }
    }
}
