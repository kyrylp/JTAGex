using System;

namespace SX.JTAG.Xilinx
{
    /// <summary>
    /// Simplified Xilinx chip functions and configuration from BSDL file
    /// NOTE: Specific to each device and package version
    /// </summary>
    public class XC2C64A_VQ44
    {
        /// <summary>
        /// Length of the IDCODE register in bits
        /// </summary>
        public const int IDCODE_REGISTER_LENGTH = 32;

        /// <summary>
        /// IDCODE instruction
        /// </summary>
        public const int IDCODE_INSTRUCTION = 0b00000001;

        /// <summary>
        /// IDCODE instruction
        /// </summary>
        public const int IDCODE_FACTORY = 0x06E5E093;

        /// <summary>
        /// EXTEST instruction
        /// </summary>
        public const int EXTEST_INSTRUCTION = 0b00000000;

        /// <summary>
        /// Instruction register length
        /// </summary>
        public const int INSTRUCTION_LENGTH = 8;

        /// <summary>
        /// Length of the BOUNDARY register in bits
        /// </summary>
        public const int BOUNDARY_REGISTER_LENGTH = 192;

        /// <summary>
        /// Hardcoded table of I/O ports which need to be toggled to flash LEDs
        /// NOTE: Simplified for V1.0
        /// </summary>
        public static readonly int[] Pins = new int[] { 95, 191 };
    }
}
