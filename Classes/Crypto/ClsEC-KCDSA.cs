using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Signum_Sharp.Classes.Crypto;

namespace Signum_Sharp.Classes.Crypto
{
    public class ClsEC_KCDSA
    {

        /// <summary>
        /// Signature generation primitive, calculates (x-h)s mod q
        /// </summary>
        /// <param name="SignHash">signature hash (of message, signature pub key, and context data)</param>
        /// <param name="SignPrivKey">signature private key</param>
        /// <param name="PrivKey">private key for signing</param>
        /// <returns>signature value</returns>
        public byte[] Sign(byte[] SignHash, byte[] SignPrivKey, byte[] PrivKey)
        {
            // v = (x - h) s  Mod q

            byte[] Dummy1 = new byte[32];
            byte[] Dummy2 = new byte[32];
            byte[] Temp_v = new byte[64];
            byte[] Dummy3 = new byte[64];

            ClsCurve25519 Curve = new ClsCurve25519();

            // Reduce modulo group order
            Curve.DivideMod( Dummy1, SignHash, 32, Curve.ORDER, 32);
            Curve.DivideMod( Dummy2, SignPrivKey, 32, Curve.ORDER, 32);

            // v = x1 - h1
            // If v Is negative, add the group order to it to become positive.
            // If v was already positive we don't have to worry about overflow
            // when adding the order because v < ORDER And 2*ORDER < 2^256
            byte[] SignValue = new byte[32];
            Curve.Multiply_Array_Small(SignValue, SignPrivKey, 0, SignHash, 32, -1);
            Curve.Multiply_Array_Small(SignValue, SignValue, 0, Curve.ORDER, 32, 1);

            // Temp_v = (x-h)*s Mod q
            Curve.Multiply_Array32(Temp_v, SignValue, PrivKey, 32, 1);
            Curve.DivideMod( Dummy3, Temp_v, 64, Curve.ORDER, 32);

            bool w = false;
            for (int i = 0; i <= 31; i++)
            {
                SignValue[i] = Temp_v[i];
                w = w | Convert.ToBoolean(SignValue[i]);
            }

            if (w != false)
                return SignValue;
            else
                return null;
        }

