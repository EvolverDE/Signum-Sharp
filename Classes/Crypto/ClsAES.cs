using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Signum_Sharp.Classes.Crypto
{
    public class ClsAES
    {

        public byte[] AES_Encrypt(byte[] Value, byte[] Key)
        {

            Aes AES = Aes.Create();
            SHA256 Sha256 = SHA256.Create();
            byte[] Output;

            AES.GenerateIV();
            byte[] IV = AES.IV;
            AES.Key = Sha256.ComputeHash(Key);

            AES.Mode = CipherMode.CBC;
            ICryptoTransform AESEncrypter = AES.CreateEncryptor();
            byte[] Buffer = Value;
            Output = AESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length);

            // Copy the IV as the first 16 bytes of the output then copy encrypted bytes
            byte[] IVAndOutput = new byte[Output.Length - 1 + 16 + 1];
            Array.Copy(IV, IVAndOutput, 16);
            Array.Copy(Output, 0, IVAndOutput, 16, Output.Length);

            return IVAndOutput;
        }

        public byte[] AES_Decrypt(byte[] EncryptedValue, byte[] Key, byte[] IVs = null)
        {
            try
            {
                byte[] IV = new byte[16];
                byte[] Buffer = new byte[EncryptedValue.Length - 1 - 16 + 1];

                if (IVs == null)
                {

                    // Extract first 16 bytes of input stream as IV.  Copy remaining bytes into encrypted buffer
                    Array.Copy(EncryptedValue, IV, 16);
                    Array.Copy(EncryptedValue, 16, Buffer, 0, EncryptedValue.Length - 16);
                }
                else
                {
                    IV = IVs.ToArray();
                    Buffer = EncryptedValue;
                }

                Aes AES = Aes.Create();
                SHA256 Sha256 = SHA256.Create();
                AES.Key = Sha256.ComputeHash(Key);
                AES.IV = IV;
                AES.Mode = CipherMode.CBC;
                ICryptoTransform AESDecrypter = AES.CreateDecryptor();

                byte[] DecryptedValue = AESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length);

                return DecryptedValue;
            }
            catch (Exception ex)
            {
                byte[] ErrorValue = null;
                return ErrorValue;
            }
        }

    }
}
