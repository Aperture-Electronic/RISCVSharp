using System;
using System.Collections.Generic;
using System.Text;

namespace RISCVSharp
{
    public interface IRVDebuggable
    {
        /// <summary>
        /// Print all of the core register's status to string
        /// </summary>
        public string PrintCoreRegisterGroupStatus();
    }
}
