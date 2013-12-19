using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ZLibrary.Networking
{
    public class ZServerSocket : ZSocket
    {
        /// <summary>
        /// Creates A New Server Socket
        /// </summary>
        /// <param name="serverIP">The IPv4 Address Of The Server</param>
        /// <param name="serverPort">The Port Of The Server</param>
        public ZServerSocket(IPAddress localAddress, int localPort)
        {
            bind(localAddress, localPort);
        }

        /// <summary>
        /// Starts The Server In A Listening State
        /// </summary>
        /// <param name="backLog">Max Number Of Backlogged Connections</param>
        public void beginListening(int backLog = 128)
        {
            socket.Listen(backLog);
        }
        /// <summary>
        /// Accepts A New Connection
        /// </summary>
        /// <returns>The Client That Connected</returns>
        public ZClientSocket acceptConnection()
        {
            if (socket.Poll(ZProtocol.ACCEPT_TIME_OUT, SelectMode.SelectRead))
            {
                return new ZClientSocket(socket.Accept(), true);
            }
            return null;
        }
    }
}
