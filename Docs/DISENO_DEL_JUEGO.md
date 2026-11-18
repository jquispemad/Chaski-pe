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

**Escenario:** Valles, caminos estrechos y pequeños riachuelos.

Wayra debe atravesar un valle con fuertes vientos, rocas y obstáculos naturales. Aquí aprende a utilizar las hojas de coca como potenciador temporal de resistencia dentro de las reglas del juego, además de recoger agua y alimentos. Al llegar a la siguiente posta, escucha rumores de extraños hombres que están vigilando los caminos.

### NIVEL 4 — El puente del abismo

**Escenario:** Quebradas profundas y puente colgante.

El protagonista llega a un puente suspendido sobre un río caudaloso. Debe cruzarlo evitando secciones de trenzado deteriorado y esperando a que cedan las ráfagas. Al otro lado encuentra pertenencias abandonadas de otro chasqui, pero ninguna señal de su dueño.

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
- En el nivel 3 se prueba una reserva de 100 puntos de resistencia. La cancha o el charqui recuperan 30, el agua recupera 20 y la coca reduce el gasto un 25 % durante 15 segundos como regla ficticia de balance. Los efectos iguales no se acumulan. Ajustar los valores después de probar el recorrido.
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
