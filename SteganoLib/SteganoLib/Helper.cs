using System;

namespace SteganoLib
{
	static class Helper
	{
		//Does a bitshift, but wraps the overflow
		public static byte RotateLeft(byte value, int count)
		{
			return (byte)((value << count) | (value >> (8 - count)));
		}

		//Does a bitshift, but wraps the overflow
		public static byte RotateRight(byte value, int count)
		{
			return (byte)((value >> count) | (value << (8 - count)));
		}

		//Gets the bit at a certain position in a byte
		public static bool GetBit(byte b, int bitNumber)
		{
			return (b & (1 << bitNumber)) != 0;
		}

		//Gets the bit at a certain position in a byte,
		//And presents it as a byte: 00000000 or 00000001
		public static byte GetBitAsByte(byte b, int bitNumber)
		{
			//This is the same function as GetBit, but we add a conversion to Byte.
			//This is possible because a boolean also takes a byte of memory.
			return Convert.ToByte((b & (1 << bitNumber)) != 0);
		}

		//Gets the bit at a certain position in an int,
		//And presents it as an int: 0x0000 or 0x0001
		public static int GetBitAsInt(int b, int bitNumber)
		{
			//This is the same function as GetBit, but we add a conversion to Byte.
			//This is possible because a boolean also takes a byte of memory.
			return Convert.ToInt32((b & (1 << bitNumber)) != 0);
		}

		//Convert a byte to a string, and pad it with leading zeroes.
		public static string PrintAsBits(byte b)
		{
			return Convert.ToString(b, 2).PadLeft(8, '0');
		}

		//Convert an int to a string, and pad it with leading zeroes.
		public static string PrintAsBits(int i)
		{
			return Convert.ToString(i, 2).PadLeft(32, '0');
		}
	}
}
