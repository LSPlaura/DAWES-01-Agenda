using AgendaContactos.Back.Cache.Common;
using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Errors.Common;
using AgendaContactos.Back.Errors.Contact;
using AgendaContactos.Back.Models;
using AgendaContactos.Back.Repositories.Contacts;
using AgendaContactos.Back.Services.Crud.Contacts;
using AgendaContactos.Back.Validators.Common;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AgendaContactos.Tests.Service;

[TestFixture]
public class ContactServiceTests
{
    [TestFixture]
    public class ValidCases
    {
        private Mock<IContactRepository> _mockRepository = null!;
        private Mock<ICache<string, Contact>> _mockCache = null!;
        private Mock<IValidate<Contact>> _mockValidator = null!;
        private IContactsService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IContactRepository>();
            _mockCache = new Mock<ICache<string, Contact>>();
            _mockValidator = new Mock<IValidate<Contact>>();
            
            _service = new ContactService(
                _mockRepository.Object,
                _mockCache.Object,
                _mockValidator.Object
            );
        }

        [Test]
        public void Create_ValidContact_ReturnsSuccessResultAndCaches()
        {
            var dto = new ContactDto("+34600000001", "ana", "anita", "ana@test.com");
            var contactNormalized = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(It.IsAny<string>())).Returns(false);
            _mockRepository.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(false));
            _mockRepository.Setup(r => r.Create(It.IsAny<Contact>())) 
                .Returns(Result.Success<Contact, DomainError>(contactNormalized));

            // Ajuste limpio de la línea anterior:
            _mockRepository.Setup(r => r.Create(It.IsAny<Contact>()))
                .Returns(Result.Success<Contact, DomainError>(contactNormalized));

            var result = _service.Create(dto);

            result.IsSuccess.Should().BeTrue();
            result.Value.PhoneNumber.Should().Be("+34600000001");
            
            _mockValidator.Verify(v => v.Validate(It.IsAny<Contact>()), Times.Once);
            _mockRepository.Verify(r => r.Create(It.IsAny<Contact>()), Times.Once);
            _mockCache.Verify(c => c.Add(contactNormalized.PhoneNumber, contactNormalized), Times.Once);
        }

        [Test]
        public void Delete_PerformsDeletionAndClearsCache()
        {
            string phone = "+34600000001";
            var contact = new Contact(phone, "Ana", "Anita", "ana@test.com");

            _mockRepository.Setup(r => r.ExistId(phone)).Returns(true);
            _mockRepository.Setup(r => r.Delete(phone))
                .Returns(Result.Success<Contact, DomainError>(contact));

            var result = _service.Delete(phone);

            result.IsSuccess.Should().BeTrue();
            _mockRepository.Verify(r => r.Delete(phone), Times.Once);
            _mockCache.Verify(c => c.Delete(phone), Times.Once);
        }

        [Test]
        public void Update_ShouldWorkCorrectly()
        {
            string phone = "+34600000001";
            var dto = new ContactDto(phone, "ana maria", "superana", "ana.maria@test.com");
            var updatedContact = new Contact(phone, "Ana Maria", "SuperAna", "ana.maria@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(phone)).Returns(true);
            _mockRepository.Setup(r => r.ExistsEmail(It.IsAny<string>()))
                .Returns(Result.Success<bool, DomainError>(false));
            _mockRepository.Setup(r => r.Update(phone, It.IsAny<Contact>()))
                .Returns(Result.Success<Contact, DomainError>(updatedContact));

            var result = _service.Update(phone, dto);

            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Ana Maria");

            _mockValidator.Verify(v => v.Validate(It.IsAny<Contact>()), Times.Once);
            _mockRepository.Verify(r => r.Update(phone, It.IsAny<Contact>()), Times.Once);
            _mockCache.Verify(c => c.Add(phone, updatedContact), Times.Once);
        }

        [Test]
        public void GetById_GetsFromCacheIfItExists()
        {
            string phone = "+34600000001";
            var contact = new Contact(phone, "Ana", "Anita", "ana@test.com");

            _mockCache.Setup(c => c.Obtain(phone)).Returns(contact);

            var result = _service.GetById(phone);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(contact);
            _mockRepository.Verify(r => r.GetById(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void GetById_GetsFromDbAndCachesIfNotInCache()
        {
            string phone = "+34600000001";
            var contact = new Contact(phone, "Ana", "Anita", "ana@test.com");

            _mockCache.Setup(c => c.Obtain(phone)).Returns((Contact?)null);
            _mockRepository.Setup(r => r.GetById(phone))
                .Returns(Result.Success<Contact, DomainError>(contact));

            var result = _service.GetById(phone);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(contact);
            _mockCache.Verify(c => c.Add(phone, contact), Times.Once);
        }
        
        [Test]
        public void GetByAlias_ReturnsMatchingContacts()
        {
            string alias = "Anita";
            var list = new List<Contact>
            {
                new("+34600000001", "Ana", "Anita", "ana@test.com")
            };
            _mockRepository.Setup(r => r.GetByAlias(alias)).Returns(list);
            
            var result = _service.GetByAlias(alias);
            
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            _mockRepository.Verify(r => r.GetByAlias(alias), Times.Once);
        }

        [Test]
        public void GetAll_GetsList()
        {
            var list = new List<Contact>
            {
                new("+34600000001", "Ana", "Anita", "ana@test.com")
            };

            _mockRepository.Setup(r => r.GetAll(0, 5)).Returns(list);

            var result = _service.GetAll(0, 5);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            _mockRepository.Verify(r => r.GetAll(0, 5), Times.Once);
        }
    }

    [TestFixture]
    public class InvalidCases
    {
        private Mock<IContactRepository> _mockRepository = null!;
        private Mock<ICache<string, Contact>> _mockCache = null!;
        private Mock<IValidate<Contact>> _mockValidator = null!;
        private IContactsService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new Mock<IContactRepository>();
            _mockCache = new Mock<ICache<string, Contact>>();
            _mockValidator = new Mock<IValidate<Contact>>();

            _service = new ContactService(
                _mockRepository.Object,
                _mockCache.Object,
                _mockValidator.Object
            );
        }

        [Test]
        public void Create_InvalidContact_ReturnsFailure()
        {
            var dto = new ContactDto("", "", "", "");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyPhoneNumber()));

            var result = _service.Create(dto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ValidationError.EmptyPhoneNumber>();
            _mockValidator.Verify(v => v.Validate(It.IsAny<Contact>()), Times.Once);
            _mockRepository.Verify(r => r.Create(It.IsAny<Contact>()), Times.Never);
        }

        [Test]
        public void Create_PhoneAlreadyExists_ReturnsFailure()
        {
            var dto = new ContactDto("+34600000001", "Ana", "Anita", "ana@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId("+34600000001")).Returns(true);

            var result = _service.Create(dto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ContactAlredyExist>();
            _mockRepository.Verify(r => r.Create(It.IsAny<Contact>()), Times.Never);
        }

        [Test]
        public void Delete_IfIdNotFound_ReturnsFailure()
        {
            string phone = "+34600000009";

            _mockRepository.Setup(r => r.ExistId(phone)).Returns(false);

            var result = _service.Delete(phone);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ContactNotFoundId>();
            _mockRepository.Verify(r => r.Delete(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void GetById_NotInCacheNorRepository_ReturnsFailure()
        {
            string phone = "+34600000009";

            _mockCache.Setup(c => c.Obtain(phone)).Returns((Contact?)null);
            _mockRepository.Setup(r => r.GetById(phone))
                .Returns(Result.Failure<Contact, DomainError>(new ContactError.ContactNotFoundId(phone)));

            var result = _service.GetById(phone);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ContactNotFoundId>();
        }
        
        [Test]
        public void Create_EmailAlreadyExists_ReturnsFailure()
        {
            var dto = new ContactDto("+34600000001", "Ana", "Anita", "ana@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(It.IsAny<string>())).Returns(false);
            _mockRepository.Setup(r => r.ExistsEmail("ana@test.com"))
                .Returns(Result.Success<bool, DomainError>(true));
            
            var result = _service.Create(dto);
            
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.EmailAlreadyExists>();
            _mockRepository.Verify(r => r.Create(It.IsAny<Contact>()), Times.Never);
        }
        
        [Test]
        public void Update_ValidatorFails_ReturnsFailure()
        {
            string phone = "+34600000001";
            var dto = new ContactDto(phone, "", "SuperAna", "ana.maria@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Failure<bool, DomainError>(new ContactError.ValidationError.EmptyName()));
            
            var result = _service.Update(phone, dto);
            
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ValidationError.EmptyName>();
            _mockValidator.Verify(v => v.Validate(It.IsAny<Contact>()), Times.Once);
            _mockRepository.Verify(r => r.Update(It.IsAny<string>(), It.IsAny<Contact>()), Times.Never);
        }

        [Test]
        public void Update_EmailBelongsToAnotherContact_ReturnsFailure()
        {
            string phone = "+34600000001";
            var dto = new ContactDto(phone, "Ana", "Anita", "existing@test.com");
            var currentContact = new Contact(phone, "Ana", "Anita", "old@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(phone)).Returns(true);
            _mockRepository.Setup(r => r.ExistsEmail("existing@test.com"))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.GetById(phone))
                .Returns(Result.Success<Contact, DomainError>(currentContact));
            
            var result = _service.Update(phone, dto);
            
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.EmailAlreadyExists>();
            _mockRepository.Verify(r => r.Update(It.IsAny<string>(), It.IsAny<Contact>()), Times.Never);
        }
        
        [Test]
        public void Update_EmailBelongsToSameContact_Succeeds()
        {
            string phone = "+34600000001";
            var dto = new ContactDto(phone, "Ana Maria", "SuperAna", "same@test.com");
            var currentContact = new Contact(phone, "Ana", "Anita", "same@test.com");
            var updatedContact = new Contact(phone, "Ana Maria", "SuperAna", "same@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(phone)).Returns(true);
            _mockRepository.Setup(r => r.ExistsEmail("same@test.com"))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.GetById(phone))
                .Returns(Result.Success<Contact, DomainError>(currentContact));
            _mockRepository.Setup(r => r.Update(phone, It.IsAny<Contact>()))
                .Returns(Result.Success<Contact, DomainError>(updatedContact));
            
            var result = _service.Update(phone, dto);
            
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be("Ana Maria");
            _mockRepository.Verify(r => r.Update(phone, It.IsAny<Contact>()), Times.Once);
        }
        
        [Test]
        public void Update_PhoneAlreadyTakenByAnotherContact_ReturnsFailure()
        {
            string currentPhone = "+34600000001";
            string takenPhone = "+34600000002";
            var dto = new ContactDto(takenPhone, "Ana", "Anita", "ana@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(takenPhone)).Returns(true);

            var result = _service.Update(currentPhone, dto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.ContactAlredyExist>();
            _mockRepository.Verify(r => r.Update(It.IsAny<string>(), It.IsAny<Contact>()), Times.Never);
        }

        [Test]
        public void Update_ExistsEmailCheckFails_ReturnsFailure()
        {
            string phone = "+34600000001";
            var dto = new ContactDto(phone, "Ana", "Anita", "ana@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(phone)).Returns(true);
            
            _mockRepository.Setup(r => r.ExistsEmail(dto.Email))
                .Returns(Result.Failure<bool, DomainError>(new ContactError.ContactNotFoundId(phone)));

            var result = _service.Update(phone, dto);
            
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.EmailAlreadyExists>();
            _mockRepository.Verify(r => r.Update(It.IsAny<string>(), It.IsAny<Contact>()), Times.Never);
        }

        [Test]
        public void Update_GetByIdFailsDuringEmailCheck_ReturnsFailure()
        {
            string phone = "+34600000001";
            var dto = new ContactDto(phone, "Ana", "Anita", "ana@test.com");

            _mockValidator.Setup(v => v.Validate(It.IsAny<Contact>()))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.ExistId(phone)).Returns(true);
            _mockRepository.Setup(r => r.ExistsEmail(dto.Email))
                .Returns(Result.Success<bool, DomainError>(true));
            _mockRepository.Setup(r => r.GetById(phone))
                .Returns(Result.Failure<Contact, DomainError>(new ContactError.ContactNotFoundId(phone)));

            var result = _service.Update(phone, dto);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ContactError.EmailAlreadyExists>();
        }
    }
}