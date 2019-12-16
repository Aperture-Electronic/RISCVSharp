using Microsoft.VisualStudio.TestTools.UnitTesting;
using RISCVSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RISCVSharp.Core.Tests
{
    [TestClass()]
    public class RV32ICoreTest
    {
        [TestMethod()]
        public void RV32ICoreInitializeTest()
        {
            RV32ICore core = new RV32ICore();
            CoreRegister<uint> ra = typeof(RV32ICore).GetField("reg_ra", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(core) as CoreRegister<uint>;
            CoreRegisterGroup<uint> rGroup = typeof(RV32ICore).GetField("coreRegisterGroup", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(core) as CoreRegisterGroup<uint>;
            ra.Value = 0x00100020;
            Assert.AreEqual(ra.Value, rGroup[1]);
        }

        [TestMethod()]
        public void PrintCoreRegisterGroupStatusTest()
        {
            RV32ICore core = new RV32ICore();
            Console.WriteLine(core.PrintCoreRegisterGroupStatus());
        }

        [TestMethod()]
        public void RV32InstructionDecoderTest()
        {
            RV32ICore core = new RV32ICore();
            UInt32 instruction = 0b0100000_00011_00010_000_00001_0110011U; // sub r1, r2, r3
            object[] args = new object[] { instruction, null, null, null, null, null, null };
            MethodInfo decode = typeof(RV32ICore).GetMethod("InstructionDecodeRV32", BindingFlags.Static | BindingFlags.NonPublic);
            decode.Invoke(core, args);

            Assert.AreEqual((uint)args[1], 0b0110011U); // Opcode
            Assert.AreEqual((uint)args[2], 0b000U); // Funct3
            Assert.AreEqual((uint)args[3], 0b0100000U); // Funct7
            Assert.AreEqual((int)args[4], 0x02); // Rs1
            Assert.AreEqual((int)args[5], 0x03); // Rs2
            Assert.AreEqual((int)args[6], 0x01); // Rd
        }
    }
}

