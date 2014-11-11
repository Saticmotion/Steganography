using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using desEncryption;

namespace DesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            java.lang.Class clazz = typeof(Encryption);
            java.lang.Thread.currentThread().setContextClassLoader(clazz.getClassLoader());

            try
            {
                if (Encryption.encrypt("D:\\Documents\\testfile.txt", "D:\\Documents\\testfile", "D:\\Documents\\dmgpokemon.xlsx", EncryptionMode.ENCRYPT) == java.lang.Boolean.TRUE)
                    Console.WriteLine("Encryption complete");
                if (Encryption.encrypt("D:\\Documents\\testfile.des", "D:\\Documents\\testfileDecrypted", "D:\\Documents\\dmgpokemon.xlsx", EncryptionMode.DECRYPT) == java.lang.Boolean.TRUE)
                    Console.WriteLine("Decryption complete");
            }
            catch(java.io.FileNotFoundException)
            {
                Console.Write("bestand niet gevonden");
            }
            Console.ReadLine();
        }
    }
}
