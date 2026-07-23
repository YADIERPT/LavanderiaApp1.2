# -*- coding: utf-8 -*-
import os
import textwrap

def xml_escape(s):
    return (str(s)
            .replace("&", "&amp;")
            .replace("<", "&lt;")
            .replace(">", "&gt;")
            .replace('"', "&quot;")
            .replace("'", "&apos;"))

def wrap_text_lines(text, width=115):
    lines = []
    for paragraph in text.split('\n'):
        if not paragraph.strip():
            lines.append("")
            continue
        wrapped = textwrap.wrap(paragraph.strip(), width=width)
        lines.extend(wrapped)
    return lines

def build_apartado_card_figma_es(apartado_num, titulo_apartado, duracion, responsables, explicacion_general, hitos_list, card_width=3454):
    # Estándar exacto de Yadier(Backend)ES.svg:
    # Ancho total: 3454px
    # Cabecera central (Banner azul): x=1104, y=104, w=1152, h=172, rx=6, fill="#3DADFF", stroke="#007AD2"
    # Contenedor interior celeste: x=120, y=408, w=3214, fill="#DBF0FF", stroke="#3DADFF"
    # Fondo exterior blanco: x=40, y=40, w=3374, fill="white", stroke="#E6E6E6"
    # Textos en #1E1E1E y subtítulos en #007AD2
    # NOTA: Sin código integrado y sin espacios o recuadros de código según solicitud del usuario.
    
    pad_top_content = 520 # y inicial para el contenido dentro de #DBF0FF
    current_y = pad_top_content
    
    svg_elements = []
    
    # 1. Explicación General de lo que se hace en ese apartado
    gen_lines = wrap_text_lines(explicacion_general, width=112)
    svg_elements.append(f'''
    <!-- Explicación General del Apartado -->
    <text x="200" y="{current_y}" font-family="'Segoe UI', Inter, -apple-system, sans-serif" font-size="38" font-weight="bold" fill="#007AD2">EXPLICACIÓN GENERAL DEL APARTADO</text>
    ''')
    current_y += 54
    
    for line in gen_lines:
        svg_elements.append(f'<text x="200" y="{current_y}" font-family="\'Segoe UI\', Inter, sans-serif" font-size="34" fill="#1E1E1E">{xml_escape(line)}</text>')
        current_y += 46
        
    current_y += 50 # Separación antes de los hitos
    
    # 2. Hitos por cada apartado unidos (sin importar roles, sin código, sin recuadro placeholder)
    for subtitulo, encargados, exp_hito in hitos_list:
        # Subtítulo Hito
        svg_elements.append(f'''
        <!-- Subtítulo del Hito -->
        <circle cx="174" cy="{current_y - 12}" r="12" fill="#007AD2"/>
        <text x="200" y="{current_y}" font-family="'Segoe UI', Inter, sans-serif" font-size="38" font-weight="bold" fill="#1E1E1E">{xml_escape(subtitulo)}</text>
        ''')
        current_y += 48
        
        # Encargado / Rol en Negrita (#007AD2 para resaltar el responsable)
        svg_elements.append(f'''
        <text x="200" y="{current_y}" font-family="'Segoe UI', Inter, sans-serif" font-size="32" font-weight="bold" fill="#007AD2">{xml_escape(f"Responsable del Hito: {encargados}")}</text>
        ''')
        current_y += 46
        
        # Explicación Técnica e Impersonal del Hito
        hito_lines = wrap_text_lines(exp_hito, width=112)
        for hline in hito_lines:
            svg_elements.append(f'<text x="200" y="{current_y}" font-family="\'Segoe UI\', Inter, sans-serif" font-size="34" fill="#1E1E1E">{xml_escape(hline)}</text>')
            current_y += 46
            
        current_y += 65 # Espacio limpio y oxigenado entre hitos (sin caja de código ni placeholder)
        
    inner_box_height = (current_y - 408) + 80
    outer_card_height = inner_box_height + 408 + 80
    
    # Construcción de las capas de fondo en orden exacto del estándar FigmaES
    background_and_header = f'''
    <!-- Pestaña superior izquierda (Estándar visual) -->
    <rect x="40" y="6" width="240" height="34" rx="4" fill="white" stroke="#E6E6E6" stroke-width="3"/>
    <text x="160" y="30" font-family="'Segoe UI', Inter, sans-serif" font-size="18" font-weight="bold" fill="#1E1E1E" text-anchor="middle">LAVANDERÍA APP</text>
    
    <!-- Fondo exterior de tarjeta blanca con borde #E6E6E6 -->
    <rect x="40" y="40" width="3374" height="{outer_card_height - 80}" rx="16" fill="white" stroke="#E6E6E6" stroke-width="4"/>
    
    <!-- Contenedor interior celeste #DBF0FF con borde #3DADFF -->
    <rect x="120" y="408" width="3214" height="{inner_box_height}" rx="12" fill="#DBF0FF" stroke="#3DADFF" stroke-width="4"/>
    
    <!-- Cabecera central (Banner azul pill #3DADFF con borde #007AD2) -->
    <rect x="1104" y="104" width="1152" height="172" rx="8" fill="#3DADFF" stroke="#007AD2" stroke-width="4"/>
    <text x="1680" y="196" font-family="'Segoe UI', Inter, -apple-system, sans-serif" font-size="42" font-weight="bold" fill="#1E1E1E" text-anchor="middle">{xml_escape(f"APARTADO {apartado_num}: {titulo_apartado.upper()}")}</text>
    <text x="1680" y="242" font-family="'Segoe UI', Inter, sans-serif" font-size="28" font-weight="600" fill="#1E1E1E" text-anchor="middle">{xml_escape(f"RESPONSABLES: {responsables} | DURACIÓN: {duracion}")}</text>
    '''
    
    card_svg = f'''
    <g class="apartado-card-es" transform="translate(0, 0)">
        {background_and_header}
        {"".join(svg_elements)}
    </g>
    '''
    return card_svg, outer_card_height

