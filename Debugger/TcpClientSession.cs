using System;
using System.Collections.Generic;
using System.Text;
using Game.Comm;
using System.Net.Sockets;
using System.Threading;
namespace Debugger {
   public class TcpClientSession : Session {
        TcpClient tcpclient;
        NetworkStream ns;
        Thread receiver;

        public delegate void OnSend(Packet packet);
        public event OnSend SendEvent;
        public delegate void OnReceive(Packet packet);
        public event OnReceive ReceivedEvent;


        public TcpClientSession(string _name,Processor _processor)
            : base(_name, _processor) {
        }

        public bool connect(string address, int port) {
            if (tcpclient != null) return false;
            tcpclient = new TcpClient();
            try {
                tcpclient.Connect(address, port);
            } catch (SocketException socketException) {
                Console.Out.WriteLine(socketException.ToString());
                tcpclient = null;
                return false;
            }
            receiver = new Thread(new ThreadStart(Receiver));
            ns = tcpclient.GetStream(); 
            receiver.Start();
            return true;
        }

        public void disconnect() {
            if (tcpclient == null) return;
            ns.Close();
            

            ns = null; 
            try {
                receiver.Abort();
            }
            catch (Exception e) {
                Console.Out.WriteLine(e.ToString());
            }
            receiver = null;

            tcpclient.Close();
            tcpclient = null;
        }

        public override bool write(Packet packet) {
            if (tcpclient == null) return false;
            if (SendEvent != null) {
                SendEvent(packet);
            }
            byte[] packetBytes = packet.getBytes();
            try {
                ns.Write(packetBytes, 0, packetBytes.Length);
            } catch (Exception e) {
                Console.Out.WriteLine(e.ToString());
                disconnect();
                return false;
            }
            return true;
        }

        private void Receiver() {
            byte[] buffer = new byte[1024];
            byte[] bytes;
            Packet p;
            int len;
   //         ns.ReadTimeout = 5000;
            try {
                while (true) {
                    len = ns.Read(buffer, 0, 1024);
                    if (len > 0) {
                        bytes = new byte[len];
                        Array.Copy(buffer, bytes, len);
                        this.appendBytes(bytes);
                        while ((p = this.getNextPacket()) != null) {
                            if (ReceivedEvent != null) {
                                ReceivedEvent(p);
                            }
                            this.process(p);
                        }
                    }
                }
            }
            catch (ThreadAbortException exception) {
                Console.Out.WriteLine(exception.ToString());
                return;
            }
            catch (System.IO.IOException ioexception) {
                Console.Out.WriteLine(ioexception.ToString());
                return;
            }
        }


        public override void close() {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
