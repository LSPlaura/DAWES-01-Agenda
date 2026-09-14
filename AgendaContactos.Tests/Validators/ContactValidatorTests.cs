namespace AgendaContactos.Tests.Validators;

using AgendaContactos.Back.Models;
using AgendaContactos.Back.Validators;
using NUnit.Framework;

[TestFixture]
public class ContactValidatorTests
{
    [TestFixture]
    public class ValidCases
    {
        private ContactValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ContactValidator();
        }

        [Test]
        public void Validate_CorrectContact_NoErrors()
        {
            var contact = new Contact("+34600111222", "Ana*", "Anita", "ana@test.com");

            var result = _validator.Validate(contact);

            Assert.That(result.IsSuccess, Is.True);
        }
    }

    [TestFixture]
    public class InvalidCases
    {
        private ContactValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new ContactValidator();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("        ")]
        [TestCase("/600111222")]
        [TestCase("+0600111222")]
        [TestCase("+100000")]
        [TestCase("+1000000000000000")]
        public void Validate_NotValidPhoneNumber(string? phoneNumber)
        {
            var contacto = new Contact(phoneNumber, "Ana", "Anita", "ana@test.com");

            var result = _validator.Validate(contacto);

            Assert.That(result.IsFailure, Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void Validate_NotValidName(string? name)
        {
            var contacto = new Contact("+34600111222", name, "Anita", "ana@test.com");

            var result = _validator.Validate(contacto);

            Assert.That(result.IsFailure, Is.True);
        }
        
        [TestCase("")]
        [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void Validate_NotValidAlias(string? alias)
        {
            var contacto = new Contact("+34600111222", "Ana", alias, "ana@test.com");

            var result = _validator.Validate(contacto);

            Assert.That(result.IsFailure, Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("a@aa.aa")]
        [TestCase("aa@a.aa")]
        [TestCase("aa@aa.a")]
        [TestCase("aabb.cc")]
        [TestCase("aa@bbcc")]
        [TestCase("aa*@bb.cc")]
        public void Validate_NotValidEmail(string? email)
        {
            var contacto = new Contact("+34600111222", "Ana", "Anita", email);

            var result = _validator.Validate(contacto);

            Assert.That(result.IsFailure, Is.True);
        }
    }
}