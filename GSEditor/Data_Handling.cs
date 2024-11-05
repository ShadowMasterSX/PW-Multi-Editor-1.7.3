using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NpcGen_Editor.GSEditor
{
    public class Data_Handling
    {

        private byte[] dataAllFile;
        public int Version = -1;

        public byte[] DataAllFile
        {
            get
            {
                return dataAllFile;
            }
            set
            {
                dataAllFile = value;
            }
        }

        public Data_Handling(Form patcher, byte[] dataAllFile)
        {
            this.dataAllFile = dataAllFile;
        }

        public void ReplaceBytes(int offsetFile, Signatures.Bytes bytes, byte[] replaceData)
        {
            byte[] dst = new byte[dataAllFile.Length];
            Buffer.BlockCopy(dataAllFile, 0, dst, 0, offsetFile);
            Buffer.BlockCopy(replaceData, 0, dst, offsetFile, (int)bytes);
            Buffer.BlockCopy(dataAllFile, (int)(offsetFile + bytes), dst, (int)(offsetFile + bytes), (int)(dataAllFile.Length - (offsetFile + bytes)));
            dataAllFile = dst;
        }

        public int SearchBytesToFile(Signatures.FunctionSignature signature, int offset, Signatures.Bytes bytes, bool GetVersionGS = false)
        {
            int num = Signatures.SizeFunctions[(int)signature];
            Console.WriteLine(signature + " SIZE: " + num);

            if (num < 0)
            {
                return -1;
            }
            int num2 = FindBytes(dataAllFile, Signatures.beginFuncSignatures[(int)signature]);
            if (num2 < 0 && signature == Signatures.FunctionSignature.GS)
            {
                num2 = FindBytes(dataAllFile, new byte[13] { 85, 137, 229, 129, 236, 56, 4, 0, 0, 131, 236, 8, 104 });

                if (num2 < 0)
                {
                    return -1;
                }
            }
            if (num2 < 0 && signature == Signatures.FunctionSignature.TOWNSCROLL2)
            {
                num2 = FindBytes(dataAllFile, new byte[17] { 4, 116, 12, 199, 69, 236, 255, 255, 255, 255, 233, 157, 1, 0, 0, 139, 69 });

                if (num2 < 0)
                {
                    return -1;
                }
            }
            if (num2 < 0 && signature == Signatures.FunctionSignature.RECURRECT_SCROLL)
            {
                num2 = FindBytes(dataAllFile, new byte[14] { 0xFF, 0xFF, 0xFF, 0xFF, 0xE9, 0x32, 0x01, 0x00, 0x00, 0x83, 0xEC, 0x0C, 0x8D, 0x45});

                if (num2 < 0)
                {
                    return -1;
                }
            }
            if (num2 < 0 && signature == Signatures.FunctionSignature.MONEY_CAPACITY3)
            {
                num2 = FindBytes(dataAllFile, new byte[12] { 0x08, 0x83, 0xEC, 0x04, 0x6A, 0x20, 0x6A, 0x00, 0x8B, 0x45, 0x08, 0x05 });

                if (num2 < 0)
                {
                    return -1;
                }
            }
            if (num2 < 0)
                return -1;
            byte[] array = new byte[num];
            if (signature == Signatures.FunctionSignature.MONEY_CAPACITY3)
            {
                num2 += num;
            }
            Buffer.BlockCopy(dataAllFile, num2, array, 0, num);
            int num3;
            if (offset > 0)
            {
                num3 = FindBytes(array, Signatures.findInFuncSignatures[(int)signature]);
                if (num3 < 0 && signature == Signatures.FunctionSignature.TOWNSCROLL2)
                {
                    num3 = FindBytes(array, new byte[10] { 0xEB, 0x29, 0x8B, 0x45, 0xFC, 0x8B, 0x00, 0x05, 0x74, 0x01 });
                }
                if (num3 < 0 && signature == Signatures.FunctionSignature.COSMETIC_SUCCESS || signature == Signatures.FunctionSignature.RECURRECT_SCROLL)
                {
                    num3 = FindBytes(array, new byte[4] { 0x00, 0x6A, 0x00, 0x68 });
                }
                if (num3 < 0)
                {
                    return -1;
                }
                num3 += num2 + offset;
            }
            else
            {
                num3 = FindBytes(array, Signatures.findInFuncSignatures[(int)signature]) + num2 + offset;
                if (num3 < 0)
                {
                    return -1;
                }
            }
            Console.WriteLine(signature + " " + num3);

            if (GetVersionGS)
            {
                byte[] array2 = new byte[(int)bytes];
                Buffer.BlockCopy(dataAllFile, num3, array2, 0, (int)bytes);
                Version = Convert.ToInt32(array2[0]);
                return 0;
            }
            return num3;
        }

        public int getSizeFunctions(string path)
        {
            IELF iELF = null;
            try
            {
                iELF = ELFReader.Load(path);  // Load the ELF file
            }
            catch (IndexOutOfRangeException)
            {
                iELF?.Dispose();
                return -1;  // Return -1 if an IndexOutOfRangeException occurs
            }

            foreach (string nameFunction in Signatures.NameFunctions)
            {
                Func<ISymbolEntry, bool> predicate = (ISymbolEntry entry) =>
                    entry.Type == SymbolType.Function && entry.Name == nameFunction;

                ISymbolEntry symbolEntry = (iELF.GetSection(".symtab") as ISymbolTable)?.Entries.FirstOrDefault(predicate);

                if (symbolEntry == null)
                {
                    // Handle specific cases where the function name is different
                    if (nameFunction == Signatures.NameFunctions[1])
                    {
                        predicate = (ISymbolEntry entry) =>
                            entry.Type == SymbolType.Function &&
                            entry.Name == "_ZN16townscroll2_item5OnUseEN4item8LOCATIONEP11gactive_impPKcj";
                    }
                    else if (nameFunction == Signatures.NameFunctions[12])
                    {
                        predicate = (ISymbolEntry entry) =>
                            entry.Type == SymbolType.Function &&
                            entry.Name == "_Z15do_player_loginRK9A3DVECTORPKN3GDB9base_infoEPKNS2_7vecdataERK11userlogin_tc";
                    }
                    else if (nameFunction == Signatures.NameFunctions[14])
                    {
                        predicate = (ISymbolEntry entry) =>
                            entry.Type == SymbolType.Function &&
                            entry.Name == "_ZN21resurrect_scroll_item15OnUseWithTargetEN4item8LOCATIONEiP11gactive_impRK3XIDc";
                    }

                    symbolEntry = (iELF.GetSection(".symtab") as ISymbolTable)?.Entries.FirstOrDefault(predicate);

                    if (symbolEntry == null)
                    {
                        Signatures.SizeFunctions.Add(-1);
                        if (Signatures.SizeFunctions[0] == -1)
                        {
                            iELF.Dispose();
                            return -1;  // If the first function size is -1, return -1
                        }
                        continue;
                    }
                }

                // Extract the size from the symbol entry
                string sizeString = symbolEntry.ToString()
                    .Split(new string[] { "size: " }, StringSplitOptions.None)[1]
                    .Split(',')[0];

                Signatures.SizeFunctions.Add(int.Parse(sizeString));
            }

            iELF.Dispose();
            return 0;  // Return 0 to indicate success
        }


        private int FindBytes(byte[] src, byte[] find)
        {
            if (src == null || find == null || src.Length == 0 || find.Length == 0 || find.Length > src.Length)
            {
                return -1;
            }
            for (int i = 0; i < src.Length - find.Length + 1; i++)
            {
                if (src[i] != find[0])
                {
                    continue;
                }
                for (int j = 1; j < find.Length && src[i + j] == find[j]; j++)
                {
                    if (j == find.Length - 1)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
