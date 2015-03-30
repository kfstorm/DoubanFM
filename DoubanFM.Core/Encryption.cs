using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace DoubanFM.Core
{
    /// <summary>
    /// 用于字符串加密的类
    /// </summary>
    static class Encryption
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        internal static string Encrypt(string rs)
        {
            byte[] desKey = Encoding.ASCII.GetBytes("DoubanFM");
            byte[] desIV = Encoding.ASCII.GetBytes("DoubanFM");

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                try
                {
                    byte[] inputByteArray = Encoding.Default.GetBytes(rs);
                    //byte[] inputByteArray=Encoding.Unicode.GetBytes(rs);

                    des.Key = desKey;  // ASCIIEncoding.ASCII.GetBytes(sKey);
                    des.IV = desIV;   //ASCIIEncoding.ASCII.GetBytes(sKey);
                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(),
                     CryptoStreamMode.Write))
                    {
                        //Write the byte array into the crypto stream
                        //(It will end up in the memory stream)
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();

                        //Get the data back from the memory stream, and into a string
                        StringBuilder ret = new StringBuilder();
                        foreach (byte b in ms.ToArray())
                        {
                            //Format as hex
                            ret.AppendFormat("{0:X2}", b);
                        }
                        ret.ToString();
                        return ret.ToString();
                    }
                }
                catch
                {
                    return rs;
                }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        internal static string Decrypt(string rs)
        {
            byte[] desKey = Encoding.ASCII.GetBytes("DoubanFM");
            byte[] desIV = Encoding.ASCII.GetBytes("DoubanFM");

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                try
                {
                    //Put the input string into the byte array
                    byte[] inputByteArray = new byte[rs.Length / 2];
                    for (int x = 0; x < rs.Length / 2; x++)
                    {
                        int i = (Convert.ToInt32(rs.Substring(x * 2, 2), 16));
                        inputByteArray[x] = (byte)i;
                    }

                    des.Key = desKey;   //ASCIIEncoding.ASCII.GetBytes(sKey);
                    des.IV = desIV;    //ASCIIEncoding.ASCII.GetBytes(sKey);
                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        //Flush the data through the crypto stream into the memory stream
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();

                        //Get the decrypted data back from the memory stream
                        return System.Text.Encoding.Default.GetString(ms.ToArray());
                    }
                }
                catch
                {
                    return rs;
                }
        }
    }
}
