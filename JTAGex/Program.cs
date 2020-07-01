// Simple JTAG tester
// USAGE: JTAGex.exe [NUMBER]
// Where NUMBER is optional FTDI interface number

using System;
using System.Diagnostics;
using System.Threading;

using FTD2XX_NET;
using SX.JTAG.Xilinx;

namespace CPLD.Exercise
{
    class Program
    {
        // FTDI 232H Cable object
        static FTDI Ftdi = new FTDI();

        // FTDI Cable Pins (MPSSE Mode)
        const byte TCK = 0;  // Orange
        const byte TDO = 1;  // Green
        const byte TDI = 2;  // Yellow
        const byte TMS = 3;  // Brown
        static byte[] Data = { 0b00000000 };

        static void Main(string[] args)
        {
            try
            {
                // Attempt to open FTDI interface specified as CLI parameter
                int index = 1;
                if ((args.Length == 1 && !int.TryParse(args[0], out index)) || args.Length > 1 )
                {
                    Console.WriteLine("USAGE: JTAGex.exe [NUMBER]\nWhere NUMBER is an FTDI interface number");
                    return;
                }

                if (Ftdi.OpenByIndex((uint)index) != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("ERROR: Can not open FTDI interface #" + index.ToString());
                    return;
                }

                if (Ftdi.SetBaudRate(9600) != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("ERROR: Can not set FTDI interface baud rate (9600 bps)");
                    return;
                }

                // Set TMS, TDI, TCK pins as OUTPUT
                // Set TDO pin as INPUT
                // Set MPSSE Option mode
                // See datasheet for details: https://www.ftdichip.com/Support/Documents/DataSheets/Cables/DS_C232HM_MPSSE_CABLE.pdf
                // Bits: ---, ---, ---, ---, TMS, TDI, TDO, TCK
                if (Ftdi.SetBitMode(0b11111101, 1) != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("ERROR: Can not set FTDI BitMode");
                    return;
                }

                // NOTE: In the future we can perform automatic discovery of JTAG-capable chips,
                // but in this version we hardcode the chip type for simplicity
                Chip chip = new XC2C64A_VQ44();

                int id = Read_IDCODE(chip);

                // Compare obtained id with expected value for this part/package
                if (id == chip.IDCODE_FACTORY)
                {
                    Console.WriteLine("IDCODE Match: 0x" + id.ToString("X8"));    // Print out IDCODE in HEX format for reference

                    // Flash LED's
                    Console.WriteLine("Flashing LEDs...\nPress Ctrl-C to stop");
                    while (true)
                    {
                        Test_Pins(chip);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: IDCODE Mismatch!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: ", ex.Message);
            }
        }

        /// <summary>
        /// Toggle pins by running EXTEST instruction
        /// </summary>
        /// <param name="chip">
        /// Chip object specifying among other properties, bits in BOUNDARY_REGISTER 
        /// which need to be flipped in order to blink the LEDs
        /// </param>
        static void Test_Pins(Chip chip)
        {
            // Test-Logic-Reset and advance TAP to Run-Test-Idle
            TAP_Reset();

            foreach (int pin in chip.Pins)
            {
                // Advance to Shift-DR
                Tms(1); Tms(0); Tms(0);

                Set(TDI, 0);

                // Shift in all zeroes except for a bit specified in pins[]
                for (int i = 0; i < chip.BOUNDARY_REGISTER_LENGTH; i++)
                {
                    if (i == pin)
                    {
                        Set(TDI, 1);
                        Tck(); Tck();
                        Set(TDI, 0);
                    }
                    else
                    {
                        Tck();
                    }
                }

                // Update-DR and go to Run-Test-Idle
                Tms(1); Tms(1); Tms(0);

                // Advance to Shift-IR
                Tms(1); Tms(1); Tms(0); Tms(0);

                // Load EXTEST instruction
                Load_Instruction(chip.EXTEST_INSTRUCTION, chip.INSTRUCTION_LENGTH);

                //Advance to Run-Test/Idle
                Tms(1); Tms(0);

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Reads IDCODE register
        /// </summary>
        /// <param name="chip">
        /// Chip object specifying among other properties, bits in BOUNDARY_REGISTER 
        /// which need to be flipped in order to blink the LEDs
        /// </param>
        /// <returns>Int32 IDCODE register value</returns>
        public static int Read_IDCODE(Chip chip)
        {
            // Test-Logic-Reset and advance TAP to Run-Test-Idle
            TAP_Reset();

            // Advance TAP to Shift-IR
            Tms(1); Tms(1); Tms(0); Tms(0);

            // Load IDCODE instruction
            Load_Instruction(chip.IDCODE_INSTRUCTION, chip.INSTRUCTION_LENGTH);

            // Advance to Shift-DR
            Tms(1); Tms(1); Tms(0); Tms(0);

            // Shift out IDCODE
            int result = 0;
            for (int i = 0; i < chip.IDCODE_REGISTER_LENGTH; i++)
            {
                int bit = Get(TDO);
                Tdi(0);
                Debug.Write(bit);

                // Reverse bit order because we are reading out LSB first
                result |= bit << i;
            }

            Debug.Write('\n');

            // Advance TAP controller to Run-Test-Idle
            Tms(1); Tms(1); Tms(0);

            return result;
        }

        /// <summary>
        /// Shifts in the instruction into IR
        /// </summary>
        /// <param name="instr">Int32 instruction value</param>
        /// <param name="length">Instruction register length</param>
        static void Load_Instruction(int instr, int length)
        {
            for (int i = 0; i < length; i++)
            {
                instr >>= i;
                Set(TDI, instr & 1);
                if (i == length - 1)
                {
                    Tms(1); // Set TMS along with the last bit to properly perform Exit1-IR
                }
                else
                {
                    Tck();
                }
            }
        }

        /// <summary>
        /// Reset JTAG TAP controller and advance to RUN_TEST_IDLE
        /// </summary>
        static void TAP_Reset()
        {
            Tms(1); Tms(1); Tms(1); Tms(1); Tms(1); Tms(0);
        }

        /// <summary>
        /// Set TMS pin High or Low
        /// </summary>
        /// <param name="value">
        /// Logic state (1 or 0)
        /// </param>
        static void Tms(int value)
        {
            Set(TMS, value);
            Tck();
        }

        /// <summary>
        /// Read FTDI Pin State of a given Pin
        /// </summary>
        /// <param name="pin">Pin to read state from</param>
        /// <returns>Pin state (1 or 0)</returns>
        static int Get(byte pin)
        {
            byte state = 0;
            Ftdi.GetPinStates(ref state);

            return state >> 1 & 1;
        }

        /// <summary>
        /// Set TDI pin High or Low
        /// </summary>
        /// <param name="value">
        /// Logic state (1 or 0)
        /// </param>
        static void Tdi(int value)
        {
            Set(TDI, value);
            Tck();
        }

        /// <summary>
        /// Set FTDI pin to a state determined by specified value
        /// </summary>
        /// <param name="pin">FTDI Pin to set High or Low</param>
        /// <param name="value">Logic state (1 or 0)</param>
        static void Set(byte pin, int value)
        {
            if (value == 1)
            {
                Data[0] |= (byte)(1 << pin);
            }
            else
            {
                Data[0] &= (byte)~(1 << pin);
            }

            try
            {
                // Write a byte to the FTDI device to set pin values
                uint bytesWritten = 0;
                Ftdi.Write(Data, 1, ref bytesWritten);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: ", ex.Message);
            }
        }

        /// <summary>
        /// Toggles TCK pin 1/0 with 1-millisecond pulse width and 50% duty cycle
        /// </summary>
        static void Tck()
        {
            Set(TCK, 1);
            Thread.Sleep(1);
            Set(TCK, 0);
            Thread.Sleep(1);
        }
    }
}