def get_all_11_apartados_unified():
    return [
        {
            "num": 1,
            "titulo": "Levantamiento de requerimientos y definición del alcance",
            "duracion": "29 días (2026-05-07 al 2026-06-02)",
            "responsables": "Yadier Pech Tun, Leyva Chan y Daniel Moo",
            "explicacion_general": "Este primer apartado tiene como propósito fundamental entender las necesidades operativas reales de una lavandería y traducirlas en un esquema arquitectónico y funcional de software claro. Se pretende establecer un mapa conceptual sólido y delimitado que resuelva los problemas cotidianos de un mostrador de lavandería: recepción y pesaje de ropa, categorización de servicios (lavado, secado, tintorería, planchado), control de turnos y estados de las máquinas, deducción de insumos por receta, cobro preciso con emisión de tickets y auditoría de ingresos. Aquí se define qué se va a construir y cuáles son los límites de la versión V0.1/V1.2.",
            "hitos": [
                (
                    "Hito 1.1: Lista de Módulos del Sistema en Interfaz y Backlog",
                    "Daniel Moo (Frontend UI / Blazor) & Yadier Pech Tun (Backend / Base de Datos)",
                    "Se implementó la estructura modular de la aplicación en 7 módulos transaccionales (General/Dashboard, Pedidos, Clientes, Inventario, Empleados, Máquinas y Finanzas/Reportes) reflejados en el menú de navegación con control condicional de permisos por rol en tiempo real (@if (TienePermisoMenu(...))), asegurando que cada operador acceda exclusivamente a las áreas autorizadas."
                ),
                (
                    "Hito 1.2: Objetivos del Sistema (Exactitud financiera monetaria e IVA)",
                    "Yadier Pech Tun (Backend / BD) & Jesus Leyva Chan (Backend)",
                    "Se programaron las reglas de negocio monetarias dentro de la entidad Pedido.cs para cumplir el objetivo de exactitud financiera y adaptabilidad fiscal. Se implementó el método CalcularTotal() que evalúa dinámicamente si el IVA está activo en la configuración de la lavandería para aplicarlo o exentarlo del subtotal, así como el cálculo automático exacto del saldo pendiente (SaldoPendiente) a partir de los abonos."
                ),
                (
                    "Hito 1.3: Alcance Validado (Aplicación Local de Escritorio Blazor Hybrid en WPF)",
                    "Daniel Moo (Frontend UI) & Yadier Pech Tun (Backend / Base de Datos)",
                    "Se implementó y validó la arquitectura de software sobre la tecnología .NET 8 Blazor Hybrid empacada en contenedor de escritorio nativo WPF con motor relacional SQLite local, garantizando que el sistema opere al 100% con cero latencia y sin tolerancia a fallos de conexión a Internet en la terminal del mostrador."
                ),
                (
                    "Hito 1.4: Prioridades de Desarrollo por Capas",
                    "Equipo Colaborativo (Yadier Pech Tun, Leyva Chan, Daniel Moo)",
                    "Se implementó un plan de desarrollo estratificado por prioridades de capas: primero el núcleo de datos y persistencia SQLite junto con la seguridad del backend, luego los modelos de dominio y transacciones, y finalmente la presentación visual responsiva en Blazor con reportes en PDF."
                )
            ]
        },
        {
            "num": 2,
            "titulo": "Definición de backlog y módulos principales",
            "duracion": "4 días (2026-06-07 al 2026-06-11)",
            "responsables": "Yadier Pech Tun (Liderazgo en BD y Modelos)",
            "explicacion_general": "Este apartado tiene el objetivo de descomponer los requerimientos funcionales en un backlog técnico relacional estructurado. Aquí se diseñan los modelos de dominio (POCOs / clases C#) que representan cada entidad del mundo real de la lavandería. Se estructuran las propiedades, constructores, métodos de validación interna y contratos de datos para los módulos de usuarios, clientes, servicios, pedidos, detalles de pedido, auditoría y reportes para asegurar una base sólida en C# antes de consultar la base de datos.",
            "hitos": [
                (
                    "Hito 2.1: Módulo de Usuarios y Roles en el Backlog",
                    "Yadier Pech Tun (Backend / Base de Datos)",
                    "Se diseñó e implementó la entidad de dominio Usuario.cs, dotándola de propiedades de identidad y datos laborales (Turno, Salario, Sucursal, Activo) junto con funciones booleanas rápidas (EsMaster, EsAdmin, EsEmpleado) y el método de comparación de contraseñas (ValidarPassword) para agilizar la verificación de seguridad y binding en la interfaz sin evaluaciones redundantes."
                ),
                (
                    "Hito 2.2: Módulo de Clientes en el Backlog",
                    "Yadier Pech Tun (Backend / Base de Datos)",
                    "Se diseñó e implementó el modelo transaccional Cliente.cs, estructurando las propiedades de contacto e introduciendo una propiedad transaccional específica (PuntosFidelidad) orientada a cuantificar y recompensar la recurrencia de los clientes de manera automática tras cada consumo."
                ),
                (
                    "Hito 2.3: Módulo de Servicios, Pedidos y Detalles de Pedido",
                    "Yadier Pech Tun (Backend / Base de Datos)",
                    "Se formalizó el núcleo operativo mediante las clases Pedido.cs y DetallePedido.cs, encapsulando la relación maestro-detalle donde cada ítem mantiene autonomía de precio unitario e incorpora el método CalcularSubtotal() con redondeo matemático (Math.Round) para impedir alteraciones históricas si las tarifas cambian en el futuro."
                )
            ]
        },
        {
            "num": 3,
            "titulo": "Diseño de interfaz y flujo en Figma",
            "duracion": "2 días (2026-06-05 al 2026-06-07)",
            "responsables": "Daniel Moo (UI/UX) con asesoría técnica de Yadier y Leyva",
            "explicacion_general": "Este apartado tiene el propósito de crear el prototipo visual interactivo en Figma antes de escribir código UI. Se persigue diseñar una experiencia de usuario que reduzca la fricción en el mostrador: botones grandes para dispositivos táctiles, colores limpios que evoquen pulcritud (azul marino, turquesa, blanco) y una jerarquía visual clara que guíe al recepcionista desde el inicio de sesión, pasando por el panel general, hasta la creación y cobro del pedido en pocos segundos.",
            "hitos": [
                (
                    "Hito 3.1: Wireframes y Estructura Visual Trasladados a Blazor",
                    "Daniel Moo (Frontend UI / Blazor)",
                    "Se diseñaron los wireframes y retículas en Figma y se trasladaron fielmente al contenedor MainLayout.razor bajo un esquema de doble retícula: una barra lateral izquierda fija para navegación (<aside class=\"sidebar\">) y una zona de contenido transaccional central (<main class=\"main-content\">), soportando variables de diseño y clases de tema (@GetThemeClass())."
                ),
                (
                    "Hito 3.2: Flujo de Pantallas y Prototipo Navegable",
                    "Daniel Moo (Frontend UI), Jesus Leyva Chan (Backend) & Yadier Pech Tun (BD)",
                    "Se validó e implementó el flujo secuencial operativo en Blazor en coherencia con el prototipo navegable de Figma: Pantalla de Login (/login) -> Dashboard con alertas (/dashboard) -> Directorio y búsqueda instantánea de Clientes (/clientes) -> Recepción y edición de Pedidos (/pedidos) -> Pantalla de liquidación y cobro multimétodo (/cobro)."
                )
            ]
        },
        {
            "num": 4,
            "titulo": "Diseño de base de datos y arquitectura",
            "duracion": "4 días (2026-06-07 al 2026-06-11)",
            "responsables": "Yadier Pech Tun y Jesus Leyva Chan",
            "explicacion_general": "Este apartado construye el motor físico de persistencia del sistema. Se selecciona y configura SQLite nativo (Lavanderia.db) por su extrema velocidad, confiabilidad e independencia de servidores externos, perfecto para una aplicación local en Windows. Aquí se construyen las tablas transaccionales, se habilitan las restricciones de integridad referencial (FOREIGN KEYs), se establecen índices y se programan rutinas de inicialización y migración automática DDL.",
            "hitos": [
                (
                    "Hito 4.1: Modelo Entidad-Relación y Estructura de Tablas en SQLite",
                    "Yadier Pech Tun (Backend / Base de Datos)",
                    "Se programó el script DDL relacional en DatabaseInitializer.cs, creando las tablas jerárquicas (Clientes, Usuarios, Pedidos, DetallesPedido, Servicios, Maquinas) con llaves primarias autoincrementables e imponiendo integridad referencial a nivel motor al ejecutar PRAGMA foreign_keys = ON;, blindando la base de datos contra registros huérfanos."
                ),
                (
                    "Hito 4.2: Reglas Base y Migración Automática de Esquemas DDL (`MigrarTabla`)",
                    "Yadier Pech Tun (Backend / Base de Datos) & Jesus Leyva Chan (Backend)",
                    "Se implementó la rutina de introspección y auto-migración MigrarTabla dentro de DatabaseInitializer.cs. Esta rutina inspecciona el archivo físico con PRAGMA table_info al arrancar la aplicación e inyecta sentencias ALTER TABLE automáticamente en milisegundos cuando se añaden nuevas propiedades operativas al modelo, garantizando evolución continua sin pérdida de datos del cliente."
                )
            ]
        },
        {
            "num": 5,
            "titulo": "Aprobación del prototipo y cierre de diseño",
            "duracion": "7 días (2026-06-12 al 2026-06-19)",
            "responsables": "Todos (Daniel Moo, Yadier Pech Tun, Jesus Leyva Chan)",
            "explicacion_general": "Este apartado representa el punto de control y compuerta de calidad entre la fase de diseño conceptual y la construcción intensiva de software. Todo el equipo somete a escrutinio el prototipo de Figma frente a las especificaciones relacionales y de negocio, validando paletas, transiciones visuales, claridad de textos y consolidando un estándar no invasivo de comunicación de alertas en la interfaz para evitar cuadros de diálogo molestos del sistema.",
            "hitos": [
                (
                    "Hito 5.1: Validación Visual y Servicio de Alertas No Invasivo (`ToastService`)",
                    "Daniel Moo (Frontend UI) & Jesus Leyva Chan (Backend)",
                    "Se implementó un estándar visual y un servicio de retroalimentación en memoria orientado a eventos (ToastService.cs con Action<ToastMessage> OnShow) en reemplazo de cuadros de diálogo intrusivos. Esto permite que cualquier proceso del backend envíe alertas de éxito, error o advertencia hacia la UI de Blazor, mostrándose como tarjetas emergentes elegantes que coinciden con los tokens visuales de Figma."
                ),
                (
                    "Hito 5.2: Aprobación Formal y Configuración de Solución (`.sln` y `.csproj`)",
                    "Equipo Colaborativo (Yadier Pech Tun, Leyva Chan, Daniel Moo)",
                    "Se congeló formalmente la especificación y se configuraron las dependencias transaccionales del proyecto .csproj para Visual Studio / Rider, organizando los directorios limpios (BaseDatos, Modelos, Repositorios, Servicios, Pages) listos para la integración modular."
                )
            ]
        },
        {
            "num": 6,
            "titulo": "Backend: autenticación y usuarios",
            "duracion": "11 días (2026-06-23 al 2026-07-04)",
            "responsables": "Yadier Pech Tun y Jesus Leyva Chan",
            "explicacion_general": "Este apartado establece la barrera de seguridad y la gestión de identidad de LavanderiaApp. Es un prerrequisito crítico que debe quedar absolutamente estable antes de construir las pantallas operativas. Se divide en responsabilidades exactas: se desarrolla la lógica de verificación de inicio de sesión (LoginServicio) y el gestor de sesión en memoria (SessionManager), y en paralelo se programa el sistema de persistencia y CRUD de usuarios (UsuarioRepositorio) con modelado jerárquico por roles.",
            "hitos": [
                (
                    "Hito 6.1: Inicio de Sesión y Verificación de Credenciales (`LoginServicio`)",
                    "Jesus Leyva Chan (Backend - Lógica de Seguridad)",
                    "Se implementó el servicio de autenticación LoginServicio.cs, que encapsula la lógica de verificación llamando a UsuarioRepositorio para evaluar el match de contraseñas (ValidarPassword) y confirmar que la cuenta tenga estado Activo = true. Tras validar, inyecta al usuario legítimo en el SessionManager."
                ),
                (
                    "Hito 6.2: Sesión Segura en Memoria para Entorno de Escritorio (`SessionManager`)",
                    "Jesus Leyva Chan (Backend - Gestión de Sesiones)",
                    "Se implementó la clase estática SessionManager.cs como gestor local en memoria que retiene la identidad del recepcionista activo (UsuarioActual) para firmar sus operaciones en WPF/Blazor sin usar archivos temporales inseguros, ofreciendo la propiedad booleana rápida IsLoggedIn y la rutina de limpieza Logout()."
                ),
                (
                    "Hito 6.3: Roles y Gestión Transaccional de Usuarios (`UsuarioRepositorio`)",
                    "Yadier Pech Tun (Backend / Base de Datos)",
                    "Se implementó el repositorio transaccional UsuarioRepositorio.cs para acceso a SQLite, construyendo el método parametrizado ObtenerPorNombreUsuario (eliminando riesgo de inyección SQL mediante @NombreUsuario) y las operaciones CRUD con evaluación de roles jerárquicos (EsAdmin, EsMaster, EsEmpleado)."
                )
            ]
        },
        {
            "num": 7,
            "titulo": "Backend: clientes, servicios y órdenes",
            "duracion": "5 días (2026-07-05 al 2026-07-14)",
            "responsables": "Yadier Pech Tun y Jesus Leyva Chan",
            "explicacion_general": "Este apartado constituye el núcleo transaccional de la lavandería. Aquí se codifican las reglas transaccionales más críticas que conectan la recepción del mostrador con la persistencia en base de datos. Se programa el CRUD Principal de Pedidos, Detalles, Servicios, Pagos y la automatización inteligente del inventario; y en paralelo se desarrolla la lógica de Registro y Gestión de Clientes en ClienteRepositorio, permitiendo altas y búsquedas instantáneas.",
            "hitos": [
                (
                    "Hito 7.1: CRUD Principal de Órdenes y Captura de ID con `last_insert_rowid()`",
                    "Yadier Pech Tun (Backend / Base de Datos)",
                    "Se implementó la capa central de persistencia de órdenes en PedidoRepositorio.cs. Se estructuró la inserción transaccional ejecutando en el mismo comando SELECT last_insert_rowid(); y capturándolo con ExecuteScalar(), lo que retorna en microsegundos el ID numérico asignado por SQLite para vincular e insertar de inmediato los ítems del detalle y abonos con exactitud absoluta."
                ),
                (
                    "Hito 7.2: Registro y Gestión de Clientes en Capa de Datos (`ClienteRepositorio`)",
                    "Jesus Leyva Chan (Backend - Lógica de Clientes)",
                    "Se implementó el repositorio ClienteRepositorio.cs, construyendo el método Guardar con parámetros SQL protegidos contra nulos (?? \"\") para registrar clientes en el mostrador al instante, y el método ListarTodo optimizado con SqliteDataReader para poblar el directorio y alimentar las búsquedas reactivas en pantalla."
                )
            ]
        },
        {
            "num": 8,
            "titulo": "Frontend: pantallas principales",
            "duracion": "7 días (2026-07-10 al 2026-07-17)",
            "responsables": "Daniel Moo (Frontend UI/UX en Blazor)",
            "explicacion_general": "Esta actividad transforma los diseños y wireframes aprobados de Figma en componentes funcionales e interactivos utilizando Blazor (.razor) dentro de WPF. Se tiene el objetivo de construir una interfaz visual moderna, ágil y reactiva donde el operador pueda navegar fluidamente entre Dashboard, Clientes, Órdenes y Cobro, respetando rigurosamente los estándares visuales de color, tipografía y modales no invasivos definidos en el diseño.",
            "hitos": [
                (
                    "Hito 8.1: Pantallas de Login, Dashboard y Clientes con Filtrado Instantáneo (`oninput`)",
                    "Daniel Moo (Frontend UI / Blazor)",
                    "Se implementaron las vistas interactivas Clientes.razor y Dashboard.razor, incorporando binding bidireccional reactivo (@bind) e interceptando el evento @bind:event=\"oninput\". Esta técnica permite re-filtrar las tablas transaccionales en tiempo real en la memoria del componente a medida que el recepcionista teclea en el buscador, sin latencias ni recargas."
                ),
                (
                    "Hito 8.2: Pantallas de Órdenes, Cobro Transaccional y Liquidación Multimétodo",
                    "Daniel Moo (Frontend UI / Blazor)",
                    "Se construyó la pantalla de liquidación Cobro.razor con un panel financiero de alto contraste que desglosa en tarjetas accesibles el Total, Monto Pagado y Saldo Pendiente del pedido formateados con 2 decimales (ToString(\"F2\")), integrando selectores de método de cobro (Efectivo/Tarjeta) grandes y con respuesta táctil inmediata (active)."
                )
            ]
        },
        {
            "num": 9,
            "titulo": "Integración frontend + backend",
            "duracion": "7 días (2026-07-10 al 2026-07-17)",
            "responsables": "Todos (Daniel Moo, Jesus Leyva Chan, Yadier Pech Tun)",
            "explicacion_general": "En esta fase colaborativa intensiva, se conectan los desarrollos: las vistas Razor (Frontend) se acoplan con los servicios de autenticación y clientes (Backend) y los repositorios transaccionales en SQLite (Base de Datos / Backend). Se retiran todos los datos simulados y se habilita la inyección de dependencias en Blazor, validando en tiempo real que cada acción del usuario en la interfaz visual dispare transacciones atómicas e íntegras en la base de datos.",
            "hitos": [
                (
                    "Hito 9.1: Conexión de Componentes Razor con Repositorios y Sesión Activa",
                    "Daniel Moo (UI), Jesus Leyva Chan (Sesión) & Yadier Pech Tun (Repositorios SQLite)",
                    "Se implementó la sincronización en el code-behind de los componentes Blazor (EditarPedido.razor). Al guardar una orden, el código consulta SessionManager.UsuarioActual para asignarle el ID del operador, invoca a PedidoRepositorio.Guardar para persistir físicamente en SQLite, emite una notificación de confirmación al usuario mediante ToastService y redirecciona fluidamente al listado con NavManager.NavigateTo(\"/pedidos\")."
                ),
                (
                    "Hito 9.2: Automatización Transaccional y Sincronización con Máquinas e Inventarios",
                    "Yadier Pech Tun (Backend / Base de Datos)",
                    "Se implementó el servicio de sincronización transaccional MaquinasAutomatizacion.cs. Cuando la UI avanza el estado del pedido a 'En Lavado' o 'En Secado', el sistema ejecuta un UPDATE que cambia el estado de la lavadora física a 'Ocupada' enlazada al ID del pedido, liberando la máquina automáticamente a 'Disponible' cuando el pedido pasa a 'Listo' o 'Entregado'."
                )
            ]
        },
        {
            "num": 10,
            "titulo": "Pruebas funcionales y corrección de errores",
            "duracion": "4 días (2026-07-15 al 2026-07-19)",
            "responsables": "Todos (Aseguramiento de calidad, defensas en BD y blindaje de UI)",
            "explicacion_general": "Esta actividad constituye el control de aseguramiento de calidad (QA) previo al empaquetado final. Todo el equipo participa activamente en la detección, tipificación y corrección de bugs: se someten a pruebas de estrés transaccional las tablas en SQLite y se valida la imposibilidad de saldos negativos en la capa de dominio; se audita la seguridad en sesiones concurrentes; y se refina el acomodo visual y capacidad de respuesta en Blazor frente a excepciones.",
            "hitos": [
                (
                    "Hito 10.1: Validación Defensiva de Integridad en Capa de Dominio (`ValidarPedido`)",
                    "Yadier Pech Tun (Backend / BD) & Jesus Leyva Chan (Backend)",
                    "Se implementó la rutina defensiva ValidarPedido() dentro de la entidad Pedido.cs. Esta capa intercepta pre-persistencia cualquier orden incompleta, en blanco o con importes negativos, generando la lista exacta de errores e impidiendo que una transacción corrupta alcance el motor SQLite."
                ),
                (
                    "Hito 10.2: Control Robustecido de Excepciones (`try-catch`) en Interfaz UI",
                    "Daniel Moo (Frontend UI / Blazor) & Equipo Colaborativo",
                    "Se integraron bloques try-catch en los métodos de acción de las vistas Blazor. Si ocurre una violación de regla en el modelo o un error del motor SQL (SqliteException), el sistema lo intercepta para evitar que la ventana de Windows (WPF) colapse y lo reporta de forma descriptiva mediante notificaciones emergentes no intrusivas (ToastService)."
                )
            ]
        },
        {
            "num": 11,
            "titulo": "Documentación final y entrega",
            "duracion": "4 días (2026-07-18 al 2026-07-22)",
            "responsables": "Todos (Cierre arquitectónico, manuales y presentación en Figma)",
            "explicacion_general": "Esta última actividad culmina el ciclo formal de desarrollo y gestión de LavanderiaApp v0.1. El equipo consolida el acervo documental, los manuales técnicos de arquitectura, los manuales de usuario y el empaquetado del software con sus dependencias embebidas y base de datos inicializada. Se documenta la estructura relacional de SQLite, los repositorios y servicios transaccionales, y se adjuntan los recursos de diseño del prototipo en Figma y maquetación en Blazor.",
            "hitos": [
                (
                    "Hito 11.1: Documentación Técnica de Arquitectura por Capas Desacopladas",
                    "Yadier Pech Tun (Backend / BD) & Jesus Leyva Chan (Backend)",
                    "Se estructuró y documentó la solución bajo un patrón limpio en carpetas (BaseDatos, Modelos, Repositorios, Servicios y Pages), explicitando las responsabilidades de cada clase para facilitar auditorías técnicas y extensiones futuras por el equipo."
                ),
                (
                    "Hito 11.2: Generador Nativo de Comprobantes y Tickets (`ReporteGeneradorNativo`)",
                    "Yadier Pech Tun (Backend / BD) & Equipo Colaborativo",
                    "Se implementó el generador nativo de tickets físicos (ReporteGeneradorNativo.cs), estructurando el comprobante en texto/PDF con encabezado oficial, datos del cliente, desglose detallado de servicios, importes e IVA calculados listos para enviarse a impresoras térmicas de mostrador."
                ),
                (
                    "Hito 11.3: Enlace de Figma Adjunto y Preservación del Prototipo Visual",
                    "Daniel Moo (Frontend UI / Prototipado en Figma)",
                    "Se entregó y documentó como anexo de cierre la estructura y el enlace oficial del prototipo navegable en Figma (/design/lavanderia-app-prototipo-v1.2) que sirvió como retícula maestra, confirmando la coincidencia visual entre el diseño y la implementación en App/Pages/."
                )
            ]
        }
    ]

