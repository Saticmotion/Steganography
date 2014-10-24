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
        public static string Extention = "";
        public static byte[] Embed(String targetPath, String inputPath)
        {
            _targetBytes = File.ReadAllBytes(targetPath);
            _inputBytes = File.ReadAllBytes(inputPath);
            
            AnalyseHeader();
            char[] extention = Path.GetExtension(inputPath).Replace(".", "").ToCharArray();
            
            //check if file is large enough 
            int byteNeeded = _inputBytes.Length * _sampleLength * 8 + extention.Length * 8 + 8;
            if (byteNeeded > _targetBytes.Length - _headerLength)
            {
                throw new FileTooLargeException("The file you are trying to embed is too large.");
            }
            _position = _headerLength + _sampleLength - 1;
            EncodeByte(_inputBytes.Length, 31);


            //embed file extention
            EncodeByte(extention.Length);
            foreach (var c in extention)
            {
                EncodeByte(c);
            }

            //embed the message
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
                _targetBytes[_position] = x;//put adjusted byte in buffer
                _position += _sampleLength;
            }
        }
        private static void AnalyseHeader()
        {
            _sampleLength = _targetBytes[34] / 8;
            int headersize = _targetBytes[16];

            switch (headersize)
            {
                case 16:
                    _headerLength = 44;
                    break;
                case 18:
                    _headerLength = 46;
                    break;
                default:
                    _headerLength = 200;
                    break;
            }
        }

        public static byte[] Extract(String targetPath)
        {
            _targetBytes = File.ReadAllBytes(targetPath);
           
            AnalyseHeader();
            _position = _headerLength + _sampleLength - 1;
            int messageLength = DecodeByte(31);
            int extentionLength = DecodeByte();
            char[] extention = new char[extentionLength];
            for (int i = 0; i < extentionLength; i++)
            {
                extention[i] = (char) DecodeByte();
            }
            Extention = new string(extention);
            _inputBytes = new byte[messageLength];
            for (int i = 0; i < messageLength; i++)
            {
                _inputBytes[i]= (byte)DecodeByte();
            }
            return _inputBytes;
        }
        private static int DecodeByte(int length = 7)
        {
            int result = 0;
            for (int pos = 0; pos <= length; pos++)
            {
                
                result = result << 1;
                result += _targetBytes[_position] & 1;
                _position += _sampleLength;
            }
            return result;
        }
    }
}
