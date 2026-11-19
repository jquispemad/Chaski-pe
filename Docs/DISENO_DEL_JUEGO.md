# CHASKI: EL ÚLTIMO MENSAJE

**Videojuego de aventura, acción y supervivencia histórica**

**Ambientación:** Imperio incaico, siglo XV.  
**Recorrido principal:** Cusco → Apurímac → Vilcashuamán (Ayacucho).  
**Personaje principal:** Wayra («viento» en quechua), un joven chasqui que recibe su primera misión oficial.

## Dirección del proyecto

**Inicio implementado:** `Assets/Scenes/Inicio/InicioChaski.unity` es la primera escena de la compilación y el punto de inicio de Play. Incluye portada, menú principal, resumen narrativo sin revelar el final, historieta ilustrada de tres páginas con nueve textos, volumen general persistente y navegación hacia los dos niveles existentes. Nueva partida reproduce la historieta (con opción de omitir) y comienza el nivel 1; Continuar usa el progreso disponible. Las ilustraciones son fondos; los textos y botones son interactivos en Unity. No incluye voces grabadas ni los niveles 3–12.

La historia de Wayra transcurre en el siglo XV. Su misión y el sabotaje de las postas son ficticios. Cusco, Apurímac y Vilcashuamán orientan el viaje. Los mapas comprimen el recorrido y los relevos permiten mostrar el paso del tiempo entre tramos.

La introducción es una historieta. La **primera escena jugable** es el pueblo en **2.5D**. Wayra y el entorno usan modelos 3D, y el jugador avanza o retrocede por el eje del sendero, sin desplazamiento lateral ni movimiento libre en profundidad. La cámara mira desde un costado y sigue a Wayra suavemente por el sendero, con ángulo y altura constantes. Wayra queda cerca del centro y se deja algo de espacio para ver los obstáculos que vienen. El ratón no gira la cámara. Esta sección dura unos tres minutos y culmina con la entrega del quipu.

El **nivel 2** continúa en tercera persona en un recorrido diseñado para su misión. **Nivel01_Valle** queda como borrador de referencia. De él podemos reutilizar el personaje, senderos, piedras, vegetación, letreros, casas, materiales y el puente pequeño según lo que necesite cada nivel. El gran puente del nivel 4 tendrá su propio cruce. Antes de devolver el control en el nivel 2, un plano muestra la salida de la posta desde atrás de Wayra y aparecen las ayudas de cámara y salto.

**Implementación actual.** Unity 6000.3.24f1 ya tiene el Valle en 3D, movimiento, salto, agacharse y cámara con ratón. El pueblo lateral, la historieta, el cambio de escena y los demás niveles se describen como diseño pendiente.

## INTRODUCCIÓN — HISTORIETA INICIAL

Antes de comenzar el juego aparece una historieta animada, con dibujos inspirados en los Andes y narración en español.

- **Viñeta 1 — El Tahuantinsuyo:** Se observan montañas, pueblos, templos y caminos. El narrador explica cómo el Estado inca amplió y conectó caminos andinos que hoy conocemos como Qhapaq Ñan.
- **Viñeta 2 — Los mensajeros del Inca:** Aparecen varios chasquis corriendo y entregándose mensajes mediante relevos. Se explica su importancia para comunicar los territorios del imperio.
- **Viñeta 3 — Sus herramientas:** Se muestra el equipo de Wayra: pututu, quipu, ojotas y chuspa. La huaraca se presenta como parte de su equipo ficticio, no como identificación del oficio.
- **Viñeta 4 — Los peligros:** Un chasqui atraviesa montañas, puentes colgantes, lluvias y territorios peligrosos. Se muestran pumas y hombres armados.
- **Viñeta 5 — Nuestro protagonista:** En un pequeño pueblo cercano a Cusco, Wayra despierta mientras su madre prepara alimentos. Un mensajero anuncia que debe presentarse en el chaskiwasi.
- **Viñeta 6 — Comienza la aventura:** Wayra recoge sus pertenencias y sale de casa. Aparece el título del videojuego.

---

## CAPÍTULO I — EL ÚLTIMO MENSAJE

### NIVEL 1 — La primera misión

**Meta del jugador.** Llegar desde la vivienda de Wayra hasta el chaskiwasi y recibir su primer encargo. El pueblo está cerca de Cusco. La historieta se reproduce antes del nivel y su duración se mide aparte.

**Duración objetivo.** Cerca de tres minutos en una primera partida. Los tiempos son orientativos. El jugador puede detenerse y volver sobre sus pasos.

#### 0:00–0:25 — La vivienda

Al amanecer, Wayra está junto a la puerta. Se oye a los vecinos empezar el día y su madre le entrega la chuspa. Un aviso breve indica «Ve a la posta». La madre dice «Ya te esperan. Sigue el sendero junto a los cultivos». El jugador toma el control antes de salir de casa.

**Encuadre.** La vivienda ocupa el fondo y la salida se distingue por la luz exterior. Mostrar W o ↑ cuando el jugador toma el control y S o ↓ si se detiene junto a la puerta.

#### 0:25–1:00 — La calle del pueblo

Wayra pasa junto a viviendas de piedra y techos de paja. Un vecino mueve una cesta y una llama descansa detrás de un cerco. Ambos quedan fuera del espacio de paso. Una piedra baja ocupa el sendero y enseña a saltar con Espacio. Dejar suelo libre antes y después para repetir el salto. Tras cruzarla, el jugador puede volver hacia la casa.

#### 1:00–1:40 — Los cultivos

El sendero pasa al pie de los andenes. Una viga baja obliga a agacharse con C. La cámara deja ver el espacio libre al otro lado. Al soltar C bajo la viga, Wayra sigue agachado hasta tener espacio para ponerse de pie. Tras pasar, un agricultor señala la posta. Su diálogo es opcional y aporta contexto sin retener al jugador.

#### 1:40–2:20 — El camino alto

Una subida corta conecta los cultivos con la posta. El jugador puede subir caminando o saltar un escalón bajo. El techo de la posta se ve al fondo desde el inicio de este tramo. Una llamada de pututu cerca de la entrada anuncia el destino. Guardar un punto de control antes de la subida.

#### 2:20–3:00 — El primer encargo

El jugador entra en el círculo amarillo junto al relevista y se queda quieto un segundo. La entrega es automática y el recado aparece en un panel con la tipografía del Valle.

> «Lleva este quipu a la próxima posta. Di que el paso del río sigue abierto y que pueden enviar provisiones».

El nivel termina al recibir el encargo. Mostrar «Quipu recibido» y guardar el avance con el objeto entregado. Cerrar con una vista al camino que sale del pueblo. La salida habilita el nivel 2 cuando esté disponible.

#### Cámara y controles

- Wayra avanza con W o ↑ y retrocede con S o ↓ por el eje del sendero. A/D y las flechas laterales quedan desactivadas en el pueblo.
- Espacio permite saltar y C agacharse desde el inicio. La entrega se activa al detenerse en el círculo amarillo.
- La cámara lateral sigue a Wayra tanto al avanzar como al retroceder. Conserva la altura al saltar y el ratón no la gira.
- El personaje se orienta en el sentido del movimiento. El escenario mantiene profundidad visual con casas, cultivos y montañas fuera del plano jugable.

#### Recursos y prueba del nivel

Reutilizar el personaje de Wayra, la chuspa, las piedras, la vegetación y las casas del Valle. Preparar para esta escena un interior sencillo, andenes visibles, viga baja, entrada de la posta y una animación breve de entrega. Usar al inicio geometría simple para medir el recorrido.

Probar una primera partida de aproximadamente 2:45–3:15. Medir desde que el jugador recibe el control hasta que acepta el quipu. Ajustar distancias y diálogos si el recorrido es demasiado corto o largo. Comprobar que se pueda retroceder entre encuadres, salir de debajo de la viga y recibir el quipu una sola vez. La duración se consigue con recorrido y acciones, evitando esperas obligatorias.

### NIVEL 2 — El primer recorrido

**Estado de implementación.** Primera versión jugable creada en `Assets/Scenes/Campana/Nivel02_PrimerRecorrido.unity`: recorrido 3D, cultivos, ladera, montones escalonados, paso bajo, arroyo con puente trenzado reutilizado del Valle, refugio y posta. Incluye entrega inicial animada del quipu con subtítulos por hablante; el relevista camina al chaskiwasi, aparta una estera de fibras atada al dintel, entra y la deja caer de nuevo antes de habilitar el movimiento; sendero visible, sombras de hojas y barreras invisibles que siguen el camino a 30 metros de sus costados. Las rocas decorativas se asientan usando los vértices de la malla y la altura real del terreno. Las cuatro viviendas tienen mampostería de piedra gris con bloques biselados en relieve y juntas, conservando sus colisiones y los techos de ichu, ahora con haces superpuestos y aleros irregulares. El mismo modelo de vivienda se reutiliza en el nivel 1. Incluye cámara con colisiones, minimapa, tres puntos de control, registro opcional, pututu con E, diálogo y manta final. El botón del nivel 1 carga esta escena. Se reutiliza el modelo de chaski para los personajes secundarios como representación provisional; los diálogos son escritos. La duración objetivo de 7–9 minutos requiere medir partidas humanas y ajustar el recorrido, no está validada. La animación específica de tocar el pututu queda pendiente de acabado.

**Modalidad.** Entorno 3D con cámara en tercera persona.  
**Escenario.** Camino entre campos de cultivo, laderas, una quebrada pequeña y una posta.  
**Duración objetivo.** Entre 7 y 9 minutos en una primera partida, incluida la llegada.  
**Escena prevista.** `Nivel02_PrimerRecorrido.unity`.  
**Objetivo principal.** Llevar el quipu y el recado oral a la siguiente posta.

#### Historia y propósito

Wayra sale con el encargo que recibió al terminar el primer nivel. Debe comunicar que el paso del río está abierto y que pueden enviar provisiones. Al llegar descubre que el relevista esperado no está. La responsable de la posta recibe el aviso, conserva el registro de llegada y le pide continuar hasta otra estación para averiguar qué ocurrió.

