# Organización del proyecto

La carpeta que se abre en Unity Hub es `ChasquiUnity`, no la raíz del repositorio.

## Unity / Assets

- `Scenes`: `Nivel01_Valle` es el nivel principal; `Chasqui` conserva la escena del personaje.
- `Personajes/Chasqui`: modelos importados, prefab, animaciones, materiales y mallas del personaje.
- `Niveles/Valle`: terreno, puente y recursos del primer nivel.
- `Resources/EstiloInca`: estilo de texto; las fuentes se preparan localmente.
- `Scripts/Personaje`: movimiento, postura, salto, cabello y ropa.
- `Scripts/Mundo`: nivel, colisiones y límites del camino.
- `Scripts/Interfaz`: letreros e interfaz del juego.
- `Scripts/Camara`: cámara del personaje.
- `Shaders`: sombreadores de piel, tejidos, cabello y otros materiales.
- `Editor/Herramientas`: construcción y ajustes del contenido desde el editor.
- `Editor/Pruebas`: comprobaciones de movimiento, colisiones y presentación.
- `Editor/Diagnosticos`: inspección de medidas y datos del personaje.

Los modelos y mallas de iteraciones previas se conservan cuando las herramientas
del editor todavía los utilizan. No eliminarlos solo por su nombre.

## Archivos locales excluidos

`Library`, `Temp`, `Logs`, `UserSettings`, `Assets/_Recovery`, cachés de Blender,
accesos directos de Windows y copias automáticas no se suben. Se mantienen en la
computadora; Unity reconstruye sus cachés cuando sea necesario.
