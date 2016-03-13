/*
 * Ported from Java implementation with some modifications by 
 * Bartosz Papis (bartoszkp@gmail.com)
 */
/*
 * 
 * Copyright (C) 2007, Brian Tanner
 *
 * http://rl-glue-ext.googlecode.com/
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 *  $Revision: 638 $
 *  $Date: 2009-02-07 14:17:29 -0700 (Sat, 07 Feb 2009) $
 *  $Author: brian@tannerpages.com $
 *  $HeadURL: http://rl-glue-ext.googlecode.com/svn/trunk/projects/codecs/Java/src/org/rlcommunity/rlglue/codec/network/ClientAgent.java $
 *
 */
using System.IO;
using System.Net;
using DotRLGlueCodec.Types;

namespace DotRLGlueCodec.Network
{
    public class ClientAgent
    {
        public RlGlueConnection.ConnectionState ConnectionState
        {
            get
            {
                return this.agentState;
            }
        }

        protected internal const string kUnknownMessage = "Unknown Message: ";
        protected internal AgentInterface agent;
        protected RlGlueConnection rlGlueConnection;

        public ClientAgent(AgentInterface agent)
        {
            this.agent = agent;
            rlGlueConnection = new RlGlueConnection();
        }

        protected internal virtual void onAgentInit()
        {
            string taskSpec;
            
            rlGlueConnection
                .Receive()
                .String(out taskSpec);

            agent.AgentInit(taskSpec);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.AgentInitialize)
                .And()
                .SizeOfState()
                .Flush();
        }

        protected internal virtual void onAgentStart()
        {
            Observation observation = rlGlueConnection.Receive().Observation();

            Action action = agent.AgentStart(observation);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.AgentStart)
                .And()
                .SizeOfState()
                .AndSizeOfAction(action)
                .And()
                .Action(action)
                .Flush();
        }

        protected internal virtual void onAgentStep()
        {
            double reward = rlGlueConnection.Receive().Double();
            Observation observation = rlGlueConnection.Receive().Observation();
            
            Action action = agent.AgentStep(reward, observation);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.AgentStep)
                .And()
                .SizeOfState()
                .AndSizeOfAction(action)
                .And()
                .Action(action)
                .Flush();
        }

        protected internal virtual void onAgentEnd()
        {
            double reward = rlGlueConnection.Receive().Double();

            agent.AgentEnd(reward);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.AgentEnd)
                .And()
                .SizeOfState()
                .Flush();
        }

        protected internal virtual void onAgentCleanup()
        {
            agent.AgentCleanup();

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.AgentCleanup)
                .And()
                .SizeOfState()
                .Flush();
        }

        protected internal virtual void onAgentMessage()
        {
            string message = rlGlueConnection.Receive().String();

            string reply = agent.AgentMessage(message);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.AgentMessage)
                .And()
                .SizeOfState()
                .AndSizeOfString(reply)
                .And()
                .String(reply)
                .Flush();
        }

        public void Connect(string host, int portNumber)
        {
            this.Connect(IPAddress.Parse(host), portNumber);
        }

        public void Connect(IPAddress ipAddress, int portNumber)
        {
            rlGlueConnection.Connect(ipAddress, portNumber);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.AgentConnection)
                .And()
                .SizeOfState()
                .Flush();
        }

        public virtual void Close()
        {
            rlGlueConnection.Close();
        }

        public virtual void runAgentEventLoop()
        {
            do
            {
                rlGlueConnection
                    .Receive()
                    .State(out agentState)
                    .And()
                    .DiscardInteger();

                switch (agentState)
                {
                    case RlGlueConnection.ConnectionState.AgentInitialize:
                        onAgentInit();
                        break;

                    case RlGlueConnection.ConnectionState.AgentStart:
                        onAgentStart();
                        break;

                    case RlGlueConnection.ConnectionState.AgentStep:
                        onAgentStep();
                        break;

                    case RlGlueConnection.ConnectionState.AgentEnd:
                        onAgentEnd();
                        break;

                    case RlGlueConnection.ConnectionState.AgentCleanup:
                        onAgentCleanup();
                        break;

                    case RlGlueConnection.ConnectionState.AgentMessage:
                        onAgentMessage();
                        break;

                    case RlGlueConnection.ConnectionState.RLTerminate:
                        break;

                    default:
                        throw new InvalidDataException(kUnknownMessage + agentState);
                }
            }
            while (agentState != RlGlueConnection.ConnectionState.RLTerminate);
        }

        private RlGlueConnection.ConnectionState agentState;
    }
}
