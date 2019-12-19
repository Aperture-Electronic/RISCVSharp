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
            core.OpenInstructionStream(BitConverter.IsLittleEndian ? buffer : buffer.Reverse().ToArray());
            core.ResetCore();

            MethodInfo fetch = typeof(RV32ICore).GetMethod($"{nameof(RISCVSharp)}.{nameof(IRV32Fetch32B)}.Fetch32B", BindingFlags.Instance | BindingFlags.NonPublic);
            fetch.Invoke(core, null);

            FieldInfo fetched = typeof(RV32ICore).GetField("fetchRegister", BindingFlags.Instance | BindingFlags.NonPublic);
            CoreRegister<uint> reg = fetched.GetValue(core) as CoreRegister<uint>;

            Console.WriteLine($"Instruction in memory = {instruction.ToString("X8")}, Instruction fetched = {reg.Value.ToString("X8") }");
            Assert.AreEqual(reg.Value, instruction);
        }

        [TestMethod()]
        public void RV32InstructionSplitDecodeTest()
        {
            RV32ICore core = new RV32ICore();
            core.ResetCore();

            uint instruction = 0b0100000_00011_00010_000_00001_0110011U; // sub r1, r2, r3

            // Direct set fetch register
            FieldInfo fetched = typeof(RV32ICore).GetField("fetchRegister", BindingFlags.Instance | BindingFlags.NonPublic);
            CoreRegister<uint> freg = new CoreRegister<uint>(instruction);
            fetched.SetValue(core, freg);

            // Decode
            MethodInfo decode = typeof(RV32ICore).GetMethod($"{nameof(RISCVSharp)}.{nameof(IRV32Decode)}.InstructionSplitDecode32B", BindingFlags.Instance | BindingFlags.NonPublic);
            decode.Invoke(core, null);

            Dictionary<string, uint> asserts = new Dictionary<string, uint>()
            {
                { "decodeOpcodeRegister", 0b0110011U },
                { "decodeFunct3Register", 0b000U },
                { "decodeFunct7Register", 0b0100000U },
                { "decodeRs1Register",0x02U },
                { "decodeRs2Register", 0x03U },
                { "decodeRdRegister" ,0x01U},
            };

            foreach (KeyValuePair<string, uint> assert in asserts)
            {
                FieldInfo decoded = typeof(RV32ICore).GetField(assert.Key, BindingFlags.Instance | BindingFlags.NonPublic);
                CoreRegister<uint> reg = decoded.GetValue(core) as CoreRegister<uint>;
                Assert.AreEqual(reg.Value, assert.Value);
            }
        }

        [TestMethod()]
        public void RV32ImmediacateDecodeTest()
        {
            RV32ICore core = new RV32ICore();

            // Sub actions
            Action<uint> SetInstruction = delegate (uint instruction)
            {
                core.ResetCore();

                // Direct set fetch register
                FieldInfo fetched = typeof(RV32ICore).GetField("fetchRegister", BindingFlags.Instance | BindingFlags.NonPublic);
                CoreRegister<uint> freg = new CoreRegister<uint>(instruction);
                fetched.SetValue(core, freg);

                // Decode
                MethodInfo decode = typeof(RV32ICore).GetMethod($"{nameof(RISCVSharp)}.{nameof(IRV32Decode)}.InstructionSplitDecode32B", BindingFlags.Instance | BindingFlags.NonPublic);
                decode.Invoke(core, null);
            };

            Func<uint> GetImmediateDecoded = delegate
            {
                // Immediate decode
                MethodInfo immeDecode = typeof(RV32ICore).GetMethod($"RV32IImmediateDecode", BindingFlags.Instance | BindingFlags.NonPublic);
                immeDecode.Invoke(core, null);

                FieldInfo imme = typeof(RV32ICore).GetField("decodeImmeRegister", BindingFlags.Instance | BindingFlags.NonPublic);
                CoreRegister<uint> ireg = imme.GetValue(core) as CoreRegister<uint>;
                return ireg.Value;
            };

            // U-type instructions
            // lui x1, 0x831F3000
            SetInstruction(0b00001_0110111U + (0x831F3000U & 0xFFFFF000U));
            Assert.AreEqual(GetImmediateDecoded(), 0x831F3000U);

            // auipc x1, 0x6714F000
            SetInstruction(0b00001_0010111U + (0x6714F000U & 0xFFFFF000U));
            Assert.AreEqual(GetImmediateDecoded(), 0x6714F000U);

            // J-type instruction
            // jal x1, 0x1CDF30
            uint jal = 0x1CDF30U;
            uint jal_20 = (jal & 0x100000U) >> 20;
            uint jal_10_1 = (jal & 0x007FEU) >> 1;
            uint jal_11 = (jal & 0x00800U) >> 11;
            uint jal_19_12 = (jal & 0xFF000U) >> 12;
            SetInstruction(0b00001_1101111U + (((jal_20 << 19) + (jal_10_1 << 9) + (jal_11 << 8) + jal_19_12) << 12));
            Assert.AreEqual(GetImmediateDecoded(), 0x1CDF30U);
        }
    }
}

