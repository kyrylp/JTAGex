using System;
using System.Collections.Generic;
using System.Text;

namespace SX.JTAG.Xilinx
{
    public abstract class Chip
    {
        /// <summary>
        /// Length of the IDCODE register in bits
        /// </summary>
        public int IDCODE_REGISTER_LENGTH;

        /// <summary>
        /// IDCODE instruction
        /// </summary>
        public int IDCODE_INSTRUCTION;

        /// <summary>
        /// IDCODE instruction
        /// </summary>
        public int IDCODE_FACTORY;

        /// <summary>
        /// EXTEST instruction
        /// </summary>
        public int EXTEST_INSTRUCTION;

        /// <summary>
        /// Instruction register length
        /// </summary>
        public int INSTRUCTION_LENGTH;

        /// <summary>
        /// Length of the BOUNDARY register in bits
        /// </summary>
        public int BOUNDARY_REGISTER_LENGTH;

        /// <summary>
        /// Hardcoded table of I/O ports which need to be toggled to flash LEDs
        /// NOTE: Simplified for V1.0
        /// </summary>
        public int[] Pins;
    }
}
