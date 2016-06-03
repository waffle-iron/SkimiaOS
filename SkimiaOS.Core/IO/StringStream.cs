using System;
using System.Collections.Generic;

namespace SkimiaOS.Core.IO
{
    public class StringStream : ICloneable
    {
        private readonly string m_str;
        private int m_pos;
        public bool HasNext
        {
            get
            {
                return this.m_pos < this.m_str.Length;
            }
        }
        public int Position
        {
            get
            {
                return this.m_pos;
            }
            set
            {
                this.m_pos = value;
            }
        }
        public int Length
        {
            get
            {
                return this.m_str.Length - this.m_pos;
            }
        }
        public string Remainder
        {
            get
            {
                string result;
                if (!this.HasNext)
                {
                    result = "";
                }
                else
                {
                    result = this.m_str.Substring(this.m_pos, this.Length);
                }
                return result;
            }
        }
        public string String
        {
            get
            {
                return this.m_str;
            }
        }
        public string this[int index]
        {
            get
            {
                return "";
            }
        }

        public StringStream(string s)
            : this(s, 0)
        {
        }

        public StringStream(string s, int initialPos)
        {
            this.m_str = s;
            this.m_pos = initialPos;
        }

        public StringStream(StringStream stream)
            : this(stream.m_str, stream.m_pos)
        {
        }

        public object Clone()
        {
            return new StringStream(this.m_str)
            {
                m_pos = this.m_pos
            };
        }

        public void Reset()
        {
            this.m_pos = 0;
        }

        public void Ignore(int charCount)
        {
            this.m_pos += charCount;
        }

        public long NextLong()
        {
            return this.NextLong(-1L, " ");
        }

        public long NextLong(long defaultVal)
        {
            return this.NextLong(defaultVal, " ");
        }

        public long NextLong(long defaultVal, string seperator)
        {
            long result;
            try
            {
                result = long.Parse(this.NextWord(seperator));
            }
            catch
            {
                result = defaultVal;
            }
            return result;
        }

        public int NextInt()
        {
            return this.NextInt(-1, " ");
        }

        public int NextInt(int defaultVal)
        {
            return this.NextInt(defaultVal, " ");
        }

        public int NextInt(int defaultVal, string seperator)
        {
            int result;
            try
            {
                result = int.Parse(this.NextWord(seperator));
            }
            catch
            {
                result = defaultVal;
            }
            return result;
        }

        public string PeekNextWord()
        {
            return this.PeekNextWord(" ");
        }

        public string PeekNextWord(string seperator)
        {
            int pos = this.m_pos;
            string result = this.NextWord(seperator);
            this.m_pos = pos;
            return result;
        }

        public string NextWord()
        {
            return this.NextWord(" ");
        }

        public string NextWord(string seperator)
        {
            int length = this.m_str.Length;
            string result;
            if (this.m_pos >= length)
            {
                result = "";
            }
            else
            {
                int num;
                while ((num = this.CustomIndexOf(seperator, this.m_pos, '"')) == 0)
                {
                    this.m_pos += seperator.Length;
                }
                if (num < 0)
                {
                    if (this.m_pos == length)
                    {
                        result = "";
                        return result;
                    }
                    num = length;
                }
                string text = this.m_str.Substring(this.m_pos, num - this.m_pos);
                this.m_pos = num + seperator.Length;
                if (this.m_pos > length)
                {
                    this.m_pos = length;
                }
                result = text;
            }
            return result;
        }

        public string NextWords()
        {
            return this.NextWords(this.Length);
        }

        public string NextWords(int count)
        {
            return this.NextWords(count, " ");
        }

        public string NextWords(int count, string seperator)
        {
            string text = "";
            int num = 0;
            while (num < count && this.HasNext)
            {
                if (num > 0)
                {
                    text += seperator;
                }
                text += this.NextWord(seperator);
                num++;
            }
            return text;
        }

        public string[] NextWordsArray(int count)
        {
            return this.NextWordsArray(count, " ");
        }

        public string[] NextWordsArray(int count, string sep)
        {
            string[] array = new string[count];
            int num = 0;
            while (num < count && this.HasNext)
            {
                array[num] = this.NextWord(sep);
                num++;
            }
            return array;
        }

        public string[] RemainingWords()
        {
            return this.RemainingWords(" ");
        }

        public string[] RemainingWords(string seperator)
        {
            List<string> list = new List<string>();
            while (this.HasNext)
            {
                list.Add(this.NextWord(seperator));
            }
            return list.ToArray();
        }

        public void ConsumeSpace()
        {
            this.Consume(' ');
        }

        public void SkipWord()
        {
            this.SkipWord(" ");
        }

        public void SkipWord(string seperator)
        {
            this.SkipWords(1, seperator);
        }

        public void SkipWords(int count)
        {
            this.SkipWords(count, " ");
        }

        public void SkipWords(int count, string seperator)
        {
            this.NextWords(count, seperator);
        }

        public void Consume(string rs)
        {
            while (this.HasNext)
            {
                int i;
                for (i = 0; i < rs.Length; i++)
                {
                    if (this.m_str[this.m_pos + i] != rs[i])
                    {
                        return;
                    }
                }
                this.m_pos += i;
            }
        }

        public void Consume(char c)
        {
            while (this.HasNext && this.m_str[this.m_pos] == c)
            {
                this.m_pos++;
            }
        }

        public void Consume(char c, int amount)
        {
            int num = 0;
            while (num < amount && this.HasNext && this.m_str[this.m_pos] == c)
            {
                this.m_pos++;
                num++;
            }
        }

        public bool ConsumeNext(char c)
        {
            bool result;
            if (this.HasNext && this.m_str[this.m_pos] == c)
            {
                this.m_pos++;
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public bool ConsumeNext(string str)
        {
            bool result;
            if (this.Remainder.StartsWith(str))
            {
                this.m_pos += str.Length;
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public bool Contains(string s)
        {
            return s.IndexOf(s, this.m_pos) > -1;
        }

        public bool Contains(char c)
        {
            return this.m_str.IndexOf(c, this.m_pos) > -1;
        }

        public override string ToString()
        {
            return this.Remainder.Trim();
        }

        public StringStream CloneStream()
        {
            return this.Clone() as StringStream;
        }

        private int CustomIndexOf(string searchedChar, int startIndex, char containerChar)
        {
            if (searchedChar.Length > 1)
            {
                throw new ArgumentException("searchedChar can only be a char into a string");
            }
            char c = searchedChar[0];
            int num = startIndex;
            bool flag = false;
            do
            {
                if (this.m_str[num] == containerChar)
                {
                    flag = !flag;
                }
                num++;
                if (num >= this.m_str.Length)
                {
                    goto IL_74;
                }
            }
            while (flag || this.m_str[num] != c);
            int result = num;
            return result;
        IL_74:
            result = -1;
            return result;
        }
    }
}