El nivel enseña a recorrer un espacio con profundidad, controlar la cámara, reconocer el camino y anunciarse con el pututu. El misterio se introduce mediante la ausencia del relevista y una respuesta incompleta de la responsable.

#### Inicio — La salida del pueblo · 0:00–0:45

Se muestra una toma breve de Wayra saliendo del chaskiwasi. El relevo que le dio el quipu dice:

> «Sigue el camino hasta la siguiente posta. Al llegar, anuncia tu presencia con el pututu».

La cámara se coloca detrás de Wayra antes de devolver el control. Un tramo ancho y despejado permite probar WASD o flechas en las cuatro direcciones y mirar con el ratón. Mostrar la ayuda de cámara solo al comenzar. El primer letrero se distingue desde la salida.

**Objetivo visible.** «Entrega el mensaje en la próxima posta».

#### Tramo 1 — Entre cultivos · 0:45–2:00

El sendero pasa entre andenes y una zona de pastoreo cercada. Los cultivos quedan a los costados y las viviendas en explanadas propias. Pequeños muros y desniveles orientan al jugador hacia el camino.

Hay un tronco bajo para saltar, un canal estrecho y una roca grande que permite elegir entre subir por piedras escalonadas o rodear por una variante más larga. Las piedras tienen superficies de apoyo visibles y contacto con el suelo.

Un agricultor ofrece un comentario opcional:

> «La posta está después del paso de agua. Hoy no he oído responder a su pututu».

**Punto de control.** Al terminar los cultivos, antes de entrar en la zona de rocas.

#### Tramo 2 — La ladera de piedra · 2:00–3:30

El camino sube por una ladera. El jugador ve el inicio y el final de cada grupo de obstáculos antes de entrar. Dos montones de piedras escalonados permiten enlazar saltos sobre superficies estables. Entre ambos hay un descanso de terreno llano.

La altura de cada apoyo aumenta gradualmente. Como guía inicial, los desniveles consecutivos serán de 0.30–0.50 metros, con apoyos de al menos 1.2 metros de profundidad. La separación horizontal se comprobará con el salto real de Wayra y se reducirá si exige una carrera demasiado precisa.

Al final aparece un paso bajo de madera inclinado, visible desde la cámara de aproximación. Un cartel tallado indica «PASO BAJO · MANTÉN C». La abertura tiene margen para la cápsula agachada y bloquea al personaje de pie. Al salir recupera su altura cuando queda espacio libre.

**Opcional.** En un mirador cercano hay un registro educativo sobre el relevo de mensajes. Se alcanza con un desvío corto que vuelve al sendero.

#### Tramo 3 — El paso de agua · 3:30–5:00

Un arroyo cruza una depresión pequeña. El sonido del agua aumenta al acercarse y el camino permite detenerse para observar. El cruce principal utiliza un paso corto de piedras y una pasarela firme, con barandas o bordes que hagan reconocible el acceso.

La superficie de paso permanece estable. El salto mayor del cruce será menor que la distancia máxima comprobada durante las pruebas. Una caída en una zona peligrosa devuelve al control anterior con el quipu y los objetivos conservados.

Un pequeño refugio después del cruce da un momento de descanso y muestra la posta a distancia. Wayra observa el destino durante unos segundos, con control del jugador.

**Punto de control.** Antes del cruce y otro al alcanzar el refugio.

#### Tramo 4 — El camino de la posta · 5:00–6:30

El último tramo combina curvas amplias, piedras bajas y una pequeña subida. Los obstáculos están más espaciados para permitir que el jugador vuelva a correr. La entrada del chaskiwasi se distingue por su techo de ichu, su letrero tallado y una explanada despejada.

Las casas quedan sobre suelo nivelado. La zona de llegada tiene espacio para detenerse junto a la responsable. El minimapa muestra el destino y la distancia que falta.

Al entrar en una zona de aviso aparece «Pulsa E para anunciar tu llegada». Wayra se detiene para tocar el pututu durante una animación breve. La acción tiene subtítulo y señal visual. Puede repetirse después de unos segundos si el jugador quiere oírla de nuevo.

La responsable sale al encuentro o gira hacia Wayra. El círculo amarillo de entrega se hace visible junto a ella.

#### Llegada — La posta sin relevista · 6:30–8:00

Wayra entra en el círculo y se queda quieto un segundo. Se detiene su desplazamiento y comienza el diálogo.

> **Wayra.** «Traigo el quipu de la posta del pueblo. El paso del río sigue abierto y pueden enviar provisiones».
>
> **Responsable.** «Esperábamos a otro corredor para tomar el relevo. Todavía no ha regresado».
>
> **Wayra.** «¿Cuándo salió?»
>
> **Responsable.** «Antes del amanecer. Lleva este aviso hasta la estación del valle. Pregunta si llegó allí».

La responsable toma el registro, confirma la recepción del recado y prepara un nuevo encargo. La desaparición queda como pregunta para el siguiente nivel. El personaje demuestra preocupación mediante gestos y pausas breves.

Al cerrar el diálogo aparece la manta bordada con el resultado y los botones de navegación. Guardar el nivel completado antes de mostrarla.

**Texto principal.** «MENSAJE ENTREGADO».  
**Texto secundario.** «La posta recibió tu aviso. Un nuevo relevo te espera».  
**Botones.** Repetir nivel, Inicio y Siguiente nivel. Cada botón se habilita cuando su destino esté disponible. El siguiente corresponde al nivel 3.

#### Controles y cámara

- WASD o flechas permiten moverse en el plano del suelo.
- El ratón gira la cámara. Al iniciar, la cámara apunta al primer tramo del camino.
- Espacio permite saltar y C agacharse.
- E activa el pututu en la zona señalada y avanza los diálogos cuando corresponda.
- Esc libera el cursor y permite acceder a los controles de interfaz.

La cámara mantiene distancia suficiente para ver los apoyos de salto y evita atravesar suelo, árboles o edificios. Al chocar con un obstáculo visual, acerca la vista de manera gradual y recupera la distancia al salir. El jugador puede reducir sensibilidad y movimiento de cámara.

Los mensajes de tutorial se ocultan al completar la acción y reaparecen solo si el jugador se queda detenido ante el obstáculo.

#### Mapa y orientación

Minimapa en la esquina superior derecha con vista aérea del recorrido completo, marcador del jugador, posta de destino y distancia restante. Los caminos y los cruces deben verse a su escala de lectura. El sentido del marcador se corresponde con la orientación usada por el mapa y su indicador de rumbo.

Cada bifurcación tiene un letrero tallado y una referencia visual del paisaje. Los desvíos opcionales regresan al camino principal. La dirección del objetivo puede consultarse con una ayuda opcional.

#### Recursos y coleccionables

El quipu se conserva como objeto de misión desde el inicio. Solo se retira del inventario al confirmar la entrega. La responsable proporciona el siguiente encargo dentro de la misma escena de cierre.

Se propone un coleccionable educativo en el mirador y un punto de descanso en el refugio. Comida y resistencia se introducirán cuando el sistema común esté implementado. La ruta principal se balancea para completarse con los controles básicos.

#### Distribución y límites del mapa

Recorrido orientativo de 400–500 metros, ajustado para la duración objetivo. La mayor parte sigue el sendero central, con dos desvíos breves. Las montañas lejanas aportan profundidad y los límites jugables se integran mediante pendientes, cercos y zonas inaccesibles visibles.

Distribuir viviendas en explanadas, cultivos en andenes y obstáculos de salto en tramos de camino con espacio. Mantener despejadas puertas, letreros, controles de reaparición y el círculo de llegada.

El Valle existente sirve de fuente de rocas, vegetación, materiales, construcciones y caminos. La nueva composición sigue esta secuencia de aprendizaje.

#### Sonido, luz y ambiente

Mañana luminosa con sombras legibles. Se oyen pasos según la superficie, viento suave, agua al acercarse al cruce y actividad agrícola distante. El pututu tiene una respuesta visual y un subtítulo para orientar también sin audio.

El techo usa el acabado de ichu trenzado con filtrado. La vegetación mantiene hojas reconocibles y evita hebras tan finas que produzcan líneas o parpadeo a distancia.

#### Guardado y condiciones de finalización

Guardar en la salida, antes de la ladera, antes del agua y en el refugio. Al reaparecer, Wayra queda sobre suelo seguro con resistencia recuperada si ese sistema está activo.

La entrega requiere haber pasado por el recorrido y estar quieto en el círculo de la responsable. El pututu abre la interacción de llegada. Tras recibir el mensaje se marca el nivel completado y se desbloquea el nivel 3 cuando esté instalado.

Los coleccionables son opcionales. El nivel se puede repetir y el guardado impide duplicar recompensas.

#### Pruebas para aceptar el nivel

1. Completar una primera partida en 7–9 minutos, incluidos tutoriales y llegada.
2. Comprobar cada salto con el personaje actual, desde velocidades normales y con margen para aterrizar.
3. Confirmar que las piedras están apoyadas, que la cápsula no atraviesa los obstáculos y que se puede salir del paso bajo.
4. Verificar cámara y minimapa al avanzar, retroceder y rodear las rocas.
5. Recorrer la escena durante varios minutos para revisar rayitas, parpadeos y rendimiento.
6. Probar cada reaparición y la entrega del quipu una sola vez.
7. Comprobar pausa, omitir diálogo, manta final y navegación disponible.

### NIVEL 3 — El valle de los vientos

**Estado:** primera versión jugable implementada en Unity (17 de noviembre de 2026). Comienza saliendo de la segunda posta, sin repetir la entrega del quipu; reproduce la casa, responsable, vegetación y relieve del final del nivel 2 al inicio, y después continúa por una bajada curva entre afloramientos rocosos, recorrido propio de unos 500 m, terreno sin hierba de líneas, sendero, casas de piedra e ichu, ráfagas con abrigo y respuesta de la capa, nueve piedras de cruce, cuatro controles, coca, cancha, ch’arki y agua con inventario y gestos de consumo. Incluye minimapa, pausa, registro opcional, pututu, diálogo y manta final. El nivel 2 permite continuar a esta escena y el menú reconoce su progreso. El nivel 4 permanece deshabilitado hasta crearlo.