        /// <summary>
        /// Signature verification primitive, calculates Y = vP + hG
        /// </summary>
        /// <param name="SignValue">signature value</param>
        /// <param name="SignHash">signature hash</param>
        /// <param name="PublicKey">public key</param>
        /// <returns>signature public key</returns>
        public byte[] Verify(byte[] SignValue, byte[] SignHash, byte[] PublicKey)
        {

            // SignPublicKey = SignValue abs(PublicKey) + SignHash G  

            ClsCurve25519 Curve = new ClsCurve25519();

            var d = new byte[32];
            ClsCurve25519.Long10[] p = new ClsCurve25519.Long10[] { new ClsCurve25519.Long10(), new ClsCurve25519.Long10() };
            ClsCurve25519.Long10[] s = new ClsCurve25519.Long10[] { new ClsCurve25519.Long10(), new ClsCurve25519.Long10() };
            ClsCurve25519.Long10[] yx = new ClsCurve25519.Long10[] { new ClsCurve25519.Long10(), new ClsCurve25519.Long10(), new ClsCurve25519.Long10() };
            ClsCurve25519.Long10[] yz = new ClsCurve25519.Long10[] { new ClsCurve25519.Long10(), new ClsCurve25519.Long10(), new ClsCurve25519.Long10() };
            ClsCurve25519.Long10[] t1 = new ClsCurve25519.Long10[] { new ClsCurve25519.Long10(), new ClsCurve25519.Long10(), new ClsCurve25519.Long10() };
            ClsCurve25519.Long10[] t2 = new ClsCurve25519.Long10[] { new ClsCurve25519.Long10(), new ClsCurve25519.Long10(), new ClsCurve25519.Long10() };
            Int32 k;
            Int32 vi = 0;
            Int32 hi = 0;
            Int32 di = 0;
            Int32 nvh = 0;

            // set p[0] to G and p[1] to P  
            Curve.Set_In_To_Out(ref p[0], 9);
            Curve.Unpack(p[1], PublicKey);
            // set s[0] to P+G and s[1] to P-G  
            // s[0] = (Py^2 + Gy^2 - 2 Py Gy)/(Px - Gx)^2 - Px - Gx - 486662  
            // s[1] = (Py^2 + Gy^2 + 2 Py Gy)/(Px - Gx)^2 - Px - Gx - 486662  
            Curve.X_To_Y2(ref t1[0], ref t2[0], p[1]);  // t2[0] = Py^2  
            Curve.Square_Root(ref t1[0], t2[0]); // t1[0] = Py or -Py  

            int Negative = Curve.Is_Negative(t1[0]);   // ... check which  
            t2[0]._0 += ClsCurve25519.C39420360;   // t2[0] = Py^2 + Gy^2  
            Curve.Multiply(t2[1], Curve.BASE_2Y, t1[0]); // t2[1] = 2 Py Gy or -2 Py Gy  
            Curve.Subtract(ref t1[Negative], t2[0], t2[1]); // t1[0] = Py^2 + Gy^2 - 2 Py Gy  
            Curve.AddUp(ref t1[1 - Negative], t2[0], t2[1]); // t1[1] = Py^2 + Gy^2 + 2 Py Gy  
            Curve.Copy(ref t2[0], p[1]);   // t2[0] = Px  
            t2[0]._0 -= 9;      // t2[0] = Px - Gx  
            Curve.Square(t2[1], t2[0]);    // t2[1] = (Px - Gx)^2  
            Curve.Reciprocal(ref t2[0], t2[1], 0); // t2[0] = 1/(Px - Gx)^2  
            Curve.Multiply(s[0], t1[0], t2[0]);  // s[0] = t1[0]/(Px - Gx)^2  
            Curve.Subtract(ref s[0], s[0], p[1]);  // s[0] = t1[0]/(Px - Gx)^2 - Px  
            s[0]._0 -= ClsCurve25519.C9 + ClsCurve25519.C486662;    // s[0] = X(P+G)  
            Curve.Multiply(s[1], t1[1], t2[0]);  // s[1] = t1[1]/(Px - Gx)^2  
            Curve.Subtract(ref s[1], s[1], p[1]);  // s[1] = t1[1]/(Px - Gx)^2 - Px  
            s[1]._0 -= ClsCurve25519.C9 + ClsCurve25519.C486662;    // s[1] = X(P-G)  
            Curve.Multiply_Small(s[0], s[0], 1); // reduce s[0] 
            Curve.Multiply_Small(s[1], s[1], 1); // reduce s[1] 
                                                 // prepare the chain  
            for (int i = 0; i <= 32 - 1; i++)
            {
                vi = vi >> 8 ^ SignValue[i] & 0xFF ^ (SignValue[i] & 0xFF) << 1;
                hi = hi >> 8 ^ SignHash[i] & 0xFF ^ (SignHash[i] & 0xFF) << 1;
                nvh = ~(vi ^ hi);
                di = nvh & (di & 0x80) >> 7 ^ vi;
                di = di ^ nvh & (di & 0x1) << 1;
                di = di ^ nvh & (di & 0x2) << 1;
                di = di ^ nvh & (di & 0x4) << 1;
                di = di ^ nvh & (di & 0x8) << 1;
                di = di ^ nvh & (di & 0x10) << 1;
                di = di ^ nvh & (di & 0x20) << 1;
                di = di ^ nvh & (di & 0x40) << 1;
                d[i] = Convert.ToByte(di & 0xFF);
            }
            di = (nvh & (di & 0x80) << 1 ^ vi) >> 8;
            // initialize state 
            Curve.Set_In_To_Out(ref yx[0], 1);
            Curve.Copy(ref yx[1], p[di]);
            Curve.Copy(ref yx[2], s[0]);
            Curve.Set_In_To_Out(ref yz[0], 0);
            Curve.Set_In_To_Out(ref yz[1], 1);
            Curve.Set_In_To_Out(ref yz[2], 1);
            // y[0] is (even)P + (even)G
            // * y[1] is (even)P + (odd)G  if current d-bit is 0
            // * y[1] is (odd)P + (even)G  if current d-bit is 1
            // * y[2] is (odd)P + (odd)G
            // 
            vi = 0;
            hi = 0;
            // and go for it! 

            for (int i = 31; i >= 0; i += -1)
            {
                vi = vi << 8 | SignValue[i] & 0xFF;
                hi = hi << 8 | SignHash[i] & 0xFF;
                di = di << 8 | d[i] & 0xFF;

                for (int j = 7; j >= 0; j += -1)
                {
                    Curve.Monty_Prepare(ref t1[0], ref t2[0], yx[0], yz[0]);
                    Curve.Monty_Prepare(ref t1[1], ref t2[1], yx[1], yz[1]);
                    Curve.Monty_Prepare(ref t1[2], ref t2[2], yx[2], yz[2]);
                    k = ((vi ^ vi >> 1) >> j & 1) + ((hi ^ hi >> 1) >> j & 1);
                    Curve.Monty_Double(ref yx[2], ref yz[2], t1[k], t2[k], ref yx[0], ref yz[0]);
                    k = di >> j & 2 ^ (di >> j & 1) << 1;
                    Curve.Monty_AddUp(ref t1[1], ref t2[1], t1[k], t2[k], yx[1], yz[1], p[di >> j & 1]);
                    Curve.Monty_AddUp(ref t1[2], ref t2[2], t1[0], t2[0], yx[2], yz[2], s[((vi ^ hi) >> j & 2) >> 1]);
                }
            }

            k = (vi & 1) + (hi & 1);
            Curve.Reciprocal(ref t1[0], yz[k], 0);
            Curve.Multiply(t1[1], yx[k], t1[0]);

            byte[] SignPublicKey = new byte[32];

            Curve.Pack(t1[1], SignPublicKey);

            return SignPublicKey;
        }

    }
}
