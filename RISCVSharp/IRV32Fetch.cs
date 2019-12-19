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
        void Fetch32B();
    }

    public interface IRV32Fetch16B
    {
        /// <summary>
        /// Fetch and get 16-bit instruction (defalut handler)
        /// </summary>
        void Fetch16B();
    }
}
