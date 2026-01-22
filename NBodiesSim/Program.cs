/*
 * NBodiesSim es un simulador realista del Sistema Solar. Partiendo de unas condiciones iniciales de posición y
 * velocidad del Sol, planetas, satélites y asteroides, simula mediante interacciones entre todos los cuerpos el comportamiento
 * de cada cuerpo en el Sistema Solar. Esta simulación hace uso de la librería de gráficos Raylib, aunque no se descarta
 * la utilización de otras librerías en el futuro.
 *
 * La simulación permite seleccionar todos los planetas del Sistema Solar, de tal forma que puedan verse los satélites
 * más característicos de cada planeta, así como sus anillos, en caso de tenerlos.
 *
 * **Mejoras pendientes del simulador**
 * - Cálculo de físicas con independencia de los FPS: el método actual diagonaliza las órbitas a bajos FPS
 * - Guía de uso inicial, con aparición pulsando la tecla F1
 * - Hud personalizado para cada planeta, con información sobre sus características, satélites, velocidades, etc.
 * - Adición del baricentro del Sistema Solar, para establecer con respecto a él el cálculo de las órbitas.
 * - Posibilidad de cargar posiciones y velocidades para una fecha elegida, utilizando la API de la NASA.
 * - Posibilidad de guardar estados de posiciones.
 * - Posibilidad de generación de caché de datos, para ahorrar tiempo en la obtención de posiciones y velocidades de
 *      cada cuerpo
 * - Posible simulación de los viajes de las Voyager 1 y 2.
 * - Posible visualización de la heliopausa.
 * - Mejora del motor de físicas mediante la implementación del algoritmo Runge-Kuta 4
 * - Interacción entre los diferentes componentes mediante interfaces.
 */

using NBodiesSim.Source;
using NBodiesSim.Source.Data;
using NBodiesSim.Source.Systems;

namespace NBodiesSim;

internal static class Program
{
    private static void Main()
    {
        // 1. Crear todas las dependencias
        PhysicsEngine physics = new PhysicsEngine(); // Motor de físicas
        RenderSystem render = new RenderSystem(); // Motor de renderizado
        DataLoader data = new DataLoader(); // Carga de datos de los cuerpos
        ConfigLoader config = new ConfigLoader(); // Carga de la configuración de la simulación
        
        // 2. Crear el InputSystem, que depende de la configuración
        InputSystems input = new InputSystems(config.CameraConf);
        
        // 3. Crear la simulación, pasándole todos los sistemas
        Simulation sim = new Simulation(physics, render, data, input);
        
        // 4. Ejecutar la simulación
        sim.Run();
    }
}