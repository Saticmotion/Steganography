using System;

namespace SteganoLib
{
	static class Helper
	{
		//Doet een bitshift naar links, maar wrapt overflow
		public static byte RotateLeft(byte value, int count)
		{
			return (byte)((value << count) | (value >> (8 - count)));
		}

		//Doet een bitshift naar rechts, maar wrapt overflow
		public static byte RotateRight(byte value, int count)
		{
			return (byte)((value >> count) | (value << (8 - count)));
		}

		//Vind de bit op een bepaalde positite in een byte.
		public static bool GetBit(byte b, int bitNumber)
		{
			return (b & (1 << bitNumber)) != 0;
		}

		//Vind de bit op een bepaalde positie en geef terug als een byte:
		//0000 0000 of 0000 0001
		public static byte GetBitAsByte(byte b, int bitNumber)
		{
			//Zelfde functie als GetBit, maar een conversie naar Byte.
			//Dit is mogelijk omdat een boolean ook een byte geheugen gebruikt.
			return Convert.ToByte((b & (1 << bitNumber)) != 0);
		}

		//Vind de bit op een bepaalde positie en geef terug als een byte:
		//0x0000 of 0x0001
		public static int GetBitAsInt(int b, int bitNumber)
		{
			//Zelfde functie als GetBit, maar een conversie naar Byte.
			//Dit is mogelijk omdat een boolean ook een byte geheugen gebruikt.
			//Hierdoor is een cast naar Int32 ook mogelijk.
			return Convert.ToInt32((b & (1 << bitNumber)) != 0);
		}

		//Converteer een byte naar een string, en voeg padding toe
		//in de vorm van nullen aan de linkerkant: 00010101
		public static string PrintAsBits(byte b)
		{
			return Convert.ToString(b, 2).PadLeft(8, '0');
		}

		//Converteer een int naar een string, en voeg padding toe
		//in de vorm van nullen aan de linkerkant
		public static string PrintAsBits(int i)
		{
			return Convert.ToString(i, 2).PadLeft(32, '0');
		}
	}
}
