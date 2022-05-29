using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Signum_Sharp.Classes.Crypto;

namespace Signum_Sharp.Modules
{
    public class ModCrypto
    {
        
        public static string GetSHA256HashString(string Input)
        {
            byte[] InputBytes = Encoding.UTF8.GetBytes(Input);

            SHA256 Sha256 = SHA256.Create();
            InputBytes = Sha256.ComputeHash(InputBytes);

            string HashString = "";

            for (int i = 0; i <= InputBytes.Length - 1; i++)
            {
                byte T_Byte = InputBytes[i];

                string T_HEXString = T_Byte.ToString("X2");

                if (T_HEXString.Length == 1)
                    T_HEXString = "0" + T_HEXString;

                HashString += T_HEXString;
            }


            return HashString;
        }

        public static string GetPubKeyHEX(string PassPhrase)
        {
            SHA256 Sha256 = SHA256.Create();
            byte[] HashPhrase = Sha256.ComputeHash(Encoding.UTF8.GetBytes(PassPhrase).ToArray());

            byte[][] Keys = Keygen(HashPhrase);

            string Pubkey = ModGlobalFunctions.ByteArrayToHEXString(Keys[0]);

            return Pubkey;
        }

        public static string GetSignKeyHEX(string PassPhrase)
        {
            SHA256 Sha256 = SHA256.Create();
            byte[] HashPhrase = Sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(PassPhrase).ToArray());

            byte[][] Keys = Keygen(HashPhrase);

            string SignKey = ModGlobalFunctions.ByteArrayToHEXString(Keys[1]);

            return SignKey;
        }

        public static string GetAgreeKeyHEX(string PassPhrase)
        {
            SHA256 Sha256 = SHA256.Create();
            byte[] HashPhrase = Sha256.ComputeHash(Encoding.UTF8.GetBytes(PassPhrase).ToArray());

            byte[][] Keys = Keygen(HashPhrase);

            string AgreeKey = ModGlobalFunctions.ByteArrayToHEXString(Keys[2]);

            return AgreeKey;
        }

        /// <summary>
        /// Generates the Masterkeys from PassPhrase 0=PublicKey; 1=SignKey; 2=AgreementKey
        /// </summary>
        /// <param name="PassPhrase"></param>
        /// <returns>List of Masterkeys<</returns>
        public static List<string> GetMasterKeys(string PassPhrase)
        {
            SHA256 Sha256 = SHA256.Create();
            byte[] HashPhrase = Sha256.ComputeHash(Encoding.UTF8.GetBytes(PassPhrase).ToArray());
            byte[][] Keys = Keygen(HashPhrase);
            List<string> KeyHESList = new List<string>
            {
                ModGlobalFunctions.ByteArrayToHEXString(Keys[0]),
                ModGlobalFunctions.ByteArrayToHEXString(Keys[1]),
                ModGlobalFunctions.ByteArrayToHEXString(Keys[2])
            };
            return KeyHESList;
        }


        /// <summary>
        /// Generates the Masterkeys from KeyHash
        /// </summary>
        /// <param name="KeyHash">The 32 Byte hashed PassPhrase</param>
        /// <returns>Array of Masterkeys</returns>
        public static byte[][] Keygen(byte[] KeyHash)
        {
            byte[] PublicKey = new byte[32];
            byte[] SignKey = new byte[32];

            ClsCurve25519 Curve = new ClsCurve25519();
            Curve.Clamp(ref KeyHash);
            byte[] temp = new byte[0];
            Curve.Core( PublicKey,  SignKey,  KeyHash,  temp);

            return new[] { PublicKey, SignKey, KeyHash };
        }

        /// <summary>
        /// Generates the Signature of the MessageHEX
        /// </summary>
        /// <param name="MessageHEX">Message as HEX String</param>
        /// <param name="PrivateKey">The Private Key to Sign with</param>
        /// <returns>Signature in HEX String</returns>
        public static string GenerateSignature(string MessageHEX, string PrivateKey)
        {
            ClsEC_KCDSA ECKCDSA = new ClsEC_KCDSA();
            byte[] Secure = ModGlobalFunctions.HEXStringToByteArray(PrivateKey);
            SHA256 Sha256 = SHA256.Create();
            byte[] MessageHash = Sha256.ComputeHash(ModGlobalFunctions.HEXStringToByteArray(MessageHEX));
            byte[] Message_Secure_Array = MessageHash.Concat(Secure).ToArray();
            byte[] Message_Secure_Hash = Sha256.ComputeHash(Message_Secure_Array);
            byte[] MessageSecureKey_Hash = Keygen(Message_Secure_Hash)[0];
            byte[] MessageHash_MSKey = MessageHash.Concat(MessageSecureKey_Hash).ToArray();
            byte[] MH_MSKey_Hash = Sha256.ComputeHash(MessageHash_MSKey);
            byte[] MH_MSKey_Hash_Copy = MH_MSKey_Hash.ToArray();
            byte[] SignValue = ECKCDSA.Sign(MH_MSKey_Hash, Message_Secure_Hash, Secure);
            byte[] SignValue_MHMSKeyHash = SignValue.Concat(MH_MSKey_Hash_Copy).ToArray();

            return ModGlobalFunctions.ByteArrayToHEXString(SignValue_MHMSKeyHash);
        }

