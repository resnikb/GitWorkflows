using System;
using GitWorkflows.Package.Extensions;
using NUnit.Framework;

namespace GitWorkflows.Package.Tests.ArgumentsTests
{
    [TestFixture]
    public class EnsureNotNullTests
    {
        [Test]
        public void WhenTypeContainsValueProperty_ThrowsInvalidOperationException()
        {
            Assert.That(() => Arguments.EnsureNotNull(new { Int = 5 }), Throws.InvalidOperationException );    
        }
                
        [Test]
        public void WhenTypeContainsNoProperties_ThrowsInvalidOperationException()
        {
            Assert.That(() => Arguments.EnsureNotNull(new {}), Throws.InvalidOperationException );    
        }

        [Test]
        public void WhenOnePropertyIsNull_ThrowsArgumentNullException()
        {
            Assert.That(
                () => Arguments.EnsureNotNull(new {x = "a", y = (object)null }),
                Throws.InstanceOf<ArgumentNullException>()
                      .With.Property("ParamName").EqualTo("y")
            );
        }

        [Test]
        public void WhenNoPropertyIsNull_DoesNotThrow()
        {
            Assert.That(
                () => Arguments.EnsureNotNull(new {x = "a", y = new object() }),
                Throws.Nothing
            );
        }

        [Test]
        public void WhenObjectIsNotOfAnonymousType_ThrowsException()
        {
            Assert.That(() => Arguments.EnsureNotNull(new object()), Throws.InstanceOf<NotSupportedException>());
        }
    }
}