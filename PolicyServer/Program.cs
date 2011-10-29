using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace PolicyServer {
    class Program {
        //socket policy docs
        //http://www.adobe.com/devnet/flashplayer/articles/socket_policy_files.html

        static string xml = "<?xml version=\"1.0\"?>" +
                "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">" +
                "<cross-domain-policy>" +
                "<site-control permitted-cross-domain-policies=\"master-only\"/>" +
                "<allow-access-from domain=\"*\" to-ports=\"*\" />" +
                "</cross-domain-policy>";

        static void Main(string[] args) {
            TcpListener listener = new TcpListener(IPAddress.Any, 843);
            
            listener.Start();

            byte[] buffer = Encoding.ASCII.GetBytes(xml);

            while (true)
            {
                try {
                    TcpClient client = listener.AcceptTcpClient();                    
                    Thread.Sleep(500);
                    client.GetStream().Write(buffer, 0, buffer.Length);
                }
                catch (Exception) { }
            }
        }
    }
}
