using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RISCVSharp
{
    interface IRV32Fetch32B
    {
        /// <summary>
        /// Fetch and 
        /// </summary>
        /// <param name=" instructionReader">Instruction memory/file stream binary reader</param>
        /// <param name="pc">Program counter</param>
        /// <param name="instruction">Fetched instruction</param>
        void Fetch32B(BinaryReader instructionReader, Core.CoreRegister<uint> pc, out uint instruction);
    }
}
