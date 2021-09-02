using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace LMM_Rewritten.Protections
{
    class StringEnc
    {
        //const string DefaultPasswordHash = "p7K95451qB88sZ7J";
        //const string DefaultSalt = "2GM23j301t60Z96T";
        //const string DefaultVI = "IzTdhG6S8uwg141S";

        public static void FixSOD()
        {
            MethodDef OnlineDecryptionMethod = null;
            foreach (MethodDef methods in Program.asm.GlobalType.Methods)
            {
                if (!methods.HasBody) { continue; }
                foreach (Instruction inst in methods.Body.Instructions)
                {
                    if (inst.OpCode == OpCodes.Ldstr && inst.Operand.ToString() == Program.Preset.OnlineEncString_URL)
                    {
                        OnlineDecryptionMethod = methods;
                        break;
                    }
                }
                if (OnlineDecryptionMethod != null) { break; }
            }
            if (OnlineDecryptionMethod == null) { return; }
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        if (inst.OpCode == OpCodes.Call && inst.Operand is MethodDef)
                        {
                            if ((MethodDef)inst.Operand == OnlineDecryptionMethod)
                            {
                                methods.Body.Instructions[x - 1] = Instruction.Create(OpCodes.Ldstr, Encoding.Default.GetString(ConvertHexStr(methods.Body.Instructions[x - 1].Operand.ToString())));
                                methods.Body.Instructions.Remove(inst);
                                x--;
                            }
                        }
                    }
                }
            }
            OnlineDecryptionMethod.DeclaringType.Methods.Remove(OnlineDecryptionMethod);
        }

        private static byte[] ConvertHexStr(string hexData)
        {
            List<string> table = new List<string>();
            List<byte> byteTable = new List<byte>();
            List<byte> result = new List<byte>();
            for (int x = 0; x < 255; x++) { table.Add(x.ToString("X2")); byteTable.Add(Convert.ToByte(x)); }
            for (int x = 0; x < hexData.Length; x += 2) { result.Add(byteTable[table.IndexOf(hexData.ToUpper().Substring(x, 2))]); }
            return result.ToArray();
        }

        public static void Fix()
        {
            int count = 0;
            byte[] resData = ReadResource(Program.Preset.EncString_ResourceName);
            string[] strings = FormatUnhushedData(UnHush(resData, Program.Preset.EncString_PasswordHash, Program.Preset.EncString_Salt));
            MethodDef stringDecrypt = null;
            foreach (MethodDef methods in Program.asm.GlobalType.Methods)
            {
                if (!methods.HasBody) { continue; }
                bool hasRijndael = false, hasRfc = false, hasDecryptor = false;
                foreach (Instruction inst in methods.Body.Instructions)
                {
                    switch (inst.OpCode.Code)
                    {
                        case Code.Newobj:
                            if (Antis.isNamespace(inst, Program.Preset.EncString_AESAlgorithm.Namespace) && Antis.isClass(inst, Program.Preset.EncString_AESAlgorithm.Class)) { hasRijndael = true; }
                            if (Antis.isNamespace(inst, Program.Preset.EncString_KeyAlgorithm.Namespace) && Antis.isClass(inst, Program.Preset.EncString_KeyAlgorithm.Class)) { hasRfc = true; }
                            break;
                        case Code.Callvirt:
                            if (Antis.isNamespace(inst, Program.Preset.EncString_SymmetricAlg.Namespace) && Antis.isClass(inst, Program.Preset.EncString_SymmetricAlg.Class)) { hasDecryptor = true; }
                            break;
                    }
                }
                if (hasRijndael && hasRfc && hasDecryptor) { stringDecrypt = methods; }
            }
            if (stringDecrypt == null) { return; }
            foreach (TypeDef type in Program.asm.Types)
            {
                foreach (MethodDef methods in type.Methods)
                {
                    if (!methods.HasBody) { continue; }
                    for (int x = 0; x < methods.Body.Instructions.Count(); x++)
                    {
                        Instruction inst = methods.Body.Instructions[x];
                        if (inst.OpCode == OpCodes.Call)
                        {
                            if (inst.Operand is MethodDef)
                            {
                                MethodDef tempMethod = (MethodDef)inst.Operand;
                                if (tempMethod == stringDecrypt)
                                {
                                    int searchIndex = methods.Body.Instructions[x - 2].GetLdcI4Value();
                                    methods.Body.Instructions[x - 2] = Instruction.Create(OpCodes.Ldstr, DecryptString(strings[searchIndex], Program.Preset.EncString_PasswordHash, Program.Preset.EncString_Salt, Program.Preset.EncString_VI));
                                    methods.Body.Instructions.RemoveAt(x - 1);
                                    methods.Body.Instructions.RemoveAt(x - 1);
                                    count++;
                                    Flags.EncryptedStrings = true;
                                }
                            }
                        }
                    }
                }
            }
            ConsoleColor fore = ConsoleColor.White;
            ConsoleColor back = ConsoleColor.DarkRed;
            if (count == 0 || !Flags.EncryptedStrings) { fore = ConsoleColor.Black; back = ConsoleColor.DarkGray; }
            Program.AddPatchesLog("Enc.String", "Fixed " + count + " Strings!", ConsoleColor.DarkBlue, ConsoleColor.White, back, fore);
        }

        public static byte[] ReadResource(string ResourceName)
        {
            MemoryStream ms = new MemoryStream();
            Program.RefASM.GetManifestResourceStream(ResourceName).CopyTo(ms);
            return ms.ToArray();
        }

        public static byte[] UnHush(byte[] data, string password, byte[] salt)
        {
            byte[] bytes = new Rfc2898DeriveBytes(password, salt).GetBytes(0x20);
            byte[] array = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                array[i] = (byte)(data[i] ^ bytes[i % bytes.Length]);
            }
            return array;
        }

        public static string[] FormatUnhushedData(byte[] data)
        {
            using (StreamReader sr = new StreamReader(new MemoryStream(data)))
            {
                return sr.ReadToEnd().Split(new string[] {Environment.NewLine}, StringSplitOptions.None).ToArray<string>();
            }
        }

        public static string DecryptString(string text, string password, byte[] salt, byte[] vi)
        {
            byte[] array = Convert.FromBase64String(text);
            byte[] bytes = new Rfc2898DeriveBytes(password, salt).GetBytes(0x20);
            RijndaelManaged rijndaelManaged = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
            ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes, vi);
            MemoryStream memoryStream = new MemoryStream(array);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
            byte[] array2 = new byte[array.Length];
            int count = cryptoStream.Read(array2, 0, array2.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(array2, 0, count).TrimEnd("\0".ToCharArray());
        }
    }
}
