using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ZLibrary.Networking
{
    public abstract class ZSocket
    {
        //The Socket
        public Socket socket = null;

        //Binding (Local) Information
        protected IPAddress localAddress;
        protected int localPort;
        protected IPEndPoint localEndPoint;

        //What Is To Be Used For Sending / Receiving Data
        public byte[] sendBuffer = new byte[1];
        public byte[] recvBuffer = new byte[1];

        public bool bind(IPAddress localAddress, int localPort)
        {
            //Set Up Binding Information
            this.localAddress = localAddress;
            this.localPort = localPort;
            localEndPoint = new IPEndPoint(localAddress, localPort);

            //Bind Socket
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(localEndPoint);
            }
            catch (Exception) { return false; }
            return true;
        }

        //Getter Methods
        public IPAddress LocalAddress{ get {return localAddress;} }
        public int LocalPort { get { return localPort; } }
    }
}
