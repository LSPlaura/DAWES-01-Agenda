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

        // Simulación de operaciones....

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