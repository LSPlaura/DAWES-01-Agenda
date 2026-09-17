using System.Text.Json;
using AgendaContactos.Back.Controller;
using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Infraestructure;
using AgendaContactos.Back.Models.Enums;
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
        var interfaceSimulator = new Controller(serviceProvider.GetRequiredService<IContactsService>());
        Log.Information("Base de datos inicializada correctamente.");
        
        void Operate(Verb verb, string? keyWord, string? value)
        {
            switch (verb)
            {
                case Verb.Post:
                    interfaceSimulator.Post(value);
                    break;

                case Verb.Get:
                    interfaceSimulator.GetOrchester(keyWord, value);
                    break;

                case Verb.Put:
                    interfaceSimulator.Put(keyWord,value);
                    break;

                case Verb.Delete:
                    interfaceSimulator.Delete(value);
                    break;
            }
        }
        // ==========================================
        // FASE 1: CREAR DATOS INICIALES (SEMBRADO)
        // ==========================================
        Log.Information("--- FASE 1: Insertando datos base en la BD ---");

        // Contacto base 1
        var dto1 = new ContactDto("+34611111111", "Carlos Perez", "Charlie", "carlos@test.com");
        Operate(Verb.Post, null, JsonSerializer.Serialize(dto1));

        // Contacto base 2
        var dto2 = new ContactDto("+34622222222", "Lucia Gomez", "Lucy", "lucia@test.com");
        Operate(Verb.Post, null, JsonSerializer.Serialize(dto2));

        // Contacto base 3 (para pruebas de borrado)
        var dto3 = new ContactDto("+34699999999", "Temporal Borrar", "Temp", "temp@test.com");
        Operate(Verb.Post, null, JsonSerializer.Serialize(dto3));


        // ==========================================
        // FASE 2: PRUEBAS DE ÉXITO (SUCCESS)
        // ==========================================
        Log.Information("--- FASE 2: Probando Casos de Éxito ---");

        // 1. Consultar por ID a un contacto recién creado
        Operate(Verb.Get, "id", "+34611111111");

        // 2. Listar todos los contactos de la agenda
        Operate(Verb.Get, "all", null);

        // 3. Consultar por alias
        Operate(Verb.Get, "alias", "Charlie");

        // 4. Actualizar datos de un contacto existente (PUT Success)
        var dtoUpdate1 = new ContactDto("+34611111111", "Carlos Alberto Perez", "Charlie Boss", "carlos.boss@test.com");
        Operate(Verb.Put, "+34611111111", JsonSerializer.Serialize(dtoUpdate1));

        // 5. Eliminar un contacto existente
        Operate(Verb.Delete, null, "+34699999999");


        // ==========================================
        // FASE 3: PRUEBAS DE FALLO Y VALIDACIONES
        // ==========================================
        Log.Information("--- FASE 3: Probando Casos de Fallo (Validaciones y Errores de Negocio) ---");
        
        // 2. POST Fallido por intentar duplicar un teléfono que ya existe
        var dtoDuplicado = new ContactDto("+34611111111", "Otro Carlos", "Duplicado", "otro@test.com");
        Operate(Verb.Post, null, JsonSerializer.Serialize(dtoDuplicado));

        // 3. GET Fallido por buscar un ID que no existe
        Operate(Verb.Get, "id", "+34600000999");

        // 4. GET Fallido por keyWord no reconocido
        Operate(Verb.Get, "desconocido", "test");

        // 5. PUT Fallido por intentar actualizar un contacto que no existe
        var dtoFantasma = new ContactDto("+34600000999", "Nadie", "Fantasma", "nadie@test.com");
        Operate(Verb.Put, "+34600000999", JsonSerializer.Serialize(dtoFantasma));

        // 6. DELETE Fallido por intentar borrar una clave que no existe
        Operate(Verb.Delete, null, "+34600000999");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "La aplicación ha fallado de manera inespera7da.");
    }
    finally 
    {
        Log.CloseAndFlush();
    }