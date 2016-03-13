using System.IO;
using System.Linq;
using System.Net.Sockets;
using DotRLGlueCodec.Types;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace DotRLGlueCodec.Network
{
    public class RlGlueReceiver
    {
        public class RlGlueOperand
        {
            public RlGlueOperand(RlGlueReceiver owner)
            {
                this.owner = owner;
            }

            public RlGlueReceiver And()
            {
                return this.owner;
            }

            private RlGlueReceiver owner;
        }

        public RlGlueReceiver(Socket socket)
        {
            this.reader = new EndianBinaryReader(EndianBitConverter.Big, new NetworkStream(socket));
            this.operand = new RlGlueOperand(this);
        }

        public void Close()
        {
            if (this.reader != null)
            {
                this.reader.Close();
            }
            
            this.reader = null;
            this.operand = null;
        }

        public RlGlueOperand State(out RlGlueConnection.ConnectionState state)
        {
            int intState = reader.ReadInt32();

            if (!System.Enum.GetValues(typeof(RlGlueConnection.ConnectionState)).Cast<int>().Contains(intState))
            {
                throw new InvalidDataException("Invalid state received: " + intState);
            }

            state = (RlGlueConnection.ConnectionState)intState;

            return this.operand;
        }

        public RlGlueOperand String(out string s)
        {
            s = ReceiveString();
            return this.operand;
        }

        public string String()
        {
            return ReceiveString();
        }

        public RlGlueOperand Integer(out int i)
        {
            i = reader.ReadInt32();
            return this.operand;
        }

        public int Integer()
        {
            return reader.ReadInt32();
        }

        public RlGlueOperand DiscardInteger()
        {
            reader.ReadInt32();
            return this.operand;
        }

        public RlGlueOperand Boolean(out bool b)
        {
            b = reader.ReadInt32() == 1;
            return this.operand;
        }

        public RlGlueOperand Double(out double r)
        {
            r = reader.ReadDouble();
            return this.operand;
        }

        public double Double()
        {
            return reader.ReadDouble();
        }

        public RlGlueOperand Observation(out Observation o)
        {
            o = ReceiveObservation();
            return this.operand;
        }

        public Observation Observation()
        {
            return ReceiveObservation();
        }

        public RlGlueOperand Action(out Action a)
        {
            a = ReceiveAction();
            return this.operand;
        }

        public Action Action()
        {
            return ReceiveAction();
        }

        private string ReceiveString()
        {
            int length = reader.ReadInt32();
            if (length <= 0)
            {
                return string.Empty;
            }

            byte[] bytes = reader.ReadBytes(length);

            return System.Text.UTF8Encoding.UTF8.GetString(bytes);
        }

        private Observation ReceiveObservation()
        {
            Observation result = new Observation();
            ReadRLAbstractType(result);
            return result;
        }

        private Action ReceiveAction()
        {
            Action result = new Action();
            ReadRLAbstractType(result);
            return result;
        }

        private void ReadRLAbstractType(RLAbstractType rlAbstractType)
        {
            int intCount = reader.ReadInt32();
            int doubleCount = reader.ReadInt32();
            int charCount = reader.ReadInt32();

            int[] intArray = Enumerable.Range(0, intCount).Select(i => reader.ReadInt32()).ToArray();
            double[] doubleArray = Enumerable.Range(0, doubleCount).Select(i => reader.ReadDouble()).ToArray();
            char[] charArray = Enumerable.Range(0, charCount).Select(i => ReadChar()).ToArray();

            rlAbstractType.SetIntArray(intArray);
            rlAbstractType.SetDoubleArray(doubleArray);
            rlAbstractType.SetCharArray(charArray);
        }

        private char ReadChar()
        {
            return System.Text.Encoding.ASCII.GetChars(reader.ReadBytes(1)).First();
        }

        private EndianBinaryReader reader;
        private RlGlueOperand operand;
    }
}
