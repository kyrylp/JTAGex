using System;
using System.Runtime.CompilerServices;

namespace SX.JTAG.Xilinx
{
    /// <summary>
    /// Simplified Xilinx chip functions and configuration from BSDL file
    /// https://bsdl.info/view.htm?sid=44118061aee25ef888ca59b002289d77
    /// NOTE: Specific to each device and package version
    /// </summary>
    public class XC2C64A_VQ44 : Chip
    {
        public XC2C64A_VQ44()
        {
            IDCODE_REGISTER_LENGTH = 32;
            IDCODE_INSTRUCTION = 0b00000001;
            IDCODE_FACTORY = 0x06E5E093;
            EXTEST_INSTRUCTION = 0b00000000;
            INSTRUCTION_LENGTH = 8;
            BOUNDARY_REGISTER_LENGTH = 192;
            Pins = new int[] { 95, 191 };
        }
    }

    public class XC2C32A_VQ44 : Chip
    {
        public XC2C32A_VQ44()
        {
            IDCODE_REGISTER_LENGTH = 32;
            IDCODE_INSTRUCTION = 0b00000001;
            IDCODE_FACTORY = 0x06E1C093;
            EXTEST_INSTRUCTION = 0b00000000;
            INSTRUCTION_LENGTH = 8;
            BOUNDARY_REGISTER_LENGTH = 97;
            Pins = new int[] { 46, 94 };
        }
    }
}
