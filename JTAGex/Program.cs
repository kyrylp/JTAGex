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
        public static FTDI ftdi = new FTDI();

        public static FTDI.FT_STATUS ft_status = FTDI.FT_STATUS.FT_OTHER_ERROR;

        // FTDI Cable Pins (MPSSE Mode)
        const byte TCK = 0;  // Orange
        const byte TDO = 1;  // Green
        const byte TDI = 2;  // Yellow
        const byte TMS = 3;  // Brown
        static byte[] data = { 0b00000000 };

        static void Main(string[] args)
        {
            try
            {
                // TODO: Add error handling for FTDI status
                ft_status = ftdi.OpenByIndex(1);
                ft_status = ftdi.SetBaudRate(9600);

                // Set TMS, TDI, TCK pins as OUTPUT
                // Set TDO pin as INPUT
                // Set MPSSE Option mode
                // See datasheet for details: https://www.ftdichip.com/Support/Documents/DataSheets/Cables/DS_C232HM_MPSSE_CABLE.pdf
                // Bits: ---, ---, ---, ---, TMS, TDI, TDO, TCK
                ft_status = ftdi.SetBitMode(0b11111101, 1);
                Thread.Sleep(500);

                int id = Read_IDCODE();

                // Compare obtained id with expected value for this part/package
                if (id == XC2C64A_VQ44.IDCODE_FACTORY)
                {
                    Console.WriteLine("IDCODE Match: 0x" + id.ToString("X8"));    // Print out IDCODE in HEX format for reference

                    // Flash LED's
                    while (true)
                    {
                        Test_Pins(XC2C64A_VQ44.Pins);
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
        /// <param name="pins">
        /// int[] specifying bits in BOUNDARY_REGISTER which 
        /// need to be flipped (1/0) in order to turn on the LEDs
        /// </param>
        static void Test_Pins(int[] pins)
        {
            // Test-Logic-Reset and go to Run-Test-Idle 
            tms(1); tms(1); tms(1); tms(1); tms(1); tms(0);

            foreach (int pin in pins)
            {
                // Advance to Shift-DR
                tms(1); tms(0); tms(0);

                set(TDI, 0);

                // Shift in all zeroes except for a bit specified in pins[]
                for (int i = 0; i < XC2C64A_VQ44.BOUNDARY_REGISTER_LENGTH; i++)
                {
                    if (i == pin)
                    {
                        set(TDI, 1);
                        tck(); tck();
                        set(TDI, 0);
                    }
                    else
                    {
                        tck();
                    }
                }

                // Update-DR and go to Run-Test-Idle
                tms(1); tms(1); tms(0);

                // Load EXTEST instruction (0b00000000)
                // Advance to Shift-IR
                tms(1); tms(1); tms(0); tms(0);

                int val = XC2C64A_VQ44.EXTEST_INSTRUCTION;
                for (int i = 0; i < XC2C64A_VQ44.INSTRUCTION_LENGTH; i++)
                {
                    val >>= i;
                    set(TDI, val & 1);
                    tck();
                }

                //Advance to Run-Test/Idle
                tms(1); tms(1); tms(0);

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Read IDCODE register
        /// </summary>
        /// <returns>
        /// Int32 value of the IDCODE register
        /// </returns>
        public static int Read_IDCODE()
        {
            // Test-Logic-Reset and advance TAP to Run-Test-Idle
            TAP_Reset();

            // Advance TAP to Shift-IR
            tms(1); tms(1); tms(0); tms(0);

            // Shift in IDCODE instruction
            tdi(1); tdi(0); tdi(0); tdi(0); tdi(0); tdi(0); tdi(0); set(TDI, 0);

            // Exit1-IR
            tms(1);

            // Advance to Shift-DR
            tms(1); tms(1); tms(0); tms(0);

            // Shift out IDCODE
            int result = 0;
            for (int i = 0; i < XC2C64A_VQ44.IDCODE_REGISTER_LENGTH; i++)
            {
                int bit = get(TDO);
                tdi(0);
                Debug.Write(bit);

                // Reverse bit order because we are reading out LSB first
                result |= bit << i;
            }

            Debug.Write('\n');

            // Advance TAP controller to Run-Test-Idle
            tms(1); tms(1); tms(0);

            return result;
        }

        /// <summary>
        /// Reset JTAG TAP controller and advance to RUN_TEST_IDLE
        /// </summary>
        static void TAP_Reset()
        {
            tms(1); tms(1); tms(1); tms(1); tms(1); tms(0);
        }

        /// <summary>
        /// Set TMS High or Low
        /// </summary>
        /// <param name="value">
        /// Logic state (0 or 1)
        /// </param>
        static void tms(int value)
        {
            set(TMS, value);
            tck();
        }

        /// <summary>
        /// Read FTDI Pin State of a given Pin
        /// </summary>
        /// <param name="pin">Pin to read state from</param>
        /// <returns>Pin state (1 or 0)</returns>
        static int get(byte pin)
        {
            byte state = 0;
            ftdi.GetPinStates(ref state);

            return state >> 1 & 1;
        }

        /// <summary>
        /// Set TDI High or Low
        /// </summary>
        /// <param name="value">
        /// Logic state (1 or 0)
        /// </param>
        static void tdi(int value)
        {
            set(TDI, value);
            tck();
        }

        /// <summary>
        /// Set FTDI pin to a state determined by specified value
        /// </summary>
        /// <param name="pin">FTDI Pin to set High or Low</param>
        /// <param name="value">Logic state (1 or 0)</param>
        static void set(byte pin, int value)
        {
            if (value == 1)
            {
                data[0] |= (byte)(1 << pin);
            }
            else
            {
                data[0] &= (byte)~(1 << pin);
            }

            try
            {
                // Write a byte to the FTDI device to set pin values
                uint bytesWritten = 0;
                ftdi.Write(data, 1, ref bytesWritten);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: ", ex.Message);
            }
        }

        /// <summary>
        /// Toggle TCK pin 1/0 with 1-millisecond pulse width and 50% duty cycle
        /// </summary>
        static void tck()
        {
            set(TCK, 1);
            Thread.Sleep(1);
            set(TCK, 0);
            Thread.Sleep(1);
        }
    }
}
