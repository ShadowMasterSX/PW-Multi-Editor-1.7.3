using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DevExpress.Utils.Frames.FrameHelper;

namespace NpcGen_Editor.GSEditor
{
    public class Signatures
    {
        public enum FunctionSignature
        {
            GS,
            TOWNSCROLL2,
            COSMETIC_SUCCESS,
            CHANGE_ELF_SECURE_STATUS,
            CONVERT_PET_TO_EGG,
            FIRST_RUN,
            REFINE_ADDON_1,
            REFINE_ADDON_2,
            CHANGE_INVENTORY_SIZE,
            TEST_PK_PROTECTED,
            PLAYER_ENABLE_PVP_STATE,
            TRASH_BOX,
            TRASH_BOX2,
            SHARED_STORAGE,
            RECURRECT_SCROLL,
            MONEY_CAPACITY1,
            MONEY_CAPACITY2,
            MONEY_CAPACITY3
        }

        public enum Bytes
        {
            BYTE = 1,
            SHORT = 2,
            INTEGER = 4
        }

        public static List<int> SizeFunctions = new List<int>();

        public static List<int> DataPosToRepl = new List<int>();

        public static List<byte[]> CurrentDataInPos = new List<byte[]>();

        public static List<string> NameFunctions = new List<string>
    {
        "_ZN14elementdataman9load_dataEPKc", "_ZN16townscroll2_item5OnUseEN4item8LOCATIONEiP11gactive_impPKcj", "_ZN11gplayer_imp15CosmeticSuccessEiiij", "_ZN8elf_item21ChangeElfSecureStatusEib", "_ZN11gplayer_imp22ServiceConvertPetToEggEj", "_ZN4GNET5Skill8FirstRunERii", "_ZN10equip_item11RefineAddonEiRiPfS1_", "_ZN10equip_item11RefineAddonEiRiPfS1_", "_ZN11gplayer_imp19ChangeInventorySizeEi", "_ZN11gplayer_imp15TestPKProtectedEv",
        "_ZN11gplayer_imp20PlayerEnablePVPStateEv", "_ZN15player_trashbox15SetTrashBoxSizeEi", "_Z15do_player_loginRK9A3DVECTORPKN3GDB9base_infoEPKNS2_7vecdataERK11userlogin_t", "_ZN15player_trashbox16SetTrashBoxSize3Ei", "_ZN21resurrect_scroll_item15OnUseWithTargetEN4item8LOCATIONEP11gactive_impRK3XIDc", "_ZN11gplayer_impC2Ev", "_ZN11gplayer_imp24PlayerOpenPersonalMarketEjPKcPi", "_ZN11gplayer_impC2Ev"
    };

        public static readonly List<byte[]> beginFuncSignatures = new List<byte[]>
    {
        new byte[14]
        {
            85, 137, 229, 83, 129, 236, 52, 4, 0, 0,
            131, 236, 8, 104
        },
        new byte[17]
        {
            4, 116, 12, 199, 69, 244, 255, 255, 255, 255,
            233, 76, 1, 0, 0, 139, 69
        },
        new byte[24]
        {
            85, 137, 229, 131, 236, 8, 139, 69, 12, 137,
            69, 252, 106, 1, 255, 117, 16, 255, 117, 12,
            255, 117, 8, 232
        },
        new byte[17]
        {
            0, 0, 0, 128, 125, 251, 0, 116, 12, 199,
            69, 244, 1, 0, 0, 0, 233
        },
        new byte[16]
        {
            0, 131, 196, 16, 133, 192, 117, 12, 199, 69,
            240, 7, 0, 0, 0, 233
        },
        new byte[16]
        {
            85, 137, 229, 83, 131, 236, 20, 139, 69, 12,
            199, 0, 255, 255, 255, 255
        },
        new byte[24]
        {
            85, 137, 229, 131, 236, 88, 199, 69, 244, 0,
            0, 0, 0, 199, 69, 240, 255, 255, 255, 255,
            131, 236, 12, 139
        },
        new byte[24]
        {
            85, 137, 229, 131, 236, 88, 199, 69, 244, 0,
            0, 0, 0, 199, 69, 240, 255, 255, 255, 255,
            131, 236, 12, 139
        },
        new byte[22]
        {
            0, 0, 0, 131, 125, 12, 31, 127, 7, 199,
            69, 12, 32, 0, 0, 0, 131, 236, 8, 255,
            117, 12
        },
        new byte[12]
        {
            85, 137, 229, 131, 236, 24, 139, 69, 8, 102,
            131, 184
        },
        new byte[15]
        {
            139, 69, 180, 139, 93, 252, 201, 195, 85, 137,
            229, 131, 236, 8, 232
        },
        new byte[14]
        {
            255, 131, 196, 16, 59, 69, 12, 124, 2, 235,
            32, 129, 125, 12
        },
        new byte[22]
        {
            131, 193, 32, 139, 85, 8, 139, 2, 137, 1,
            139, 66, 4, 137, 65, 4, 139, 66, 8, 137,
            65, 8
        },
        new byte[24]
        {
            255, 131, 196, 16, 201, 195, 144, 85, 137, 229,
            131, 236, 8, 131, 236, 12, 139, 69, 8, 131,
            192, 80, 80, 232
        },
        new byte[14]
        {
            255, 255, 255, 255, 233, 51, 1, 0, 0, 131,
            236, 12, 141, 69
        },
        new byte[26]
        {
            139, 69, 248, 139, 0, 131, 192, 8, 255, 117,
            248, 139, 0, 255, 208, 131, 196, 16, 201, 195,
            85, 137, 229, 83, 131, 236
        },
        new byte[15]
        {
            131, 196, 16, 184, 1, 0, 0, 0, 201, 195,
            144, 144, 85, 137, 229
        },
        new byte[12]
        {
            8, 131, 236, 4, 106, 32, 106, 0, 139, 69,
            8, 5
        }
    };