**Verificación realizada:** compilación en el editor, 241 muestras de suelo y nueve apoyos del arroyo sin errores en la comprobación geométrica; prueba en Play de devolución de control, referencias de audio, efectos de los cuatro consumibles, apoyo en ladera y transición del diálogo al panel final. Estas comprobaciones no sustituyen una partida humana completa: quedan por medir la duración, el rendimiento y el acabado de las animaciones durante todo el recorrido.

**Ajustes verificados:** el diálogo del caminante conserva prioridad sobre avisos de control; la escena arranca apoyada junto a la responsable de la posta anterior, no contiene arco ni entrega repetida y los cuatro consumibles muestran geometría, terminan el gesto y devuelven el control sin duplicar el gasto de inventario. Resistencia rebalanceada a 1,5 puntos/s corriendo y 3 por salto. El ajuste de los letreros se aplica al renderizar el texto y al reconstruirse su fuente.

**Corrección del agua suspendida:** el objeto del arroyo conservaba por error una traslación de +5 m en Y y +1,96 m en X al adaptar el inicio del nivel. Se restableció su transformación y se proyectó la superficie sobre el TerrainCollider con una separación de 0,018 m; se verificó en Play con rayos de colisión y vista oblicua del cauce. Se excluye el arroyo de las futuras traslaciones del pueblo.

**Caudal aumentado (18 de noviembre de 2026):** agua del cruce a 0,28 m, aproximadamente un metro por encima de la versión anterior, con mayor anchura y riberas adaptadas al nivel. Los nueve apoyos quedan al menos 0,22 m por encima del agua. Los apoyos tienen pequeñas variaciones laterales y de giro; 45 rocas decorativas se distribuyen con posiciones y tamaños irregulares, asentadas sobre el terreno. Compilación y guardado en Unity completados sin errores.

**Río y final revisados:** agua con pendiente suave dentro de un lecho excavado y riberas de piedra, sin láminas suspendidas. El final está en un abrigo abierto de ichu y piedra junto a una quebrada, con pretil y paredes rocosas; no repite la casa del nivel 2. En pausa el puntero queda libre y la cámara no captura clics. Durante el juego, doble clic activa la cámara. Desde la revisión del 18/11/2026, esperar o descansar no recupera resistencia: se utilizan las provisiones repartidas por el camino.

**Continuidad visual:** el nivel 3 comienza en la misma posta donde se entregó el quipu al terminar el nivel 2; conserva su distribución y no repite la entrega. Tras salir aparece el nuevo recorrido. No hay arco de agacharse en este nivel. Los avisos de interacción se muestran aparte del diálogo para que no lo borren; los textos de los letreros se ajustan a la tabla con márgenes.

**Escena creada:** `Assets/Scenes/Campana/Nivel03_ValleVientos.unity`. Para probar directamente: menú de Unity **Chasqui → Abrir nivel 3 · Valle de los vientos**, después Play. Para iniciar la campaña: **Chasqui → Abrir portada y menú principal**.
**Modalidad:** 3D, cámara en tercera persona con ratón.
**Duración objetivo:** 7–9 minutos en una primera partida, sin contar reintentos prolongados.
**Longitud orientativa:** 450–550 metros de ruta principal y dos desvíos opcionales cortos. Ajustar después de medir una partida con la velocidad real del personaje.
**Objetivo:** llevar el nuevo aviso a la estación del valle y preguntar por el relevista desaparecido.
**Tema jugable:** interpretar el viento, elegir un momento seguro para avanzar y administrar el esfuerzo sin perder el mensaje.

#### Continuidad con el nivel 2

Al terminar el nivel 2, la responsable de la posta recibe el recado sobre el río y encarga a Wayra seguir hasta la estación del valle porque el relevista no ha regresado. Al comenzar el nivel 3, Wayra ya lleva ese aviso; no se repite la entrega ni aparece otro emisor. Wayra conserva este encargo como objeto de misión; el quipu anterior ya fue recibido. No duplicar quipus entre escenas ni presentar el nuevo registro como una traducción escrita de cada frase.

El nivel introduce indicios de personas que observan los caminos. Su identidad todavía es desconocida. La trama continúa en el siglo XV: no mostrar españoles, armas de fuego ni noticias sobre Atahualpa. Los hechos y personajes de esta aventura son ficticios.

#### Estructura y ritmo del recorrido

**Inicio — El encargo continúa · 0:00–0:35.**

Una secuencia breve muestra la salida de la posta. Si se llega desde el nivel 2, evitar repetir toda la entrega anterior: la responsable confirma el destino, Wayra ajusta su chuspa y mira hacia el valle.

> **Responsable:** «Lleva este aviso a la estación del valle. Pregunta por el corredor que salió antes del amanecer».
>
> **Wayra:** «Si pasó por allí, encontraré noticias de él».
>
> **Responsable:** «El viento arrecia en la ladera. Busca abrigo antes de seguir».

Subtítulos con nombre del hablante y tiempo suficiente para leer. La cámara pasa de un plano de ambos personajes a la vista de juego mediante una transición suave. Devolver el control al terminar. Permitir omitir la secuencia; omitirla debe dejar al personaje en el mismo punto, con el encargo recibido y los controles activos.

**Tramo 1 — El valle abierto · 0:35–1:45.**

El camino desciende suavemente entre andenes y árboles. Un agricultor trabaja fuera del sendero, con ropa de labor, sin capa ni accesorios de chaski. Los cultivos tienen hojas reconocibles y los árboles quedan fuera de las parcelas.

Hay dos obstáculos separados: un tronco bajo y una pequeña acumulación de piedras escalonadas. Presentarlos con espacio para ver el aterrizaje. No colocar obstáculos delante de una puerta o de un cartel.

La capa, las copas y el sonido del aire muestran una primera ráfaga leve. Aquí el viento es principalmente visual y sonoro: el jugador aprende a reconocerlo sin riesgo de caída.

> **Ayuda breve:** «Observa las plantas y escucha el viento antes de entrar en la ladera».

**Punto de control 1:** explanada al final de los cultivos, aproximadamente a los 100 metros.

**Tramo 2 — La ladera expuesta · 1:45–3:15.**

El sendero recorre una ladera con vistas al valle. Su parte transitable mide inicialmente 3–4 metros de ancho, con rocas de abrigo en dos ensanchamientos. La cámara debe mostrar el siguiente refugio antes de abandonar el anterior.

Las ráfagas tienen tres fases legibles: aviso, intensidad máxima y calma. Propuesta inicial: 1,5 segundos de aviso, 3 segundos de ráfaga y 4–5 segundos de calma, con variación pequeña. Las señales visuales y sonoras coinciden con el momento de la fuerza física.

Durante la primera ráfaga expuesta se indica:

> «Mantén C para agacharte. Refúgiate junto a las rocas cuando sople con fuerza».

Agacharse reduce la influencia del viento y la velocidad. El abrigo de una roca ancha elimina el empuje al estar detrás de ella respecto a la dirección de la ráfaga. El viento puede desviar ligeramente al personaje, pero no debe quitarle por completo el control ni empujarlo fuera de un salto inevitable. No introducir sacudidas intensas de cámara ni rayas rojas en pantalla.

**Punto de control 2:** refugio de piedra antes del cruce de agua, aproximadamente a los 210 metros.

**Tramo 3 — El arroyo de la quebrada · 3:15–4:35.**

Un arroyo nace en una grieta del cerro, desciende por un cauce irregular y atraviesa el valle. Las orillas muestran piedra húmeda, tierra y algunas plantas anchas; evitar rectángulos de agua y extremos visibles del río.

El cruce de este nivel utiliza tres o cuatro apoyos grandes y un paso de piedra corto. Reservar el gran puente colgante como elemento principal del nivel 4. Los apoyos deben sobresalir del agua y tener áreas de aterrizaje claras.

Propuesta inicial: desniveles consecutivos de 0,25–0,40 metros y huecos de 0,5–0,8 metros, sujetos a pruebas con el salto actual. Cada apoyo tendrá al menos 1,4 metros de profundidad útil. El tramo se puede completar sin potenciadores.

El viento baja de intensidad en el cruce. No combinar ráfaga máxima y salto obligatorio en la primera presentación del arroyo.

El agua profunda devuelve al control anterior con un fundido breve. No fingir natación si no está implementada. El agua poco profunda, si existe, tiene suelo y contacto claramente definidos. La condición de caída depende del cauce real, no de una franja rectangular invisible.

**Punto de control 3:** margen segura después del cruce, aproximadamente a los 285 metros.

**Tramo 4 — El refugio del caminante · 4:35–5:45.**

Un refugio de piedra y techo de ichu ofrece una pausa. Una persona del camino puede comentar que vio a alguien vigilar el sendero desde una loma. No hay combate en este nivel.

> **Caminante:** «Vi a dos hombres en la loma. Miraban a quienes pasaban, pero no bajaron al camino».
>
> **Wayra:** «¿Llevaban el quipu de alguna posta?»
>
> **Caminante:** «No alcancé a verlo. Sigue con cuidado».

La conversación es opcional y se activa con E cerca del personaje. No convertir su frase en un letrero de madera. La ruta principal continúa visible y despejada.

Aquí se presenta un punto de descanso y un pequeño abastecimiento opcional. Ver las reglas de resistencia y recursos más abajo. Si el jugador omite la interacción, puede seguir hasta la posta.

**Tramo 5 — La última ráfaga · 5:45–7:00.**

Un recorrido algo más abierto combina dos refugios naturales, una sección baja para agacharse y una última subida corta. El jugador aplica lo aprendido: observa la ráfaga, alcanza el abrigo, espera o avanza agachado y vuelve a correr al llegar la calma.

El paso bajo tiene un dintel inclinado visible y altura compatible con la cápsula agachada. De pie bloquea el paso; agachado permite cruzar en ambos sentidos. Colocar las rocas y muros para orientar, manteniendo una alternativa clara si el diseño no exige cruzarlo.

Al abandonar la ladera se distingue la posta y cambia el sonido: menos viento, más ambiente de la explanada. No añadir enemigos que aparezcan de golpe ni persecución todavía.

