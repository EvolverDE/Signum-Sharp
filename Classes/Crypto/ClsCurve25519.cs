/* Ported parts from Java to C# and refactored by Hans Wolff, 17/09/2013 */

/* Ported from C to Java by Dmitry Skiba [sahn0], 23/02/08.
 * Original: http://code.google.com/p/curve25519-java/
 */

/* Generic 64-bit integer implementation of Curve25519 ECDH
 * Written by Matthijs van Duin, 200608242056
 * Public domain.
 *
 * Based on work by Daniel J Bernstein, http://cr.yp.to/ecdh.html
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signum_Sharp.Classes.Crypto
{

    public class ClsCurve25519
    {

        // public constants
        public const int C1 = 1;
        public const int C9 = 9;
        public const int C486662 = 486662;
        public const int C39420360 = 39420360;

        // smallest multiple of the order that's >= 2^255 
        public byte[] ORDER_TIMES_8 { get; } = new[] { (byte)104, (byte)159, (byte)174, (byte)231, (byte)210, (byte)24, (byte)147, (byte)192, (byte)178, (byte)230, (byte)188, (byte)23, (byte)245, (byte)206, (byte)247, (byte)166, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)128 };
        // constants 2Gy and 1/(2Gy) 
        public Long10 BASE_2Y { get; } = new Long10(39999547, 18689728, 59995525, 1648697, 57546132, 24010086, 19059592, 5425144, 63499247, 16420658);
        public Long10 BASE_R2Y { get; } = new Long10(5744, 8160848, 4790893, 13779497, 35730846, 12541209, 49101323, 30047407, 40071253, 6226132);

        // key size 
        public int KEY_SIZE { get; } = 32;
        // 0 
        public byte[] ZERO { get; } = new[] { (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0 };
        // the prime 2^255-19 
        public byte[] PRIME { get; } = new[] { (byte)237, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)255, (byte)127 };
        // group order (a prime near 2^252+2^124) 
        public byte[] ORDER { get; } = new[] { (byte)237, (byte)211, (byte)245, (byte)92, (byte)26, (byte)99, (byte)18, (byte)88, (byte)214, (byte)156, (byte)247, (byte)162, (byte)222, (byte)249, (byte)222, (byte)20, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)16 };

        #region ******* KEY AGREEMENT ********

        /// <summary>
        /// Private key clamping
        /// </summary>
        /// <param name="AgreementKey">[in] 32 random bytes, [out] your private key for key agreement</param>
        public void Clamp(ref byte[] AgreementKey)
        {
            AgreementKey[31] = Convert.ToByte(AgreementKey[31] & 0x7F);
            AgreementKey[31] = Convert.ToByte(AgreementKey[31] | 0x40);
            AgreementKey[0] = Convert.ToByte(AgreementKey[0] & 0xF8);
        }


        /// <summary>
        /// Key-Pair generation * WARNING: if s is not NULL, this function has data-dependent timing
        /// </summary>
        /// <param name="PublicKey">[out] your public key</param>
        /// <param name="SignKey">[out] your private key for signing</param>
        /// <param name="AgreementKey">[in] 32 random bytes, [out] your private key for key agreement</param>
        public void KeyGen(ref byte[] PublicKey, ref byte[] SignKey, ref byte[] AgreementKey)
        {
            Clamp( ref AgreementKey);
            byte[] dummy = new byte[] { };
            Core( PublicKey,  SignKey,  AgreementKey,  dummy);
        }

        /// <summary>
        /// Key agreement
        /// </summary>
        /// <param name="SharedSecret">[out] shared secret (needs hashing before use)</param>
        /// <param name="AgreementKey">[in]  your private key for key agreement</param>
        /// <param name="PublicKey">[in] peer's public key</param>
        public void GetSharedSecret(ref byte[] SharedSecret, byte[] AgreementKey, byte[] PublicKey)
        {
            byte[] dummy = new byte[] { };
            Core( SharedSecret, dummy, AgreementKey, PublicKey);
        }


        #endregion

        // sahn0:
        /// <summary>
        /// Using this class instead of long[10] to avoid bounds checks.
        /// </summary>
        public class Long10
        {
            public Long10()
            {
            }
            public Long10(long _0, long _1, long _2, long _3, long _4, long _5, long _6, long _7, long _8, long _9)
            {
                this._0 = _0; this._1 = _1; this._2 = _2; this._3 = _3; this._4 = _4;
                this._5 = _5; this._6 = _6; this._7 = _7; this._8 = _8; this._9 = _9;
            }
            public long _0, _1, _2, _3, _4, _5, _6, _7, _8, _9;
        }


        // ******************* radix 2^8 math ********************
        /// <summary>
        /// Copies 32 Bytes from Input to Output
        /// </summary>
        /// <param name="Output">[out] Copy of Input</param>
        /// <param name="Input">[in] Input to Copy to Output</param>
        public void Copy32(byte[] Output, byte[] Input)
        {
            int i;
            for (i = 0; i <= 32 - 1; i++)
                Output[i] = Input[i];
        }


        // p[m..n+m-1] = q[m..n+m-1] + z * x 
        // n is the size of x 
        // n+m is the size of p and q 
        public int Multiply_Array_Small(byte[] p, byte[] q, int m, byte[] x, int n, int z)
        {
            int v = 0;
            int i = 0;
            while (i < n)
            {
                v += (q[i + m] & 0xFF) + z * (x[i] & 0xFF);
                p[i + m] = Convert.ToByte(v & 0xFF);
                v >>= 8;
                i += 1;
            }
            return v;
        }


        // p += x * y * z  where z is a small  Integer 
        // 	 * x is size 32, y is size t, p is size 32+t
        // 	 * y is allowed to overlap with p+32 if you don't care about the upper half
        public int Multiply_Array32(byte[] p, byte[] x, byte[] y, int t, int z)
        {
            int n = 31;
            int w = 0;
            int i = 0;
            while (i < t)
            {
                int zy = z * (y[i] & 0xFF);
                w += Multiply_Array_Small(p, p, i, x, n, zy) + (p[i + n] & 0xFF) + zy * (x[n] & 0xFF);
                p[i + n] = Convert.ToByte(w & 0xFF);
                w >>= 8;
                i += 1;
            }
            p[i + n] = Convert.ToByte((w + (p[i + n] & 0xFF)) & 0xFF);
            return w >> 8;
        }


        // divide r (size n) by d (size t), returning quotient q and remainder r
        // 	 * quotient is size n-t+1, remainder is size t
        // 	 * requires t > 0 && d[t-1] != 0
        // 	 * requires that r[-1] and d[-1] are valid memory locations
        // 	 * q may overlap with r+t 
        public void DivideMod( byte[] q, byte[] r, int n, byte[] d, int t)
        {
            int rn = 0;
            int dt = (d[t - 1] & 0xFF) << 8;
            if (t > 1)
                dt = dt | d[t - 2] & 0xFF;
            while (Math.Max(Interlocked.Decrement(ref n), n + 1) >= t)
            {
                int z = rn << 16 | (r[n] & 0xFF) << 8;
                if (n > 0)
                    z = z | r[n - 1] & 0xFF;
                z /= dt;
                rn += Multiply_Array_Small(r, r, n - t + 1, d, t, -z);
                q[n - t + 1] = Convert.ToByte(z + rn & 0xFF); // rn is 0 or -1 (underflow) 
                Multiply_Array_Small(r, r, n - t + 1, d, t, -rn);
                rn = r[n] & 0xFF;
                r[n] = 0;
            }
            r[t - 1] = Convert.ToByte(rn);
        }


        public int NumberSize(byte[] x, int n)
        {
            while (Math.Max(Interlocked.Decrement(ref n), n + 1) != 0 && x[n] == 0)
            {
            }
            return n + 1;
        }


        // Returns x if a contains the gcd, y if b.
        // 	 * Also, the returned buffer contains the inverse of a mod b,
        // 	 * as 32-byte signed.
        // 	 * x and y must have 64 bytes space for temporary use.
        // 	 * requires that a[-1] and b[-1] are valid memory locations  
        public byte[] Egcd32( byte[] x, byte[] y, byte[] a, byte[] b)
        {
            int an, qn, i;
            int bn = 32;
            for (i = 0; i <= 32 - 1; i++)
            {
                y[i] = 0; x[i] = 0;
            }
            x[0] = 1;
            an = NumberSize(a, 32);
            if (an == 0)
                return y; // division by zero 
            byte[] temp = new byte[32];
            while (true)
            {
                qn = bn - an + 1;
                DivideMod(temp, b, bn, a, an);
                bn = NumberSize(b, bn);
                if (bn == 0)
                    return x;
                Multiply_Array32(y, x, temp, qn, -1);
                qn = an - bn + 1;
                DivideMod(temp, a, an, b, bn);
                an = NumberSize(a, an);
                if (an == 0)
                    return y;
                Multiply_Array32(x, y, temp, qn, -1);
            }
            throw new Exception();
        }


        // ******************* radix 2^25.5 GF(2^255-19) math ********************
        public int P25 { get; } = 33554431;  // (1 << 25) - 1 
        public int P26 { get; } = 67108863;  // (1 << 26) - 1


        // Convert to internal format from little-endian byte format
        public void Unpack( Long10 x, byte[] m)
        {
            x._0 = m[0] & 0xFF | (m[1] & 0xFF) << 8 | (m[2] & 0xFF) << 16 | (m[3] & 0xFF & 3) << 24;
            x._1 = (m[3] & 0xFF & ~3) >> 2 | (m[4] & 0xFF) << 6 | (m[5] & 0xFF) << 14 | (m[6] & 0xFF & 7) << 22;
            x._2 = (m[6] & 0xFF & ~7) >> 3 | (m[7] & 0xFF) << 5 | (m[8] & 0xFF) << 13 | (m[9] & 0xFF & 31) << 21;
            x._3 = (m[9] & 0xFF & ~31) >> 5 | (m[10] & 0xFF) << 3 | (m[11] & 0xFF) << 11 | (m[12] & 0xFF & 63) << 19;
            x._4 = (m[12] & 0xFF & ~63) >> 6 | (m[13] & 0xFF) << 2 | (m[14] & 0xFF) << 10 | (m[15] & 0xFF) << 18;
            x._5 = m[16] & 0xFF | (m[17] & 0xFF) << 8 | (m[18] & 0xFF) << 16 | (m[19] & 0xFF & 1) << 24;
            x._6 = (m[19] & 0xFF & ~1) >> 1 | (m[20] & 0xFF) << 7 | (m[21] & 0xFF) << 15 | (m[22] & 0xFF & 7) << 23;
            x._7 = (m[22] & 0xFF & ~7) >> 3 | (m[23] & 0xFF) << 5 | (m[24] & 0xFF) << 13 | (m[25] & 0xFF & 15) << 21;
            x._8 = (m[25] & 0xFF & ~15) >> 4 | (m[26] & 0xFF) << 4 | (m[27] & 0xFF) << 12 | (m[28] & 0xFF & 63) << 20;
            x._9 = (m[28] & 0xFF & ~63) >> 6 | (m[29] & 0xFF) << 2 | (m[30] & 0xFF) << 10 | (m[31] & 0xFF) << 18;
        }


        // Check if reduced-form input >= 2^255-19 
        public bool Is_Overflow(Long10 x)
        {
            return x._0 > P26 - 19 && (x._1 & x._3 & x._5 & x._7 & x._9) == P25 && (x._2 & x._4 & x._6 & x._8) == P26 || x._9 > P25;
        }


        // Convert from internal format to little-endian byte format.  The 
        // 	 * number must be in a reduced form which is output by the following ops:
        // 	 *     unpack, mul, sqr
        // 	 *     set --  if input in range 0 .. P25
        // 	 * If you're unsure if the number is reduced, first multiply it by 1.  

        public void Pack(Long10 x, byte[] m)
        {
            int ld = (Is_Overflow(x) ? 1 : 0) - ((x._9 < 0) ? 1 : 0);
            int ud = ld * -(P25 + 1);
            ld *= 19;
            long t = ld + x._0 + (x._1 << 26);


            m[0] = Convert.ToByte(t & 0xFF);
            m[1] = Convert.ToByte((t >> 8) & 0xFF);
            m[2] = Convert.ToByte((t >> 16) & 0xFF);
            m[3] = Convert.ToByte((t >> 24) & 0xFF);
            t = (t >> 32) + (x._2 << 19);
            m[4] = Convert.ToByte(t & 0xFF);
            m[5] = Convert.ToByte((t >> 8) & 0xFF);
            m[6] = Convert.ToByte((t >> 16) & 0xFF);
            m[7] = Convert.ToByte((t >> 24) & 0xFF);
            t = (t >> 32) + (x._3 << 13);
            m[8] = Convert.ToByte(t & 0xFF);
            m[9] = Convert.ToByte((t >> 8) & 0xFF);
            m[10] = Convert.ToByte((t >> 16) & 0xFF);
            m[11] = Convert.ToByte((t >> 24) & 0xFF);
            t = (t >> 32) + (x._4 << 6);
            m[12] = Convert.ToByte(t & 0xFF);
            m[13] = Convert.ToByte((t >> 8) & 0xFF);
            m[14] = Convert.ToByte((t >> 16) & 0xFF);
            m[15] = Convert.ToByte((t >> 24) & 0xFF);
            t = (t >> 32) + x._5 + (x._6 << 25);
            m[16] = Convert.ToByte(t & 0xFF);
            m[17] = Convert.ToByte((t >> 8) & 0xFF);
            m[18] = Convert.ToByte((t >> 16) & 0xFF);
            m[19] = Convert.ToByte((t >> 24) & 0xFF);
            t = (t >> 32) + (x._7 << 19);
            m[20] = Convert.ToByte(t & 0xFF);
            m[21] = Convert.ToByte((t >> 8) & 0xFF);
            m[22] = Convert.ToByte((t >> 16) & 0xFF);
            m[23] = Convert.ToByte((t >> 24) & 0xFF);
            t = (t >> 32) + (x._8 << 12);
            m[24] = Convert.ToByte(t & 0xFF);
            m[25] = Convert.ToByte((t >> 8) & 0xFF);
            m[26] = Convert.ToByte((t >> 16) & 0xFF);
            m[27] = Convert.ToByte((t >> 24) & 0xFF);
            t = (t >> 32) + (x._9 + ud << 6);
            m[28] = Convert.ToByte(t & 0xFF);
            m[29] = Convert.ToByte((t >> 8) & 0xFF);
            m[30] = Convert.ToByte((t >> 16) & 0xFF);
            m[31] = Convert.ToByte((t >> 24) & 0xFF);

           
        }


        // Copy a number
        public void Copy(ref Long10 _out, Long10 _in)
        {
            _out._0 = _in._0;
            _out._1 = _in._1;
            _out._2 = _in._2;
            _out._3 = _in._3;
            _out._4 = _in._4;
            _out._5 = _in._5;
            _out._6 = _in._6;
            _out._7 = _in._7;
            _out._8 = _in._8;
            _out._9 = _in._9;
        }
        // Set a number to value, which must be in range -185861411 .. 185861411 
        public void Set_In_To_Out(ref Long10 _out, int _in)
        {
            _out._0 = _in;
            _out._1 = 0;
            _out._2 = 0;
            _out._3 = 0;
            _out._4 = 0;
            _out._5 = 0;
            _out._6 = 0;
            _out._7 = 0;
            _out._8 = 0;
            _out._9 = 0;
        }

        // Add/subtract two numbers.  The inputs must be in reduced form, and the 
        // 	 * output isn't, so to do another addition or subtraction on the output, 
        // 	 * first multiply it by one to reduce it. 
        public void AddUp(ref Long10 xy, Long10 x, Long10 y)
        {
            xy._0 = x._0 + y._0;
            xy._1 = x._1 + y._1;
            xy._2 = x._2 + y._2;
            xy._3 = x._3 + y._3;
            xy._4 = x._4 + y._4;
            xy._5 = x._5 + y._5;
            xy._6 = x._6 + y._6;
            xy._7 = x._7 + y._7;
            xy._8 = x._8 + y._8;
            xy._9 = x._9 + y._9;
        }

        public void Subtract(ref Long10 xy, Long10 x, Long10 y)
        {
            xy._0 = x._0 - y._0;
            xy._1 = x._1 - y._1;
            xy._2 = x._2 - y._2;
            xy._3 = x._3 - y._3;
            xy._4 = x._4 - y._4;
            xy._5 = x._5 - y._5;
            xy._6 = x._6 - y._6;
            xy._7 = x._7 - y._7;
            xy._8 = x._8 - y._8;
            xy._9 = x._9 - y._9;
        }

        // Multiply a number by a small  Integer  in range -185861411 .. 185861411.
        // 	 * The output is in reduced form, the input x need not be.  x and xy may point
        // 	 * to the same buffer. 
        public Long10 Multiply_Small(Long10 xy, Long10 x, long y)
        {
            long t;
            t = x._8 * y;
            xy._8 = t & (1 << 26) - 1;
            t = (t >> 26) + x._9 * y;
            xy._9 = t & (1 << 25) - 1;
            t = 19 * (t >> 25) + x._0 * y;
            xy._0 = t & (1 << 26) - 1;
            t = (t >> 26) + x._1 * y;
            xy._1 = t & (1 << 25) - 1;
            t = (t >> 25) + x._2 * y;
            xy._2 = t & (1 << 26) - 1;
            t = (t >> 26) + x._3 * y;
            xy._3 = t & (1 << 25) - 1;
            t = (t >> 25) + x._4 * y;
            xy._4 = t & (1 << 26) - 1;
            t = (t >> 26) + x._5 * y;
            xy._5 = t & (1 << 25) - 1;
            t = (t >> 25) + x._6 * y;
            xy._6 = t & (1 << 26) - 1;
            t = (t >> 26) + x._7 * y;
            xy._7 = t & (1 << 25) - 1;
            t = (t >> 25) + xy._8;
            xy._8 = t & (1 << 26) - 1;
            xy._9 += t >> 26;
            return xy;
        }

        // Multiply two numbers.  The output is in reduced form, the inputs need not 
        // 	 * be. 
        public Long10 Multiply(Long10 xy, Long10 x, Long10 y)
        {
            long x_0 = x._0;
            long x_1 = x._1;
            long x_2 = x._2;
            long x_3 = x._3;
            long x_4 = x._4;
            long x_5 = x._5;
            long x_6 = x._6;
            long x_7 = x._7;
            long x_8 = x._8;
            // sahn0:
            // * Using local variables to avoid class access.
            // * This seem to improve performance a bit...
            // 
            long x_9 = x._9;
            long y_0 = y._0;
            long y_1 = y._1;
            long y_2 = y._2;
            long y_3 = y._3;
            long y_4 = y._4;
            long y_5 = y._5;
            long y_6 = y._6;
            long y_7 = y._7;
            long y_8 = y._8;
            long y_9 = y._9;
            long t;
            t = x_0 * y_8 + x_2 * y_6 + x_4 * y_4 + x_6 * y_2 + x_8 * y_0 + 2 * (x_1 * y_7 + x_3 * y_5 + x_5 * y_3 + x_7 * y_1) + 38 * (x_9 * y_9);
            xy._8 = t & (1 << 26) - 1;
            t = (t >> 26) + x_0 * y_9 + x_1 * y_8 + x_2 * y_7 + x_3 * y_6 + x_4 * y_5 + x_5 * y_4 + x_6 * y_3 + x_7 * y_2 + x_8 * y_1 + x_9 * y_0;
            xy._9 = t & (1 << 25) - 1;
            t = x_0 * y_0 + 19 * ((t >> 25) + x_2 * y_8 + x_4 * y_6 + x_6 * y_4 + x_8 * y_2) + 38 * (x_1 * y_9 + x_3 * y_7 + x_5 * y_5 + x_7 * y_3 + x_9 * y_1);
            xy._0 = t & (1 << 26) - 1;
            t = (t >> 26) + x_0 * y_1 + x_1 * y_0 + 19 * (x_2 * y_9 + x_3 * y_8 + x_4 * y_7 + x_5 * y_6 + x_6 * y_5 + x_7 * y_4 + x_8 * y_3 + x_9 * y_2);
            xy._1 = t & (1 << 25) - 1;
            t = (t >> 25) + x_0 * y_2 + x_2 * y_0 + 19 * (x_4 * y_8 + x_6 * y_6 + x_8 * y_4) + 2 * (x_1 * y_1) + 38 * (x_3 * y_9 + x_5 * y_7 + x_7 * y_5 + x_9 * y_3);
            xy._2 = t & (1 << 26) - 1;
            t = (t >> 26) + x_0 * y_3 + x_1 * y_2 + x_2 * y_1 + x_3 * y_0 + 19 * (x_4 * y_9 + x_5 * y_8 + x_6 * y_7 + x_7 * y_6 + x_8 * y_5 + x_9 * y_4);
            xy._3 = t & (1 << 25) - 1;
            t = (t >> 25) + x_0 * y_4 + x_2 * y_2 + x_4 * y_0 + 19 * (x_6 * y_8 + x_8 * y_6) + 2 * (x_1 * y_3 + x_3 * y_1) + 38 * (x_5 * y_9 + x_7 * y_7 + x_9 * y_5);
            xy._4 = t & (1 << 26) - 1;
            t = (t >> 26) + x_0 * y_5 + x_1 * y_4 + x_2 * y_3 + x_3 * y_2 + x_4 * y_1 + x_5 * y_0 + 19 * (x_6 * y_9 + x_7 * y_8 + x_8 * y_7 + x_9 * y_6);
            xy._5 = t & (1 << 25) - 1;
            t = (t >> 25) + x_0 * y_6 + x_2 * y_4 + x_4 * y_2 + x_6 * y_0 + 19 * (x_8 * y_8) + 2 * (x_1 * y_5 + x_3 * y_3 + x_5 * y_1) + 38 * (x_7 * y_9 + x_9 * y_7);
            xy._6 = t & (1 << 26) - 1;
            t = (t >> 26) + x_0 * y_7 + x_1 * y_6 + x_2 * y_5 + x_3 * y_4 + x_4 * y_3 + x_5 * y_2 + x_6 * y_1 + x_7 * y_0 + 19 * (x_8 * y_9 + x_9 * y_8);
            xy._7 = t & (1 << 25) - 1;
            t = (t >> 25) + xy._8;
            xy._8 = t & (1 << 26) - 1;
            xy._9 += t >> 26;
            return xy;
        }

        // Square a number.  Optimization of  mul25519(x2, x, x)
        public Long10 Square(Long10 x2, Long10 x)
        {
            long x_0 = x._0;
            long x_1 = x._1;
            long x_2 = x._2;
            long x_3 = x._3;
            long x_4 = x._4;
            long x_5 = x._5;
            long x_6 = x._6;
            long x_7 = x._7;
            long x_8 = x._8;
            long x_9 = x._9;
            long t;
            t = x_4 * x_4 + 2 * (x_0 * x_8 + x_2 * x_6) + 38 * (x_9 * x_9) + 4 * (x_1 * x_7 + x_3 * x_5);
            x2._8 = t & (1 << 26) - 1;
            t = (t >> 26) + 2 * (x_0 * x_9 + x_1 * x_8 + x_2 * x_7 + x_3 * x_6 + x_4 * x_5);
            x2._9 = t & (1 << 25) - 1;
            t = 19 * (t >> 25) + x_0 * x_0 + 38 * (x_2 * x_8 + x_4 * x_6 + x_5 * x_5) + 76 * (x_1 * x_9 + x_3 * x_7);
            x2._0 = t & (1 << 26) - 1;
            t = (t >> 26) + 2 * (x_0 * x_1) + 38 * (x_2 * x_9 + x_3 * x_8 + x_4 * x_7 + x_5 * x_6);
            x2._1 = t & (1 << 25) - 1;
            t = (t >> 25) + 19 * (x_6 * x_6) + 2 * (x_0 * x_2 + x_1 * x_1) + 38 * (x_4 * x_8) + 76 * (x_3 * x_9 + x_5 * x_7);
            x2._2 = t & (1 << 26) - 1;
            t = (t >> 26) + 2 * (x_0 * x_3 + x_1 * x_2) + 38 * (x_4 * x_9 + x_5 * x_8 + x_6 * x_7);
            x2._3 = t & (1 << 25) - 1;
            t = (t >> 25) + x_2 * x_2 + 2 * (x_0 * x_4) + 38 * (x_6 * x_8 + x_7 * x_7) + 4 * (x_1 * x_3) + 76 * (x_5 * x_9);
            x2._4 = t & (1 << 26) - 1;
            t = (t >> 26) + 2 * (x_0 * x_5 + x_1 * x_4 + x_2 * x_3) + 38 * (x_6 * x_9 + x_7 * x_8);
            x2._5 = t & (1 << 25) - 1;
            t = (t >> 25) + 19 * (x_8 * x_8) + 2 * (x_0 * x_6 + x_2 * x_4 + x_3 * x_3) + 4 * (x_1 * x_5) + 76 * (x_7 * x_9);
            x2._6 = t & (1 << 26) - 1;
            t = (t >> 26) + 2 * (x_0 * x_7 + x_1 * x_6 + x_2 * x_5 + x_3 * x_4) + 38 * (x_8 * x_9);
            x2._7 = t & (1 << 25) - 1;
            t = (t >> 25) + x2._8;
            x2._8 = t & (1 << 26) - 1;
            x2._9 += t >> 26;
            return x2;
        }

        // Calculates a reciprocal.  The output is in reduced form, the inputs need not 
        // 	 * be.  Simply calculates  y = x^(p-2)  so it's not too fast. 
        // When sqrtassist is true, it instead calculates y = x^((p-5)/8) 
        public void Reciprocal(ref Long10 y, Long10 x, int sqrtassist)
        {
            Long10 t0 = new Long10();
            Long10 t1 = new Long10();
            Long10 t2 = new Long10();
            Long10 t3 = new Long10();
            Long10 t4 = new Long10();
            int i;
            // the chain for x^(2^255-21) is straight from djb's implementation 
            Square(t1, x); // 2 == 2 * 1	
            Square(t2, t1);  // 4 == 2 * 2	
            Square(t0, t2);  // 8 == 2 * 4	
            Multiply(t2, t0, x); // 9 == 8 + 1	
            Multiply(t0, t2, t1);  // 11 == 9 + 2	
            Square(t1, t0);  // 22 == 2 * 11	
            Multiply(t3, t1, t2);  // 31 == 22 + 9 == 2^5   - 2^0	
            Square(t1, t3);  // 2^6   - 2^1	
            Square(t2, t1);  // 2^7   - 2^2	
            Square(t1, t2);  // 2^8   - 2^3	
            Square(t2, t1);  // 2^9   - 2^4	
            Square(t1, t2);  // 2^10  - 2^5	
            Multiply(t2, t1, t3);  // 2^10  - 2^0	
            Square(t1, t2);  // 2^11  - 2^1	
            Square(t3, t1);  // 2^12  - 2^2	
            for (i = 1; i <= 5 - 1; i++)
            {
                Square(t1, t3);
                Square(t3, t1);
            } // t3 
              // 2^20  - 2^10	
            Multiply(t1, t3, t2);  // 2^20  - 2^0	
            Square(t3, t1);  // 2^21  - 2^1	
            Square(t4, t3);  // 2^22  - 2^2	
            for (i = 1; i <= 10 - 1; i++)
            {
                Square(t3, t4);
                Square(t4, t3);
            } // t4 
              // 2^40  - 2^20	
            Multiply(t3, t4, t1);  // 2^40  - 2^0	
            for (i = 0; i <= 5 - 1; i++)
            {
                Square(t1, t3);
                Square(t3, t1);
            } // t3 
              // 2^50  - 2^10	
            Multiply(t1, t3, t2);  // 2^50  - 2^0	
            Square(t2, t1);  // 2^51  - 2^1	
            Square(t3, t2);  // 2^52  - 2^2	
            for (i = 1; i <= 25 - 1; i++)
            {
                Square(t2, t3);
                Square(t3, t2);
            } // t3 
              // 2^100 - 2^50 
            Multiply(t2, t3, t1);  // 2^100 - 2^0	
            Square(t3, t2);  // 2^101 - 2^1	
            Square(t4, t3);  // 2^102 - 2^2	
            for (i = 1; i <= 50 - 1; i++)
            {
                Square(t3, t4);
                Square(t4, t3);
            } // t4 
              // 2^200 - 2^100 
            Multiply(t3, t4, t2);  // 2^200 - 2^0	
            for (i = 0; i <= 25 - 1; i++)
            {
                Square(t4, t3);
                Square(t3, t4);
            } // t3 
              // 2^250 - 2^50	
            Multiply(t2, t3, t1);  // 2^250 - 2^0	
            Square(t1, t2);  // 2^251 - 2^1	
            Square(t2, t1);  // 2^252 - 2^2	
            if (sqrtassist != 0)
                Multiply(y, x, t2);  // 2^252 - 3 
            else
            {
                Square(t1, t2);  // 2^253 - 2^3	
                Square(t2, t1);  // 2^254 - 2^4	
                Square(t1, t2);  // 2^255 - 2^5	
                Multiply(y, t1, t0); // 2^255 - 21	
            }
        }

        // checks if x is "negative", requires reduced input
        public int Is_Negative(Long10 x)
        {
            return Convert.ToInt32(Is_Overflow(x) || x._9 < 0 ? 1 : 0 ^ (x._0 & 1));
        }

        // a square root
        public void Square_Root(ref Long10 x, Long10 u)
        {
            Long10 v = new Long10();
            Long10 t1 = new Long10();
            Long10 t2 = new Long10();
            AddUp(ref t1, u, u);  // t1 = 2u		
            Reciprocal(ref v, t1, 1);  // v = (2u)^((p-5)/8)	
            Square(x, v);    // x = v^2		
            Multiply(t2, t1, x); // t2 = 2uv^2		
            t2._0 -= 1;    // t2 = 2uv^2-1		
            Multiply(t1, v, t2); // t1 = v(2uv^2-1)	
            Multiply(x, u, t1);  // x = uv(2uv^2-1)	
        }

        // ******************* Elliptic curve ********************
        // y^2 = x^3 + 486662 x^2 + x  over GF(2^255-19)
        // t1 = ax + az
        // 	 * t2 = ax - az 
        public void Monty_Prepare(ref Long10 t1, ref Long10 t2, Long10 ax, Long10 az)
        {
            AddUp(ref t1, ax, az);
            Subtract(ref t2, ax, az);
        }

        // A = P + Q   where
        // 	 *  X(A) = ax/az
        // 	 *  X(P) = (t1+t2)/(t1-t2)
        // 	 *  X(Q) = (t3+t4)/(t3-t4)
        // 	 *  X(P-Q) = dx
        // 	 * clobbers t1 and t2, preserves t3 and t4
        public void Monty_AddUp(ref Long10 t1, ref Long10 t2, Long10 t3, Long10 t4, Long10 ax, Long10 az, Long10 dx)
        {
            Multiply(ax, t2, t3);
            Multiply(az, t1, t4);
            AddUp(ref t1, ax, az);
            Subtract(ref t2, ax, az);
            Square(ax, t1);
            Square(t1, t2);
            Multiply(az, t1, dx);
        }

        // B = 2 * Q   where
        // 	 *  X(B) = bx/bz
        // 	 *  X(Q) = (t3+t4)/(t3-t4)
        // 	 * clobbers t1 and t2, preserves t3 and t4  
        public void Monty_Double(ref Long10 t1, ref Long10 t2, Long10 t3, Long10 t4, ref Long10 bx, ref Long10 bz)
        {
            Square(t1, t3);
            Square(t2, t4);
            Multiply(bx, t1, t2);
            Subtract(ref t2, t1, t2);
            Multiply_Small(bz, t2, 121665);
            AddUp(ref t1, t1, bz);
            Multiply(bz, t1, t2);
        }

        // Y^2 = X^3 + 486662 X^2 + X
        // 	 * t is a temporary 
        public void X_To_Y2(ref Long10 t, ref Long10 y2, Long10 x)
        {
            Square(t, x);
            Multiply_Small(y2, x, C486662);
            AddUp(ref t, t, y2);
            t._0 += 1;
            Multiply(y2, t, x);
        }

        // P = kG   and  s = sign(P)/k 
        public void Core(byte[] Px, byte[] s, byte[] k, byte[] Gx)
        {
            Long10 dx = new Long10();
            Long10 t1 = new Long10();
            Long10 t2 = new Long10();
            Long10 t3 = new Long10();
            Long10 t4 = new Long10();
            Long10[] x = new Long10[] { new Long10(), new Long10() };
            Long10[] z = new Long10[] { new Long10(), new Long10() };
            int i, j;
            // unpack the base 
            if (Gx.Length != 0)
                Unpack(dx, Gx);
            else
                Set_In_To_Out(ref dx, 9);
            // 0G = point-at-infinity 
            Set_In_To_Out(ref x[0], 1);
            Set_In_To_Out(ref z[0], 0);
            // 1G = G 
            Copy(ref x[1], dx);
            Set_In_To_Out(ref z[1], 1);
            i = 32;
            while (Math.Max(Interlocked.Decrement(ref i), i + 1) != 0)
            {
                if (i == 0)
                    i = 0;
                j = 8;
                while (Math.Max(Interlocked.Decrement(ref j), j + 1) != 0)
                {
                    // swap arguments depending on bit 
                    int bit1 = (k[i] & 0xFF) >> j & 1;
                    int bit0 = ~(k[i] & 0xFF) >> j & 1;
                    Long10 ax = x[bit0];
                    Long10 az = z[bit0];
                    Long10 bx = x[bit1];
                    Long10 bz = z[bit1];
                    // a' = a + b	
                    // b' = 2 b	
                    Monty_Prepare(ref t1, ref t2, ax, az);
                    Monty_Prepare(ref t3, ref t4, bx, bz);
                    Monty_AddUp(ref t1, ref t2, t3, t4, ax, az, dx);
                    Monty_Double(ref t1, ref t2, t3, t4, ref bx, ref bz);
                }
            }
            Reciprocal(ref t1, z[0], 0);
            Multiply(dx, x[0], t1);
            Pack(dx, Px);
            // calculate s such that s abs(P) = G  .. assumes G is std base point 
            if (s.Length != 0)
            {
                X_To_Y2(ref t2, ref t1, dx);  // t1 = Py^2  
                Reciprocal(ref t3, z[1], 0); // where Q=P+G ... 
                Multiply(t2, x[1], t3);  // t2 = Qx  
                AddUp(ref t2, t2, dx);  // t2 = Qx + Px  
                t2._0 += C9 + C486662;  // t2 = Qx + Px + Gx + 486662  
                dx._0 -= C9;  // dx = Px - Gx  
                Square(t3, dx);  // t3 = (Px - Gx)^2  
                Multiply(dx, t2, t3);  // dx = t2 (Px - Gx)^2  
                Subtract(ref dx, dx, t1);  // dx = t2 (Px - Gx)^2 - Py^2  
                dx._0 -= C39420360;   // dx = t2 (Px - Gx)^2 - Py^2 - Gy^2  
                Multiply(t1, dx, BASE_R2Y);  // t1 = -Py  
                if (Is_Negative(t1) != 0)
                    Copy32( s, k);      // sign is -1, so negate  
                else
                    Multiply_Array_Small(s, ORDER_TIMES_8, 0, k, 32, -1);
                // reduce s mod q
                // * (is this needed?  do it just in case, it's fast anyway) 
                // divmod((dstptr) t1, s, 32, order25519, 32);
                // take reciprocal of s mod q 
                var temp1 = new byte[32];
                var temp2 = new byte[64];
                var temp3 = new byte[64];
                Copy32( temp1, ORDER);
                Copy32( s, Egcd32(temp2, temp3, s, temp1));
                if ((s[31] & 0x80) != 0)
                    Multiply_Array_Small(s, s, 0, ORDER, 32, 1);
            }
        }


    }

}
