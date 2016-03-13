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
 *  $HeadURL: http://rl-glue-ext.googlecode.com/svn/trunk/projects/codecs/Java/src/org/rlcommunity/rlglue/codec/network/ClientEnvironment.java $
 *
 */
using System.IO;
using System.Net;
using DotRLGlueCodec.Types;

namespace DotRLGlueCodec.Network
{
    public class ClientEnvironment
    {
        public RlGlueConnection.ConnectionState ConnectionState
        {
            get
            {
                return this.environmentState;
            }
        }

        protected internal const string kUnknownMessage = "ClientEnvironment.java :: Unknown Message: ";
        protected internal EnvironmentInterface environment;
        protected internal RlGlueConnection rlGlueConnection;

        public ClientEnvironment(EnvironmentInterface environment)
        {
            this.environment = environment;
            this.rlGlueConnection = new RlGlueConnection();
        }

        protected internal virtual void onEnvInit()
        {
            string taskSpec = environment.EnvironmentInit();

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.EnvironmentInitialize)
                .And()
                .SizeOfState()
                .AndSizeOfString(taskSpec)
                .And()
                .String(taskSpec)
                .Flush();
        }

        protected internal virtual void onEnvStart()
        {
            Observation observation = environment.EnvironmentStart();

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.EnvironmentStart)
                .And()
                .SizeOfState()
                .AndSizeOfObservation(observation)
                .And()
                .Observation(observation)
                .Flush();
        }

        protected internal virtual void onEnvStep()
        {
            Action action = rlGlueConnection.Receive().Action();
            RewardObservationTerminal rewardObservation = environment.EnvironmentStep(action);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.EnvironmentStep)
                .And()
                .SizeOfState()
                .AndSizeOfInteger()
                .AndSizeOfDouble()
                .AndSizeOfObservation(rewardObservation.Observation)
                .And()
                .Terminal(rewardObservation.Terminal)
                .And()
                .Double(rewardObservation.Reward)
                .And()
                .Observation(rewardObservation.Observation)
                .Flush();
        }

        protected internal virtual void onEnvCleanup()
        {
            environment.EnvironmentCleanup();

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.EnvironmentCleanup)
                .And()
                .SizeOfState()
                .Flush();
        }

        protected internal virtual void onEnvMessage()
        {
            string message = rlGlueConnection.Receive().String();
            string reply = environment.EnvironmentMessage(message);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.EnvironmentMessage)
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
                .State(RlGlueConnection.ConnectionState.EnvironmentConnection)
                .And()
                .SizeOfState()
                .Flush();
        }

        public virtual void Close()
        {
            rlGlueConnection.Close();
        }

        public virtual void runEnvironmentEventLoop()
        {
            do
            {
                rlGlueConnection
                    .Receive()
                    .State(out environmentState)
                    .And()
                    .DiscardInteger();

                switch (environmentState)
                {
                    case RlGlueConnection.ConnectionState.EnvironmentInitialize:
                        onEnvInit();
                        break;

                    case RlGlueConnection.ConnectionState.EnvironmentStart:
                        onEnvStart();
                        break;

                    case RlGlueConnection.ConnectionState.EnvironmentStep:
                        onEnvStep();
                        break;

                    case RlGlueConnection.ConnectionState.EnvironmentCleanup:
                        onEnvCleanup();
                        break;

                    case RlGlueConnection.ConnectionState.EnvironmentMessage:
                        onEnvMessage();
                        break;

                    case RlGlueConnection.ConnectionState.RLTerminate:
                        break;

                    default:
                        throw new InvalidDataException(kUnknownMessage + environmentState);
                }

            }
            while (environmentState != RlGlueConnection.ConnectionState.RLTerminate);
        }

        private RlGlueConnection.ConnectionState environmentState;
    }
}
