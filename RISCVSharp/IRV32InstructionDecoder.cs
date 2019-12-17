using System;
using System.Collections.Generic;
using System.Text;

namespace RISCVSharp
{
    public interface IRV32InstructionDecoder
    {
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
        bool InstructionDecode32B(uint instruction, out uint opcode, out uint funct3, out uint funct7, out int rs1, out int rs2, out int rd);
    }
}
