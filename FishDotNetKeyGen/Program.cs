using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Linq;

namespace FishDotNetKeyGen
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("please select product:");

            int i = 0;
            List<string> nameItems = License.GetProductNames();
            foreach (string productName in nameItems)
            {
                Console.WriteLine("{0}.{1}", i, productName);
                i++;
            }
            int iName = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("please select edition type:");
            try
            {
                i = 0;
                List<string> typeItems = License.GetEditonTypes(nameItems[iName]);
                foreach (string typeName in typeItems)
                {
                    Console.WriteLine("{0}.{1}", i, typeName);
                    i++;
                }
                int iType = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("please input user name:");
                string user = Console.ReadLine();

                Console.WriteLine("please input use counts(>=200):");
                string count = Console.ReadLine();

                Console.WriteLine("please wait the key:");
                string key = License.GenerateKey(user, count, nameItems[iName], typeItems[iType]);
                Console.WriteLine(key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadKey();
        }

    }

    class License
    {
        static License()
        {
            AddEditionTypes();
        }

        private License()
        {

        }

        private static void AddEditionTypes()
        {
            foreach (emProductName productName in Enum.GetValues(typeof(emProductName)))
            {
                //add default editions to every product
                List<string> editionTypes = new List<string>()
                    {emEditionType.Enterprise.ToString(), emEditionType.Professional.ToString(),
                        emEditionType.Plus.ToString(), emEditionType.Free.ToString()};
                //add special editions to product
                switch (productName)
                {
                    case emProductName.fishDatabase:
                    case emProductName.fishDatabase5X64:
                        editionTypes.AddRange(new[] { emEditionType.Standard.ToString() });
                        break;
                    case emProductName.fishDBMigration:
                        break;
                    case emProductName.fishCapture:
                        break;
                    case emProductName.fishCodeLib:
                        break;
                    case emProductName.fishConvert:
                        break;
                    case emProductName.fishDictionary:
                        break;
                    case emProductName.fishResource:
                        break;
                    case emProductName.fishVocabulary:
                        break;
                    default:
                        break;
                }

                _dicProductEditions.Add(productName.ToString(), editionTypes);
            }
        }
        public static List<string> GetProductNames()
        {
            return Enum.GetNames(typeof(License.emProductName)).ToList();
        }
        public static List<string> GetEditonTypes(string name)
        {
            List<string> typeNames = null;
            if (_dicProductEditions.ContainsKey(name))
            {
                typeNames = new List<string>();
                typeNames.AddRange(_dicProductEditions[name].ToList());
            }

            return  typeNames;
        }
        private static byte[] GetDesKey(byte[] seed)
        {
            int num = _tripleDESCryptoServiceProvider.Key.Length;
            checked
            {
                byte[] result = null;
                if (seed.Length != num)
                {
                    byte[] array = new byte[num];
                    Array.Copy(_sha256Managed.ComputeHash(seed), array, num - 1);
                    result = array;
                }
                else
                {
                    result = seed;
                }

                return result;
            }
        }
        private static byte[] GetDesIV(byte[] key, int len)
        {
            checked
            {
                byte[] array = new byte[len - 1 + 1];
                Array.Copy(_sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(key))), array, len - 1);
                return array;
            }
        }
        private static string DesEncrypt(string info, byte[] key)
        {
            string Result = string.Empty;

            try
            {
                byte[] inputArray = Encoding.UTF8.GetBytes(info);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ICryptoTransform crptoTransform = _tripleDESCryptoServiceProvider.CreateEncryptor(key,
                        GetDesIV(key, _tripleDESCryptoServiceProvider.IV.Length)))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, crptoTransform, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(inputArray, 0, inputArray.Length);
                            cryptoStream.FlushFinalBlock();
                        }
                    }

                    Result = Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Result;
        }
        public static string GenerateKey(string user, string count, string productName, string editionType)
        {
            string Result = string.Empty;
            string info = string.Empty;
            string TimeOps = string.Empty;
            byte[] seed = null;
            byte[] key = null;

            emProductName name = (emProductName)Enum.Parse(typeof(emProductName), productName);
            emEditionType edition = (emEditionType) Enum.Parse(typeof(emEditionType), editionType);
            switch (name)
            {
                case emProductName.fishDatabase:
                case emProductName.fishDictionary:
                case emProductName.fishConvert:
                case emProductName.fishCodeLib:
                case emProductName.fishCapture:
                case emProductName.fishResource:
                case emProductName.fishVocabulary:
                    TimeOps = String.Empty;
                    editionType = String.Empty;
                    break;
                case emProductName.fishDBMigration:
                    TimeOps = char.ConvertFromUtf32(2468 + 41);
                    TimeOps = TimeOps + Convert.ToString(DateTime.Now.Subtract(DateTime.Now).Seconds);
                    break;
                case emProductName.fishDatabase5X64:
                    TimeOps = char.ConvertFromUtf32(9786 + 30);
                    productName = Enum.GetName(name.GetType(), emProductName.fishDatabase);
                    TimeOps = TimeOps + Convert.ToString(DateTime.Now.Subtract(DateTime.Now).Seconds);
                    
                    break;
                default:
                    throw new Exception("Invalid Product Name");
            }

            seed = Encoding.UTF8.GetBytes(productName + TimeOps + (int)edition);
            info = productName + _seperator + user + " " + _seperator + count + _seperator + count +
                   _seperator + editionType + _seperator + "@" + user;
            key = GetDesKey(seed);
            Result = DesEncrypt(info, key);

            return Result;
        }

        private enum emProductName
        {
            fishDatabase,
            fishDictionary,
            fishConvert,
            fishCodeLib,
            fishCapture,
            fishResource,
            fishVocabulary,
            fishDBMigration,
            fishDatabase5X64
        }

        private enum emEditionType
        {
            Enterprise,
            Professional,
            Free,
            Plus,
            Standard,
        }

        private static Dictionary<string,List<string>> _dicProductEditions = new Dictionary<string, List<string>>();
        private static TripleDESCryptoServiceProvider _tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
        private static SHA256Managed _sha256Managed = new SHA256Managed();
        private static string _seperator = "|";
    }
}
