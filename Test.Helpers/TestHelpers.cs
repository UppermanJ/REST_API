using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace Test.Helpers
{
    public static class TestHelpers
    {
        public static string GetDefault500Message(string traceId)
        {
            return $"Something went wrong.  Log ID: {traceId}";
        }
        public static void AssertConstructorThrowsNullExceptionsWhenArgumentsAreNotProvided(Type type)
        {
            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                var mocks = parameters.Select(
                    p =>
                    {
                        Type mockType = typeof(Mock<>).MakeGenericType(new[] { p.ParameterType });
                        return (Mock)Activator.CreateInstance(mockType);
                    }).ToArray();

                for (int i = 0; i < parameters.Length; i++)
                {
                    var mocksCopy = mocks.Select(m => m.Object).ToArray();
                    mocksCopy[i] = null;
                    try
                    {
                        constructor.Invoke(mocksCopy);
                        Assert.Fail("ArgumentNullException expected for parameter {0} of constructor, but no exception was thrown", parameters[i].Name);
                    }
                    catch (TargetInvocationException ex)
                    {
                        Assert.AreEqual(typeof(ArgumentNullException),
                            ex.InnerException.GetType(),
                            string.Format("ArgumentNullException expected for parameter {0} of constructor, but exception of type {1} was thrown", parameters[i].Name, ex.InnerException.GetType()));
                    }
                }
            }
        }
    }
}
