using System;
using System.Collections.Generic;
using System.Text;

namespace RISCVSharp
{
    interface IRV32InstructionDecoder
    {
        /// <summary>
        /// Decode the 32-bit instruction (defualt handler)
        /// </summary>
        /// <param name="instruction">32-bit instruction from instruction memory</param>
        /// <param name="opcode">7-bit opcode</param>
        /// <param name="funct3">3-bit extended opcode</param>
        /// <param name="funct7">7-bit extended opcode</param>
        /// <param name="rs1">Register source I</param>
        /// <param name="rs2">Register source II</param>
        /// <param name="rd">Register destination</param>
        /// <returns>Is the instruction 32-bit format correct?</returns>
        private static bool InstructionDecodeRV32(uint instruction, out uint opcode, out uint funct3, out uint funct7, out int rs1, out int rs2, out int rd)
        {
            // Default decoder
            opcode = 0b0000000;
            funct3 = 0x00;
            funct7 = 0x00;
            rs1 = 0x00;
            rs2 = 0x00;
            rd = 0x00;
            return false;
        }
    }
}
