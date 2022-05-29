using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Signum_Sharp.Classes.Converters;

namespace Signum_Sharp.Modules
{
    public class ModGlobalFunctions
    {

        public static string GetID()
        {
            Random rnd = new Random();
            for (int i = 0; i <= DateTime.Now.Millisecond; i++)
            {
                rnd.Next();
            }
            

            string IDMix = Environment.MachineName + Environment.UserDomainName + Environment.UserName + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString() + rnd.ToString();

            return ModCrypto.GetSHA256HashString(IDMix);
        }

        public static ulong GetAccountID(string PublicKeyHEX)
        {
            byte[] PubKeyAry = ModGlobalFunctions.HEXStringToByteArray(PublicKeyHEX);
            SHA256 Sha256 = SHA256.Create();
            PubKeyAry = Sha256.ComputeHash(PubKeyAry);
            return BitConverter.ToUInt64(new byte[]{ PubKeyAry[0], PubKeyAry[1], PubKeyAry[2], PubKeyAry[3], PubKeyAry[4], PubKeyAry[5], PubKeyAry[6], PubKeyAry[7] });
        }

        public static string GetAccountRS(string PublicKeyHEX)
        {
            return ClsReedSolomon.Encode(GetAccountID(PublicKeyHEX));
        }
        public static string GetAccountRSFromID(ulong AccountID)
        {
            return ClsReedSolomon.Encode(AccountID);
        }
        public static ulong GetAccountIDFromRS(string AccountRS)
        {
            return ClsReedSolomon.Decode(AccountRS);
        }

        public static byte[] RandomBytes(int Length)
        {
            Random rnd = new Random();
            byte[] b = new byte[Length + 1];
            rnd.NextBytes(b);

            return b;
        }

        public static bool MessageIsHEXString(string Message)
        {
            if (Message.Length % 2 != 0)
                return false;

            char[] CharAry = Message.ToUpper().ToCharArray();

            foreach (char Chr in CharAry)
            {
                switch (Chr)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        {
                            break;
                        }

                    default:
                        {
                            return false;
                        }
                }
            }

            return true;
        }

        /// <summary>
        /// converts accountID (and publicKey) from given address (0=AccountID; 1=PublicKey)
        /// </summary>
        /// <param name="Address">the address (e.g. (T)S-2222-2222-2222-22222(-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ))</param>
        /// <returns>List(Of String)("12345678901234567890","a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2")</returns>
        public static List<string> ConvertAddress(string Address)
        {
            List<string> ReturnList = new List<string>();

            try
            {
                if (Address.Trim() == "")
                    return ReturnList;

                string PreFix = "";

                if (Address.Contains("-"))
                    PreFix = Address.Remove(Address.IndexOf("-") + 1);
                else
                    return ReturnList;


                //if (PreFix.Contains(ClsSignumAPI._AddressPreFix))
                Address = Address.Substring(Address.IndexOf(PreFix) + PreFix.Length);

                switch (CharCnt(Address, "-"))
                {
                    case 3:
                        {
                            if (IsReedSolomon(Address))
                            {
                                ulong AccID = GetAccountIDFromRS(Address);
                                ReturnList.Add(AccID.ToString());
                            }

                            break;
                        }

                    case 4:
                        {
                            string PubKeyBase36 = Address.Substring(Address.LastIndexOf("-") + 1);
                            Address = Address.Remove(Address.IndexOf(PubKeyBase36) - 1);

                            if (IsReedSolomon(Address))
                            {
                                ulong AccID = GetAccountIDFromRS(Address);
                                ReturnList.Add(AccID.ToString());

                                string PubKeyHex = ClsBase36.DecodeBase36ToHex(PubKeyBase36);
                                ReturnList.Add(PubKeyHex);
                            }

                            break;
                        }
                }
            }
            catch (Exception ex)
            {

                // ClsMsgs.MBox(ex.Message, "Error",,, ClsMsgs.Status.Erro)

                //if (GetINISetting(E_Setting.InfoOut, false))
                //{
                //    ClsOut Out = new ClsOut(Application.StartupPath);
                //    Out.ErrorLog2File(Application.ProductName + "-error in ModConverts.vb -> ConvertAddress(): -> " + ex.Message);
                //}
            }

            return ReturnList;
        }

        public static bool IsReedSolomon(string RSString)
        {
            if (!(RSString.Length == 20))
                return false;

            char[] CharAry = RSString.ToUpper().ToCharArray();

            foreach (char Chr in CharAry)
            {
                switch (Chr)
                {
                    case '-':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                        {
                            break;
                        }

                    default:
                        {
                            return false;
                        }
                }
            }

            return true;
        }
        public static int CharCnt(string Input, string Search)
        {
            int Cnter = 0;
            for (int i = 0; i <= Input.Length - 1; i++)
            {
                string Chr = Input.Substring(i, 1);

                if (Chr == Search)
                    Cnter += 1;
            }

            return Cnter;
        }
        public bool IsNumber(string Input)
        {
            char[] CharAry = Input.ToUpper().ToCharArray();

            foreach (char Chr in CharAry)
            {
                switch (Chr)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            break;
                        }

                    default:
                        {
                            return false;
                        }
                }
            }

            return true;
        }

        #region Converts

        public static string ByteArrayToHEXString(byte[] BytAry)
        {
            string RetStr = "";

            List<byte> ParaBytes = BytAry.ToList();

            foreach (byte ParaByte in ParaBytes)
            {
                string T_RetStr = ParaByte.ToString("X2");

                if (T_RetStr.Length < 2)
                    T_RetStr = "0" + T_RetStr;

                RetStr += T_RetStr;
            }

            return RetStr.ToLower();
        }
        public static byte[] HEXStringToByteArray(string HEXStr)
        {
            List<byte> TempBytlist = new List<byte>();

            if (ModGlobalFunctions.MessageIsHEXString(HEXStr))
            {
                if (HEXStr.Length % 2 > 0)
                    HEXStr += "0";

                for (int i = 0; i <= HEXStr.Length - 1; i += 2)
                {
                    string TStr = HEXStr.Substring(i, 2);
                    TempBytlist.Add(Convert.ToByte(TStr, 16));
                }

                return TempBytlist.ToArray();
            }
            else
                return TempBytlist.ToArray();
        }
        public static string StringToHEXString(string Input)
        {
            List<byte> BytAry = new List<byte>();
            for (int i = 0; i <= Input.Length - 1; i++)
            {
                string T_HEXStr = Input.Substring(i, 1);

                byte chrbyt = Convert.ToByte(char.Parse(T_HEXStr));
                BytAry.Add(chrbyt);

            }

            return Convert.ToHexString(BytAry.ToArray());
        }

        #endregion

    }
}
