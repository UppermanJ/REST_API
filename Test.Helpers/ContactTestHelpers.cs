using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models;
using NUnit.Framework;

namespace Test.Helpers
{
    public class ContactTestHelpers
    {
        public static Contact GenerateContact()
        {
            return new Contact()
            {
                Id = 0,
                Name = new Name
                {
                    First = "name.first-" + Guid.NewGuid(),
                    Middle = "name.middle-" + Guid.NewGuid(),
                    Last = "name.last-" + Guid.NewGuid()
                },
                Address = new Address
                {
                    Street = "address.street-" + Guid.NewGuid(),
                    City = "address.city-" + Guid.NewGuid(),
                    State = "address.state-" + Guid.NewGuid(),
                    Zip = "address.zip-" + Guid.NewGuid()
                },
                Phone = new List<Phone>()
                {
                    new Phone
                    {
                        Number = "phone.mobile-" + Guid.NewGuid(),
                        Type = PhoneType.Mobile
                    },
                    new Phone
                    {
                        Number = "phone.work-" + Guid.NewGuid(),
                        Type = PhoneType.Work
                    },
                    new Phone
                    {
                        Number = "phone.home-" + Guid.NewGuid(),
                        Type = PhoneType.Home
                    }
                },
                Email = "email-" + Guid.NewGuid()
            };
        }

        public static void AssertContactMatchesAllButId(Contact actual, Contact expected)
        {
            if (expected.Name == null)
            {
                Assert.That(actual.Name, Is.Null);
            }
            else
            {
                var actualName = actual.Name;
                var expectedName = expected.Name;
                Assert.That(actualName.First, Is.EqualTo(expectedName.First));
                Assert.That(actualName.Last, Is.EqualTo(expectedName.Last));
                Assert.That(actualName.Middle, Is.EqualTo(expectedName.Middle));
            }

            if (expected.Address == null)
            {
                Assert.That(actual.Address, Is.Null);
            }
            else
            {
                var actualAddress = actual.Address;
                var expectedAddress = expected.Address;
                Assert.That(actualAddress.City, Is.EqualTo(expectedAddress.City));
                Assert.That(actualAddress.State, Is.EqualTo(expectedAddress.State));
                Assert.That(actualAddress.Street, Is.EqualTo(expectedAddress.Street));
                Assert.That(actualAddress.Zip, Is.EqualTo(expectedAddress.Zip));
            }

            Assert.That(actual.Phone, Has.Count.EqualTo(expected.Phone.Count()));
            var index = 0;
            while (index < actual.Phone.Count())
            {
                var actualPhone = actual.Phone.ToArray()[index];
                var expectedPhone = expected.Phone.ToArray()[index];
                Assert.That(actualPhone.Number, Is.EqualTo(expectedPhone.Number));
                Assert.That(actualPhone.Type, Is.EqualTo(expectedPhone.Type));
                index++;
            }

            Assert.That(actual.Email, Is.EqualTo(expected.Email));
        }

        public static void AssertAllContactsMatch(IEnumerable<Contact> actual, IEnumerable<Contact> expected)
        {
            var expectedContacts = expected as Contact[] ?? expected.ToArray();
            var actualContacts = actual as Contact[] ?? actual.ToArray();

            Assert.That(actualContacts, Has.Length.EqualTo(expectedContacts.Count()));
            foreach (var expectedContact in expectedContacts)
            {
                var actualContact = actualContacts.First(contact => contact.Id == expectedContact.Id);
                Assert.That(actualContact.Id, Is.EqualTo(expectedContact.Id));
                AssertContactMatchesAllButId(actualContact, expectedContact);
            }
        }

        public static void AssertCallRecordMatchesContact(CallRecord actual, Contact expected)
        {
            Assert.That(actual.Name.First, Is.EqualTo(expected.Name.First));
            Assert.That(actual.Name.Middle, Is.EqualTo(expected.Name.Middle));
            Assert.That(actual.Name.Last, Is.EqualTo(expected.Name.Last));
            Assert.That(actual.Phone, Is.EqualTo(expected.Phone.First(p => p.Type == PhoneType.Home).Number));
        }
    }
}
