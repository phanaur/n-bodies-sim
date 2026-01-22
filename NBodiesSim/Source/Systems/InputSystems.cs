/*
 * Esta clase genera un objeto encargado de leer las intrucciones del usuario, procesarlas y ajustar los valores de la
 * cámara y la configuración de la simulación. En función de la tecla que el usuario presione, se devuelve una configuración
 * del diccionario que contiene las configuraciones de cada cuerpo con su tecla de teclado correspondiente.
 */

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