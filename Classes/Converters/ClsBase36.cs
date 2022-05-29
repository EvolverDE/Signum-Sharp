
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Signum_Sharp.Modules;

namespace Signum_Sharp.Classes.Converters
{
    public class ClsBase36
    {
        //static Modules.ModConverts ModConverts = new Modules.ModConverts();

        private const string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string EncodeHexToBase36(string HexStr)
        {
            System.Numerics.BigInteger BigInt = System.Numerics.BigInteger.Parse(HexStr, System.Globalization.NumberStyles.AllowHexSpecifier);

            char[] Charay = Chars.ToCharArray();

            string ResultValue = "";

            if (BigInt < 0)
            {
                HexStr = "00" + HexStr;
                BigInt = System.Numerics.BigInteger.Parse(HexStr, System.Globalization.NumberStyles.AllowHexSpecifier);
                BigInt = System.Numerics.BigInteger.Abs(BigInt);
            }

            if (BigInt < 36)
            {
                int CIdx = Convert.ToInt32(BigInt);
                ResultValue = Charay[CIdx].ToString();
            }
            else
                while (BigInt != 0)
                {
                    int Remainder = Convert.ToInt32((BigInt % 36).ToString());
                    ResultValue = Charay[Remainder] + ResultValue;
                    BigInt = BigInt / 36;
                }

            return ResultValue;
        }


        public static string DecodeBase36ToHex(string Base36)
        {
            System.Numerics.BigInteger ReturnValue = 0;
            bool Negative = false;

            Base36 = Base36.ToUpper().Trim();

            if (Base36.Contains("-") && Base36.Length > 1)
                Negative = true;

            if (Base36.IndexOfAny("$,+-".ToCharArray()) > -1)
                Base36 = CleanValue(Base36);

            Base36 = TrimLeadingZeros(Base36);
            for (int i = 0; i <= Base36.Length - 1; i++)
            {
                char Digit = Convert.ToChar(Base36.Substring(i, 1));
                int Idx = Array.IndexOf(Chars.ToCharArray(), Digit);

                int PlaceValue = Base36.Length - i - 1;

                System.Numerics.BigInteger Pow = System.Numerics.BigInteger.Pow(36, PlaceValue);
                System.Numerics.BigInteger PowIdx = Pow * Idx;

                System.Numerics.BigInteger DigitValue = PowIdx;

                ReturnValue += DigitValue;
            }

            if (Negative)
                ReturnValue *= -1;

            byte[] BigByteArray = ReturnValue.ToByteArray();
            var PubKeyHex = ModGlobalFunctions.ByteArrayToHEXString(BigByteArray.Reverse().ToArray());

            return TrimLeadingZeros(PubKeyHex);
        }

        public static string CleanValue(string DecimalValue)
        {
            string ResultValue = DecimalValue;

            if (ResultValue.IndexOfAny("$,+-".ToCharArray()) > -1)
            {
                ResultValue = ResultValue.Replace("$", "");
                ResultValue = ResultValue.Replace(",", "");
                ResultValue = ResultValue.Replace("+", "");
                ResultValue = ResultValue.Replace("-", "");
            }
            return ResultValue;
        }

        private static string TrimLeadingZeros(string Value)
        {
            int RemoveIdx = -1;
            for (int i = 0; i <= Value.Length - 1; i++)
            {
                string Zero = Value.Substring(i, 1);

                if (Zero == "0")
                    RemoveIdx = i;
                else
                    break;
            }

            if (!(RemoveIdx == -1))
                Value = Value.Substring(RemoveIdx + 1);

            return Value;
        }
    }
}
