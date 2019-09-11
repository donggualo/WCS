// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using log4net;
using MHttpServer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MHttpServer
{

    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener _mListener;
        private HttpProcessor _mProcessor;
        private bool RunningServer = true;

        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(HttpServer));

        #region Public Methods

        /// <summary>
        /// 服务构造函数
        /// </summary>
        /// <param name="port">服务端口</param>
        /// <param name="routes">处理路由</param>
        public HttpServer(int port, List<Route> routes)
        {
            Port = port;
            _mProcessor = new HttpProcessor();

            foreach (var route in routes)
            {
                _mProcessor.AddRoute(route);
            }
        }

        /// <summary>
        /// 启动任务监听
        /// </summary>
        public void Listen()
        {
            try
            {
                _mListener = new TcpListener(IPAddress.Any, this.Port);
                _mListener.Start();
                while (RunningServer)
                {
                    TcpClient s = _mListener.AcceptTcpClient();
                    new Thread(() =>
                    {
                        _mProcessor.HandleClient(s);
                    }).Start();
                    Thread.Sleep(1);
                }
            }catch(Exception e)
            {
                log.Error(e);
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Close()
        {
            RunningServer = false;
        }

        #endregion

    }
}



