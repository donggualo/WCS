using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace WCS_phase1
{
    public class IniFiles
    {
        public string inipath;
        ASCIIEncoding ascii;
        //声明API函数

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, Byte[] retVal, int size, string filePath);
        /// <summary> 
        /// 构造方法 
        /// </summary> 
        /// <param name="INIPath">文件路径</param> 
        public IniFiles(string INIPath)
        {
            inipath = INIPath;
            ascii = new ASCIIEncoding();
        }

        public IniFiles() { }

        /// <summary> 
        /// 写入INI文件 
        /// </summary> 
        /// <param name="Section">项目名称(如 [TypeName] )</param> 
        /// <param name="Key">键</param> 
        /// <param name="Value">值</param> 
        public void WriteValue(string Section, string Key, object Value)
        {
            if (Value.GetType().Equals(0.GetType()))
            {
                Value = "" + Value;
            }else if (Value.GetType().Equals(true.GetType()))
            {
                Value = Value.ToString();
            }
            WritePrivateProfileString(Section, Key, (string)Value, this.inipath);
        }
        /// <summary> 
        /// 读出INI文件 
        /// </summary> 
        /// <param name="Section">项目名称(如 [TypeName] )</param> 
        /// <param name="Key">键</param> 
        public byte[] ReadValue(string Section, string Key)
        {
            byte[] temp = new byte[255];

            int i = GetPrivateProfileString(Section, Key, "", temp, 1024, this.inipath);
            return temp;
        }

        public string ReadStrValue(string Section, string Key)
        {
            return ascii.GetString(ReadValue(Section, Key)).Replace("\0", "");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ReadBool(string section,string key)
        {
            string value = ascii.GetString(ReadValue(section, key));
            return value.Equals("True") ? true : false;
        }

        public Dictionary<string,string> ReadAllValue(string section)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            string str = ascii.GetString(ReadValue(section, null)).Replace("\0\0","");
            string[] list = str.Split(new char[1] { '\0' });

            foreach(string s in list)
            {
                dic.Add(s, ReadStrValue(section, s));
            }

            return dic;
        }


        /// <summary> 
        /// 验证文件是否存在 
        /// </summary> 
        /// <returns>布尔值</returns> 
        public bool ExistINIFile()
        {
            return File.Exists(inipath);
        }
    }
}
