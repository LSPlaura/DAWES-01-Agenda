using AgendaContactos.Back.Cache;
using AgendaContactos.Back.Models;
using FluentAssertions;
using NUnit.Framework;

namespace AgendaContactos.Tests.Cache;

[TestFixture]
public class CacheLruTest
{
    [TestFixture]
    public class ValidCases
    {
        private LruCache _cache = null!;

        [SetUp]
        public void SetUp()
        {
            _cache = new LruCache(2);
        }
      
        [Test]
        public void Add_MaxCache_RemovesLeastRecentlyUsed()
        {
            var contact1 = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            var contact2 = new Contact("+34600000002", "Beto", "Betito", "beto@test.com");
            var contact3 = new Contact("+34600000003", "Carlos", "Carlitos", "carlos@test.com");
          
            _cache.Add(contact1.PhoneNumber, contact1);
            _cache.Add(contact2.PhoneNumber, contact2);
          
            var obtainedC1 = _cache.Obtain(contact1.PhoneNumber);
            obtainedC1.Should().NotBeNull();
           
            _cache.Add(contact3.PhoneNumber, contact3);
            var obtainedC2 = _cache.Obtain(contact2.PhoneNumber);
            obtainedC2.Should().BeNull();
        }
      
        [Test]
        public void Add_AlreadyAddedContact_UpdatesPosition()
        {
            var c1 = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            var c2 = new Contact("+34600000002", "Beto", "Betito", "beto@test.com");
            var c3 = new Contact("+34600000003", "Carlos", "Carlitos", "carlos@test.com");
          
            _cache.Add(c1.PhoneNumber, c1);
            _cache.Add(c2.PhoneNumber, c2);
            _cache.Add(c1.PhoneNumber, c1);
            
            _cache.Add(c3.PhoneNumber, c3);
            _cache.Obtain(c1.PhoneNumber).Should().NotBeNull().And.BeSameAs(c1);
        }

        [Test]
        public void Obtain_GetsContact()
        {
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            _cache.Add(contact.PhoneNumber, contact);

            var obtained = _cache.Obtain(contact.PhoneNumber);
            obtained.Should().NotBeNull();
            obtained!.PhoneNumber.Should().Be(contact.PhoneNumber);
        }
      
        [Test]
        public void Obtain_UpdatesPosition()
        {
            var c1 = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            var c2 = new Contact("+34600000002", "Beto", "Betito", "beto@test.com");
            var c3 = new Contact("+34600000003", "Carlos", "Carlitos", "carlos@test.com");
          
            _cache.Add(c1.PhoneNumber, c1);
            _cache.Add(c2.PhoneNumber, c2);
            _cache.Obtain(c1.PhoneNumber);
            
            _cache.Add(c3.PhoneNumber, c3);
            _cache.Obtain(c1.PhoneNumber).Should().NotBeNull();
            _cache.Obtain(c2.PhoneNumber).Should().BeNull();
        }
      
        [Test]
        public void Delete_DeletesContact()
        {
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
          
            _cache.Add(contact.PhoneNumber, contact);
            _cache.Delete(contact.PhoneNumber).Should().BeTrue();
            _cache.Obtain(contact.PhoneNumber).Should().BeNull();
        }
    }
   
    [TestFixture]
    public class InvalidCases
    {
        private LruCache _cache = null!;

        [SetUp]
        public void SetUp()
        {
            _cache = new LruCache(2);
        }
        
        [Test]
        public void DefaultParameter_SetIfCapacityNotValid()
        {
            _cache = new LruCache(0);
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            _cache.Add(contact.PhoneNumber, contact);
            var result = _cache.Obtain(contact.PhoneNumber);
            result.Should().NotBeNull();
        }
      
        [Test]
        public void Add_NotAddedIfKeyAndPhoneAreDifferent()
        {
            var contact = new Contact("+34600000001", "Ana", "Anita", "ana@test.com");
            _cache.Add("+34699999999", contact);

            var obtained = _cache.Obtain(contact.PhoneNumber);
            obtained.Should().BeNull();
        }
       
        [Test]
        public void Obtain_NotAdded_ReturnsNull()
        {
            _cache.Obtain("+34699999999").Should().BeNull();
        }
       
        [Test]
        public void Delete_NotAdded_ReturnsFalse()
        {
            _cache.Delete("+34699999999").Should().BeFalse();
        }
    }
}