**Punto de control 4:** explanada de aproximación a la posta, aproximadamente a los 420 metros.

**Llegada — La noticia incompleta · 7:00–8:15.**

A unos 15 metros de la entrada aparece «E · Anunciar llegada». Wayra se detiene, toma el pututu de la cintura, lo lleva a la boca, toca y lo guarda. La música baja durante el sonido y recupera su volumen al acabar. Los subtítulos indican «[Suena el pututu]».

La persona encargada de la estación aparece en la entrada o gira hacia Wayra. Se activa el círculo amarillo junto a ella, sin colocarlo sobre una roca o una pendiente. Para entregar, Wayra debe estar dentro, sobre suelo y quieto durante un segundo.

> **Wayra:** «Traigo un aviso de la posta anterior. Busco al corredor que salió antes del amanecer».
>
> **Encargado:** «No llegó a tomar el relevo. Un caminante lo vio seguir hacia la quebrada».
>
> **Wayra:** «¿Hacia el puente?»
>
> **Encargado:** «Sí. Y otros hombres preguntaron por él. Lleva este registro a la posta del otro lado. Allí podrán decirte más».

La entrega del nuevo registro se muestra de forma visible, con una sola transferencia. El encargado recibe el aviso de Wayra y proporciona el encargo siguiente. No mostrar un inventario que siga reteniendo el objeto ya entregado.

Una toma breve del camino hacia la quebrada prepara el nivel 4. No mostrar todavía las pertenencias abandonadas: ese descubrimiento pertenece al siguiente nivel.

#### Resistencia, descanso y potenciadores

Estos sistemas son nuevos respecto a los niveles anteriores y deben implementarse y probarse antes de anunciarlos al jugador. No dejar objetos recogibles sin un efecto real.

- **Resistencia:** 100 puntos. Caminar no consume; correr consume 1,5 puntos por segundo; saltar consume 3. Desde el nivel 3 no hay regeneración gratuita al esperar, descansar ni volver a un control. Si se agota, Wayra puede caminar hasta las siguientes provisiones; no queda inmóvil ni pierde un salto que ya comenzó. Recuperar al menos 15 permite volver a correr.
- **Refugios y controles:** protegen del viento y guardan el avance; no reponen resistencia. Mantener E permite abrir la chuspa y consumir lo recogido.
- **Presentación de los consumibles:** cuencos abiertos, granos de maíz tostado, hojas curvadas con nervadura, tiras de carne seca y vaso de cerámica con agua visible. El gesto lleva la mano desde la chuspa hacia la boca, mantiene una breve fase de masticar o beber y baja el brazo. Durante el gesto, una cámara cercana de tres cuartos permite ver la acción y evita obstáculos; al terminar vuelve al seguimiento.

- **Agua — recuperación moderada:** una ración repone hasta 30 puntos de resistencia al completar una animación de beber de 3 segundos. Se encuentra en un recipiente junto al refugio. No da bonificaciones temporales ni se bebe desde una bolsa de tela.
- **Cancha — recuperación rápida:** una porción de maíz tostado repone hasta 20 puntos al completar una animación de comer de 2,8 segundos. Es el alimento más frecuente y permite recuperar un esfuerzo corto sin una pausa larga. No modifica la velocidad ni el gasto de resistencia.
- **Ch’arki — recuperación mayor:** una ración de carne deshidratada repone hasta 45 puntos al completar una animación de comer de 3,6 segundos. Es menos frecuente que la cancha y conviene reservarla para después de un tramo exigente. No aumenta la resistencia máxima ni concede velocidad adicional.
- **Hojas de coca — recuperación pequeña y ahorro temporal:** recuperan hasta 10 puntos y reducen un 20 % el gasto al correr durante 20 segundos, de 1,5 a 1,2 puntos por segundo. No cambian el coste del salto, la velocidad ni su altura. Ambos efectos comienzan al completar el gesto de 2,2 segundos. El temporizador se detiene en pausa; no acumular activaciones. Son reglas ficticias de balance.

**Diferencia de uso:** cancha para una recuperación pequeña y rápida; agua para una recuperación intermedia; ch’arki para una recuperación mayor que requiere más tiempo; coca para gastar menos al correr durante un periodo breve. Son reglas ficticias de balance, no equivalencias nutricionales, afirmaciones médicas ni efectos históricos comprobados.

**Distribución implementada:** ninguna provisión junto al inicio. En los metros 80, 125, 185, 245, 310, 365, 420 y 465 hay respectivamente cancha, coca, ch’arki, agua, cancha, agua, ch’arki y cancha. Están a 2,1 m del eje del sendero sobre apoyos accesibles. La separación máxima es de 65 m. Hay suministros para sostener la carrera y los saltos previstos; no exigir que el jugador encuentre comida escondida o atraviese cultivos.

**Chuspa e interacción:** E guarda una ración cercana sin consumirla. Mantener E abre el selector también junto a las provisiones. Elegir con las flechas y confirmar con E; Esc cierra sin consumir. El selector pausa el movimiento y los temporizadores. Capacidad: cuatro canchas y dos de cada otro recurso. Al terminar el gesto se actualizan barra y contador y se muestra la recuperación efectiva; también se guarda la resistencia.

**Consumo y animaciones:** consumir solo sobre suelo y fuera de saltos, ráfagas expuestas e interacciones de misión. Wayra toma la ración de su recipiente, realiza la acción y vuelve a guardarlo. Cancelar al moverse; si se cancela antes de completarse, no descontar el objeto ni aplicar su efecto. Descontar y aplicar una sola vez al terminar. Agua, cancha y ch’arki no se consumen con la resistencia completa, nunca superan los 100 puntos y no otorgan efectos acumulables. La coca sí puede activarse con la resistencia completa para preparar un tramo.

Si hay varios objetos cerca, seleccionar uno a la vez y mostrar su nombre antes de interactuar. Ninguno debe confundirse con el quipu de misión. Durante la cinemática y la pausa no se pueden recoger ni consumir recursos. Al guardar partida, conservar cantidades de cada ración y tiempo restante de la coca para evitar duplicaciones al cargar.

**Balance:** los consumibles sostienen la carrera; caminar permite alcanzar provisiones si se agota la resistencia. Los refugios no dan energía. Verificar con partidas humanas el gasto por desvíos y saltos adicionales.

#### Coleccionable educativo opcional

Un mirador protegido, conectado al camino mediante un desvío de 10–15 metros, contiene un registro sobre la comunicación mediante relevos. Activarlo con E abre una ficha de lectura con diseño de manta y un botón para cerrar.

Texto propuesto:

> «Los recados recorrían largas distancias mediante relevos entre mensajeros. El recorrido dependía de caminos, postas y personas que conocían el territorio».

Contabilizar «Registros 0/1» y guardar su descubrimiento una sola vez. Los datos históricos y los nombres locales que se incorporen a la ficha deberán verificarse con fuentes antes de publicar. La ficha no atribuye una fecha exacta de origen al oficio.

#### Animaciones y actuación

**Entrega inicial y final.** Los personajes miran primero a su interlocutor y después al objeto. Un hombro inicia el gesto, el brazo se extiende con aceleración suave y el receptor responde con un pequeño retraso. El quipu permanece sujeto hasta el momento de contacto y cambia de mano una sola vez. Evitar brazos sincronizados como espejo, codos que se invierten, muñecas caídas y cordones que flotan. Añadir respiración y movimientos sutiles de cabeza, sin balancear todo el cuerpo continuamente.

**Caminar y correr.** Ajustar la velocidad de la animación al desplazamiento real; sin pies hacia atrás ni deslizamiento perceptible. Al girar, reducir la marcha, orientar el cuerpo y acelerar otra vez. El personaje no cruza un muro para alcanzar una marca de animación.

**Viento.** En ráfaga, inclinar ligeramente torso y cabeza contra el aire, con brazos que ayudan al equilibrio. Los pies mantienen apoyo. Agacharse entra y sale con transición; bajo un techo se conserva la postura baja hasta poder levantarse.

**Salto.** Anticipación corta, impulso, fase aérea, recepción con flexión de rodillas y retorno al movimiento. No reproducir aterrizaje antes de tocar suelo ni fijar la duración del vuelo independientemente de la física.

**Pututu.** Sacar de la cintura, sujetar, acercar a la boca, reproducir sonido y guardar. El instrumento no se duplica en la cintura y la mano. La animación espera la duración real del audio; el sonido comienza cuando la caracola llega a la boca.

**Personajes secundarios.** Ropa y siluetas distintas para campesino, caminante y encargado. Si alguien entra en una vivienda, debe caminar por su abertura, apartar la estera, cruzar el umbral y soltarla. No usar una hoja rígida que gire como puerta moderna.

**Ropa y cabello.** En reposo, la lliclla cae hacia la espalda por gravedad. Al correr o recibir viento, responde con retraso y se abre moderadamente hacia los costados. No alargar el tejido para simular viento. Comprobar contacto con cuello, hombros, chuspa, uncu y brazos. El cabello se mueve con amplitud pequeña, conservando su volumen y sin atravesar la cara.

#### Cámara, controles y pausa

WASD o flechas para moverse; ratón para mirar; Espacio para saltar; C para agacharse; E para interacción contextual. Control no activa agacharse. Mantener la misma carrera automática del nivel 2; si se añade una tecla de carrera, explicarla y aplicarla de forma coherente a ambos niveles.

La cámara sigue al personaje, reduce la distancia ante obstáculos y se recupera al despejarse. No atravesar casas, troncos ni laderas. Evitar que la copa de un árbol oculte permanentemente al jugador. Ofrecer sensibilidad ajustable y desactivar cualquier sacudida opcional.

Esc abre una manta de pausa con Reanudar, Reiniciar desde el control y Menú principal. Detiene movimiento, cronómetros de ráfaga, consumibles y cinemáticas. Reanudar conserva estados, sin lanzar un salto ni una interacción por una tecla mantenida. Al volver al menú se restaura la escala de tiempo.

#### Construcción del mapa y colisiones

El sendero tiene tierra compactada y tramos de piedra con bordes irregulares, y se reconoce también en el minimapa. Su superficie visual y su colisión coinciden. Ningún acceso al arroyo, apoyo o umbral deja hundirse los pies. Las uniones importantes tienen transiciones suaves y se prueban en ambos sentidos.

