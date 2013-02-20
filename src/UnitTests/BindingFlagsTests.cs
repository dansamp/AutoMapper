using System.Reflection;
using NUnit.Framework;
using System.Linq;
using Should;

namespace AutoMapper.UnitTests
{
    [TestFixture]
    public class BindingFlagsTests
    {
        public class Foo
        {
            public int PrivateSetter { get; private set; }
        }

        [Test]
        public void TestCase()
        {
            var bindingFlags = typeof (Foo).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty).OfType<PropertyInfo>().Where(mi => mi.CanRead).ToArray();

            bindingFlags.Any(pi => pi.Name == "PrivateSetter").ShouldBeTrue();

            var publicProps = typeof (Foo)
                .GetRuntimeProperties()
                .Where(propertyInfo => propertyInfo.CanRead && propertyInfo.GetMethod.IsPublic);

            publicProps.Any(pi => pi.Name == "PrivateSetter").ShouldBeTrue();
        }
    }
}
