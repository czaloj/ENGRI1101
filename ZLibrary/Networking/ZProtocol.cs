using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ZLibrary.Networking
{
    public struct Arrow
    {
        //Number Of Bytes Sent In Arrow
        public const int ByteLength = 26;
        //Verifying System For Arrow Consistency
        public const byte Verifier = 0xab;

        //The ID Of The Incoming Chunk
        public long ID;

        //Full Length Of Message And Chunk Breakdown
        public long MessageLength;
        public int ChunkLength;
        public int ChunkNumber;

        //The Bytes Of The Arrow To Be Sent
        public byte[] Bytes;

        public Arrow(long id, long length, int chunkLength)
        {
            ID = id;
            MessageLength = length;
            ChunkLength = chunkLength;
            ChunkNumber = (int)(MessageLength / ChunkLength);

            Bytes = new byte[ByteLength];
            Bytes[0] = Verifier;
            Array.Copy(BitConverter.GetBytes(ID), 0, Bytes, 1, 8);
            Array.Copy(BitConverter.GetBytes(MessageLength), 0, Bytes, 9, 8);
            Array.Copy(BitConverter.GetBytes(ChunkLength), 0, Bytes, 17, 4);
            Array.Copy(BitConverter.GetBytes(ChunkNumber), 0, Bytes, 21, 4);
            Bytes[ByteLength - 1] = Verifier;
        }
        public Arrow(byte[] bytes)
        {
            Bytes = bytes;
            ID = BitConverter.ToInt64(Bytes, 1);
            MessageLength = BitConverter.ToInt64(Bytes, 9);
            ChunkLength = BitConverter.ToInt32(Bytes, 17);
            ChunkNumber = BitConverter.ToInt32(Bytes, 21);
        }

        public void toBytes(byte[] buffer, int offset)
        {
            Array.Copy(Bytes, 0, buffer, offset, ByteLength);
        }
    }

    public delegate void OnMessageProcess(ZMessage message);

    public class ZMessage
    {
        //The Arrow Giving Message Information
        public Arrow ArrowMutual;

        //Overall Amount Of Information Left To Be Read
        protected long dataRemaining;
        public long DataRemaining
        {
            get
            {
                return dataRemaining;
            }
        }

        //Hold Each Chunk Worth Of Data
        public byte[] Data;
        protected int chunksRemaining;
        public int ChunksToProcess
        {
            get
            {
                return chunksRemaining;
            }
        }
        protected int chunkIndex;

        //Fired When A Full Chunk Is Read
        public OnMessageProcess OnChunkReceivedEvent;

        //Fired When A Full Message Is Processed
        public OnMessageProcess OnMessageCompletion;

        public ZMessage(Arrow a)
        {
            //Keep State Of The Arrow
            ArrowMutual = a;

            //Set Up Chunk Reading Information
            dataRemaining = ArrowMutual.MessageLength;
            chunksRemaining = ArrowMutual.ChunkNumber;

            Data = new byte[ArrowMutual.ChunkLength];
        }
        public ZMessage(byte[] data, int off, Arrow a)
        {
            //Keep State Of The Arrow
            ArrowMutual = a;

            //Copy Data To Be Sent
            Data = new byte[a.MessageLength];
            Array.Copy(data, off, Data, 0, a.MessageLength);
            resetSending();
        }

        public void resetReading()
        {
            dataRemaining = ArrowMutual.MessageLength;
            chunksRemaining = ArrowMutual.ChunkNumber;
        }
        public void readMessage(ZSocket socket, out SocketError error)
        {
            do
            {
                readData(socket, out error);
            }
            while ((error & SocketError.Success) == SocketError.Success && dataRemaining > 0);
            if (OnMessageCompletion != null) { OnMessageCompletion(this); }
        }
        public void readData(ZSocket socket, out SocketError error)
        {
            dataRemaining -= socket.socket.Receive(Data, 0, ArrowMutual.ChunkLength, SocketFlags.None, out error);
            if ((error & SocketError.Success) == SocketError.Success)
            {
                chunksRemaining--;
                if (OnChunkReceivedEvent != null) { OnChunkReceivedEvent(this); }
            }
        }

        public void resetSending()
        {
            dataRemaining = ArrowMutual.MessageLength;
            chunksRemaining = ArrowMutual.ChunkNumber;
            chunkIndex = 0;
        }
        public void sendMessage(ZSocket socket, out SocketError error)
        {
            do
            {
                sendData(socket, out error);
            }
            while ((error & SocketError.Success) == SocketError.Success && dataRemaining > 0);
            if (OnMessageCompletion != null) { OnMessageCompletion(this); }
        }
        public void sendData(ZSocket socket, out SocketError error)
        {
            dataRemaining -= socket.socket.Send(Data, chunkIndex,
                ((ArrowMutual.ChunkLength > dataRemaining) ? (int)dataRemaining : ArrowMutual.ChunkLength),
                SocketFlags.None, out error);

            if ((error & SocketError.Success) == SocketError.Success)
            {
                chunksRemaining--;
                chunkIndex += ArrowMutual.ChunkLength;
            }
        }
    }


    public enum ZP_SIGNAL : byte
    {
        PING = 0x00,
        DESIRE_CONNECTION = 0x01,
        QUIT = 0x10,
        NO_SIGNAL = 0xff
    };

    public enum ZP_DATA_TYPE : int
    {
        STRING = 0,
        BYTE_ARRAY = 1,
        SIGNAL = 2,
        FILE = 100,
        FILE_INFO = 110
    };

    public enum ZP_INTEGRITY : int
    {
        COMPLETE = 40000,
        INCOMPLETE = 40001
    }

    /// <summary>
    /// Controls How Data Is Transmitted Through ZSockets
    /// </summary>
    public static class ZProtocol
    {
        public static bool shootArrow(ZSocket socket, Arrow a, out SocketError error)
        {
            socket.socket.Send(a.Bytes, 0, Arrow.ByteLength, SocketFlags.None, out error);
            if ((error & SocketError.Success) == SocketError.Success)
            {
                return true;
            }
            return false;
        }
        public static bool catchArrow(ZSocket socket, out Arrow a, out SocketError error)
        {
            byte[] b = new byte[Arrow.ByteLength];
            socket.socket.Receive(b, 0, Arrow.ByteLength, SocketFlags.None, out error);

            if ((error & SocketError.Success) == SocketError.Success)
            {
                if (b[0] == Arrow.Verifier && b[Arrow.ByteLength - 1] == Arrow.Verifier)
                {
                    a = new Arrow(b);
                    return true;
                }
            }
            a = new Arrow();
            return false;
        }

        /// Protocol Specifications:
        /// 
        /// The Maximum Size Of A Packet Is 1024 Bytes
        /// 
        /// Start At Beginning Of Packet:
        /// 4 Bytes - Data Type Information
        /// 8 Bytes - Packet Length
        /// 1000 Bytes - Data
        /// 8 Bytes - Packet ID
        /// 4 Bytes - Packet Complete Flag
        /// 
        /// A Signal Packet Is 16 Bytes In Length:
        /// 
        /// Start At Beginning Of Packet:
        /// 4 Bytes - Signal Data Type
        /// 4 Bytes - Signal Type
        /// 8 Bytes - Signal ID
        /// 4 Bytes - Signal Complete Flag
        /// 
        /// A File Packet Is 8024 Bytes In Length:
        /// 
        /// Start At Beginning Of Packet:
        /// 4 Bytes - Data Type
        /// 8 Bytes - Data Length In Current Packet
        /// 8000 Bytes - Signal Type
        /// 8 Bytes - Packet ID
        /// 4 Bytes - Packet Complete Flag

        public const int HEADER_LENGTH = 21;
        public const int STAMP_LENGTH = 4;

        private static int[] PACKET_LENGTH = new int[] { 1, 250, 1000, 4000, 8000 };
        private static int[] PACKET_DATA_LENGTH = new int[] { 1, 250, 1000, 4000, 8000 };

        private static byte[] INTEGRITY_STAMP = new byte[4];

        public const int SEND_SLEEP_WAIT = 5;
        public const int RECV_SLEEP_WAIT = 5;
        public const int ACCEPT_TIME_OUT = 20;

        /// <summary>
        /// Static Initializer
        /// </summary>
        static ZProtocol()
        {
            for (int i = 0; i < PACKET_LENGTH.Length; i++)
            {
                PACKET_LENGTH[i] += HEADER_LENGTH + STAMP_LENGTH;
            }
            INTEGRITY_STAMP = BitConverter.GetBytes((int)ZP_INTEGRITY.COMPLETE);
        }

        /// <summary>
        /// Writes A Stamp Signifying That The Packet Is Complete
        /// </summary>
        /// <param name="buffer">Current Packet</param>
        public static void stampIntegrity(byte[] buffer)
        {
            byte[] b = BitConverter.GetBytes((int)ZP_INTEGRITY.COMPLETE);
            Array.Copy(INTEGRITY_STAMP, 0, buffer, buffer.Length - 4, 4);
        }
        /// <summary>
        /// Reads A Stamp Signifying If A Packet Is Complete
        /// </summary>
        /// <param name="buffer">Current Packet</param>
        /// <returns>Verification Stamp</returns>
        public static ZP_INTEGRITY readIntegrity(byte[] buffer)
        {
            return (ZP_INTEGRITY)BitConverter.ToInt32(buffer, buffer.Length - 4);
        }

        public static ZP_SIGNAL readSignal(byte[] buffer)
        {
            return (ZP_SIGNAL)buffer[HEADER_LENGTH];
        }

        /// <summary>
        /// Calculates The Number Of Packets Needed To Send A Message
        /// </summary>
        /// <param name="dataLength">The Length Of All The Data Bytes</param>
        /// <returns>Number Of Packets Needed To Transmit Message</returns>
        public static long numberOfPacketsNeeded(long dataLength, int packetDataSize)
        {
            long n = dataLength / packetDataSize;
            if (dataLength % packetDataSize > 0) { n++; }
            return n;
        }

        #region Sending
        public static bool sendFullPacket(ZSocket socket, byte[] buffer)
        {
            int packetLength = buffer.Length;
            int bytesSent = 0;
            while (bytesSent < packetLength)
            {
                try
                {
                    bytesSent += socket.socket.Send(buffer, bytesSent, packetLength - bytesSent, SocketFlags.None);
                    if (bytesSent < packetLength)
                    {
                        Thread.Sleep(SEND_SLEEP_WAIT);
                    }
                }
                catch (SocketException)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool sendData(ZSocket socket, byte[] data, int dataType, ZPacketID id, ZP_PACKET_SIZE packetSize = ZP_PACKET_SIZE.MEDIUM)
        {
            lock (socket.sendBuffer)
            {
                //Make The Header
                ZPUH header = new ZPUH(packetSize, dataType, id.nextID(), data.LongLength);
                sendHeader(socket, header, id);

                //Create The Send Buffer
                int packetLength = PACKET_LENGTH[(int)packetSize];
                int packetDataLength = PACKET_DATA_LENGTH[(int)packetSize];
                socket.sendBuffer = new byte[packetLength];

                //Indexer In The Data Array
                long index = 0;

                //Send Each Packet
                do
                {
                    #region Write To The Send Buffer
                    //Write The Header
                    header.writeHeader(socket.sendBuffer);

                    //Write The Data
                    if (header.dataLength < packetLength)
                    {
                        Array.Copy(data, index, socket.sendBuffer, HEADER_LENGTH, header.dataLength);
                        index += header.dataLength;
                        header.dataLength = 0;
                    }
                    else
                    {
                        Array.Copy(data, index, socket.sendBuffer, HEADER_LENGTH, packetDataLength);
                        index += packetDataLength;
                        header.dataLength -= packetDataLength;
                    }

                    //Stamp For The Integrity
                    stampIntegrity(socket.sendBuffer);
                    #endregion

                    //Make Sure Packet Is Fully Sent
                    while (!sendFullPacket(socket, socket.sendBuffer)) { Thread.Sleep(SEND_SLEEP_WAIT); }
                }
                while (header.dataLength > packetLength);
            }
            return true;
        }
        public static bool sendData(ZSocket socket, String s, int dataType, ZPacketID id)
        {
            return sendData(socket, Encoding.ASCII.GetBytes(s), dataType, id);
        }
        public static bool sendFileInfo(ZSocket socket, ZPFileInfo fileInfo, ZPacketID id)
        {
            lock (socket.sendBuffer)
            {
                //Create The Send Buffer
                int packetLength = PACKET_LENGTH[(int)ZP_PACKET_SIZE.MEDIUM];
                socket.sendBuffer = new byte[packetLength];

                //Make The Header
                ZPUH header = new ZPUH(ZP_PACKET_SIZE.MEDIUM, (int)ZP_DATA_TYPE.FILE_INFO, id.nextID(), PACKET_DATA_LENGTH[(int)ZP_PACKET_SIZE.MEDIUM]);
                sendHeader(socket, header, id);

                #region Write To The Send Buffer
                //Write The Header
                header.writeHeader(socket.sendBuffer);

                //Write The Data
                fileInfo.writeToPacket(socket.sendBuffer);

                //Stamp For The Integrity
                stampIntegrity(socket.sendBuffer);
                #endregion

                //Make Sure Packet Is Fully Sent
                while (!sendFullPacket(socket, socket.sendBuffer)) { Thread.Sleep(SEND_SLEEP_WAIT); }
            }
            return true;
        }
        public static bool sendStream(ZSocket socket, Stream fs, int dataType, ZPacketID id)
        {
            lock (socket.sendBuffer)
            {
                //Create The Send Buffer
                int packetLength = PACKET_LENGTH[(int)ZP_PACKET_SIZE.FILE];
                socket.sendBuffer = new byte[packetLength];

                //Indexer In The Data Array
                long index = 0;

                //Make The Header
                ZPUH header = new ZPUH(ZP_PACKET_SIZE.FILE, dataType, id.nextID(), fs.Length);
                sendHeader(socket, header, id);

                //Send Each Packet
                do
                {
                    #region Write To The Send Buffer
                    //Write The Header
                    header.writeHeader(socket.sendBuffer);

                    //Write The Data
                    if (header.dataLength < packetLength)
                    {
                        fs.Read(socket.sendBuffer, HEADER_LENGTH, (int)header.dataLength);
                        index += header.dataLength;
                        header.dataLength = 0;
                    }
                    else
                    {
                        fs.Read(socket.sendBuffer, HEADER_LENGTH, packetLength);
                        index += packetLength;
                        header.dataLength -= packetLength;
                    }

                    //Stamp For The Integrity
                    stampIntegrity(socket.sendBuffer);
                    #endregion

                    //Make Sure Packet Is Fully Sent
                    while (!sendFullPacket(socket, socket.sendBuffer)) { Thread.Sleep(SEND_SLEEP_WAIT); }
                }
                while (header.dataLength > packetLength);
            }
            return true;
        }
        public static bool sendSignal(ZSocket socket, ZP_SIGNAL signal, ZPacketID id)
        {
            lock (socket.sendBuffer)
            {
                //Create The Send Buffer
                int packetLength = PACKET_LENGTH[(int)ZP_PACKET_SIZE.SIGNAL];
                socket.sendBuffer = new byte[packetLength];

                //Make The Header
                ZPUH header = new ZPUH(ZP_PACKET_SIZE.SIGNAL, (int)ZP_DATA_TYPE.SIGNAL, id.nextID(), PACKET_DATA_LENGTH[(int)ZP_PACKET_SIZE.SIGNAL]);

                #region Write To The Send Buffer
                //Write The Header
                header.writeHeader(socket.sendBuffer);

                //Write The Data
                socket.sendBuffer[HEADER_LENGTH] = (byte)signal;

                //Stamp For The Integrity
                stampIntegrity(socket.sendBuffer);
                #endregion

                //Make Sure Packet Is Fully Sent
                while (!sendFullPacket(socket, socket.sendBuffer)) { Thread.Sleep(SEND_SLEEP_WAIT); }
            }
            return true;
        }
        public static bool sendHeader(ZSocket socket, ZPUH header, ZPacketID id)
        {
            //Create The Send Buffer
            int packetLength = PACKET_LENGTH[(int)ZP_PACKET_SIZE.SIGNAL];
            socket.sendBuffer = new byte[packetLength];

            #region Write To The Send Buffer
            //Write The Header
            header.writeHeader(socket.sendBuffer);

            //Tell That It Is Not A Signal
            socket.sendBuffer[HEADER_LENGTH] = (byte)ZP_SIGNAL.NO_SIGNAL;

            //Stamp For The Integrity
            stampIntegrity(socket.sendBuffer);
            #endregion

            //Make Sure Packet Is Fully Sent
            while (!sendFullPacket(socket, socket.sendBuffer)) { Thread.Sleep(SEND_SLEEP_WAIT); }
            return true;
        }
        #endregion

        #region Receiving
        public static ZPUH recvFullHeader(ZSocket socket)
        {
            socket.recvBuffer = new byte[(int)PACKET_LENGTH[(byte)ZP_PACKET_SIZE.SIGNAL]];
            int bytesReceived = 0;
            while (bytesReceived < socket.recvBuffer.Length)
            {
                if (socket.socket.Available > 0)
                {
                    try
                    {
                        bytesReceived += socket.socket.Receive(socket.recvBuffer, bytesReceived, socket.recvBuffer.Length - bytesReceived, SocketFlags.None);
                        if (bytesReceived == socket.recvBuffer.Length)
                        {
                            return ZPUH.readHeader(socket.recvBuffer);
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.Out.WriteLine(e.Message);
                        return ZPUH.BAD_HEADER;
                    }
                }
                else
                {
                    Thread.Sleep(RECV_SLEEP_WAIT);
                }
            }
            return ZPUH.BAD_HEADER;
        }
        public static bool recvFullPacket(ZSocket socket, byte[] buffer, int bytesReceived = 0)
        {
            int packetLength = buffer.Length;
            while (bytesReceived < packetLength)
            {
                if (socket.socket.Available > 0)
                {
                    try
                    {
                        bytesReceived += socket.socket.Receive(buffer, bytesReceived, packetLength - bytesReceived, SocketFlags.None);
                    }
                    catch (SocketException e)
                    {
                        Console.Out.WriteLine(e.Message);
                        return false;
                    }
                }
                else
                {
                    Thread.Sleep(RECV_SLEEP_WAIT);
                }
            }
            return true;
        }

        public static bool recvData(ZSocket socket, out byte[] data, out ZPUH outHeader)
        {
            lock (socket.recvBuffer)
            {
                //Receive The First Header
                ZPUH header = recvFullHeader(socket);
                while (ZPUH.isBadHeader(header))
                {
                    header = recvFullHeader(socket);
                    Thread.Sleep(RECV_SLEEP_WAIT);
                }

                //It Was Actually A Signal, Not A Pre-Header
                if (readSignal(socket.recvBuffer) != ZP_SIGNAL.NO_SIGNAL && readIntegrity(socket.recvBuffer) == ZP_INTEGRITY.COMPLETE)
                {
                    outHeader = header;
                    data = new byte[HEADER_LENGTH + 1 + STAMP_LENGTH];
                    header.writeHeader(data);
                    data[HEADER_LENGTH] = socket.recvBuffer[HEADER_LENGTH];
                    stampIntegrity(data);
                    return true;
                }

                //Make Sure The Real Header Is Returned
                outHeader = header;

                //Make The Arrays
                int packetLength = PACKET_LENGTH[(int)header.size];
                int packetDataLength = PACKET_DATA_LENGTH[(int)header.size];
                socket.recvBuffer = new byte[packetLength];
                data = new byte[header.dataLength];

                //Fully Receive The First Packet
                int packetsReceived = 0;
                while (!recvFullPacket(socket, socket.recvBuffer)) { };

                #region Copy Information
                Array.Copy(
                        socket.recvBuffer,
                        HEADER_LENGTH,
                        data,
                        packetsReceived * packetDataLength,
                        header.dataLength > packetDataLength ? packetDataLength : header.dataLength
                        );
                header.dataLength -= packetDataLength;
                packetsReceived++;
                #endregion

                if (readIntegrity(socket.recvBuffer) == ZP_INTEGRITY.INCOMPLETE) { return false; }

                while (header.dataLength > packetLength)
                {
                    recvFullPacket(socket, socket.recvBuffer);

                    #region Copy Information
                    Array.Copy(
                            socket.recvBuffer,
                            HEADER_LENGTH,
                            data,
                            packetsReceived * packetDataLength,
                            header.dataLength > packetDataLength ? packetDataLength : header.dataLength
                            );
                    header.dataLength -= packetDataLength;
                    packetsReceived++;
                    #endregion

                    if (readIntegrity(socket.recvBuffer) == ZP_INTEGRITY.INCOMPLETE) { return false; }
                }

                return true;
            }
        }
        public static bool recvData(ZSocket socket, ref Stream data, out ZPUH outHeader)
        {
            lock (socket.recvBuffer)
            {
                //Receive The First Header
                ZPUH header = recvFullHeader(socket);
                while (ZPUH.isBadHeader(header))
                {
                    header = recvFullHeader(socket);
                    Thread.Sleep(RECV_SLEEP_WAIT);
                }

                //Make Sure The Real Header Is Returned
                outHeader = header;

                //Make The Arrays
                int packetLength = PACKET_LENGTH[(int)header.size];
                int packetDataLength = PACKET_DATA_LENGTH[(int)header.size];
                socket.recvBuffer = new byte[packetLength];

                //Fully Receive The First Packet
                int packetsReceived = 0;
                recvFullPacket(socket, socket.recvBuffer);

                #region Copy Information
                data.Write(
                        socket.recvBuffer,
                        HEADER_LENGTH,
                        header.dataLength > packetDataLength ? packetDataLength : (int)header.dataLength
                        );
                header.dataLength -= packetDataLength;
                packetsReceived++;
                #endregion

                if (readIntegrity(socket.recvBuffer) == ZP_INTEGRITY.INCOMPLETE) { return false; }

                while (header.dataLength > packetLength)
                {
                    recvFullPacket(socket, socket.recvBuffer);

                    #region Copy Information
                    data.Write(
                        socket.recvBuffer,
                        HEADER_LENGTH,
                        header.dataLength > packetDataLength ? packetDataLength : (int)header.dataLength
                        );
                    header.dataLength -= packetDataLength;
                    packetsReceived++;
                    #endregion

                    if (readIntegrity(socket.recvBuffer) == ZP_INTEGRITY.INCOMPLETE) { return false; }
                }

                return true;
            }
        }
        #endregion
    }

    /// <summary>
    /// Used To Get Packet ID's In A Threadsafe Manner
    /// </summary>
    public class ZPacketID
    {
        private object idLock = new object();
        private long ID = 0;

        /// <summary>
        /// Gets And Increments The ID
        /// </summary>
        /// <returns>Current ID</returns>
        public long nextID()
        {
            lock (idLock)
            {
                return ++ID;
            }
        }
        /// <summary>
        /// Resets The ID Counter To A New ID Counter
        /// </summary>
        /// <param name="ID">The Reset Counter</param>
        public void resetID(long ID)
        {
            lock (idLock)
            {
                this.ID = ID;
            }
        }
    }

    /// <summary>
    /// Represents File Information That Is Usually Sent Before A File Is Sent
    /// </summary>
    public struct ZPFileInfo
    {
        //All The File Information To Be Sent Over Network Before File
        public String fileName;
        public String suggestedDir;
        public String version;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="fileName">The Name Of The File</param>
        /// <param name="suggestedDir">The Suggested Directory To Be Placed When Received</param>
        /// <param name="version">The File Version</param>
        public ZPFileInfo(String fileName, String suggestedDir, String version)
        {
            this.fileName = fileName;
            this.suggestedDir = suggestedDir;
            this.version = version;
        }

        /// <summary>
        /// Writes The Struct Into Buffer Array
        /// Following ZProtocol
        /// </summary>
        /// <param name="buffer">The Buffer</param>
        public void writeToPacket(byte[] buffer)
        {
            //Temp Buffers
            byte[] b;
            int i = ZProtocol.HEADER_LENGTH;

            //Write The File Name And Length
            b = Encoding.ASCII.GetBytes(fileName);
            Array.Copy(BitConverter.GetBytes(b.Length), 0, buffer, i, 4);
            i += 4;
            Array.Copy(b, 0, buffer, i, b.Length);
            i += b.Length;

            //Write The Suggested Directory And Length
            b = Encoding.ASCII.GetBytes(suggestedDir);
            Array.Copy(BitConverter.GetBytes(b.Length), 0, buffer, i, 4);
            i += 4;
            Array.Copy(b, 0, buffer, i, b.Length);
            i += b.Length;

            //Write The Version And Length
            b = Encoding.ASCII.GetBytes(version);
            Array.Copy(BitConverter.GetBytes(b.Length), 0, buffer, i, 4);
            i += 4;
            Array.Copy(b, 0, buffer, i, b.Length);
        }
        /// <summary>
        /// Reads File Info From Buffer Array Following ZProtocol
        /// </summary>
        /// <param name="buffer">The Buffer Array</param>
        /// <returns>A Struct Representing The Info</returns>
        public static ZPFileInfo readFromPacket(byte[] buffer, int i = ZProtocol.HEADER_LENGTH)
        {
            //Temp Buffers
            int l;

            //Read The File Name
            l = BitConverter.ToInt32(buffer, i);
            i += 4;
            String fileName = Encoding.ASCII.GetString(buffer, i, l);
            i += l;

            //Read The Suggested Directory
            l = BitConverter.ToInt32(buffer, i);
            i += 4;
            String suggestedDir = Encoding.ASCII.GetString(buffer, i, l);
            i += l;

            //Read The Version
            l = BitConverter.ToInt32(buffer, i);
            i += 4;
            String version = Encoding.ASCII.GetString(buffer, i, l);

            return new ZPFileInfo(fileName, suggestedDir, version);
        }

        /// <summary>
        /// Shows All The File Information In A Neat Manner
        /// </summary>
        /// <returns>The File Information In Presentable String Form</returns>
        public override string ToString()
        {
            return
                "File Name:     " + fileName + 
                "\nSuggested Dir: " + suggestedDir + 
                "\nVersion:       " + version
                ;
        }
    }

    /// <summary>
    /// Different Packet Sizes That Can Be Transmitted
    /// </summary>
    public enum ZP_PACKET_SIZE : byte
    {
        SIGNAL = 0x00,
        SMALL = 0x01,
        MEDIUM = 0x02,
        LARGE = 0x03,
        FILE = 0x04
    }

    /// <summary>
    /// ZProtocol Universal Header
    /// </summary>
    public struct ZPUH
    {
        public ZP_PACKET_SIZE size;
        public int dataType;
        public long ID;
        public long dataLength;

        public static ZPUH BAD_HEADER = new ZPUH(ZP_PACKET_SIZE.SIGNAL, -1, -1, -1);

        public ZPUH(ZP_PACKET_SIZE size, int dataType, long ID, long dataLength)
        {
            this.size = size;
            this.dataType = dataType;
            this.ID = ID;
            this.dataLength = dataLength;
        }

        public void writeHeader(byte[] buffer)
        {
            buffer[0] = (byte)size;
            Array.Copy(BitConverter.GetBytes(dataType), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(ID), 0, buffer, 5, 8);
            Array.Copy(BitConverter.GetBytes(dataLength), 0, buffer, 13, 8);

            if (buffer[0] == (byte)size)
            {
                return;
            }
            else
            {
                return;
            }
        }

        public static ZPUH readHeader(byte[] buffer)
        {
            ZP_PACKET_SIZE size = (ZP_PACKET_SIZE)buffer[0];
            int dataType = BitConverter.ToInt32(buffer, 1);
            long ID = BitConverter.ToInt64(buffer, 5);
            long dataLength = BitConverter.ToInt64(buffer, 13);
            return new ZPUH(
                size,
                dataType,
                ID,
                dataLength
                );
        }

        public static bool isBadHeader(ZPUH header)
        {
            return
                header.dataType == BAD_HEADER.dataType ||
                header.dataLength == BAD_HEADER.dataLength
                ;
        }

        public override string ToString()
        {
            return 
                "Size:        " + Convert.ToString(size) +
                "\nData Type:   " + Convert.ToString(dataType) +
                "\nID:          " + Convert.ToString(ID) +
                "\nData Length: " + Convert.ToString(dataLength);
        }
    }
}
