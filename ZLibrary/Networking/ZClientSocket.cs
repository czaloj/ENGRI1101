using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ZLibrary.Networking
{
    public class ZClientSocket : ZSocket
    {
        //Connection Information
        protected IPAddress remoteAddress;
        protected int remotePort;
        protected IPEndPoint remoteEndPoint;

        //What Is To Be Used For Sending / Receiving Data
        private byte[] sendData;
        private byte[] recvData;

        /// <summary>
        /// Creates A New Socket That Attempts To Connect To The Server
        /// </summary>
        /// <param name="remoteIP">The IPv4 Address Of The Server</param>
        /// <param name="remotePort">The Port Of The Server</param>
        /// <exception cref="ArgumentException">When Socket Was Unable To Connect To IP Address Or Port</exception>
        public ZClientSocket(IPAddress localAddress, int localPort)
        {
            bind(localAddress, localPort);
        }
        /// <summary>
        /// Creates A Client Socket From The Server Side
        /// </summary>
        /// <param name="s">The Socket Accepted</param>
        public ZClientSocket(Socket s, bool isRemote)
        {
            //Set The Accepted Socket
            socket = s;

            //Set IP End Points Based On If It Is A Remote Socket
            if (isRemote)
            {
                localEndPoint = (IPEndPoint)s.RemoteEndPoint;
                remoteEndPoint = (IPEndPoint)s.LocalEndPoint;
            }
            else
            {
                localEndPoint = (IPEndPoint)s.LocalEndPoint;
                remoteEndPoint = (IPEndPoint)s.RemoteEndPoint;
            }

            //Set Local Information Of Socket
            localAddress = localEndPoint.Address;
            localPort = localEndPoint.Port;

            //Set Remote Connection Information
            remoteAddress = remoteEndPoint.Address;
            remotePort = remoteEndPoint.Port;
        }

        /// <summary>
        /// Connects The Socket To A Remote IP End Point
        /// </summary>
        /// <param name="remoteAddress">The Remote IP Address</param>
        /// <param name="remotePort">The Remote Port</param>
        /// <returns>True If A Connection Was Made</returns>
        public bool connect(IPAddress remoteAddress, int remotePort)
        {
            //Set Up Server Information
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;
            remoteEndPoint = new IPEndPoint(remoteAddress, remotePort);

            //Attempt A Connection
            try
            {
                socket.Connect(remoteEndPoint);
            }
            catch (Exception) { return false; }

            //Return The Connectivity
            if (socket.Connected)
            {
                return true;
            }
            return false;
        }
    }
}
