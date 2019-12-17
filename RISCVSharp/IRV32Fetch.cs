using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RISCVSharp
{
    public interface IRV32Fetch32B
    {
        /// <summary>
        /// Fetch and get 32-bit instruction (defalut handler)
        /// </summary>
        /// <param name=" instructionStream">Instruction memory/file stream</param>
        /// <param name="pc">Program counter</param>
        /// <param name="instruction">Fetched instruction</param>
        void Fetch32B(Stream instructionStream, Core.CoreRegister<uint> pc, out uint instruction);
    }

    public interface IRV32Fetch16B
    {
        /// <summary>
        /// Fetch and get 16-bit instruction (defalut handler)
        /// </summary>
        /// <param name=" instructionReader">Instruction memory/file stream</param>
        /// <param name="pc">Program counter</param>
        /// <param name="instruction">Fetched instruction</param>
        void Fetch16B(Stream instructionStream, Core.CoreRegister<uint> pc, out ushort instruction);
    }
}
