using Microsoft.VisualStudio.TestTools.UnitTesting;
using RISCVSharp.Core;
using System;

namespace UnitTestCore
{
    [TestClass]
    public class Register
    {
        [TestMethod]
        public void RegisterInitialize()
        {
            CoreRegisterGroup<UInt32> x = new CoreRegisterGroup<uint>(32);
        }
    }
}
