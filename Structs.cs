using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcGen_Editor
{
    using System;
    using System.Drawing;
    using System.IO;

    class ElementsField
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Image Icon { get; set; }
    }

    class ElementsValues
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string ObjectType { get; set; }
    }

    class AWScriptFile
    {
        private StreamReader m_Script;
        private const int MAX_LINELEN = 256;
        private string m_szToken = "";

        public AWScriptFile(string filePath)
        {
            m_Script = new StreamReader(filePath);
        }

        public AWScriptFile(StreamReader sr)
        {
            m_Script = sr;
        }

        public bool GetNextToken(bool bCrossLine)
        {
        NewLine:
            // Search for the first character which is larger than 32
            while (!m_Script.EndOfStream)
            {
                int nextChar = m_Script.Peek();
                if (nextChar > 32 && nextChar != ';' && nextChar != ',')
                    break;

                if (nextChar == '\n')
                {
                    m_Script.ReadLine();
                    if (!bCrossLine)
                    {
                        return false;
                    }

                    // Increment line count
                    m_Script.ReadLine();
                    goto NewLine;
                }
                m_Script.Read();
            }

            if (m_Script.EndOfStream)
                return false;

            // Skip comment lines that begin with '//'
            if (m_Script.Peek() == '/' && m_Script.Peek() == '/')
            {
                // This is a comment line, read until the end of line
                m_Script.ReadLine();

                if (!bCrossLine) // Don't search cross line
                    return false;

                // Increment line count
                m_Script.ReadLine();
                goto NewLine;
            }

            // Text between /* */ are also comments
            if (m_Script.Peek() == '/' && m_Script.Peek() == '*')
            {
                bool bError = false;

                m_Script.Read(); // Skip '/'
                m_Script.Read(); // Skip '*'

                while (true)
                {
                    if (m_Script.Peek() == '\n')
                    {
                        if (!bCrossLine)
                        {
                            // This is a fatal error, we should return false.
                            // But we must search the '*/' so that next time our begin point
                            // isn't in comment paragraph
                            bError = true;
                        }
                        m_Script.ReadLine(); // Skip '\n'

                        // Increment line count
                        m_Script.ReadLine();
                    }
                    else if (m_Script.Peek() == '*' && m_Script.Peek() == '/')
                    {
                        m_Script.Read(); // Skip '*'
                        m_Script.Read(); // Skip '/'
                        break;
                    }
                    else
                    {
                        m_Script.Read();
                    }

                    if (m_Script.EndOfStream) // Found nothing
                        return false;
                }

                if (bError)
                    return false;

                goto NewLine;
            }

            int i = 0;

            // Copy string in "" or () pair
            if (m_Script.Peek() == '"' || m_Script.Peek() == '(')
            {
                char cEnd;
                if (m_Script.Peek() == '"')
                    cEnd = '"';
                else
                    cEnd = ')';

                m_Script.Read(); // Skip " or (

                while (m_Script.Peek() != cEnd)
                {
                    if (i >= MAX_LINELEN - 1)
                        return false;

                    m_szToken += (char)m_Script.Read();
                    i++;
                }

                m_Script.Read(); // Skip " or )
            }
            else // Is a normal token
            {
                while (m_Script.Peek() > 32 && m_Script.Peek() != ';' && m_Script.Peek() != ',')
                {
                    if (i >= MAX_LINELEN - 1)
                        return false;

                    m_szToken += (char)m_Script.Read();
                    i++;
                }
            }

            return true;
        }
    }

}
