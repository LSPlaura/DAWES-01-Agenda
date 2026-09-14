using AgendaContactos.Back.Configuration;
using AgendaContactos.Back.DTOs;
using AgendaContactos.Back.Utils;
using FluentAssertions;

namespace AgendaContactos.Tests.Utils;

[TestFixture]
public class ContactNormalizerTests
{
    [TestFixture]
    public class ValidCases
    {
        [Test]
        public void Normalize_CompleteData_NormalizesCorrectly()
        {
            var dto = new ContactDto(" 600111222 ", "  Ana  ", "  Anita  ", "  ANA@TEST.COM  ");

            var result = ContactNormalizer.Normalize(dto);

            result.Should().NotBeNull();
            result.PhoneNumber.Should().Be($"{Config.DefaultCountryCode}600111222");
            result.Name.Should().Be("Ana");
            result.Alias.Should().Be("Anita");
            result.Email.Should().Be("ana@test.com");
        }

        [Test]
        public void Normalize_PhoneWithHyphensAndSpaces_CleansAndAddsPrefix()
        {
            var dto = new ContactDto(" 600-111-222 ", "Ana", "Anita", "ana@test.com");

            var result = ContactNormalizer.Normalize(dto);

            result.PhoneNumber.Should().Be($"{Config.DefaultCountryCode}600111222");
        }

        [Test]
        public void Normalize_PhoneWithInternationalPrefix_DoesNotModifyIt()
        {
            var dto = new ContactDto("+34600111222", "Ana", "Anita", "ana@test.com");

            var result = ContactNormalizer.Normalize(dto);

            result.PhoneNumber.Should().Be("+34600111222");
        }

        [Test]
        public void Normalize_EmptyAlias_AssignsNameToAlias()
        {
            var dto = new ContactDto("+34600111222", "Ana Maria", "", "ana@test.com");

            var result = ContactNormalizer.Normalize(dto);

            result.Alias.Should().Be("Ana Maria");
        }

        [Test]
        public void Normalize_AliasWithBlankSpaces_AssignsNameToAlias()
        {
            var dto = new ContactDto("+34600111222", "Ana Maria", "   ", "ana@test.com");

            var result = ContactNormalizer.Normalize(dto);

            result.Alias.Should().Be("Ana Maria");
        }

        [Test]
        public void Normalize_UppercaseEmail_ConvertsToLowerCase()
        {
            var dto = new ContactDto("+34600111222", "Ana", "Anita", "  TEST.USER@GMAIL.COM  ");

            var result = ContactNormalizer.Normalize(dto);

            result.Email.Should().Be("test.user@gmail.com");
        }
    }

    [TestFixture]
    public class InvalidCases
    {
        [Test]
        public void Normalize_NullPhone_ReturnsEmpty()
        {
            var dto = new ContactDto(null, "Ana", "Anita", "ana@test.com");

            var result = ContactNormalizer.Normalize(dto);

            result.PhoneNumber.Should().BeEmpty();
        }

        [Test]
        public void Normalize_EmptyPhone_ReturnsEmpty()
        {
            var dto = new ContactDto("", "Ana", "Anita", "ana@test.com");

            var result = ContactNormalizer.Normalize(dto);

            result.PhoneNumber.Should().BeEmpty();
        }

        [Test]
        public void Normalize_NullEmail_ReturnsEmpty()
        {
            var dto = new ContactDto("+34600111222", "Ana", "Anita", null);

            var result = ContactNormalizer.Normalize(dto);

            result.Email.Should().BeEmpty();
        }

        [Test]
        public void Normalize_NullName_ReturnsEmpty()
        {
            var dto = new ContactDto("+34600111222", null, "Anita", "ana@test.com");

            var result = ContactNormalizer.Normalize(dto);

            result.Name.Should().BeEmpty();
        }
    }
}