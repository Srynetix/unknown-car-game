using Godot;
using Godot.Collections;

public class LoadCache: Node {
    public Dictionary<string, object> _Cache = new Dictionary<string, object>();
    private static LoadCache _GlobalInstance;

    public LoadCache() {
        if (_GlobalInstance == null) {
            _GlobalInstance = this;
        }
    }

    public static LoadCache GetInstance() {
        return _GlobalInstance;
    }

    public void StoreScene(string name, string path) {
        var scene = GD.Load<PackedScene>(path);
        _Cache[name] = scene;
    }

    public T LoadScene<T>(string name) {
        return (T)_Cache[name];
    }

    public PackedScene LoadScene(string name) {
        return LoadScene<PackedScene>(name);
    }
}
