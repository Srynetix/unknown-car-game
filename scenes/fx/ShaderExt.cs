using Godot;
using System;

namespace ShaderExt {
    public static class ShaderExt {
        public static T GetShaderParam<T>(this CanvasItem canvasItem, string name) {
            return (T)((ShaderMaterial)canvasItem.Material).GetShaderParam(name);
        }

        public static void SetShaderParam(this CanvasItem canvasItem, string name, object value) {
            ((ShaderMaterial)canvasItem.Material).SetShaderParam(name, value);
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    sealed class ShaderAttribute: Attribute {
        public ShaderAttribute(string paramName) {
            ParamName = paramName;
        }

        public string ParamName { get; }
    }
}
