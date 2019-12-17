using System;
using System.IO;

namespace RISCVSharp
{
    namespace Core
    {
        public class RV32ICore : RV32Core, IRVDebuggable, IRV32InstructionDecoder, IRV32Fetch32B
        {
            // Register group
            private const int coreRegisterCount = 32;
            private readonly CoreRegisterGroup<uint> coreRegisterGroup;

            #region Specific function registers (linked to registers in the core group)
            /// <summary>
            /// x0 register, constant 0
            /// </summary>
            private readonly CoreRegister<uint> constant_0;
            /// <summary>
            /// x1 register, return address, link register
            /// </summary>
            private readonly CoreRegister<uint> abi_ra;
            /// <summary>
            /// x2 register, stack pointer
            /// </summary>
            private readonly CoreRegister<uint> abi_sp;
            /// <summary>
            /// x3 register, global pointer
            /// </summary>
            private readonly CoreRegister<uint> abi_gp;
            /// <summary>
            /// x4 register, thread pointer
            /// </summary>
            private readonly CoreRegister<uint> abi_tp;
            #endregion

            // Program counter
            private readonly CoreRegister<uint> reg_pc;

            /// <summary>
            /// Create a RV32I 32-bit integer RISC-V Core
            /// </summary>
            public RV32ICore()
            {
                coreRegisterGroup = new CoreRegisterGroup<uint>(coreRegisterCount);

                // Link the specific function registers
                constant_0 = coreRegisterGroup.LinkRegister(0);
                abi_ra = coreRegisterGroup.LinkRegister(1);
                abi_sp = coreRegisterGroup.LinkRegister(2);
                abi_gp = coreRegisterGroup.LinkRegister(3);
                abi_tp = coreRegisterGroup.LinkRegister(4);

                // The x0 register always 0
                constant_0.Value = 0;

                // All register set to zero
                foreach (CoreRegister<uint> register in coreRegisterGroup)
                {
                    register.Value = 0;
                }

                // Clear program counter (PC)
                reg_pc = new CoreRegister<uint>(0);
            }

            #region User interface
            /// <summary>
            /// Instruction stream (simulated instruction bus)
            /// </summary>
            public Stream InstructionStream { get; private set; }

            /// <summary>
            /// Data stream (simulated data bus)
            /// </summary>
            public Stream DataStream { get; private set; }

            /// <summary>
            /// Connect a stream to the core as a data stream
            /// </summary>
            /// <param name="stream">Linked stream</param>
            public override void LinkDataStream(Stream stream) => DataStream = stream;

            /// <summary>
            /// Connect a stream to the kernel as a data stream
            /// </summary>
            /// <param name="buffer">instruction buffer</param>
            public override void OpenInstructionStream(byte[] buffer) => InstructionStream = new MemoryStream(buffer);

            /// <summary>
            /// Create an instruction stream from a file
            /// </summary>
            /// <param name="path">Instruction binary file</param>
            public override void OpenInstructionStream(string path) => InstructionStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            #endregion

            #region Stage I: Fetch
            /// <summary>
            /// Fetch and get 32-bit instruction
            /// </summary>
            /// <param name=" instructionStream">Instruction memory/file stream</param>
            /// <param name="pc">Program counter</param>
            /// <param name="instruction">Fetched instruction</param>
            void IRV32Fetch32B.Fetch32B(Stream instructionStream, CoreRegister<uint> pc, out uint instruction)
            {
                uint pc_addr = pc.Value & 0xFFFFFFFCU; // Simulate word-address memory address bus
                instructionStream?.Seek(pc_addr, SeekOrigin.Begin);
                byte[] buffer = new byte[4];
                instructionStream?.Read(buffer, 0, 4); // Simulate 32-bit memory data bus

                // Little endian
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                instruction = BitConverter.ToUInt32(buffer);
            }
            #endregion

            #region Stage II: Decode
            /// <summary>
            /// Decode the 32-bit instruction
            /// </summary>
            /// <param name="instruction">32-bit instruction from instruction memory</param>
            /// <param name="opcode">7-bit opcode</param>
            /// <param name="funct3">3-bit extended opcode</param>
            /// <param name="funct7">7-bit extended opcode</param>
            /// <param name="rs1">Register source I</param>
            /// <param name="rs2">Register source II</param>
            /// <param name="rd">Register destination</param>
            /// <returns>Is the instruction 32-bit format correct?</returns>
            bool IRV32InstructionDecoder.InstructionDecode32B(
                uint instruction, out uint opcode, out uint funct3, out uint funct7, out int rs1, out int rs2, out int rd)
            {
                // Get the opcode
                opcode = instruction & 0b0111_1111U;

                // 32-bits instruction 
                // Format: XXXB BB11 (BBB != 111)
                if (((opcode & 0b11) != 0b11) || ((opcode & 0b11100) == 0b11100))
                {
                    // Not 32-bit instruction
                    funct3 = 0x00;
                    funct7 = 0x00;
                    rs1 = 0x00;
                    rs2 = 0x00;
                    rd = 0x00;
                    return false;
                }

                // Decoding
                rd = (int)((instruction & (0b1_1111U << 7)) >> 7);
                funct3 = (instruction & (0b111U << 12)) >> 12;
                rs1 = (int)((instruction & (0b1_1111U << 15)) >> 15);
                rs2 = (int)((instruction & (0b1_1111U << 20)) >> 20);
                funct7 = (instruction & (0b111_1111U << 25)) >> 25;

                return true;
            }

            #endregion

            #region Debugger Interface
            /// <summary>
            /// Print all of the core register's status to string
            /// </summary>
            string IRVDebuggable.PrintCoreRegisterGroupStatus()
            {
                string result = "";

                for (int i = 0; i < coreRegisterCount; i++)
                {
                    result += $"r{i} = {coreRegisterGroup[i].ToString("X8")}\n";
                }
                result += $"pc = {reg_pc.Value.ToString("X8")}\n";

                return result;
            }
            #endregion
        }
    }
}
