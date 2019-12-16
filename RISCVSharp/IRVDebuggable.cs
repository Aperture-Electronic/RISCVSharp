using System;
using System.Collections.Generic;
using System.Text;

namespace RISCVSharp
{
    interface IRISCVDebuggable
    {
        public string PrintCoreRegisterGroupStatus();
    }
}
