using DevExpress.Utils;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static NpcGen_Editor.MapTools.Precinct_Editor.CELPrecinct;

namespace NpcGen_Editor.MapTools.Precinct_Editor
{
    public struct PRECINCTFILEHEADER5
    {
        public uint dwVersion;     //	File version
        public int iNumPrecinct;   //	Number of NPC generator
        public uint dwTimeStamp;   //	Time stamp of this data file
    };

    public class CELPrecinct
    {
        public CELPrecinct()
        {
            m_iPriority = 0;
            m_idDstInst = 0;
            m_idSrcInst = 0;
            m_idDomain = 0;
            m_fLeft = 999999.0f;
            m_fTop = 999999.0f;
            m_fRight = -999999.0f;
            m_fBottom = -999999.0f;

            m_vCityPos.X = 0.0f;
            m_vCityPos.Y = 0.0f;
            m_vCityPos.Z = 0.0f;

            m_aMarks = new List<MARK>();
            m_aMusicFiles = new List<MUSICFILES>();
            m_aPoints = new List<VECTOR3>();

            // Cliente
            m_dwID = 0;
            m_iMusicInter = 0;
            m_iMusicLoop = 0;
            m_bNightSFX = false;
            // Fim
        }

        public enum MusicLoopType
        {
            LOOP_NONE = 0,
            LOOP_WHOLE,
            LOOP_SKIPFIRST,
        }
        public struct MARK
        {
            public string Nome { get { return strName; } set { strName = value; } }
            public string strName;
            public VECTOR3 vPos;
        };

        public struct MUSICFILES
        {
            public string Nome { get { return strName; } set { strName = value; } }
            public string strName;
        }

        public struct VECTOR3
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public VECTOR3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public string Nome
        {
            get { return m_strName.Replace("\"", ""); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_strName = value;
                else
                    throw new ArgumentException("O nome não pode ser vazio.");
            }
        }

        // Cliente
        public string m_strName = "";
        public List<MARK> m_aMarks;
        public string m_strSound = "";
        public string m_strSound_n = "";
        public List<MUSICFILES> m_aMusicFiles;
        public uint m_dwID;
        public int m_iMusicInter;      //	Music interval
        public int m_iMusicLoop;       //	Music loop
        public bool m_bNightSFX;       //	flag indicates current night sfx is activated
        // Fim

        public int m_idDstInst;    //	ID of instance m_vCityPos belongs to
        public int m_idSrcInst;    //	ID of source instance
        public int m_iPriority;    //	Precinct priority
        public VECTOR3 m_vCityPos;     //	City position
        public int m_idDomain;     //	ID of domain
        public bool m_bPKProtect;   //  是否是PK保护区

        public double m_fLeft;      //	Bound box of precinct
        public double m_fTop;
        public double m_fRight;
        public double m_fBottom;

        public List<VECTOR3> m_aPoints; //	Precinct points

