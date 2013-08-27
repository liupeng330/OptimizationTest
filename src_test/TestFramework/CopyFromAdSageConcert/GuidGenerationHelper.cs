using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace AdSage.Concert.Hosting.Client.Core.Utility
{
    public static class GuidGenerationHelper
    {
        public static Guid StringToGUID(string value)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            return new Guid(data);
        }

        public static Guid StringToGuiD(int engineType, int objectType, long parentId, long id)
        {
            return StringToGUID(engineType.ToString() + objectType.ToString() + parentId.ToString() + id.ToString());
        }
    }
}
