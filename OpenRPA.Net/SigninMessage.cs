﻿using OpenRPA.Interfaces.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace OpenRPA.Net
{
    public class SigninMessage : SocketCommand
    {
        public SigninMessage() : base()
        {
            msg.command = "signin";
        }
        public SigninMessage(string username, SecureString password, string clientversion) : base()
        {
            msg.command = "signin";
            this.clientversion = clientversion;
            this.username = username;
            this.password = new System.Net.NetworkCredential(string.Empty, password).Password;
        }
        public SigninMessage(string jwt, string clientversion, string request_token_key="") : base()
        {
            msg.command = "signin";
            this.clientversion = clientversion;
            this.jwt = jwt;
            this.request_token_key = request_token_key;
        }
        public SigninMessage(SecureString jwt, string clientversion, string request_token_key = "") : base()
        {
            msg.command = "signin";
            this.clientversion = clientversion;
            this.jwt = new System.Net.NetworkCredential(string.Empty, jwt).Password;
            this.request_token_key = request_token_key;
        }

        // 获取token时生成的key，发送到uniplore后、可以在前端显示机器人连接状态
        public string request_token_key { get; set; }

        public bool validate_only { get; set; }
        public bool longtoken { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public TokenUser user { get; set; }
        public int websocket_package_size { get; set; }
        public string clientagent { get; set; } = "openrpa";
        public string clientversion { get; set; } = "0.0.0.1";
        public string openflow_uniqueid { get; set; } = "";
        public bool enable_analytics { get; set; } = true;
        public string otel_trace_url { get; set; } = "";
        public string otel_metric_url { get; set; } = "";
        public int otel_trace_interval { get; set; } = 0;
        public int otel_metric_interval { get; set; } = 0;
    }
}
