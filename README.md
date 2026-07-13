# EpsLaNuestra
Proyecto para prueba como Ingeniero de Desarrollo Senior con manejor de Azure | Net 8 C# (API y RAZOR)
**Developed with**:
- Clean Architecture DDD
- Design Patterns (MediatR[CQRS], UnitOfWork, Repository, Singleton, Polly)
- Best Practices based on some SOLID Principles
- Security with JWT
- UnitTest with XUnit and Moq
- Entity Framework for SqlServer DB
- Base de Datos MongoDB para almacenamiento de documentos complejos
- Blazor con SignalR

## Arquitectura:
- El diagrama de arquitectura se encuentra cargado en la carpeta **"Arquitectura"**.

## Respuestas a las preguntas del primer punto de la prueba:

**1\.** ¿Cómo configuras el escalamiento horizontal de la aplicación Blazor Server para evitar la pérdida de estado y el agotamiento de conexiones (Sockets) en los App Services?

**R//** Utilizando **Azure SignalR Service** en modo Default, delegando la gestión de los WebSockets al servicio de **Azure App Service Autoscale** activando el **Application Gateway** para garantizar el **"sticky sessions"**, enviando al usuario siempre al mismo servidor donde reside su circuito, si un servidor muere, el circuito se reconecta gracias a que Azure SignalR Service retiene el estado de la conexión temporalmente.

**2.** ¿Cómo y dónde almacenas de forma segura las cadenas de conexión de SQL Server y MongoDB para cumplir con normativas de seguridad (ej. HIPAA)?

**R//** Utilizando **Azure KeyVault,** en este componente se crea un secreto para cada conexión; en la configuración de las **Access Policies**, se otorgan los permisos List/Get al GUID que corresponde a la API por medio de **System-Assigned Managed Identity**; por último, en la configuración (Variables de entorno) del AppService de la API se establece a cada una el valor del secreto usando **“@Microsoft.KeyVault(SecretUri=https://.vault.azure.net/secrets/)”**.

**3\.** Si la base de datos SQL sufre una caída temporal de 5 segundos, ¿qué estrategia en la nube o en código implementas para no perder la admisión del paciente?

**R//** Desacoplo el proceso de Sql usando cola de mensajes, es decir, el proceso de MongoDB que lo realice y posteriormente que se conecte a un **Storage Queue** y deje un mensaje con el DocumentoPaciente y el ValorCopago, este mensaje lo toma una **Azure Function** y llama a otro EndPoint de la API, en este proceso se implementa “**Polly**” para que reintente 3 veces exponencialmente cada 1, 3 y 5 segundos (solo para el error de conexión de BD), si después del último intento persiste el fallo la función de Azure deja el mensaje con un **Delay** (puede ser de 1 minuto), luego de este tiempo se vuelve a procesar el mensaje y si la Base de Datos revive no se perdería la admisión del paciente.

## Respuestas al punto 4 (Code Review y Optimización):
1. Identifica al menos tres (3) problemas graves de rendimiento (Performance / N+1 / Memory Allocation) en este fragmento de código.
- **R//**
  - El uso de .ToList() al inicio descarga toda la tabla de Pacientes y Atenciones a la RAM. Si la base de datos es grande, el servidor se quedará sin memoria de inmediato.
  - Los filtros p.Estado == "Activo" y a.RequiereAuditoria se ejecutan con C# y no con el motor de SQL Server.
  - El método está marcado como async, pero utiliza .ToList(), frena el resto de peticiones mientras termina.
  - No se usa proyección, se descargan columnas innecesarias para la consulta.
2. Escribe la versión optimizada de este método utilizando las mejores prácticas de .NET 8 y EF Core (ej. proyección, consultas asíncronas, filtrado en servidor).
- **R//**
```javascript
[HttpGet("reporte-mensual")]
public async Task<IActionResult> GenerarReporteMensual([FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 50)
{
    // Evitar valores negativos o cero
    if (pagina < 1) pagina = 1;
    if (tamanoPagina < 1 || tamanoPagina > 100) tamanoPagina = 50;

    // 1. Crear el query con el fitro sin ir aún a la base de datos
    var queryBase = _dbContext.Pacientes
        .Where(p => p.Estado == "Activo" && p.Atenciones.Any(a => a.RequiereAuditoria));

    // 2. Importante enviar este valor al front
    int totalRegistros = await queryBase.CountAsync();

    // 3. Aplicar paginación y proyección directamente en SQL Server
    var datos = await queryBase
        .OrderBy(p => p.Apellido)
        .ThenBy(p => p.Nombre)
        .Skip((pagina - 1) * tamanoPagina)
        .Take(tamanoPagina)
        .Select(p => new ReporteDto
        {
            NombreCompleto = p.Nombre + " " + p.Apellido,
            TotalAuditar = p.Atenciones
                .Where(a => a.RequiereAuditoria)
                .Sum(a => a.Valor)
        })
        .AsNoTracking() // optimiza un poco ya que son datos de solo lectura
        .ToListAsync();

    // 4. Retorno de data con info de la paginación
    var respuesta = new
    {
        TotalRegistros = totalRegistros,
        PaginaActual = pagina,
        TamanoPagina = tamanoPagina,
        TotalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina),
        Resultado = datos
    };
    
    return Ok(respuesta);
}
```

## Para tener en cuenta para probar el desarrollo:
- La Base de Datos relacional fu usada localmente con Sql Server "**SQLEXPRESS**" (Dependiendo del server que se pruebe hay que cambiarlo en el appsettings "ConStringSqlServer" el valor **'<SERVER'>**) y trabaja con el usuario **developer**. Los scripts de esta BD están en: **"ScriptsDb/SQL/"**. 
  - **1-createUserForDb.sql** creación del usuario en la base de datos.
  - **2-scriptsDb.sql** creación de la tabla.
- La Base de Datos Mongo fue probada con COMPASS y se debe generar con Docker usando el script que se encuentra en **"ScriptsDb/MONGO/"**.
- Por seguridad, se debe generar primero un token con el EndPoint GET **"Authentication"** (La idea es que a futuro se valide el usuario con la Base de Datos y así generarlo o no, mientras tanto para probar se está generando el TOKEN a todos los usuarios), luego se pasa a través de la cabecera utilizando autenticación Bearer.
- Las Variables de entorno como se explicó en la respuesta de la prueba, en un despliegue hacia una AppService de Azure, la sesión **"SecretsValues"** serán gestionadas desde los secretos de KeyVault... Con esto, por motivos de seguridad, el archivo **"(appsettings.json)"** ya no debería contener dichas variables de entorno.
