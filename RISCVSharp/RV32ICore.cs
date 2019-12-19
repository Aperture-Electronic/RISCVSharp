using System;
using System.IO;

namespace RISCVSharp
{
    namespace Core
    {
        public class RV32ICore : RV32Core, IRVDebuggable, IRV32Decode, IRV32Fetch32B
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
            /// Connect a stream to the core as a instruction stream
            /// </summary>
            /// <param name="stream">Linked stream</param>
            public override void LinkInstructionStream(Stream stream) => InstructionStream = stream;

            /// <summary>
            /// Connect a stream to the core as a data stream
            /// </summary>
            /// <param name="buffer">instruction buffer</param>
            public override void OpenInstructionStream(byte[] buffer) => InstructionStream = new MemoryStream(buffer);

            /// <summary>
            /// Create an instruction stream from a file
            /// </summary>
            /// <param name="path">Instruction binary file</param>
            public override void OpenInstructionStream(string path) => InstructionStream = new FileStream(path, FileMode.Open, FileAccess.Read);

            /// <summary>
            /// Reset core and all registers
            /// </summary>
            public override void ResetCore()
            {
                foreach (CoreRegister<uint> register in coreRegisterGroup)
                {
                    register.Value = 0;
                }

                reg_pc.Value = 0;
            }
            #endregion

            #region Stage I: Fetch
            /// <summary>
            /// Fetch instruction temporary register
            /// </summary>
            private readonly CoreRegister<uint> fetchRegister = new CoreRegister<uint>(0);

            /// <summary>
            /// Fetch and get 32-bit instruction
            /// </summary>
            /// <param name="instruction">Fetched instruction</param>
            void IRV32Fetch32B.Fetch32B()
            {
                uint pc_addr = reg_pc.Value & 0xFFFFFFFCU; // Simulate word-address memory address bus
                InstructionStream?.Seek(pc_addr, SeekOrigin.Begin);
                byte[] buffer = new byte[4];
                InstructionStream?.Read(buffer, 0, 4); // Simulate 32-bit memory data bus

                // Little endian
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(buffer);
                }

                fetchRegister.Value = BitConverter.ToUInt32(buffer);
            }
            #endregion

            #region Stage II: Decode
            /// <summary>
            /// Decode opcode temporary register (significand = LSB6)
            /// </summary>
            private readonly CoreRegister<uint> decodeOpcodeRegister = new CoreRegister<uint>(0);

            /// <summary>
            /// Decode funct3 temporary register (significand = LSB3)
            /// </summary>
            private readonly CoreRegister<uint> decodeFunct3Register = new CoreRegister<uint>(0);

            /// <summary>
            /// Decode funct7 temporary register (significand = LSB7)
            /// </summary>
            private readonly CoreRegister<uint> decodeFunct7Register = new CoreRegister<uint>(0);

            /// <summary>
            /// Decode source register 2 index temporary register (significand = LSB5)
            /// </summary>
            private readonly CoreRegister<uint> decodeRs1Register = new CoreRegister<uint>(0);

            /// <summary>
            /// Decode source register 2 index temporary register (significand = LSB5)
            /// </summary>
            private readonly CoreRegister<uint> decodeRs2Register = new CoreRegister<uint>(0);

            /// <summary>
            /// Decode destination register index temporary register (significand = LSB5)
            /// </summary>
            private readonly CoreRegister<uint> decodeRdRegister = new CoreRegister<uint>(0);

            /// <summary>
            /// Decode immediate temporary register
            /// </summary>
            private readonly CoreRegister<uint> decodeImmeRegister = new CoreRegister<uint>(0);

            /// <summary>
            /// This variable will be set when 32-bit instruction decode success
            /// </summary>
            private bool Is32BDecodeDone = false;

            /// <summary>
            /// Decode the 32-bit instruction
            /// </summary>
            void IRV32Decode.InstructionSplitDecode32B()
            {
                uint instruction = fetchRegister.Value;
                uint opcode, funct3, funct7, rs1, rs2, rd;

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
                    Is32BDecodeDone = false;
                }
                else
                {
                    // Decoding
                    rd = ((instruction & (0b1_1111U << 7)) >> 7);
                    funct3 = (instruction & (0b111U << 12)) >> 12;
                    rs1 = ((instruction & (0b1_1111U << 15)) >> 15);
                    rs2 = ((instruction & (0b1_1111U << 20)) >> 20);
                    funct7 = (instruction & (0b111_1111U << 25)) >> 25;
                    Is32BDecodeDone = true;
                }

                // Write to temporary registers
                decodeOpcodeRegister.Value = opcode;
                decodeRdRegister.Value = rd;
                decodeFunct3Register.Value = funct3;
                decodeRs1Register.Value = rs1;
                decodeRs2Register.Value = rs2;
                decodeFunct7Register.Value = funct7;
            }

            /// <summary>
            /// Decode the immediate by opcode for RV32I instruction set
            /// </summary>
            private void RV32IImmediateDecode()
            {
                // Get instruction and opcode from temporary register
                uint instruction = fetchRegister.Value;
                uint opcode = decodeOpcodeRegister.Value;

                uint imme = 0x00;

                if ((opcode == 0b0110111) || (opcode == 0b0010111)) // For LUI/AUIPC (U-type)
                {
                    imme = instruction & (~0b11111_1111111U);
                }
                else if (opcode == 0b1101111) // For JAL (J-type)
                {
                    uint bit20 = (instruction & 0x8000_0000U) >> 31;
                    uint bit10_1 = (instruction & 0x7FE0_0000U) >> 21;
                    uint bit11 = (instruction & 0x0010_0000U) >> 20;
                    uint bit19_12 = (instruction & 0x000F_FFFFU) >> 12;

                    imme = (bit20 << 20) + (bit19_12 << 12) + (bit11 << 11) + (bit10_1 << 1);
                }
                else if ((opcode == 0b1100111) || (opcode == 0b0000011)) // For JALR, other instructions (I-type)
                {
                    imme = (instruction & (~0b11111_111_11111_1111111U)) >> 20;
                }
                else if (opcode == 0b1100011) // For conditional brench instructions (B-type)
                {
                    uint bit12 = (instruction & 0x8000_0000U) >> 31;
                    uint bit10_5 = (instruction & 0x7E00_0000U) >> 25;
                    uint bit4_1 = (instruction & 0x0000_1F00U) >> 8;
                    uint bit11 = (instruction & 0x0000_0080U) >> 7;

                    imme = (bit12 << 12) + (bit11 << 11) + (bit10_5 << 5) + (bit4_1 << 1);
                }
                else if (opcode == 0b0100011) // For SB, SH, SW (S-type)
                {
                    uint bit11_5 = (instruction & 0xFE00_0000U) >> 25;
                    uint bit4_0 = (instruction & 0x0000_1F80U) >> 7;

                    imme = (bit11_5 << 5) + (bit4_0 << 0);
                }

                // Write immediate to temporary register
                decodeImmeRegister.Value = imme;
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