        string returnLine(StreamReader sr)
        {
            string line;
            int i = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Trim().StartsWith("//") || line.Length == 0)
                {
                    continue;
                }
                return line.Replace("\"", "");
            }
            return string.Empty;
        }


        public bool LoadSev(BinaryReader reader, int version)
        {
            int iNumPoint = reader.ReadInt32();
            m_iPriority = reader.ReadInt32();
            m_idDstInst = reader.ReadInt32();
            if (version >= 4)
                m_idSrcInst = reader.ReadInt32();
            else
                m_idSrcInst = 1;
            if (version >= 6)
                m_idDomain = reader.ReadInt32();
            else
                m_idDomain = 0;
            if (version >= 7)
                m_bPKProtect = reader.ReadBoolean();
            else
                m_bPKProtect = false;

            float[] v = new float[3];
            for (int i = 0; i < 3; i++)
            {
                v[i] = reader.ReadSingle();
            }
            m_vCityPos = new VECTOR3(v[0], v[1], v[2]);

            for (int i = 0; i < iNumPoint; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    v[j] = reader.ReadSingle();
                }
                VECTOR3 v1 = new VECTOR3(v[0], v[1], v[2]);
                m_aPoints.Add(v1);
            }
            return true;
        }

        public bool LoadClt(StreamReader sr, int version)
        {
            m_strName = returnLine(sr);
            string[] valores = returnLine(sr).Replace("  ", " ").Split(' ');
            int iNumPoint;

            if (version < 2)
            {
                m_dwID = 0;

                int.TryParse(valores[1], out iNumPoint);
            }
            else // dwVersion >= 2
            {
                // Read precinct ID
                uint.TryParse(valores[0], out m_dwID);
                // Read point number
                int.TryParse(valores[1], out iNumPoint);
            }

            // Read mark number
            int iNumMark;
            int.TryParse(valores[2], out iNumMark);
            // Read priority
            int.TryParse(valores[3], out m_iPriority);
            // Read destination instance ID
            int.TryParse(valores[4], out m_idDstInst);
            // Read music file number
            int iNumMusic;
            int.TryParse(valores[5], out iNumMusic);
            // Read music interval
            int.TryParse(valores[6], out m_iMusicInter);
            // Read music loop type
            int.TryParse(valores[7], out m_iMusicLoop);

            // Source instance ID

            m_idSrcInst = (version >= 4) ? int.Parse(valores[8]) : 1;

            // ID of domain
            m_idDomain = (version >= 6) ? int.Parse(valores[9]) : 0;

            // pk protect
            m_bPKProtect = (version >= 7) ? int.Parse(valores[10]) > 0 : false;

            // Read city position
            string[] cityPos = returnLine(sr).Trim().Split(',');
            m_vCityPos = new VECTOR3
            {
                X = double.Parse(cityPos[0].Replace('.', ',')),
                Y = double.Parse(cityPos[1].Replace('.', ',')),
                Z = double.Parse(cityPos[2].Replace('.', ','))
            };

            // Read vertices ...
            for (int i = 0; i < iNumPoint; i++)
            {
                string[] vPos = returnLine(sr).Replace("  ", " ").Split(' ');
                VECTOR3 v = new VECTOR3
                {
                    X = double.Parse(vPos[0].Replace('.', ',')),
                    Y = double.Parse(vPos[1].Replace('.', ',')),
                    Z = double.Parse(vPos[2].Replace('.', ','))
                };

                m_aPoints.Add(v);
            }
            // Read mark ...
            for (int i = 0; i < iNumMark; i++)
            {
                MARK pMark = new MARK();
                string[] mark = returnLine(sr).Replace("  ", " ").Split(' ');
                pMark.strName = mark[0];
                pMark.vPos.X = double.Parse(mark[1].Replace('.', ','));
                pMark.vPos.Y = double.Parse(mark[2].Replace('.', ','));
                pMark.vPos.Z = double.Parse(mark[3].Replace('.', ','));
                m_aMarks.Add(pMark);
            }
            // Read sound file

            m_strSound = returnLine(sr);
            // Read music files ...
            for (int i = 0; i < iNumMusic; i++)
            {
                MUSICFILES pMusicFile = new MUSICFILES();
                pMusicFile.strName = returnLine(sr);
                m_aMusicFiles.Add(pMusicFile);
            }
            // Read sound file at night
            if (version >= 3)
            {
                m_strSound_n = returnLine(sr);
            }
            // Build bound box
            BuildBoundBox();

            return true;
        }

        public void BuildBoundBox()
        {
            m_fLeft = 999999.0f;
            m_fTop = 999999.0f;
            m_fRight = -999999.0f;
            m_fBottom = -999999.0f;

            foreach (var v in m_aPoints)
            {
                if (v.X < m_fLeft) m_fLeft = v.X;
                if (v.X > m_fRight) m_fRight = v.X;
                if (v.Z < m_fTop) m_fTop = v.Z;
                if (v.Z > m_fBottom) m_fBottom = v.Z;
            }
        }

        public bool SaveClt(StreamWriter sw, int version)
        {
            sw.WriteLine($"\"{m_strName}\"");
            if (version < 2)
            {
                sw.WriteLine(0);
                sw.Write($"  {m_aPoints.Count}");
            }
            else // dwVersion >= 2
            {
                sw.Write(m_dwID);
                sw.Write($"  {m_aPoints.Count}");
            }
            sw.WriteLine($"  {m_aMarks.Count}  {m_iPriority}  {m_idDstInst}  {m_aMusicFiles.Count}  {m_iMusicInter}  {m_iMusicLoop}  {(version >= 4 ? m_idSrcInst : 1)}  {(version >= 6 ? m_idDomain : 0)}  {Convert.ToByte((version >= 7 ? m_bPKProtect : false))}");
            sw.WriteLine($"{m_vCityPos.X.ToString("F6", CultureInfo.InvariantCulture)}, {m_vCityPos.Y.ToString("F6", CultureInfo.InvariantCulture)}, {m_vCityPos.Z.ToString("F6", CultureInfo.InvariantCulture)}");
            for (int i = 0; i < m_aPoints.Count; i++)
            {
                sw.WriteLine($"{m_aPoints[i].X.ToString("F6", CultureInfo.InvariantCulture)}  {m_aPoints[i].Y.ToString("F6", CultureInfo.InvariantCulture)}  {m_aPoints[i].Z.ToString("F6", CultureInfo.InvariantCulture)}");
            }
            sw.WriteLine();
            for (int i = 0; i < m_aMarks.Count; i++)
            {
                sw.WriteLine($"\"{m_aMarks[i].strName}\"  {m_aMarks[i].vPos.X.ToString("F6", CultureInfo.InvariantCulture)}  {m_aMarks[i].vPos.Y.ToString("F6", CultureInfo.InvariantCulture)}  {m_aMarks[i].vPos.Z.ToString("F6", CultureInfo.InvariantCulture)}");
            }
            sw.WriteLine();

            if (m_aMusicFiles.Count == 0)
                sw.WriteLine("\"\"");
            for (int i = 0; i < m_aMusicFiles.Count; i++)
            {
                sw.WriteLine("\"\"");
                sw.WriteLine($"\"{m_aMusicFiles[i].strName}\"");
            }
            if (version >= 3)
            {
                sw.WriteLine($"\"{m_strSound_n}\"");
            }
            sw.WriteLine();
            return true;
        }

        public void SaveSev(BinaryWriter writer, int version)
        {
            writer.Write(m_aPoints.Count); 
            writer.Write(m_iPriority);
            writer.Write(m_idDstInst); 
            writer.Write(version >= 4 ? m_idSrcInst : 1); 
            writer.Write(version >= 6 ? m_idDomain : 0); 
            writer.Write(version >= 7 ? m_bPKProtect : false);

            writer.Write((float)m_vCityPos.X);
            writer.Write((float)m_vCityPos.Y);
            writer.Write((float)m_vCityPos.Z);

            for(int i = 0; i < m_aPoints.Count; i++)
            {
                writer.Write((float)m_aPoints[i].X);
                writer.Write((float)m_aPoints[i].Y);
                writer.Write((float)m_aPoints[i].Z);
            }
        }
    }

    public class CELPrecinctSet
    {
        public long m_dwTimeStamp = 0;
        public int version = 0;
        public List<CELPrecinct> m_aPrecincts = new List<CELPrecinct>();

        public bool SaveClt(string path, int version)
        {
            DateTime currentDateTime = DateTime.UtcNow;
            m_dwTimeStamp = ((DateTimeOffset)currentDateTime).ToUnixTimeSeconds();

            try
            {
                using (StreamWriter sw = new StreamWriter(path, false, new UnicodeEncoding(false, true)))
                {
                    sw.WriteLine("//  Element pricinct file (client version) - PW AIO Editor");
                    sw.WriteLine();
                    sw.WriteLine($"version  {version}");
                    sw.WriteLine(m_dwTimeStamp);
                    sw.WriteLine();
                    for (int i = 0; i < m_aPrecincts.Count; i++)
                    {
                        m_aPrecincts[i].SaveClt(sw, version);
                    }
                }

                Console.WriteLine("O conteúdo do arquivo foi atualizado com sucesso.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Ocorreu um erro ao escrever no arquivo:");
                Console.WriteLine(e.Message);
            }
            return true;
        }

        public bool LoadClt(string szFileName)
        {
            using (StreamReader reader = new StreamReader(szFileName))
            {
                string pattern = @"\d*\s+\d*\s+\d*\s+\d*\s+\d*\s+\d*\s+\d*\s+\d*\s+\d*\s+\d*\s+\d*";
                int count = Regex.Matches(reader.ReadToEnd(), pattern).Count;
                reader.BaseStream.Position = 0;

                string line;
                int i = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("//") || line.Length == 0)
                    {
                        continue;
                    }
                    if (line.Contains("version"))
                    {
                        int.TryParse(line.Replace("version", "").Trim(), out version);
                    }
                    if (i == 1 && version >= 5)
                    {
                        long.TryParse(line, out m_dwTimeStamp);
                        break;
                    }
                    i++;
                }

                for (int o = 0; o < count; o++)
                {
                    CELPrecinct precint = new CELPrecinct();
                    if (precint.LoadClt(reader, version))
                        m_aPrecincts.Add(precint);
                }
            }
            return true;
        }

        public bool LoadSev(string szFileName)
        {
            try
            {
                using (FileStream fs = new FileStream(szFileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        version = reader.ReadInt32();
                        fs.Seek(0, SeekOrigin.Begin);
                        PRECINCTFILEHEADER5 header = new PRECINCTFILEHEADER5
                        {
                            dwVersion = reader.ReadUInt32(),
                            iNumPrecinct = reader.ReadInt32(),
                            dwTimeStamp = version < 5 ? 0 : reader.ReadUInt32(),
                        };

                        for (int i = 0; i < header.iNumPrecinct; i++)
                        {
                            CELPrecinct pPrecinct = new CELPrecinct();
                            {
                                if (pPrecinct.LoadSev(reader, version))
                                {
                                    pPrecinct.m_strName = $"#{(i + 1).ToString("D4")}\tPosições: {pPrecinct.m_aPoints.Count}";
                                    m_aPrecincts.Add(pPrecinct);
                                }
                            }
                        }

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool SaveSev(string szFileName, int ver)
        {
            try
            {
                using (FileStream fs = new FileStream(szFileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        DateTime currentDateTime = DateTime.UtcNow;
                        m_dwTimeStamp = ((DateTimeOffset)currentDateTime).ToUnixTimeSeconds();
                        writer.Write(ver);
                        writer.Write(m_aPrecincts.Count);
                        if (ver >= 5)
                            writer.Write((uint)m_dwTimeStamp);

                        for (int i = 0; i < m_aPrecincts.Count; i++)
                        {
                            m_aPrecincts[i].SaveSev(writer, version);
                        }

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

    }
}
