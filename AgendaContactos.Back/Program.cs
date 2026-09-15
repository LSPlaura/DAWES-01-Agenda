using AgendaContactos.Back.Infraestructure;
using AgendaContactos.Back.Interface;
using AgendaContactos.Back.Repositories;
using AgendaContactos.Back.Services.Crud.Contacts;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

    try
    {
        Log.Information("Iniciando la aplicación de Agenda de Contactos...");
        
        var serviceProvider = DependenciesProvider.BuildServiceProvider();
        
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        dbContext.EnsureCreated();
        Log.Information("Base de datos inicializada correctamente.");
        
        var interfaceSimulator = new Interface(serviceProvider.GetRequiredService<IContactsService>());

        // ==========================================
        // FASE 1: CREAR DATOS INICIALES (SEMBRADO)
        // ==========================================
        Log.Information("--- FASE 1: Insertando datos base en la BD ---");

        // Contacto base 1
        interfaceSimulator.Operate("POST", phone: "+34611111111", name: "Carlos Perez", alias: "Charlie", email: "carlos@test.com");
        
        // Contacto base 2
        interfaceSimulator.Operate("POST", phone: "+34622222222", name: "Lucia Gomez", alias: "Lucy", email: "lucia@test.com");
        
        // Contacto base 3 (para pruebas de borrado)
        interfaceSimulator.Operate("POST", phone: "+34699999999", name: "Temporal Borrar", alias: "Temp", email: "temp@test.com");


        // ==========================================
        // FASE 2: PRUEBAS DE ÉXITO (SUCCESS)
        // ==========================================
        Log.Information("--- FASE 2: Probando Casos de Éxito ---");

        // // 1. Consultar de forma individual a un contacto recién creado
        interfaceSimulator.Operate("GET", key: "+34611111111");
        //
        // // 2. Listar todos los contactos de la agenda (paginación por defecto 0,5)
        interfaceSimulator.Operate("GET", key: "all");
        //
        // // 3. Actualizar datos de un contacto existente (PUT Success)
        interfaceSimulator.Operate("PUT", key: "+34611111111", phone: "+34611111111", name: "Carlos Alberto Perez", alias: "Charlie Boss", email: "carlos.boss@test.com");
        //
        // // 4. Eliminar un contacto existente
        interfaceSimulator.Operate("DELETE", key: "+34699999999");
        
        // 5. Eliminar un contacto existente
        interfaceSimulator.Operate("GET", alias: "charlie");


        // ==========================================
        // FASE 3: PRUEBAS DE FALLO Y VALIDACIONES
        // ==========================================
        Log.Information("--- FASE 3: Probando Casos de Fallo (Validaciones y Errores de Negocio) ---");

        // 1. POST Fallido por parámetros incompletos (Faltan name, alias, email)
        interfaceSimulator.Operate("POST", phone: "+34633333333");

        // 2. POST Fallido por intentar duplicar un teléfono que ya existe
        interfaceSimulator.Operate("POST", phone: "+34611111111", name: "Otro Carlos", alias: "Duplicado", email: "otro@test.com");

        // 3. GET Fallido por buscar un ID que no existe en la base de datos
        interfaceSimulator.Operate("GET", key: "+34600000999");

        // 4. PUT Fallido por intentar actualizar un contacto que no existe
        interfaceSimulator.Operate("PUT", key: "+34600000999", phone: "+34600000999", name: "Nadie", alias: "Fantasma", email: "nadie@test.com");

        // 5. DELETE Fallido por intentar borrar una clave que no existe
        interfaceSimulator.Operate("DELETE", key: "+34600000999");

        // 6. Verbo HTTP/Operación no contemplada (Entrará en el default y dejará advertencia en el log)
        interfaceSimulator.Operate("OPTIONS", key: "+34611111111");

    Log.Information("=== FIN DE TODAS LAS PRUEBAS DE INTEGRACIÓN ===");
        Log.Information("Simulación finalizada con éxito.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "La aplicación ha fallado de manera inesperada.");
    }
    finally 
    {
        Log.CloseAndFlush();
    }