using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signum_Sharp.Classes
{
    public class ClsGZip
    {

        /// <summary>
        /// Receives bytes, returns compressed bytes.
        /// </summary>
        public static byte[] Compress(byte[] raw)
        {

            // Clean up memory with Using-statements.
            using (MemoryStream memory = new MemoryStream())
            {
                // Create compression stream.
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    // Write.
                    gzip.Write(raw, 0, raw.Length);
                }
                // Return array.

                return memory.ToArray();
            }
        }

        /// <summary>
        /// Receives bytes, returns decompressed bytes.

        /// </summary>
        public static string Inflate(byte[] raw)
        {
            string OutputStr = "";

            // Clean up memory with Using-statements.
            using (MemoryStream memory = new MemoryStream(raw))
            {
                // Create compression stream.
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Decompress, true))
                {
                    // Read
                    using (StreamReader Reader = new StreamReader(gzip))
                    {
                        OutputStr = Reader.ReadToEnd();
                    }
                }
                // Return String.

                return OutputStr;
            }
        }

    }
}