        public static readonly List<byte[]> findInFuncSignatures = new List<byte[]>
    {
        new byte[3] { 0, 0, 48 },
        new byte[10] { 37, 131, 236, 4, 139, 69, 252, 139, 0, 5 },
        new byte[4] { 1, 0, 0, 104 },
        new byte[5] { 0, 131, 196, 16, 5 },
        new byte[8] { 0, 131, 196, 16, 131, 236, 8, 104 },
        new byte[10] { 255, 255, 131, 196, 16, 235, 72, 131, 125, 16 },
        new byte[9] { 199, 0, 0, 0, 0, 0, 199, 69, 244 },
        new byte[6] { 217, 109, 182, 199, 69, 192 },
        new byte[10] { 85, 137, 229, 83, 131, 236, 4, 131, 125, 12 },
        new byte[12]
        {
            85, 137, 229, 131, 236, 24, 139, 69, 8, 102,
            131, 184
        },
        new byte[9] { 0, 0, 0, 139, 69, 8, 102, 131, 184 },
        new byte[7] { 126, 2, 235, 21, 131, 236, 8 },
        new byte[8] { 251, 255, 255, 15, 126, 24, 129, 189 },
        new byte[7] { 126, 2, 235, 21, 131, 236, 8 },
        new byte[4] { 1, 0, 0, 104 },
        new byte[11]
        {
            0, 0, 0, 0, 0, 0, 139, 69, 8, 198,
            128
        },
        new byte[8] { 0, 0, 119, 23, 139, 69, 8, 186 },
        new byte[11]
        {
            0, 0, 0, 0, 0, 0, 139, 69, 8, 198,
            128
        }
    };

        public static byte[] DecimalToBytes(Bytes bytes, string textReplace)
        {
            switch (bytes)
            {
                case Bytes.BYTE:
                    {
                        byte[] bytes2 = BitConverter.GetBytes(Convert.ToInt32(textReplace));
                        return new byte[1] { bytes2[0] };
                    }
                case Bytes.SHORT:
                    return BitConverter.GetBytes(Convert.ToInt16(textReplace));
                case Bytes.INTEGER:
                    return BitConverter.GetBytes(Convert.ToInt32(textReplace));
                default:
                    return null;
            }
        }

        public static int BytesToDecimal(Bytes bytes, byte[] data)
        {
            try
            {
                switch (bytes)
                {
                    case Bytes.BYTE:
                        return sbyte.Parse(data[0].ToString());
                    case Bytes.SHORT:
                        return BitConverter.ToInt16(data, 0);
                    case Bytes.INTEGER:
                        return BitConverter.ToInt32(data, 0);
                    default:
                        return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }
    }
}
