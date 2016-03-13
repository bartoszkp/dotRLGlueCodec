using System.Net.Sockets;
using DotRLGlueCodec.Types;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace DotRLGlueCodec.Network
{
    public class RlGlueSender
    {
        public class RlGlueOperand
        {
            public RlGlueOperand(RlGlueSender owner)
            {
                this.owner = owner;
            }

            public RlGlueSender And()
            {
                return owner;
            }

            public void Flush()
            {
                this.owner.Flush();
            }

            private RlGlueSender owner;
        }

        public class RlGlueSizeSummingOperand
        {
            public RlGlueSizeSummingOperand(RlGlueSender owner)
            {
                this.owner = owner;
            }

            public RlGlueSizeSummingOperand ResetAccumulatedSize()
            {
                this.totalSize = 0;
                return this;
            }

            public RlGlueSizeSummingOperand AndSizeOfInteger()
            {
                this.totalSize += IntSize;
                return this;
            }

            public RlGlueSizeSummingOperand AndSizeOfDouble()
            {
                this.totalSize += DoubleSize;
                return this;
            }

            public RlGlueSizeSummingOperand AndSizeOfString(string s)
            {
                this.totalSize += System.Text.UTF8Encoding.UTF8.GetByteCount(s) + IntSize;

                return this;
            }

            public RlGlueSizeSummingOperand AndSizeOfObservation(Observation o)
            {
                this.totalSize += GetSizeOf(o);
                return this;
            }

            public RlGlueSizeSummingOperand AndSizeOfAction(Action a)
            {
                this.totalSize += GetSizeOf(a);
                return this;
            }

            public RlGlueSender And()
            {
                owner.Integer(this.totalSize);
                return owner;
            }

            public void Flush()
            {
                And().Flush();
            }

            private int GetSizeOf(RLAbstractType abstractType)
            {
                int result = IntSize * 3;
                
                if (abstractType != null)
                {
                    result += IntSize * abstractType.IntCount
                        + DoubleSize * abstractType.DoubleCount
                        + CharSize * abstractType.CharCount;
                }

                return result;
            }
            
            private const int IntSize = 4;
            private const int DoubleSize = 8;
            private const int CharSize = 1;
            private RlGlueSender owner;
            private int totalSize;
        }

        public RlGlueSender(Socket socket)
        {
            this.writer = new EndianBinaryWriter(EndianBitConverter.Big, new NetworkStream(socket));
            this.operand = new RlGlueOperand(this);
            this.sizeOfOperand = new RlGlueSizeSummingOperand(this);
        }

        public void Flush()
        {
            this.writer.Flush();
        }

        public void Close()
        {
            if (this.writer != null)
            {
                this.writer.Close();
            }
            
            this.writer = null;
            this.operand = null;
        }
        
        public RlGlueSizeSummingOperand SizeOfState()
        {
            return this.sizeOfOperand.ResetAccumulatedSize();
        }

        public RlGlueOperand State(RlGlueConnection.ConnectionState state)
        {
            SendInt((int)state);
            return this.operand;
        }

        public RlGlueOperand Integer(int integer)
        {
            SendInt(integer);
            return this.operand;
        }

        public RlGlueOperand Double(double value)
        {
            SendDouble(value);
            return this.operand;
        }

        public RlGlueOperand Observation(Observation observation)
        {
            WriteRLAbstractType(observation);
            return this.operand;
        }

        public RlGlueOperand Action(Action action)
        {
            WriteRLAbstractType(action);
            return this.operand;
        }

        public RlGlueOperand Terminal(bool terminal)
        {
            return Integer(terminal ? 1 : 0);
        }

        public RlGlueOperand String(string s)
        {
            SendString(s);
            return this.operand;
        }

        private void SendInt(int value)
        {
            writer.Write(value);
        }

        private void SendDouble(double value)
        {
            writer.Write(value);
        }

        private void SendString(string s)
        {
            if (s == string.Empty)
            {
                writer.Write(0);
            }
            else
            {
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(s);

                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
        }

        private void WriteRLAbstractType(RLAbstractType rlAbstractType)
        {
            int intCount = 0;
            int doubleCount = 0;
            int charCount = 0;

            if (rlAbstractType != null)
            {
                intCount = rlAbstractType.IntCount;
                doubleCount = rlAbstractType.DoubleCount;
                charCount = rlAbstractType.CharCount;
            }

            writer.Write(intCount);
            writer.Write(doubleCount);
            writer.Write(charCount);

            if (rlAbstractType != null)
            {
                foreach (int v in rlAbstractType.IntArray)
                {
                    writer.Write(v);
                }
                foreach (double v in rlAbstractType.DoubleArray)
                {
                    writer.Write(v);
                }
                foreach (char v in rlAbstractType.CharArray)
                {
                    writer.Write(v);
                }
            }

            writer.Flush();
        }
        
        private EndianBinaryWriter writer;
        private RlGlueOperand operand;
        private RlGlueSizeSummingOperand sizeOfOperand;
    }
}
