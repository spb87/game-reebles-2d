// Sanity tests for the Reebles2D test harness. Verifies the Unity Test
// Framework pipeline works and guards the pinned editor version (DEC-1).

using NUnit.Framework;
using UnityEngine;

namespace Reebles2D.Tests.EditMode
{
    /// <summary>
    /// Minimal EditMode tests proving the test assembly compiles and runs.
    /// </summary>
    public class SanityTests
    {
        /// <summary>
        /// The project pins Unity 6000.3.x (DEC-1). This fails loudly if the
        /// suite is ever run under a different editor major.minor version.
        /// </summary>
        [Test]
        public void UnityVersion_IsPinned6000_3()
        {
            Assert.That(Application.unityVersion, Does.StartWith("6000.3"));
        }

        /// <summary>
        /// Trivial arithmetic check — proves NUnit assertions execute at all.
        /// </summary>
        [Test]
        public void Arithmetic_BasicInvariant_Holds()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
            Assert.That(2 + 2, Is.Not.EqualTo(5));
        }
    }
}
