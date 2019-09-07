using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolManager
{
    /// <summary>
    /// 日志保存
    /// 位置：log/20xx-xx-xx/agvTask0x.txt
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Keep track on trace file
        /// </summary>
        private string traceName;

        /// <summary>
        /// To count up trace number text
        /// </summary>
        private int countUpTraceNumber = 0;

        /// <summary>
        /// How many trace files to keep
        /// </summary>
        private int maxNumberOfTracefiles = 999;

        /// <summary>
        /// How many lines in one trace file
        /// </summary>
        private int lineCountNumber = 999;

        /// <summary>
        /// Used for trace file
        /// </summary>
        private List<FileInfo> fileList = new List<FileInfo>();

        /// <summary>
        /// Name of trace log
        /// </summary>
        private string TraceLogName = "";

        /// <summary>
        /// Path of trace log file
        /// </summary>
        private string TraceLogFilePath = "log";

        /// <summary>
        /// Folder of trace log file
        /// </summary>
        private string TraceLogFolder = "";

        private bool appendFile = true;

        public Log(string tracelogname)
        {
            TraceLogName = tracelogname;

            UpdateTraceFileName();

            SetupTraceFile();

            WriteToLogFile("LOG START");
        }

        #region Log

        /// <summary>
        /// Add information to loggfile.
        /// </summary>
        /// <param name="text"></param>
        public void LOG(string text)
        {
            Console.WriteLine(text);
            CheckLogFile();
            WriteToLogFile(text);

        }

        /// <summary>
        /// Update trace file folder
        /// </summary>
        private void UpdateTraceFileName()
        {
            TraceLogFolder = DateTime.Now.ToString("yyyy-MM-dd");

            traceName = TraceLogFilePath + "/" + TraceLogFolder + "/" + TraceLogName + countUpTraceNumber + ".txt";
        }

        /// <summary>
        /// Config the tracefiles at startup
        /// </summary>
        private void SetupTraceFile()
        {
            if (!Directory.Exists(TraceLogFilePath + "/" + TraceLogFolder))
            {
                Directory.CreateDirectory(TraceLogFilePath + "/" + TraceLogFolder);
            }
            while (File.Exists(traceName))
            {
                if (fileList.Count == 0)
                    fileList.Add(new FileInfo(traceName));
                else if (countUpTraceNumber != maxNumberOfTracefiles)
                {
                    countUpTraceNumber++;
                    UpdateTraceFileName();//traceName = TraceLogName + countUpTraceNumber + ".txt";
                    fileList.Add(new FileInfo(traceName));
                }
                else
                {
                    countUpTraceNumber = 0;

                    for (int i = 0; i < fileList.Count; i++)
                    {
                        if (i == fileList.Count - 1)
                        {
                            countUpTraceNumber = i;
                            UpdateTraceFileName();// traceName = TraceLogName + countUpTraceNumber + ".txt";
                            File.Delete(traceName);
                            break;
                        }
                        else if (fileList[i].LastWriteTime > fileList[i + 1].LastWriteTime)
                        {
                            countUpTraceNumber = i + 1;
                            UpdateTraceFileName(); //traceName = TraceLogName + countUpTraceNumber + ".txt";
                            File.Delete(traceName);
                            break;
                        }
                        else if (i == 0)
                        {
                            countUpTraceNumber = i;
                            UpdateTraceFileName();// traceName = TraceLogName + countUpTraceNumber + ".txt";
                            File.Delete(traceName);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查文件日志行数，确定是否需要新增日志文件
        /// </summary>
        void CheckLogFile()
        {
            UpdateTraceFileName();
            int lineCount = File.ReadLines(traceName).Count();
            appendFile = true;
            if (lineCount > lineCountNumber)
            {
                if (countUpTraceNumber != maxNumberOfTracefiles)
                    countUpTraceNumber++;
                else
                    countUpTraceNumber = 0;

                UpdateTraceFileName();//traceName = TraceLogName + countUpTraceNumber + ".txt";
                appendFile = false;
            }

        }


        /// <summary>
        /// 写日志到日志文件
        /// </summary>
        /// <param name="text"></param>
        void WriteToLogFile(string text)
        {
            try
            {

                //Write to current tracefile
                using (StreamWriter write = new StreamWriter(traceName, appendFile))
                {
                    write.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + text);
                }
                appendFile = true;
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }


}
