using AgendaContactos.Back.Errors.Contact;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Repositories;
using AgendaContactos.Back.Repositories.Contacts;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace AgendaContactos.Tests.Repository;

[TestFixture]
public class ContactRepositoryTests
{
    private const string InMemoryConnection = "Data Source=:memory:";

    [TestFixture]
    public class CasosValidos
    {
        private IContactRepository _repositorio = null!;
        private SqliteConnection _connection = null!;
        AppDbContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection(InMemoryConnection);
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repositorio = new ContactRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
        
        [Test]
        public void Create_SinErrores()
        {
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");

            var result = _repositorio.Create(contact);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.PhoneNumber.Should().Be(contact.PhoneNumber);
        }
            
        [Test]
        public void Delete_EliminaCorrectamente()
        {
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            var agregado = _repositorio.Create(contact);

            var result = _repositorio.Delete(agregado.Value.PhoneNumber);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.PhoneNumber.Should().Be(contact.PhoneNumber);
            
            _repositorio.ExistId(contact.PhoneNumber).Should().BeFalse();
        }
            
        [Test]
        public void GetById_ContactoEncontrado()
        { 
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            _repositorio.Create(contact);
            
            var result = _repositorio.GetById(contact.PhoneNumber);
          
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.PhoneNumber.Should().Be(contact.PhoneNumber);
        }
    
        [Test]
        public void GetByAlias_ContactosEncontrados()
        { 
            var contact1 = new Contact("+34600000001", "Ana", "Amiga", "ana@test.com");
            var contact2 = new Contact("+34600000002", "Anita", "Amiga", "anita@test.com");
            
            _repositorio.Create(contact1);
            _repositorio.Create(contact2);
            
            var result = _repositorio.GetByAlias("Amiga").ToList();
          
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
        }
            
        [Test]
        public void Update_ContactoActualizado()
        {
            var contactoAntiguo = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            _repositorio.Create(contactoAntiguo);
            
            var contactoNuevo = new Contact("+34600000001", "Ana Maria", "SuperAna", "ana.maria@test.com");
            var result = _repositorio.Update(contactoAntiguo.PhoneNumber, contactoNuevo);
    
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Ana Maria");
            result.Value.Alias.Should().Be("SuperAna");
            result.Value.Email.Should().Be("ana.maria@test.com");
        }

        [Test]
        public void Update_CambiandoClavePrimaria()
        {
            var contactoAntiguo = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            _repositorio.Create(contactoAntiguo);
            
            var contactoNuevo = new Contact("+34699999999", "Ana", "Anita", "ana@test.com");
            var result = _repositorio.Update(contactoAntiguo.PhoneNumber, contactoNuevo);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.PhoneNumber.Should().Be("+34699999999");
            
            _repositorio.ExistId(contactoAntiguo.PhoneNumber).Should().BeFalse();
            _repositorio.ExistId("+34699999999").Should().BeTrue();
        }

        [Test]
        public void GetAll_RetornaPaginado()
        {
            _repositorio.Create(new Contact("+34600000001", "C1", "A1", "1@test.com"));
            _repositorio.Create(new Contact("+34600000002", "C2", "A2", "2@test.com"));
            _repositorio.Create(new Contact("+34600000003", "C3", "A3", "3@test.com"));

            var pagina = _repositorio.GetAll(0, 2).ToList();
            pagina.Should().HaveCount(2);
        }
    }

    [TestFixture]
    public class CasosInvalidos
    {
        private IContactRepository _repositorio = null!;
        private SqliteConnection _connection = null!;
        AppDbContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection(InMemoryConnection);
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _repositorio = new ContactRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
       
        [Test]
        public void Delete_ErrorEncontrarId()
        {
            var result = _repositorio.Delete("+34600000009");
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ContactNotFoundId>();
        }
        
        [Test]
        public void GetById_ErrorNoEncontrado()
        { 
            var result = _repositorio.GetById("+34600000009");
    
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ContactNotFoundId>();
        }
    
        [Test]
        public void Update_ErrorContactoNoEncontrado()
        {
            var contactoNuevo = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            var result = _repositorio.Update("+34600000009", contactoNuevo);
    
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ContactNotFoundId>();
        }

        [Test]
        public void ExistId_RetornaFalseSiNoExiste()
        {
            _repositorio.ExistId("+34600000009").Should().BeFalse();
        }

        [Test]
        public void ExistsEmail_RetornaTrueSiExiste()
        {
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            _repositorio.Create(contact);

            var result = _repositorio.ExistsEmail("ana@test.com");
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }
    }
}