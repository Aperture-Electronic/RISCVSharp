using RISCVSharp.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RISCVSharp
{
    public abstract class RV32Core
    {
        public abstract void OpenInstructionStream(byte[] buffer);

        public abstract void OpenInstructionStream(string path);

        public abstract void LinkDataStream(Stream link);
    }
}
