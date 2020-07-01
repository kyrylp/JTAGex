Simple JTAG Exerciser

This is an initial version of a JTAG test tool which has a very limited scope and currently supports only one JTAG compatible device in one package (QF44).

A NuGet package library (SX.JTAG.Xilinx) contains description of device-specific JTAG/Boundary Scan parameters and can be extended to support additional devices.

The main project (JTAGex) has dependency on SX.JTAG.Xilinx library as well as on FTDI NuGet library (FTD2XX_NET).

===========================================================================================
Build Instructions:
1. Download/Unpack ZIP into C:\Dev\JTAGex
2. Open C:\Dev\JTAGex\JTAGex.sln using VS2019
3. Build solution

NOTE: If NuGet packages can not be found, make sure that "C:\Dev\JTAGex\packages" path is included in NuGet package sources

===========================================================================================
USAGE: JTAGex.exe [NUMBER]
Where NUMBER is optional FTDI interface number
===========================================================================================

