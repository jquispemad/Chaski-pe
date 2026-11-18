# Chaski-pe

Videojuego 3D en desarrollo sobre un chasqui que recorre el Camino del Inca.
Incluye un primer nivel de valle, movimiento, salto, agacharse y un personaje
con animaciones y simulación de ropa.

## Abrir el proyecto

1. Instalar **Unity 6000.3.24f1** desde Unity Hub y **Git LFS**.
2. Clonar el repositorio y descargar los archivos grandes:

   ```powershell
   git clone https://github.com/jquispemad/Chaski-pe.git
   cd Chaski-pe
   git lfs install
   git lfs pull
   powershell -ExecutionPolicy Bypass -File .\Tools\PrepararProyecto.ps1
   ```

3. En Unity Hub, agregar la carpeta **ChasquiUnity** como proyecto existente.
4. Abrir `Assets/Scenes/Inicio/InicioChaski.unity` y pulsar **Play**.

El inicio incluye portada, menú principal, historia, historieta y opciones de
volumen. Nueva partida conduce al nivel 1 en 2.5D; su final permite continuar al
nivel 2 en tercera persona. Los niveles 3–12 siguen en diseño.

La preparación copia las fuentes Georgia de la instalación local de Windows.
Ejecutarla **antes de abrir Unity por primera vez**. Las fuentes comerciales
no se distribuyen en este repositorio; sus archivos `.meta` sí se conservan.

## Controles

| Acción | Teclas |
|---|---|
| Moverse | WASD o flechas |
| Saltar | Espacio |
| Agacharse | C |
| Mirar | Ratón, después de hacer clic en el juego |
| Liberar cursor | Esc |

## Carpetas

```text
Art/Blender/                 Modelo fuente editable y caché local de Blender
ChasquiUnity/Assets/         Escenas, personaje, materiales, scripts y niveles
ChasquiUnity/Packages/       Dependencias y versiones de paquetes
ChasquiUnity/ProjectSettings/ Configuración compartida de Unity
Docs/                       Organización y recursos externos
Tools/                      Preparación de una copia nueva del proyecto
```

Ver [la organización de Assets](Docs/ESTRUCTURA.md) y
[los recursos externos](Docs/RECURSOS.md).

## Estado

Prototipo en desarrollo. El primer nivel es el Valle; el personaje, la ropa,
las colisiones y la presentación visual siguen en ajuste. Los botones de la
pantalla final son por ahora elementos visuales.

No se versionan cachés, recuperaciones ni preferencias personales. Conservar
siempre los archivos `.meta` al mover recursos de Unity. Los modelos, texturas
y mallas grandes se almacenan mediante Git LFS.
