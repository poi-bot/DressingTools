﻿using Chocopoi.DressingTools.Dresser;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Dresser
{
    public class DresserRegistryTest : DTEditorTestBase
    {
        [Test]
        public void GetDresserByTypeNameTest()
        {
            Assert.NotNull(DresserRegistry.GetDresserByTypeName(typeof(DefaultDresser).FullName));
            Assert.Null(DresserRegistry.GetDresserByTypeName("Some.Random.Name.Does.Not.Exist"));
        }

        [Test]
        public void GetAvailableDresserKeysTest()
        {
            var keys = DresserRegistry.GetAvailableDresserKeys();
            Assert.NotNull(keys);
            Assert.GreaterOrEqual(keys.Length, 1);
        }

        [Test]
        public void GetDresserByIndexTest()
        {
            Assert.NotNull(DresserRegistry.GetDresserByIndex(0));
        }

        [Test]
        public void GetDresserKeyIndexByTypeNameTest()
        {
            Assert.AreEqual(DresserRegistry.GetDresserKeyIndexByTypeName(typeof(DefaultDresser).FullName), 0);
            Assert.AreEqual(DresserRegistry.GetDresserKeyIndexByTypeName("Some.Random.Name.Does.Not.Exist"), -1);
        }

        [Test]
        public void GetDresserByNameTest()
        {
            Assert.NotNull(DresserRegistry.GetDresserByName("Default Dresser"));
        }
    }
}