Las barreras invisibles siguen las curvas a unos 30 metros de los costados del camino, con altura suficiente para no atravesarlas saltando. Los desvíos, refugios y coleccionables quedan dentro de este espacio. Las montañas de fondo no exigen poblar todo el terreno de objetos interactivos.

Los letreros se ubican cerca del borde del sendero, con el cartel entero fuera del paso. Dejar despejada la vista de aproximación; ningún árbol o roca debe ocultarlos. Usar mensajes cortos tallados, como «PASO DEL VIENTO», «ABRIGO» y «POSTA DEL VALLE». Las conversaciones aparecen como subtítulos, no como letreros que hablan.

Las viviendas usan paredes de piedra con juntas y techos de haces de ichu. Quedan sobre explanadas propias, fuera de los andenes. Las esteras de entrada son flexibles y usan sonido de fibras, sin golpes de tablones.

Asentar las rocas usando su geometría real y la altura del terreno en varios puntos. Los apoyos transitables tienen colisión continua, sin huecos ni bordes invisibles elevados. Los árboles bloquean con el tronco; las hojas no forman jaulas de colisión que atrapen al jugador. Despejar los cultivos y el espacio donde permanecen los personajes secundarios.

#### Iluminación y sonido

Mañana algo más fresca que en el nivel 2. La niebla lejana aporta profundidad, pero permite ver el siguiente apoyo. Las copas proyectan sombras de hojas; su alcance y calidad se ajustan al rendimiento disponible.

La mezcla tiene música, ambiente, efectos e interfaz separados. El viento aumenta durante la ráfaga y baja tras una roca; no saturar los altavoces. El arroyo se oye suavemente cerca del cauce y disminuye al alejarse. Los pasos alternan grabaciones según pasto, tierra, piedra o cubierta de fibras; su cadencia sigue la distancia recorrida y se detiene en el aire.

El salto usa impulso de pie y roce de ropa. Al aterrizar se oye el contacto correspondiente a la superficie. Agacharse utiliza un roce breve, no un sonido que se repite cada fotograma.

Al tocar el pututu, bajar suavemente la música aproximadamente al 40 % de su volumen previo y el ambiente al 55 %. Recuperarlos después de guardar la caracola. Si la entrega termina durante esta acción, la mezcla final tiene prioridad y no debe volver a subir por una rutina anterior.

Al aparecer la manta de misión completada, música y ambiente bajan al 50 % de sus valores de exploración en aproximadamente 1,2 segundos. Conservar el volumen general elegido por el jugador.

#### Minimapa e interfaz

Mostrar el recorrido completo con norte fijo, ubicación y rumbo de Wayra, arroyo, cruces y posta. La posición se calcula desde la cámara real del minimapa para evitar inversión de ejes. No dibujar una línea de ruta sobre un mapa que representa otro terreno.

Mantener objetivo breve, distancia al destino, quipu de misión y registro opcional. La barra de resistencia aparece al gastar esfuerzo y se atenúa al recuperarse. Mostrar el potenciador solo mientras esté activo. No cubrir el centro del salto con tutoriales o mensajes.

Los textos conservan el estilo de tejido y tonos tierra, con suficiente contraste. El tamaño debe ser legible a 1280 × 720. La historia completa sigue accesible desde el menú principal, sin mezclarla con avisos de control.

#### Guardado y finalización

Guardar al comenzar, en cada punto de control y al completar. Registrar escena, punto seguro, encargo vigente, coleccionable y consumibles recogidos. No reaparecer con el personaje dentro de una roca, en el cauce o bajo una viga.

Al caer, conservar el encargo y el coleccionable; reponer la resistencia básica. No duplicar consumibles ya recogidos ni descontar una recompensa dos veces. Al reiniciar el nivel desde cero, explicar qué progreso local se reinicia, manteniendo los niveles desbloqueados.

La llegada requiere haber recorrido los controles obligatorios, anunciarse, guardar el pututu y detenerse junto al encargado. Bloquear la repetición del diálogo final durante su ejecución.

**Manta final:**

> **AVISO ENTREGADO**
>
> El relevista no llegó al valle. Su rastro continúa hacia la quebrada.

Botones ordenados: Repetir nivel, Menú principal y Siguiente nivel. El último carga `Nivel04_PuenteAbismo` solo cuando exista y esté incluido en la campaña; mientras tanto, deshabilitarlo y mostrar «Nivel 4 en preparación». Guardar la finalización antes de habilitar navegación.

#### Reutilización y organización en Unity

- `Assets/Scenes/Campana/Nivel03_ValleVientos.unity`: escena del nivel.
- `Assets/Niveles/ValleVientos/`: terreno, cauce, mallas de apoyos y composición propia.
- `Assets/Audio/Nivel03/`: sonidos nuevos con sus fuentes y licencias; reutilizar las bibliotecas anteriores cuando correspondan.
- `Assets/Scripts/Mundo/`: misión, zonas de viento y controles de progreso. Separar estos comportamientos del dibujo de interfaz.
- Prefabs compartidos para vivienda, letrero, rocas y personajes; variantes para secundarios y cambios de ropa. No modificar el prefab del jugador para vestir a un campesino.

Reutilizar el acabado de piedra, ichu y tejidos; los árboles con sombras, los pasos y el pututu. Crear una distribución nueva, sin copiar casas o carteles a posiciones que entren en conflicto con el relieve. El Valle anterior continúa como borrador y no sustituye esta escena.

#### Rendimiento y pruebas de aceptación

El objetivo es una versión estable en el equipo actual. No prometer ausencia absoluta de errores ni fluidez sin medir. Probar Editor y compilación por separado; no generar un EXE hasta que se solicite.

1. Medir una primera partida y ajustar la ruta al objetivo de 7–9 minutos sin esperas artificiales.
2. Completar sin consumibles, sin coleccionables y usando únicamente los controles básicos.
3. Recorrer el camino en ambos sentidos, bajo cada fase de viento, y comprobar que se puede salir de refugios, pasos bajos y alrededores de árboles.
4. Probar cada salto caminando y corriendo, incluyendo aterrizaje en el borde y caída al arroyo. No permitir pies hundidos ni saltos obligatorios sin margen.
5. Revisar piedras, casas, cultivos y letreros desde la cámara del jugador. Ninguna roca flota, ningún personaje ocupa una piedra y ningún árbol invade el cultivo o tapa el mensaje.
6. Comprobar las barreras a ambos lados de cada curva y que todo desvío opcional sea accesible.
7. Revisar cada entrega, giro, manipulación de la estera y uso del pututu a velocidad normal y lenta. Confirmar un solo instrumento y un solo quipu durante sus transferencias.
8. Pausar durante ráfaga, salto, consumo, conversación y cinemática; reanudar sin saltos de estado ni sonidos duplicados.
9. Caer y cargar partida desde cada control, cerrar y volver a abrir, y completar dos veces. Confirmar guardado correcto, sin recompensas duplicadas.
10. Verificar minimapa, subtítulos y botones a 1280 × 720 y 1920 × 1080, y comprobar el volumen durante pututu y manta final.
11. Medir CPU, GPU y memoria en las zonas de árboles, viento y agua. Ajustar sombras, distancia de dibujo y frecuencia del minimapa según los resultados.
12. Mantener geometría de paja agrupada y texturas filtradas; comprobar varios minutos que no reaparezcan rayitas, parpadeos, errores de consola o acumulación de objetos temporales.
13. Probar la cadena completa menú → nivel 1 → nivel 2 → nivel 3, y la vuelta al menú desde la pausa y la manta final.

### NIVEL 4 — El puente del abismo

**Estado:** primera versión jugable construida en Unity el 18 de noviembre de 2026. Escena `Assets/Scenes/Campana/Nivel04_PuenteAbismo.unity`, conectada al botón Siguiente del nivel 3 y a Continuar en el menú. Incluye el mirador compartido, descenso, puente de ichu de 36 metros, marcha cuidadosa, viento sobre cuerdas y personaje, río al fondo, controles, inventario persistente, alimentos, pausa, caídas mortales, reintento y examen animado de pertenencias con subtítulos y manta final.

**Verificación realizada:** 145/145 muestras del piso del puente con apoyo, recorrido automatizado mediante el mismo controlador del personaje hasta la otra orilla y los controles, muerte por caída de más de seis metros y por caer al abismo, reaparición en el último control, cursor libre en pausa, consumo de cancha y final tras examinar los objetos. El nivel 5 permanece deshabilitado mientras no exista. La duración de 5–7 minutos y el rendimiento sostenido requieren medir partidas humanas; no se han validado como resultados.

**Acabado y límites actuales:** locomoción y gestos utilizan el rig existente. Se conserva el piso curvo estable y se limita el movimiento a cuerdas superiores, compensación corporal y ropa para no separar los pies del apoyo. No se simula estructuralmente todo el puente con cuerpos rígidos. Los diálogos son escritos; no hay voces grabadas.

**Escena creada:** `Assets/Scenes/Campana/Nivel04_PuenteAbismo.unity`. Para probarla directamente: **Chasqui → Abrir nivel 4 · El puente del abismo**, después Play.

**Ajustes de continuidad y chuspa:** al pasar desde el nivel 3 se conservan la posición relativa del jugador, su orientación y el encuadre de la cámara. El mirador comparte relieve, vegetación y camino inicial de 5,2 m de ancho. E recoge provisiones próximas; mantener E abre la chuspa incluso junto a ellas. Capacidad: cuatro raciones de cancha y dos de cada otro recurso. Se prepara al menos una coca y un ch’arki al entrar; el resto del inventario se conserva. El gesto de consumo alcanza el bolso real antes de mostrar el alimento y llevarlo a la boca. Verificados recogida con E, prevención de duplicados y consumo de ch’arki y coca.

**Salida del nivel:** después de las pertenencias, el sendero continúa hasta unos 423 m y gira hacia la montaña. El final narrativo ocurre al examinar el hallazgo; el camino visible sigue más allá y se pierde tras el recodo.

