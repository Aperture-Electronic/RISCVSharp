using Microsoft.VisualStudio.TestTools.UnitTesting;
using RISCVSharp.Core;
using System;

namespace UnitTestCore
{
    [TestClass]
    public class RegisterTest
    {
        [TestMethod]
        public void RegisterInitialize()
        {
            CoreRegisterGroup<uint> x = new CoreRegisterGroup<uint>(16);
            x[0] = 0xFF000000;
            Assert.AreEqual(x[0], 0xFF000000);
        }

        [TestMethod]
        public void RegisterLink()
        {
            CoreRegisterGroup<uint> x = new CoreRegisterGroup<uint>(16);
            CoreRegister<uint> x0 = x.LinkRegister(0);
            x0.Value = 0xF1FF0000;
            Assert.AreEqual(x0.Value, x[0]);
        }
    }
}
