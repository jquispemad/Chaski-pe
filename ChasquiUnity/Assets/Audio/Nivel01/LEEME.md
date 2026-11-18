# Audio del nivel 1

Biblioteca descargada el 17 de noviembre de 2026. **37 archivos de audio**,
seleccionados para el recorrido de la primera misión. Música, ambiente, pasos,
saltos, aterrizajes, roce de tela, entrega y panel final están conectados mediante
`AudioPrimeraMision`. Los crujidos y el enfoque de interfaz quedan disponibles
para futuros eventos que los necesiten.

## Contenido y uso previsto

- `Musica`: Pan, de mart; 76 segundos de flauta sintetizada. Música provisional
  de fondo, volumen bajo. Ajustar la unión del bucle antes de reproducirla continuamente.
- `Pututu`: caracola soplada por Devin, grabada por RoofDog; 11 segundos.
  Para el aviso del chaski y el cierre de la entrega. Es una caracola real,
  **no una grabación documentada de pututu andino**. Se descargó el MP3 de
  preescucha HQ ofrecido públicamente; no es el WAV original.
- `Ambiente`: viento entre árboles y aves, de felix.blume; 205 segundos.
  Grabación exterior, sin identificación de fauna andina. Preescucha HQ.
- `Pasos/Tierra`, `Pasos/Piedra`, `Pasos/Madera`: cinco variantes por superficie.
  Alternarlas según la distancia recorrida y el suelo. Los de tierra proceden
  de una grabación real sobre pasto de jedg (Freesound, CC0), recortada en cinco
  variantes con fundidos. No son sandalias documentadas. Piedra y madera se
  reservan para sus superficies; caminar y correr ajustan la cadencia a la velocidad.
- `Movimiento`: dos impulsos de salto, tres aterrizajes suaves y dos caídas
  fuertes. Son contactos de pies/impactos de foley; evitar pitidos de salto.
- `Ropa`: cuatro roces para agacharse, levantarse y movimientos de la capa.
  Agacharse no necesita un sonido artificial propio: usar tela y pasos suaves.
- `Quipu`: dos manipulaciones de cuero como sustituto del roce de la entrega.
- `Ambiente/Madera_cruje*`: tres crujidos para estructuras, solo cuando corresponda.
- `Interfaz`: confirmar, enfocar y abrir la manta final.

La llamada de caracola también puede acompañar el final de misión, sin añadir
una fanfarria moderna. No se necesitan efectos de combate, daño, agua o animales
inexistentes en este nivel. Las voces y los diálogos hablados no están grabados.

## Licencias y procedencia

Todos los audios seleccionados se publicaron bajo **CC0 1.0**, que permite
adaptarlos e incluirlos en un juego comercial. Conservamos los créditos aunque
la licencia no exige atribución. `CATALOGO.json` relaciona cada archivo local
con su autor, nombre original, fuente y uso previsto.

- Kenney: https://kenney.nl/assets/impact-sounds
- Kenney: https://kenney.nl/assets/rpg-audio
- Kenney: https://kenney.nl/assets/ui-audio
- mart: https://opengameart.org/content/pan-flute
- RoofDog: https://freesound.org/people/RoofDog/sounds/78974/
- felix.blume: https://freesound.org/people/felix.blume/sounds/414109/
- CC0: https://creativecommons.org/publicdomain/zero/1.0/
- Pasos sobre pasto, jedg: https://freesound.org/people/jedg/sounds/505833/

Las licencias originales de los tres paquetes Kenney están en `Licencias`.
No se descargaron canciones comerciales ni sonidos extraídos de otros juegos.

## Integración posterior

Música y ambiente en fuentes separadas; efectos cortos por eventos, sin crear
una fuente por cada paso. Usar streaming para las pistas largas y variación
moderada de volumen/tono en pasos. Evitar un sonido por fotograma al mantener C.
Antes de publicar, escuchar y ajustar mezcla, cortes y transiciones dentro del juego.
