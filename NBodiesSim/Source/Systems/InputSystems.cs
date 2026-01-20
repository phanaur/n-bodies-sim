using NBodiesSim.Source.Core;
using NBodiesSim.Source.Data;
using NBodiesSim.Source.Models.DTOs;
using Raylib_cs;

namespace NBodiesSim.Source.Systems;

public class InputSystems
{
    private readonly Dictionary<KeyboardKey, AstroConfig> _cameraConfigs;
    public InputSystems(Dictionary<KeyboardKey, AstroConfig> cameraConfigs)
    {
        _cameraConfigs = cameraConfigs;
    }

    public AstroConfig? ProcessInput()
    {
        // Set Camera
        int key = Raylib.GetKeyPressed();
        if (key != (int)KeyboardKey.Null && _cameraConfigs.ContainsKey((KeyboardKey)(key)))
        {
            return _cameraConfigs[(KeyboardKey)(key)];
        }
        return null;

    }
    
}