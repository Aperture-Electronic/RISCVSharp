using System;
using System.Collections.Generic;
using System.Text;

namespace RISCVSharp
{
    public interface IRV32Decode
    {
        /// <summary>
        /// Decode the 32-bit instruction
        /// </summary>
        void InstructionSplitDecode32B();
    }
}