**Río, consumo y reintento (18/11/2026):** agua elevada de −21,15 m a −18,5 m; superficie recortada según la intersección con las orillas del terreno. Se retiraron las filas de grandes rocas y se sustituyeron por doce piedras dispersas, principalmente sumergidas. Hay un control adicional a los 68 m, antes de la ladera expuesta: al reintentar se regresa al apoyo seguro sin reproducir la presentación. El gesto de comer elige el hombro más cercano a la chuspa, mantiene el codo hacia fuera y lleva la mano por delante del torso. Oculta el puntero durante el gesto, conserva el modo de cámara anterior y respeta el cursor de pausa.
**Modalidad:** 3D en tercera persona, cámara con ratón y protección contra obstáculos.
**Duración objetivo:** 5–7 minutos en una primera partida. Es una estimación de diseño que deberá medirse jugando; no alargar artificialmente la travesía con esperas.
**Longitud orientativa:** 350–450 metros de recorrido, incluido un puente de unos 30–40 metros. Ajustar las medidas al movimiento real del personaje.
**Objetivo:** atravesar la quebrada, buscar al relevista desaparecido y seguir su rastro hacia la montaña.
**Tema jugable:** avanzar con cuidado sobre un puente de fibras, interpretar el viento y observar el entorno.
**Diferencia respecto al nivel 3:** recorrido más estrecho y vertical, gran profundidad visible, puente como elemento central y final de investigación. No repetir el paseo por el valle ni otra entrega de quipu.

#### Continuidad exacta con el nivel 3

El nivel comienza en el mismo mirador de la quebrada donde terminó el nivel 3: conservar el abrigo abierto de ichu, los pilares, banco, pretil, rocas y encargado en sus posiciones relativas. Wayra aparece donde se detuvo para hablar. No volver a la posta del nivel 2 ni cambiar inexplicablemente de lugar.

El inicio muestra la salida que antes quedaba más allá del mirador. El camino desciende pegado a la pared de roca y revela el puente al doblar un recodo. Compartir esta pequeña zona de enlace mediante un prefab o subescena para que futuros cambios se reflejen en ambos niveles.

Wayra conserva el registro y el recado recibidos al finalizar el nivel 3. No recibe otro quipu al empezar ni devuelve su encargo a una copia del mismo personaje.

**Presentación inicial, 5–8 segundos y omisible:**

- Plano breve desde el mirador hacia el recodo; todavía no enseñar todo el puente.
- Encargado: «El paso sigue abierto. Si buscas al corredor, pregunta en el camino de la montaña».
- Wayra asiente, ajusta la chuspa y se orienta hacia la salida. No realizar otra animación de entrega.
- La cámara vuelve suavemente detrás de Wayra y devuelve el control. E permite omitir; Esc pausa también esta presentación.

La aventura mantiene la época y la ficción narrativa ya definidas. No introducir españoles, armas de fuego ni noticias sobre Atahualpa en este nivel.

#### Aspecto del escenario

La quebrada debe sentirse distinta del valle: paredes rocosas altas, repisas, un sendero tallado en la ladera, vegetación dispersa y agua al fondo. La profundidad se comunica con perspectiva, bruma suave y sonido lejano, sin ocultar los apoyos del jugador.

El río recorre el fondo del barranco, entre riberas que tocan el agua. Su malla, posición del objeto y altura del terreno se verifican juntas: no volver a producir una cinta suspendida ni rocas flotantes. La mayoría de las piedras del cauce estarán parcialmente sumergidas; unas pocas quedarán enterradas en las orillas.

No llenar las paredes verticales de piedras pequeñas como si estuvieran pegadas en el aire. Los salientes deben pertenecer a la forma de la pared o estar apoyados sobre una repisa real. Usar conjuntos grandes de roca antes que cientos de objetos sueltos.

Luz de tarde algo más fría que en el valle, con reflejos suaves en el agua. Mantener legibilidad bajo el abrigo y en el interior de los recodos. La niebla no debe convertir la otra orilla en una pared opaca.

#### Recorrido completo

**Tramo 1 — Salida del mirador, aproximadamente 0:00–0:45.** Wayra deja atrás al encargado y avanza por un sendero de piedra junto al pretil. Un cartel junto al camino indica «PASO DE LA QUEBRADA». La composición muestra una salida clara; no colocar otro arco para agacharse ni repetir las hileras de obstáculos del nivel anterior.

No colocar comida junto al banco inicial. La primera provisión del nivel 4 aparece a los 64 m; las siguientes sostienen el recorrido. No obligar a comer con la resistencia completa.

**Tramo 2 — Descenso por la ladera, aproximadamente 0:45–1:45.** El sendero baja mediante peldaños anchos e irregulares. Hay dos pequeñas interrupciones que se superan con saltos cortos y una repisa de descanso. Cada lugar de recepción debe verse antes de saltar. No poner paredes invisibles sobre un apoyo que parece accesible.

En el recodo se revela por primera vez el puente completo y el río muy abajo. Una breve orientación de cámara puede acompañar el descubrimiento, sin quitar el control ni girar bruscamente la vista. Aquí se registra el primer punto de control.

**Tramo 3 — Preparación en el estribo, aproximadamente 1:45–2:20.** Una zona de piedra firme permite observar las cuerdas y la otra orilla. El puente se mueve ligeramente, las fibras crujen y el viento se escucha con más claridad. No lanzar al jugador al puente durante una ráfaga máxima.

Ayuda: «El puente se mueve con el viento. Avanza con calma y mantén C para bajar el cuerpo». El texto aparece una vez y vuelve a consultarse al acercarse al estribo. No interrumpir la observación con un panel grande.

**Tramo 4 — Cruce del puente, aproximadamente 2:20–3:30.** El primer tercio permite aprender el balanceo con viento suave. En el centro llega una ráfaga anunciada; la postura baja reduce el desplazamiento y la agitación de la ropa. El último tercio tiene algunas fibras superficiales desgastadas que crujen, pero conserva una ruta de apoyo continua.

El puente no es una secuencia de tablones de madera flotantes. La dificultad consiste en avanzar y mantener una dirección estable; no exigir saltar sobre agujeros invisibles ni depender de un botón pulsado en un único fotograma. El cruce no tiene que durar más por obligación si el jugador comprende el comportamiento rápidamente.

**Tramo 5 — La otra orilla, aproximadamente 3:30–4:30.** El segundo estribo conduce a una explanada de roca, visualmente distinta de la salida. Se registra otro control al tocar suelo firme. Wayra puede recuperar el aliento o revisar su chuspa, mientras el sonido del puente se aleja.

Un desvío breve permite observar los anclajes del puente y consultar un registro educativo opcional. No colocar el coleccionable al borde de una caída sin apoyo ni exigir volver a cruzar para encontrarlo.

**Tramo 6 — Las pertenencias abandonadas, aproximadamente 4:30–6:00.** A unos metros del sendero, en una repisa segura protegida por una roca, se ve una chuspa gastada, una sandalia suelta y fibras de una faja. La disposición debe parecer una escena encontrada, no una fila de premios brillantes.

Wayra se acerca y aparece «E · Examinar las pertenencias». Mantener el texto de interacción separado de los subtítulos. Al pulsar E, se agacha, observa la chuspa y levanta brevemente una pieza de tejido; sus manos deben llegar al objeto y devolverlo al mismo lugar.

Subtítulos propuestos:

- Wayra: «Una chuspa de corredor… ¿por qué la habrá dejado aquí?».
- Wayra: «El sendero continúa hacia la montaña. Tengo que seguir».

El hallazgo no demuestra por sí solo una muerte ni un ataque. No mostrar al relevista muerto ni explicar todavía qué ocurrió. El nivel 5 desarrolla la siguiente pista.

#### Puente de fibras: forma y funcionamiento

La referencia visual es la tecnología de puentes de fibras vegetales representada por Q’eswachaka, ya recogida en la fuente H10. El lugar del juego es ficticio: no anunciar que se reproduce exactamente el puente real ni atribuirle dimensiones inventadas.

- Cables principales gruesos y trenzados, piso tejido, amarres visibles y pasamanos de fibras a ambos lados.
- Estribos de piedra conectados al sendero; ninguna entrada elevada sobre el terreno ni pies hundidos al pasar del suelo al tejido.
- Anchura suficiente para leer los límites y caminar con la cámara detrás del personaje. Propuesta inicial: 1,6–1,9 metros útiles.
- Curva de suspensión suave: la superficie visual y la colisión recorren la misma altura, sin utilizar un collider plano debajo de un piso curvo.
- Balanceo contenido, con inicio y retorno progresivos. No estirar el tejido como goma ni separar las cuerdas del estribo.
- Barreras laterales ajustadas a los pasamanos; no una caja invisible alta que impida saltar lejos del puente.
- No permitir que el personaje atraviese las cuerdas o se quede atrapado entre varios colliders pequeños.

**Propuesta técnica inicial:** segmentar la superficie de apoyo y mantener colisiones simples y continuas. El movimiento visual del piso debe coincidir con el apoyo del personaje. Si aún no se logra una plataforma móvil estable, reducir el balanceo antes de entregar una versión en la que los pies floten o atraviesen el tejido. No resolverlo aumentando la cámara temblorosa.

En el puente, el desplazamiento pasa suavemente a una marcha cuidadosa; no correr automáticamente sobre el vacío. Al salir, recuperar el control normal sin cambiar la dirección elegida. Antes de entrar debe quedar clara esta diferencia de movimiento.

#### Viento, resistencia y recursos

El viento conserva señales previas: sonido, movimiento de cuerdas, ropa y vegetación. La ráfaga fuerte se anuncia al menos 1,5 segundos antes. No aplicar una fuerza lateral oculta que empuje al personaje fuera del puente sin tiempo para reaccionar.

C agacha a Wayra y reduce la exposición. No modificar su altura de forma que atraviese el piso o deje de caber entre los pasamanos. La cámara acompaña la postura sin esconder el camino.

Mantener el balance del nivel 3: 100 puntos, correr gasta 1,5 por segundo y saltar 3. No hay recuperación automática ni energía gratuita al reintentar. Caminar y la marcha cuidadosa del puente no gastan resistencia. Agotarse no bloquea el paso ni provoca una caída automática.