        /// <summary>
        /// Verify the Signature of the UnsignedMessageHEX String
        /// </summary>
        /// <param name="Signature">The Signature as HEX String</param>
        /// <param name="UnsignedMessageHex">The Unsigned Message as HEX String</param>
        /// <param name="PublicKey">The Public Key as HEX String</param>
        /// <returns></returns>
        public static bool VerifySignature(string Signature, string UnsignedMessageHex, string PublicKey)
        {
            if (!ModGlobalFunctions.MessageIsHEXString(Signature) | !ModGlobalFunctions.MessageIsHEXString(UnsignedMessageHex) | !ModGlobalFunctions.MessageIsHEXString(PublicKey))
                return false;

            byte[] publicKeyBytes = ModGlobalFunctions.HEXStringToByteArray(PublicKey);
            byte[] SignValue = ModGlobalFunctions.HEXStringToByteArray(Signature.Substring(0, 64));
            byte[] SignHash = ModGlobalFunctions.HEXStringToByteArray(Signature.Substring(64));

            ClsEC_KCDSA ECKCDSA = new ClsEC_KCDSA();
            byte[] VerifyHash = ECKCDSA.Verify(SignValue, SignHash, publicKeyBytes);

            SHA256 Sha256 = SHA256.Create();
            byte[] MessageHash = Sha256.ComputeHash(ModGlobalFunctions.HEXStringToByteArray(UnsignedMessageHex));
            byte[] MessageHash_VerifyHash = MessageHash.Concat(VerifyHash).ToArray();

            Sha256 = SHA256.Create();
            byte[] MessageVerify_Hash = Sha256.ComputeHash(MessageHash_VerifyHash);

            string SignHashHEXString = ModGlobalFunctions.ByteArrayToHEXString(SignHash);
            string MessageVerifyHashHEXString = ModGlobalFunctions.ByteArrayToHEXString(MessageVerify_Hash);

            if (SignHashHEXString == MessageVerifyHashHEXString)
                return true;
            else
                return false;
        }

        public static string AESEncrypt2HEXStr(string Input, string Password)
        {
            ClsAES T_AES = new ClsAES();

            byte[] T_DecryptedBytes = T_AES.AES_Encrypt(Encoding.UTF8.GetBytes(Input), Encoding.UTF8.GetBytes(Password));

            return ModGlobalFunctions.ByteArrayToHEXString(T_DecryptedBytes);
        }

        public static byte[] AESEncrypt2ByteArray(string Input, string Password)
        {
            ClsAES T_AES = new ClsAES();

            byte[] T_DecryptedBytes = T_AES.AES_Encrypt(Encoding.UTF8.GetBytes(Input), Encoding.UTF8.GetBytes(Password));

            return T_DecryptedBytes;
        }

        public static string AESDecrypt(string Input, string Password)
        {
            if (Input.Trim() == "")
                return "";

            ClsAES T_AES = new ClsAES();

            byte[] T_EncryptedBytes = T_AES.AES_Decrypt(ModGlobalFunctions.HEXStringToByteArray(Input), Encoding.UTF8.GetBytes(Password));

            if (T_EncryptedBytes == null)
                return Input;
            else
                return Encoding.UTF8.GetString(T_EncryptedBytes);
        }

        public static string AESDecrypt(byte[] Input, string Password)
        {
            if (Input.Length == 0)
                return "";

            ClsAES T_AES = new ClsAES();

            byte[] T_EncryptedBytes = T_AES.AES_Decrypt(Input, Encoding.UTF8.GetBytes(Password));

            if (T_EncryptedBytes == null)
                return "";
            else
                return Encoding.UTF8.GetString(T_EncryptedBytes);
        }

    }
}
