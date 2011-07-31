using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace DoubanFM.Core
{
    internal class Encryption
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static string Encrypt(string rs)
        {
            byte[] desKey = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };
            byte[] desIV = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            try
            {
                byte[] inputByteArray = Encoding.Default.GetBytes(rs);
                //byte[] inputByteArray=Encoding.Unicode.GetBytes(rs);

                des.Key = desKey;  // ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = desIV;   //ASCIIEncoding.ASCII.GetBytes(sKey);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(),
                 CryptoStreamMode.Write);
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
            catch
            {
                return rs;
            }
            finally
            {
                des = null;
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        public static string Decrypt(string rs)
        {
            byte[] desKey = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };
            byte[] desIV = new byte[] { 0x16, 0x09, 0x14, 0x15, 0x07, 0x01, 0x05, 0x08 };

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
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
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
                //Flush the data through the crypto stream into the memory stream
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                //Get the decrypted data back from the memory stream
                StringBuilder ret = new StringBuilder();

                return System.Text.Encoding.Default.GetString(ms.ToArray());
            }
            catch
            {
                return rs;
            }
            finally
            {
                des = null;
            }
        }
    }
}
