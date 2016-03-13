using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using DotRLGlueCodec.Types;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace DotRLGlueCodec.Network
{
    /// <summary>
    /// Not thread safe.
    /// </summary>
    public class RlGlueConnection
    {
        public enum ConnectionState
        {
            Unknown = 0,

            ExperimentConnection = 1,
            AgentConnection = 2,
            EnvironmentConnection = 3,
            
            AgentInitialize = 4,
            AgentStart = 5,
            AgentStep = 6,
            AgentEnd = 7,
            AgentCleanup = 8,
            AgentMessage = 10,
            
            EnvironmentInitialize = 11,
            EnvironmentStart = 12,
            EnvironmentStep = 13,
            EnvironmentCleanup = 14,
            EnvironmentMessage = 19,
            
            RLInit = 20,
            RLStart = 21,
            RLStep = 22,
            RLCleanup = 23,
            RLReturn = 24,
            RLNumSteps = 25,
            RLNumEpisodes = 26,
            RLEpisode = 27,
            RLAgentMessage = 33,
            RLEnvironmentMessage = 34,
            RLTerminate = 35,
            RLEnvironmentStart = 36,
            RLEnvironmentStep = 37,
            RLAgentStart = 38,
            RLAgentStep = 39,
            RLAgentEnd = 40
        };

        public void Connect(string host, int portNumber)
        {
            this.Connect(IPAddress.Parse(host), portNumber);
        }

        public void Connect(IPAddress ipAddress, int portNumber)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipAddress, portNumber);

            this.sender = new RlGlueSender(socket);
            this.receiver = new RlGlueReceiver(socket);
        }

        public void Accept(Socket socket)
        {
            this.socket = socket;
            this.sender = new RlGlueSender(socket);
            this.receiver = new RlGlueReceiver(socket);
        }

        public void Flush()
        {
            this.sender.Flush();
        }

        public void Close()
        {
            if (this.sender != null)
            {
                this.sender.Close();
            }

            if (this.receiver != null)
            {
                this.receiver.Close();
            }

            if (this.socket != null)
            {
                socket.Close();
            }

            socket = null;
        }

        public RlGlueSender Send()
        {
            return this.sender;
        }

        public RlGlueReceiver Receive()
        {
            return this.receiver;
        }

        private Socket socket;
        private RlGlueSender sender;
        private RlGlueReceiver receiver;
    }
}
