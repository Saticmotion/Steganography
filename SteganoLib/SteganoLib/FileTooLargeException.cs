using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteganoLib
{
	public class FileTooLargeException : Exception
	{
		public FileTooLargeException(string message) : base(message){}
	}
}
