using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signum_Sharp.Classes.Converters
{
    public class ClsReedSolomon
    {

        private static readonly List<int> initial_codeword = new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly List<int> gexp = new List<int> { 1, 2, 4, 8, 16, 5, 10, 20, 13, 26, 17, 7, 14, 28, 29, 31, 27, 19, 3, 6, 12, 24, 21, 15, 30, 25, 23, 11, 22, 9, 18, 1 };
        private static readonly List<int> glog = new List<int> { 0, 0, 1, 18, 2, 5, 19, 11, 3, 29, 6, 27, 20, 8, 12, 23, 4, 10, 30, 17, 7, 22, 28, 26, 21, 25, 9, 16, 13, 14, 24, 15 };
        private static readonly List<int> codeword_map = new List<int> { 3, 2, 1, 0, 7, 6, 5, 4, 13, 14, 15, 16, 12, 8, 9, 10, 11 };
        private static readonly string alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        private static readonly int base_32_length = 13;
        private static readonly int base_10_length = 19;

        string C_PreFix = "";
        public string PreFix
        {
            get { return C_PreFix; }
            set { C_PreFix = value; }
        }

        public static string Encode(ulong plain)
        {
            string cypher_string_builder = "";

            try
            {
                string plain_string = Convert.ToString(plain);
                int length = plain_string.Length;
                int[] plain_string_10 = new int[base_10_length + 1];
                for (int i = 0; i <= length - 1; i++)
                    plain_string_10[i] = int.Parse(plain_string.Substring(i, 1));

                int codeword_length = 0;
                // Dim codeword(UBound(initial_codeword.ToArray)) As Integer
                int[] codeword = new int[initial_codeword.Count - 1 + 1];

                int new_length;
                int digit_32;
                do
                {
                    new_length = 0;
                    digit_32 = 0;
                    for (int i = 0; i <= length - 1; i++)
                    {
                        digit_32 = digit_32 * 10 + plain_string_10[i];
                        if (digit_32 >= 32)
                        {
                            plain_string_10[new_length] = digit_32 >> 5;
                            digit_32 = digit_32 & 31;
                            new_length += 1;
                        }
                        else if (new_length > 0)
                        {
                            plain_string_10[new_length] = 0;
                            new_length += 1;
                        }
                    }

                    length = new_length;
                    codeword[codeword_length] = digit_32;
                    codeword_length += 1;
                } while (!(length <= 0));

                var p = new int[] { 0, 0, 0, 0 };
                int fb = 0;
                for (int i = base_32_length - 1; i >= 0; i += -1)
                {
                    fb = codeword[i] ^ p[3];
                    p[3] = p[2] ^ Gmult(30, fb);
                    p[2] = p[1] ^ Gmult(6, fb);
                    p[1] = p[0] ^ Gmult(9, fb);
                    p[0] = Gmult(17, fb);
                }

                Array.Copy(p, 0, codeword, base_32_length, initial_codeword.Count - base_32_length);



                int codework_index = 0;
                int alphabet_index = 0;
                for (int i = 0; i <= 16; i++)
                {
                    codework_index = codeword_map[i];
                    alphabet_index = codeword[codework_index];
                    cypher_string_builder += alphabet.Substring(alphabet_index, 1);

                    if ((i & 3) == 3 & i < 13)
                        cypher_string_builder += "-";
                }
            }
            catch (Exception ex)
            {

                // ClsMsgs.MBox(ex.Message, "Error",,, ClsMsgs.Status.Erro)

                //if (GetINISetting(E_Setting.InfoOut, false))
                //{
                //    ClsOut Out = new ClsOut(Application.StartupPath);
                //    Out.ErrorLog2File(Application.ProductName + "-error in ClsReedSolomon.vb -> Encode(): -> " + ex.Message);
                //}
            }

            return cypher_string_builder;
        }


        public static ulong Decode(string cypher_string)
        {
            ulong retval = 0L;

            try
            {
                //string PreFix = cypher_string.Substring(0, ClsSignumAPI._AddressPreFix.Length);

                //if (PreFix == ClsSignumAPI._AddressPreFix)
                //    cypher_string = cypher_string.Substring(ClsSignumAPI._AddressPreFix.Length);

                cypher_string = cypher_string.Replace("-", ""); // lets remove the - in the address

                // Dim codeword(UBound(initial_codeword.ToArray)) As Integer
                int[] codeword = new int[initial_codeword.Count - 1 + 1];
                Array.Copy(initial_codeword.ToArray(), 0, codeword, 0, initial_codeword.Count);
                int codeword_length = 0;


                int position_in_alphabet;
                int codework_index;
                for (var i = 0; i <= cypher_string.Length - 1; i++)
                {
                    position_in_alphabet = alphabet.IndexOf(cypher_string.Substring(i, 1));
                    if (position_in_alphabet <= -1 | position_in_alphabet > alphabet.Length)
                        return 0;
                    if (codeword_length > 16)
                        return 0;
                    codework_index = codeword_map[codeword_length];
                    codeword[codework_index] = position_in_alphabet;
                    codeword_length += 1;
                }

                if (!Is_codeword_valid(codeword))
                    return 0;
                int length = base_32_length;

                int[] cypher_string_32 = new int[length - 1 + 1];

                for (int i = 0; i <= length - 1; i++)
                    cypher_string_32[i] = codeword[length - i - 1];


                string plain_string_builder = ""; // New System.Text.StringBuilder()

                int new_length = 0;
                int digit_10 = 0;
                do
                {
                    new_length = 0;
                    digit_10 = 0;
                    for (int i = 0; i <= length - 1; i++)
                    {
                        digit_10 = digit_10 * 32 + cypher_string_32[i];
                        if (digit_10 >= 10)
                        {
                            cypher_string_32[new_length] = digit_10 / 10;
                            digit_10 = digit_10 % 10;
                            new_length += 1;
                        }
                        else if (new_length > 0)
                        {
                            cypher_string_32[new_length] = 0;
                            new_length += 1;
                        }
                    }

                    length = new_length;
                    char zero = '0';
                    int zero1 = zero;
                    int dig = digit_10 + zero1;
                    char tempchr = (char)dig;

                    plain_string_builder = tempchr.ToString() + plain_string_builder;
                }
                while (length > 0);

                retval = ulong.Parse(plain_string_builder);
            }
            catch (Exception ex)
            {

                // ClsMsgs.MBox(ex.Message, "Error",,, ClsMsgs.Status.Erro)

                //if (GetINISetting(E_Setting.InfoOut, false))
                //{
                //    ClsOut Out = new ClsOut(Application.StartupPath);
                //    Out.ErrorLog2File(Application.ProductName + "-error in ClsReedSolomon.vb -> Decode(): -> " + ex.Message);
                //}
            }

            return retval;
        }

        private static int Gmult(int a, int b)
        {
            if (a == 0 | b == 0)
                return 0;
            int idx = (glog[a] + glog[b]) % 31;
            return gexp[idx];
        }

        private static bool Is_codeword_valid(int[] codeword)
        {
            int sum = 0;
            int t = 0;
            int pos = 0;
            for (int i = 1; i <= 4; i++)
            {
                t = 0;
                for (int j = 0; j <= 30; j++)
                {
                    if (j > 12 & j < 27)
                        continue;

                    pos = j;
                    if (j > 26)
                        pos -= 14;
                    t = t ^ Gmult(codeword[pos], gexp[i * j % 31]);
                }
                sum = sum | t;
            }
            if (sum == 0)
                return true;
            else
                return false;
        }

    }
}