def main():
    base_dir = os.path.join("App", "DOCUMENTACION", "Por_Apartado_FigmaES")
    os.makedirs(base_dir, exist_ok=True)
    
    apartados = get_all_11_apartados_unified()
    card_width = 3454
    spacing = 150
    
    cards_render = []
    current_y = 0
    
    # 1. Generar cada Apartado individual e ir acumulando para el maestro
    for apt in apartados:
        card_svg, card_h = build_apartado_card_figma_es(
            apartado_num=apt["num"],
            titulo_apartado=apt["titulo"],
            duracion=apt["duracion"],
            responsables=apt["responsables"],
            explicacion_general=apt["explicacion_general"],
            hitos_list=apt["hitos"],
            card_width=card_width
        )
        
        # Guardar archivo individual por Apartado con formato exacto Yadier(Backend)ES.svg
        single_svg = f'<?xml version="1.0" encoding="utf-8"?><svg width="{card_width}" height="{card_h}" viewBox="0 0 {card_width} {card_h}" fill="none" xmlns="http://www.w3.org/2000/svg">{card_svg}</svg>'
        
        single_path = os.path.join(base_dir, f'Apartado_{apt["num"]}_FigmaES.svg')
        with open(single_path, 'w', encoding='utf-8') as f:
            f.write(single_svg)
            
        # Acumular en maestro desplazado verticalmente
        pos_master = card_svg.replace('transform="translate(0, 0)"', f'transform="translate(0, {current_y})"')
        cards_render.append(pos_master)
        current_y += card_h + spacing
        
    total_height = current_y + 100
    
    # 2. Guardar el archivo Maestro con los 11 Apartados unidos bajo el estándar Yadier(Backend)ES.svg
    master_svg = f'''<?xml version="1.0" encoding="utf-8"?>
<svg width="{card_width}" height="{total_height}" viewBox="0 0 {card_width} {total_height}" fill="none" xmlns="http://www.w3.org/2000/svg">
    <!-- Colección de 11 Apartados con el estándar exacto Yadier(Backend)ES.svg (Sin código, sin cajas placeholder) -->
    {"".join(cards_render)}
</svg>'''

    master_path = os.path.join("App", "DOCUMENTACION", "Documentacion_Completa_11_Apartados_FigmaES.svg")
    with open(master_path, 'w', encoding='utf-8') as f:
        f.write(master_svg)
    print(f"✅ Archivo Maestro de 11 Apartados (Estándar Yadier(Backend)ES.svg) generado en: {os.path.abspath(master_path)}")
    print(f"✅ 11 Tarjetas individuales (Estándar Yadier(Backend)ES.svg) generadas en: {os.path.abspath(base_dir)}")

if __name__ == "__main__":
    main()
