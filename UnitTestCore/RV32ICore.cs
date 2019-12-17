using Microsoft.VisualStudio.TestTools.UnitTesting;
using RISCVSharp.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RISCVSharp.Core.Tests
{
    [TestClass()]
    public class RV32ICoreTest
    {
        [TestMethod()]
        public void RV32ICoreInitializeTest()
        {
            RV32ICore core = new RV32ICore();
            CoreRegister<uint> ra = typeof(RV32ICore).GetField("abi_ra", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(core) as CoreRegister<uint>;
            CoreRegisterGroup<uint> rGroup = typeof(RV32ICore).GetField("coreRegisterGroup", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(core) as CoreRegisterGroup<uint>;
            ra.Value = 0x00100020;
            Assert.AreEqual(ra.Value, rGroup[1]);
        }

        [TestMethod()]
        public void PrintCoreRegisterGroupStatusTest()
        {
            RV32ICore core = new RV32ICore();
            Console.WriteLine((core as IRVDebuggable).PrintCoreRegisterGroupStatus());
        }

        [TestMethod()]
        public void RV32InstructionFetch32BTest()
        {
            RV32ICore core = new RV32ICore();

            uint instruction = 0b0100000_00011_00010_000_00001_0110011U; // sub r1, r2, r3
            byte[] buffer = BitConverter.GetBytes(instruction);
            MemoryStream ms = new MemoryStream(BitConverter.IsLittleEndian ? buffer : buffer.Reverse().ToArray());

            CoreRegister<uint> pc = new CoreRegister<uint>(0x00000000U);
            object[] args = new object[] { ms, pc, null };
            MethodInfo decode = typeof(RV32ICore).GetMethod($"{nameof(RISCVSharp)}.{nameof(IRV32Fetch32B)}.Fetch32B", BindingFlags.Instance | BindingFlags.NonPublic);
            decode.Invoke(core, args);

            Console.WriteLine($"Instruction in memory = {instruction.ToString("X8")}, Instruction fetched = {((uint)args[2]).ToString("X8") }");
            Assert.AreEqual((uint)args[2], instruction);
        }

        [TestMethod()]
        public void RV32InstructionDecoderTest()
        {
            RV32ICore core = new RV32ICore();
            uint instruction = 0b0100000_00011_00010_000_00001_0110011U; // sub r1, r2, r3
            object[] args = new object[] { instruction, null, null, null, null, null, null };
            MethodInfo decode = typeof(RV32ICore).GetMethod($"{nameof(RISCVSharp)}.{nameof(IRV32InstructionDecoder)}.InstructionDecode32B", BindingFlags.Instance | BindingFlags.NonPublic);
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