Conservar los efectos diferenciados y sus animaciones:

- Cancha: recupera hasta 20 puntos en 2,8 segundos.
- Ch’arki: recupera hasta 45 puntos en 3,6 segundos.
- Agua: recupera hasta 30 puntos en 3 segundos, con vaso de cerámica.
- Coca: recupera hasta 10 puntos y reduce un 20 % el gasto al correr durante 20 segundos, tras el gesto de 2,2 segundos.

Provisiones del nivel 4: cancha a los 64 m, agua a los 139 m, ch’arki a los 219 m, agua a los 280 m y ch’arki a los 330 m. La coca se conserva en la chuspa; no es requisito para cruzar. Los objetos tienen nombres legibles y se guardan antes de consumirlos.

**Continuidad de inventario implementada:** transferir las cantidades y el estado de los recursos desde el nivel 3. No reiniciar la chuspa al cambiar de escena, no duplicar las provisiones al cargar y no introducir valores negativos. Las raciones recogidas persisten al volver al control; una ración cancelada no se descuenta.

Comer y beber solo se permiten en suelo firme, fuera del puente y de sus entradas estrechas. La cámara de consumo debe evitar paredes y volver al seguimiento; Esc congela el gesto y libera el puntero. El nivel puede completarse caminando y sin usar coca.

#### Animaciones necesarias

**Salida:** asentimiento y ajuste breve de la chuspa, sin otra entrega del mensaje.

**Descenso y saltos:** apoyo sobre peldaños, anticipación, impulso, vuelo y recepción según el contacto real. No repetir un ciclo de carrera hacia atrás ni hacer aterrizajes antes de tocar el suelo.

**Cruce:** pasos cortos, torso que compensa ligeramente el balanceo y brazos que se abren con moderación. Si una mano toca un pasamanos, debe seguir su posición; no atravesarlo ni agarrar aire. La pose de equilibrio no sustituye la animación de los pies.

**Ropa y cabello:** la capa responde al viento y cae al detenerse, con colisiones contra cuerpo y chuspa. El cabello acompaña el movimiento con amplitud contenida; no debe comportarse como un casco rígido ni atravesar los hombros.

**Hallazgo final:** orientar el cuerpo hacia la chuspa, bajar, extender la mano, observar el tejido y devolverlo antes de levantarse. Interpolar cada fase y permitir omitirla con E, dejando los objetos y al personaje en un estado válido.

No llamar terminada una animación solo porque el texto aparece: comprobar también manos, contacto con el piso, objetos y devolución del control.

#### Cámara, interfaz y sonido

La cámara mantiene a Wayra cerca del centro. En los recodos evita la pared sin atravesarla ni acercarse tanto que oculte los pies. En el puente no oscila con toda la amplitud del tejido ni obliga a mirar al fondo del barranco.

Minimapa completo en la esquina superior derecha, con dirección consistente, ubicación del jugador, puente y salida. Marcar el destino del tramo; no revelar la chuspa abandonada desde el inicio. Actualizar con frecuencia limitada para evitar otra cámara de alto coste en cada cuadro.

Indicaciones, letreros y subtítulos mantienen el estilo legible del juego. El texto debe ajustarse al soporte con margen, incluidos los carteles pequeños. Las ayudas son una convención de interfaz del videojuego.

Doble clic activa la cámara durante el juego. Con Esc abierto, el puntero está visible y libre; ningún clic de los botones puede volver a capturarlo. Reanudar, volver al control y menú funcionan con un clic normal. Las mismas reglas se aplican a la manta final.

Sonido previsto: agua lejana con posición espacial, viento por exposición, crujidos de fibra por paso y tensión, sandalias sobre piedra o tejido, ropa y acciones. No utilizar golpes de madera para un piso de fibras. Variar los pasos sin repetir siempre la misma muestra.

La música deja oír el río y las cuerdas. Si se permite tocar el pututu desde el estribo, la música baja mientras suena y se recupera después; tocarlo no genera una respuesta que contradiga el misterio. Durante los subtítulos importantes y al terminar, reducir el fondo aproximadamente a la mitad, con una transición suave.

#### Controles, caídas y guardado

Tres controles seguros: salida del mirador, estribo antes del puente y explanada después del cruce. Guardar al estar apoyado; no guardar en mitad de un salto, sobre el puente o durante una caída.

**Caída mortal al barranco.** Si Wayra pierde el apoyo de la ladera rocosa o del puente y cae al abismo, muere. Las zonas profundas del barranco y el río del fondo son mortales; no permiten aterrizar y seguir caminando debajo del nivel. La profundidad y los bordes deben comunicar el peligro antes de saltar. No colocar una pared invisible que impida toda caída.

**Caídas a superficies seguras.** Un salto normal o una caída pequeña sobre una repisa transitable no mata al personaje: reproduce recepción y recuperación del equilibrio. Como valor inicial de balance, una caída libre de 6 metros o más hasta una superficie firme es mortal; medir el descenso vertical real desde que pierde apoyo, no la distancia horizontal ni cada peldaño de una bajada. Ajustar el umbral después de probar los saltos previstos. Entrar en una zona de muerte del abismo es mortal independientemente de ese umbral.

**Secuencia de muerte.** Bloquear el movimiento y las interacciones una sola vez, cancelar el consumo sin gastar una ración incompleta y dejar de reproducir pasos. Mostrar una caída breve, sin detalle gráfico, seguida de un fundido y el mensaje «Has caído al abismo». No mantener al jugador esperando hasta tocar el fondo de un barranco muy profundo. Dar la opción «Reintentar» y permitir confirmarla con E o con un clic; Esc conserva acceso al menú de pausa.

Al reintentar, reaparecer en el último control seguro con un fundido corto y sin reconstruir duplicados del personaje. Reiniciar velocidad vertical y horizontal, postura, animación de caída y estado de la cámara. Conservar misión, coleccionables y cantidades válidas del inventario; no duplicar alimentos ni devolver los ya consumidos. Restaurar la resistencia al valor registrado al activar el control, con un mínimo de 15 puntos para evitar un bloqueo; guardar el nuevo inventario al consumir, sin convertir la muerte en una forma de reabastecerse. No reproducir múltiples impactos o pasos por el teletransporte.

**Detección implementada en la primera versión de Unity:** volúmenes de muerte limitados al abismo y seguimiento de la altura de caída hasta el contacto real con suelo. Excluir al propio personaje de las comprobaciones de apoyo y respetar el movimiento del puente para no acumular caídas falsas con su balanceo. Una bandera de muerte impide disparar varias veces la secuencia. Probar caída desde ladera, desde puente y desde un salto, además de peldaños, repisas y reaparición.

Evitar paredes invisibles sobre el camino o límites atravesando el puente. En el resto del mapa, colocar los límites donde el paisaje comunica que termina la zona accesible. No usar una barrera para ocultar una entrada mal alineada.

Pausar y volver al menú no debe perder el control alcanzado. Repetir nivel reinicia sus objetivos de forma explícita y no borra los niveles anteriores.

#### Final y enlace con el nivel 5

El nivel termina al examinar las pertenencias y completar u omitir válidamente la animación. No colocar otro encargado esperando un quipu ni repetir el círculo de entrega del nivel 3.

La cámara encuadra el sendero que sube hacia la montaña. Wayra guarda su propio encargo, se levanta y mira la salida. Música baja y aparece la manta bordada:

**«AL OTRO LADO DEL ABISMO»**

«El puente quedó atrás. Las pertenencias del corredor están abandonadas, pero su rastro continúa hacia la montaña».

Botones ordenados: repetir, menú principal y siguiente nivel. El botón siguiente carga `Nivel05_MontanaSagrada` únicamente cuando exista y esté incluido en la campaña; mientras tanto, mostrar «Nivel 5 en preparación» sin permitir una carga fallida.

El nivel 5 empieza exactamente en esta explanada: mismas pertenencias, roca de abrigo y acceso al sendero de montaña. No repetir la búsqueda ni desplazar a Wayra a otro pueblo.

#### Recursos reutilizables y trabajo nuevo

Reutilizar personaje, locomoción, ropa, cámara, chuspa, comida, audio compatible, interfaz tejida, roca y materiales de ichu. Reutilizar el mirador final del nivel 3 como zona de enlace compartida.

Crear el relieve de la quebrada, sendero de descenso, estribos, puente largo, comportamiento de marcha cuidadosa, apoyos sincronizados, animación de equilibrio, conjunto de pertenencias y examen final. El puente del Valle sirve de referencia visual, no se estira sin revisar malla, colisiones y trenzado.

Agrupar fibras y rocas lejanas, reutilizar materiales y limitar sombras pequeñas. No generar cada hebra como objeto con collider ni simular todo el puente como miles de rigidbodies. Priorizar fluidez y apoyo estable antes de añadir detalle.

#### Comprobaciones antes de dar el nivel por terminado

1. La transición 3 → 4 conserva la posta y el encargo; no se repite la entrega.
2. Todo el recorrido es completable con teclado, cámara y salto habituales, también con resistencia baja.
3. Estribos, piso y personaje coinciden: no hay escalones invisibles, hundimientos ni pies en el aire.
4. Viento y balanceo tienen aviso, amplitud razonable y colisiones coherentes.
5. Agua dentro del cauce, transformación del objeto correcta y rocas apoyadas o parcialmente sumergidas.
6. La ropa y las manos no atraviesan cuerpo, cuerdas ni objetos examinados.
7. Esc pausa presentación, movimiento, consumo y examen; los botones aceptan clic sin capturar la cámara.
8. Inventario y controles sobreviven a reintentos y cambios de escena sin duplicaciones.
9. El registro es opcional y el hallazgo final solo se completa una vez.
10. La manta final y el botón siguiente gestionan correctamente un nivel 5 todavía no construido.
11. Medir una primera partida para ajustar la duración; probar varios minutos en la computadora del proyecto y revisar consola, memoria y rendimiento.

### NIVEL 5 — La montaña sagrada

**Escenario:** Montañas elevadas, caminos de piedra y zonas frías.

