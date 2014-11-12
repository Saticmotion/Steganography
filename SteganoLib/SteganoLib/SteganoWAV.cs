using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SteganoLib
{
    public static class SteganoWav
    {
        private static byte[] _targetBytes;
        private static byte[] _inputBytes;
        private static int _sampleLength, _headerLength, _position;
        public static String Extention = "";
        public static byte[] Embed(String targetPath, String inputPath)
        {
			//lees alle bytes van target(carrier file) en input(message) in.
            _targetBytes = File.ReadAllBytes(targetPath);
            _inputBytes = File.ReadAllBytes(inputPath);
            
            AnalyseHeader();
            char[] extention = Path.GetExtension(inputPath).Replace(".", "").ToCharArray();
            
            //controleer of er genoeg bytes in _targetBytes beschikbaar zijn om de message in te zetten
            int byteNeeded = _inputBytes.Length * _sampleLength * 8 + extention.Length * 
				_sampleLength * 8 + 8 *_sampleLength +32 * _sampleLength;
            if (byteNeeded > _targetBytes.Length - _headerLength)
            {
                throw new FileTooLargeException("The file you are trying to embed is too large.");
            }
			_position = _headerLength;//
            EncodeByte(_inputBytes.Length, 31);


            //zet de extentie in _targetBytes
            EncodeByte(extention.Length);
            foreach (var c in extention)
            {
                EncodeByte(c);
            }

            //zet het bericht in _targetBytes
            foreach (byte t in _inputBytes)
            {
                EncodeByte(t);
            }


            return _targetBytes;
        }
        private static void EncodeByte(int m, int length = 7)
        {
            for (int pos = length; pos >= 0; pos--)
            {
                //get a single bit out of m
                byte temp = (byte)(((m & (1 << pos)) > 0) ? 1 : 0);//either 1 or 0

                byte x = _targetBytes[_position];
                //change last bit of x
                if (x % 2 == 0 && temp == 1)
                {
                    x++;
                }
                else if (x % 2 == 1 && temp == 0)
                {
                    x--;
                }
                _targetBytes[_position] = x;//put adjusted byte in targetBytes
                _position += _sampleLength;
            }
        }
        private static void AnalyseHeader()
        {
			//haal de samplelength uit de wav file + in bytes
            _sampleLength = (_targetBytes[34] + _targetBytes[35] *(int)Math.Pow(2,8)) / 8;
			
			//controleer chunksize bytes 16-19, little endian, zet om naar int
            int chunksize = _targetBytes[16] + _targetBytes[17]*(int)Math.Pow(2,8) 
				+ _targetBytes[18] *(int)Math.Pow(2,16)+ _targetBytes[19]*(int)Math.Pow(2,24);
	        //set de headerlength
			_headerLength = 20 + chunksize + 8;

        }

        public static byte[] Extract(String targetPath)
        {
			
            _targetBytes = File.ReadAllBytes(targetPath);
           
            AnalyseHeader();
			_position = _headerLength;//

            int messageLength = DecodeByte(31);
            int extentionLength = DecodeByte();
			//controleer of message wel in deze file kan
			int byteNeeded = messageLength * _sampleLength * 8 + extentionLength *
				_sampleLength * 8 + 8 * _sampleLength + 32 * _sampleLength;
	        if (messageLength < 0)
	        {
		        throw  new ArithmeticException("messageLength is kleiner dan 0");
	        }
			if (byteNeeded > _targetBytes.Length - _headerLength)
			{
				throw new FileTooLargeException("De message is te groot om in deze file te kunnen zitten.");
			}

            char[] extention = new char[extentionLength];
			//haal de extentie van de message uit de file
            for (int i = 0; i < extentionLength; i++)
            {
                extention[i] = (char) DecodeByte();
            }
			
            Extention = new string(extention);
			//controleer of de extentie geldig is
	        if (Extention.IndexOfAny(Path.GetInvalidFileNameChars())>=0)
	        {
		        throw new ArgumentException("extentie is niet geldig");
	        }
			
			//maak array die zolang is als message
            _inputBytes = new byte[messageLength];
			
            for (int i = 0; i < messageLength; i++)
            {
                _inputBytes[i]= (byte)DecodeByte();
            }
            return _inputBytes;
        }
        //haalt het resultaat uit de _targetBytes
		private static int DecodeByte(int length = 7)
        {
            int result = 0;
            for (int pos = 0; pos <= length; pos++)
            {
                //shift result 1 plaats naar links
				//tel laatste bit van _targetBytes[p] op bij result
                result = result << 1;
                result += _targetBytes[_position] & 1;
                _position += _sampleLength;
            }
            return result;
        }
    }
}
