using System;
using System.Collections;
using System.Collections.Generic;

namespace RISCVSharp
{
    namespace Core
    {
        /// <summary>
        /// A core register in RISC-V core
        /// </summary>
        /// <typeparam name="T">Register type, for RV32 that is UInt32, for RV64 that is UInt64</typeparam>
        public class CoreRegister<T> where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            /// <summary>
            /// Value of register
            /// </summary>
            public T Value { set; get; }

            /// <summary>  
            /// Create a core register by a initinal value
            /// </summary>
            /// <param name="initinal">Initinal value</param>
            public CoreRegister(T initinal)
            {
                if ((typeof(T) != typeof(UInt32)) && (typeof(T) != typeof(UInt64))) throw new Exception("Unable to use a type other than 32-bit/64-bit unsigned integer type.");
                Value = initinal;
            }
        }

        /// <summary>
        /// Core register group in RISC-V
        /// </summary>
        /// <typeparam name="T">Register type, for RV32 that is UInt32, for RV64 that is UInt64</typeparam>
        public class CoreRegisterGroup<T> : IEnumerable<CoreRegister<T>> where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            private readonly List<CoreRegister<T>> registerGroup;

            /// <summary>
            /// Get the register value
            /// </summary>
            /// <param name="i">Register index</param>
            public T this[int i]
            {
                get => registerGroup[i].Value;
                set => registerGroup[i].Value = value;
            }

            /// <summary>
            /// Link the register object
            /// </summary>
            /// <param name="index">Register index</param>
            public CoreRegister<T> LinkRegister(int index) => registerGroup[index];

            /// <summary>
            /// Get the enumerator of register group
            /// </summary>
            public IEnumerator<CoreRegister<T>> GetEnumerator() => registerGroup.GetEnumerator();

            /// <summary>
            /// Get the enumerator of register group
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator() => registerGroup.GetEnumerator();

            /// <summary>
            /// Create a core register group by a initinal value
            /// </summary>
            /// <param name="count">Number of registers</param>
            public CoreRegisterGroup(int count)
            {
                registerGroup = new List<CoreRegister<T>>();
                for (int i = 0; i < count; i++)
                {
                    registerGroup.Add(new CoreRegister<T>(default)); // Create the register and add it to list
                }
            }
        }
    }
}