Wayra debe subir una montaña enfrentando agotamiento, poca visibilidad y bajas temperaturas. El jugador administra su resistencia y los recursos de su chuspa. Al llegar al tambo encuentra a un viajero herido, quien advierte que algunas postas están siendo atacadas.

### NIVEL 6 — Los ojos del puma

**Escenario:** Bosques andinos, rocas y senderos solitarios.

Mientras atraviesa un bosque, Wayra detecta la presencia de un puma. Primero debe esconderse y evitar provocarlo. Si el animal se aproxima, puede utilizar su huaraca para lanzar piedras y ahuyentarlo. Al terminar, consigue llegar a un campamento donde aprende técnicas básicas de defensa.

### NIVEL 7 — Territorio rebelde

**Escenario:** Aldeas y caminos controlados por un grupo armado.

Un grupo de guerreros que se opone al dominio incaico ha bloqueado el camino. Wayra debe atravesar la zona utilizando sigilo, escondites y su huaraca para distraer o defenderse de sus perseguidores. Descubre que los guerreros están buscando específicamente a los mensajeros del Inca.

### NIVEL 8 — El tambo destruido

**Escenario:** Centro de abastecimiento atacado.

Wayra llega a un tambo parcialmente incendiado. Debe buscar sobrevivientes, conseguir suministros y recuperar un quipu perdido. Un anciano le cuenta que varios mensajes han desaparecido y que alguien está saboteando las comunicaciones imperiales.

### NIVEL 9 — El mensaje falso

**Escenario:** Camino hacia Andahuaylas y pequeño centro administrativo.

Wayra encuentra a otro chasqui que transporta un mensaje supuestamente idéntico al suyo. Ambos acuden a un quipucamayoc, quien identifica diferencias sospechosas entre los registros. El protagonista descubre que están circulando órdenes falsificadas y que su misión puede ser mucho más importante de lo que imaginaba.

### NIVEL 10 — La persecución

**Escenario:** Desfiladeros, cavernas y caminos nocturnos.

Un grupo de guerreros intercepta a Wayra para arrebatarle el mensaje. El jugador debe escapar utilizando velocidad, saltos, escondites y su huaraca. Durante la persecución pierde parte de sus provisiones y descubre que sus enemigos conocen de antemano las rutas de los chasquis.

### NIVEL 11 — El último camino

**Escenario:** Montañas, tormentas y caminos cercanos a Vilcashuamán.

Wayra debe superar la travesía más peligrosa: lluvias, derrumbes, senderos estrechos y un tramo destruido del Qhapaq Ñan. Con sus últimas provisiones consigue alcanzar un mirador desde el cual observa humo elevándose sobre la ciudad de destino.

### NIVEL 12 — El silencio del pututu

**Escenario:** Centro administrativo de Vilcashuamán.

Wayra llega a su destino y toca el pututu, pero nadie responde. Encuentra guardias atacados y descubre que un funcionario imperial está colaborando con quienes interceptan los mensajes. Debe infiltrarse, superar a los enemigos y entregar el quipu auténtico a una autoridad de confianza.

Al recibir el quipu, la autoridad de confianza contrasta el registro con el recado oral de Wayra y descubre una advertencia: varias rutas del imperio han sido comprometidas y el próximo objetivo podría ser Cusco.

Wayra cree que su misión ha terminado, pero un chasqui herido llega desde el norte y pronuncia:

**«Las postas han dejado de responder... y el peligro ya está en camino hacia Cusco».**

La cámara se aleja lentamente mientras se escucha un pututu en las montañas.

**PANTALLA NEGRA.**

**CONTINUARÁ...**

---

## MECÁNICAS GENERALES DEL VIDEOJUEGO

- **Resistencia:** Correr, saltar y escalar consume energía.
- **Hojas de coca:** Potenciador temporal para reducir el desgaste de resistencia.
- **Alimentación:** Cancha, charqui, agua y otros alimentos para recuperar energía.
- **Pututu:** Se utiliza para anunciar la llegada a las postas y activar los relevos.
- **Huaraca:** Arma de defensa y herramienta para lanzar piedras a objetivos.
- **Quipu:** Objeto principal de las misiones; debe protegerse y entregarse.
- **Sigilo:** Esconderse de enemigos y evitar enfrentamientos innecesarios.
- **Clima:** Viento, lluvia, niebla, frío y tormentas que afectan el recorrido.
- **Puntos de control:** Chaskiwasis y tambos permiten guardar avances y reabastecerse.
- **Coleccionables:** Quipus, relatos y objetos históricos que desbloquean información sobre la civilización inca.

## FINAL Y CONTINUACIÓN

**Capítulo I: El último mensaje** termina con Wayra descubriendo el sabotaje de las rutas imperiales.

**Capítulo II: La sombra sobre Cusco** comenzaría con una misión urgente para advertir a la capital, descubrir quién está detrás de los ataques y evitar que la red de mensajeros colapse.

El segundo capítulo podría incorporar nuevas regiones, enemigos, personajes y decisiones que afecten el desarrollo de la historia.

## Reglas de juego para el prototipo

- En el pueblo, W o flecha arriba para avanzar y S o flecha abajo para retroceder. Espacio permite saltar, C permite agacharse y E se propone para hablar. A/D y las flechas laterales no desplazan al personaje. La cámara lateral muestra el avance y retroceso en pantalla con el encuadre fijo. En tercera persona se mantienen WASD o flechas, ratón, Espacio para saltar y C para agacharse. E para interactuar y Q para consumir son controles propuestos.
- Desde el nivel 3: 100 puntos; correr gasta 1,5/s y saltar 3. Cancha +20, ch’arki +45, agua +30; coca +10 y gasto al correr −20 % durante 20 s. Sin regeneración gratuita en refugios, controles o al esperar. Son valores ficticios de balance. No acumular bonificaciones.
- Las postas guardan el progreso y reponen recursos. Las caídas devuelven a un punto seguro con el mensaje y el inventario guardados. El recorrido obligatorio debe poder completarse sin consumibles.
- El pututu avisa en puntos señalados. Los quipus de misión se entregan junto con un recado oral. Los coleccionables desbloquean notas breves y los objetos de misión quedan identificados por separado.
- Las secciones de fauna, sigilo y persecución priorizan rutas de escape y peligros anticipables. Se reserva la escalada para puntos preparados, después de validar salto y controles.

## Orden de implementación

1. Construir una versión sencilla del pueblo con una pantalla lateral, la vivienda y la posta. Comprobar que el personaje conserve su aspecto y que W/S muevan a Wayra solo por el sendero y que el salto y la recuperación al agacharse funcionen bajo la viga.
2. Conectar la entrega del quipu con el prototipo del nivel 2 y probar el paso de cámara fija a tercera persona, incluidas las ayudas de control y la reaparición.
3. Añadir la historieta con narración, subtítulos y opción de omitir. Después probar el nivel 2 con el objetivo del relevo ausente.
4. Prototipar los demás niveles con geometría simple antes de invertir en decoración. Reutilizar postas, roca, vegetación y tramos de puente.

## Base histórica y fuentes

### Qué eran los chasquis

Eran mensajeros del Estado inca que transmitían información mediante relevos. Las crónicas describen noticias orales y transporte de quipus. No se debe atribuir a un solo corredor la distancia que cubría diariamente toda una cadena de mensajeros. [H3]

Un **chaskiwasi** era una estación de relevo; un **tambo** era una instalación de alojamiento y apoyo de la red vial. No construiremos todos como edificios idénticos ni trataremos los dos nombres como sinónimos. La separación entre puestos y su personal variaban. Las distancias indicadas por una crónica no son una regla universal. [H3, H4]

### Cuándo aparecieron y con qué inca

**No hay un año exacto de fundación ni un único inca fundador demostrado por las fuentes que verificamos.** No presentaremos como hecho que los inventó Inca Roca, Pachacútec u otro gobernante en una fecha precisa.

La red de caminos aprovechó antecedentes andinos y alcanzó gran expansión durante la consolidación del Tawantinsuyu, especialmente en el **siglo XV**, con continuidad en el **siglo XVI**. La cronología tradicional sitúa a **Pachacútec aproximadamente entre 1438 y 1471**; después gobernaron Túpac Inca Yupanqui y Huayna Cápac. Esto contextualiza la expansión, pero no demuestra quién creó el oficio de chasqui. Las investigaciones arqueológicas también cuestionan explicar toda la formación del Estado como obra de una sola persona. [H1, H2, H5]

Texto educativo recomendado: «En los siglos XV y XVI, una red de mensajeros por relevos mantenía comunicados los territorios del Tawantinsuyu».

- **[H1] UNESCO — Qhapaq Ñan, Andean Road System.** Antecedentes y expansión de la red: https://whc.unesco.org/en/list/1459/
- **[H2] Smithsonian, National Museum of the American Indian — Inka Road History Timeline.** Cronología educativa: https://americanindian.si.edu/nk360/inka-water/timeline/timeline
- **[H3] Inca Garcilaso de la Vega — Comentarios reales, primera parte, «Postas y correos, y los despachos que llevaban».** Crónica publicada en 1609 que requiere lectura crítica; edición alojada por el Museo Histórico Regional de Cusco: https://museogarcilaso.culturacusco.gob.pe/mediaelement/pdf/3-ComentariosReales.pdf
- **[H4] Consejo de Monumentos Nacionales de Chile — Qhapaq Ñan: el sistema vial andino y los incas en el norte de Chile.** Apartado sobre chasquis y correo: https://www.monumentos.gob.cl/sites/default/files/articles-55450_doc_pdf.pdf
- **[H5] University Press of Colorado — «The Inka Capital Cusco as the Model of an Imperial Cultural Landscape».** Formación estatal y revisión de atribuciones a Pachacútec: https://read.upcolorado.com/read/d0ed6e41-13e1-48ab-8647-13eb36ce32d7/section/f1b00e58-d5ff-4223-9b1b-1444bfd9c887
- **[H10] Smithsonian Folklife Festival — Q’eswachaka Bridge.** Tecnología y mantenimiento comunitario: https://festival.si.edu/2015/peru/traditional-knowledge/qeswachaka-bridge/smithsonian
