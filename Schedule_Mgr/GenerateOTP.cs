using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OtpNet;

namespace Schedule_Mgr
{
   class GenerateOTP
    {
        public void main()
        {
            var key = KeyGeneration.GenerateRandomKey(20);

            var base32String = Base32Encoding.ToString(key);
            var base32Bytes = Base32Encoding.ToBytes(base32String);

            Console.WriteLine(base32String);
            var totp = new Totp(base32Bytes);
        }
    }
}
