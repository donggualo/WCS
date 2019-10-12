﻿// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using log4net;
using NLog;
using MHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MHttpServer
{

    /// <summary>
    /// Http 处理类
    /// </summary>
    public class HttpProcessor
    {

        #region Fields

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        private List<Route> Routes = new List<Route>();

        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(HttpProcessor));

        #endregion

        #region Constructors

        public HttpProcessor()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="tcpClient"></param>
        public void HandleClient(TcpClient tcpClient)
        {
                Stream inputStream = GetInputStream(tcpClient);
                Stream outputStream = GetOutputStream(tcpClient);
                HttpRequest request = GetRequest(inputStream, outputStream);

                // route and handle the request...
                HttpResponse response = RouteRequest(inputStream, outputStream, request);

                string msg = string.Format("{0} {1}", response.StatusCode, request.Url);
                string data = request.Content;
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + msg + (data!=null ? data.Replace(" ","").Replace("\n",""):""));
                log.Info(msg);
                // build a default response for errors
                if (response.Content == null) {
                    if (response.StatusCode != "200") {
                        response.ContentAsUTF8 = string.Format("{0} {1} <p> {2}", response.StatusCode, request.Url, response.ReasonPhrase);
                    }
                }

                //WriteResponse(outputStream, response);
                WriteResponse(tcpClient, response);

                //outputStream.Flush();
                //outputStream.Close();
                //outputStream = null;

                //inputStream.Close();
                inputStream = null;
                //Console.WriteLine("请求结束：" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                //log.Info("请求结束：" + DateTime.Now.ToString("yyyyMMddHHmmss"));
        }



        private void WriteResponse(TcpClient client, HttpResponse response)
        {
            if (response.Content == null)
            {
                response.Content = new byte[] { };
            }

            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            builder.Append(string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            builder.Append("\r\n\r\n");


            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            byte[] content = new byte[bytes.Length + response.Content.Length];

            bytes.CopyTo(content, 0);
            response.Content.CopyTo(content, bytes.Length);

            client.GetStream().BeginWrite(
              content, 0, content.Length, HandleDatagramWritten, client);
        }

        /// <summary>
        /// this formats the HTTP response...
        /// 格式化HTTP回应包并输出
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="response"></param>
        private static void WriteResponse(Stream stream, HttpResponse response) {            
            if (response.Content == null) {           
                response.Content = new byte[]{};
            }
            
            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type")) {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            Write(stream, string.Format("HTTP/1.0 {0} {1}\r\n",response.StatusCode,response.ReasonPhrase));
            Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            Write(stream, "\r\n\r\n");

            stream.Write(response.Content, 0, response.Content.Length);       
        }

        /// <summary>
        /// 添加路由
        /// </summary>
        /// <param name="route"></param>
        public void AddRoute(Route route)
        {
            this.Routes.Add(route);
        }


        private void HandleDatagramWritten(IAsyncResult ar)
        {

            ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
            ((TcpClient)ar.AsyncState).GetStream().Flush();
            Thread.Sleep(1000);
            ((TcpClient)ar.AsyncState).GetStream().Close();
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static string Readline(Stream stream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="text"></param>
        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 获取用于发送的数据流
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        /// <summary>
        /// 获取用于接收的数据流
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }


        /// <summary>
        /// 解析请求并查找对应的路由并返回结果
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual HttpResponse RouteRequest(Stream inputStream, Stream outputStream, HttpRequest request)
        {

            List<Route> routes = this.Routes.Where(x => Regex.Match(request.Url, x.UrlRegex).Success).ToList();

            if (!routes.Any())
                return HttpBuilder.NotFound();

            Route route = routes.SingleOrDefault(x => x.Method == request.Method);

            if (route == null)
                return new HttpResponse()
                {
                    ReasonPhrase = "Method Not Allowed",
                    StatusCode = "405",

                };

            // extract the path if there is one
            var match = Regex.Match(request.Url,route.UrlRegex);
            if (match.Groups.Count > 1) {
                request.Path = match.Groups[1].Value;
            } else {
                request.Path = request.Url;
            }

            // trigger the route handler...
            request.Route = route;
            try {
                return route.Callable(request);
            } catch(Exception ex) {
                log.Error(ex);
                return HttpBuilder.InternalServerError();
            }

        }



        private HttpRequest GetRequest(Stream inputStream, Stream outputStream)
        {
            //Read Request Line
            string request = Readline(inputStream);

            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = Readline(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            string content = null;
            if (headers.ContainsKey("Content-Length"))
            {
                int totalBytes = Convert.ToInt32(headers["Content-Length"]);
                int bytesLeft = totalBytes;
                byte[] bytes = new byte[totalBytes];
               
                while(bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > 1024? 1024 : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);
                    buffer.CopyTo(bytes, totalBytes - bytesLeft);

                    bytesLeft -= n;
                }

                content = Encoding.ASCII.GetString(bytes);
            }


            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                Content = content
            };
        }

        #endregion


    }
}
