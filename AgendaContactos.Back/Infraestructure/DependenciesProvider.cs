using AgendaContactos.Back.Cache;
using AgendaContactos.Back.Cache.Common;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Repositories;
using AgendaContactos.Back.Repositories.Contacts;
using AgendaContactos.Back.Services.Crud.Contacts;
using AgendaContactos.Back.Validators;
using AgendaContactos.Back.Validators.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaContactos.Back.Infraestructure;

public static class DependenciesProvider
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        
        RegisterRepository(services);
        RegisterCache(services);
        RegisterValidators(services);
        RegisterServices(services);
        
        return services.BuildServiceProvider();
    }

    private static void RegisterRepository(IServiceCollection services)
    {
        services.AddSingleton<AppDbContext>(sp => 
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite("Data Source=agenda.db");
        
            return new AppDbContext(optionsBuilder.Options);
        });

        services.AddSingleton<IContactRepository, ContactRepository>();
    }
    
    private static void RegisterCache(IServiceCollection services)
    {
        services.AddSingleton<ICache<string, Contact>, LruCache>(provider => new LruCache(3));
    }
    
    private static void RegisterValidators(IServiceCollection services)
    {
        services.AddTransient<IValidate<Contact>>(sp => new ContactValidator());
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IContactsService, ContactService>(sp => new ContactService(
            sp.GetRequiredService<IContactRepository>(),
            sp.GetRequiredService<ICache<string, Contact>>(),
            sp.GetRequiredService<IValidate<Contact>>()
        ));
    }
}