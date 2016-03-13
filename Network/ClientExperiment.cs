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
 *  $HeadURL: http://rl-glue-ext.googlecode.com/svn/trunk/projects/codecs/Java/src/org/rlcommunity/rlglue/codec/NetGlue.java $
 *
 */
using System.IO;
using DotRLGlueCodec.Types;

namespace DotRLGlueCodec.Network
{
    public class ClientExperiment : ExperimentInterface
    {
        private RlGlueConnection rlGlueConnection;

        public ClientExperiment()
        {
            rlGlueConnection = new RlGlueConnection();
        }

        public void Connect(string host, int port)
        {
            rlGlueConnection = new RlGlueConnection();
            rlGlueConnection.Connect(host, port);

            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.ExperimentConnection)
                .And()
                .SizeOfState()
                .Flush();
        }

        public virtual string RLInit()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLInit)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLInit);

            return rlGlueConnection.Receive().String();
        }

        public virtual ObservationAction RLStart()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLStart)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLStart);

            Observation observation;
            Action action;

            rlGlueConnection
                .Receive()
                .Observation(out observation)
                .And()
                .Action(out action);

            return new ObservationAction()
            {
                Observation = observation,
                Action = action
            };
        }

        public virtual Observation RLEnvStart()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLEnvironmentStart)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLEnvironmentStart);

            return rlGlueConnection.Receive().Observation();
        }

        public virtual RewardObservationTerminal RLEnvStep(Action theAction)
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLEnvironmentStep)
                .And()
                .SizeOfState()
                .AndSizeOfAction(theAction)
                .And()
                .Action(theAction)
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLEnvironmentStep);

            double reward;
            Observation observation;
            bool terminal;

            rlGlueConnection
                .Receive()
                .Double(out reward)
                .And()
                .Observation(out observation)
                .And()
                .Boolean(out terminal);

            return new RewardObservationTerminal()
            {
                Reward = reward,
                Observation = observation,
                Terminal = terminal
            };
        }

        public virtual Action RLAgentStart(Observation theObservation)
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLAgentStart)
                .And()
                .SizeOfState()
                .AndSizeOfObservation(theObservation)
                .And()
                .Observation(theObservation)
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLAgentStart);
            
            return rlGlueConnection.Receive().Action();
        }

        public virtual Action RLAgentStep(double theReward, Observation theObservation)
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLStep)
                .And()
                .SizeOfState()
                .AndSizeOfDouble()
                .AndSizeOfObservation(theObservation)
                .And()
                .Double(theReward)
                .And()
                .Observation(theObservation)
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLStep);

            return rlGlueConnection.Receive().Action();
        }

        public virtual void RLAgentEnd(double theReward)
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLAgentEnd)
                .And()
                .SizeOfState()
                .AndSizeOfDouble()
                .And()
                .Double(theReward)
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLAgentEnd);
        }

        public virtual RewardObservationActionTerminal RLStep()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLStep)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLStep);

            bool terminal;
            double reward;
            Observation observation;
            Action action;

            rlGlueConnection
                .Receive()
                .Boolean(out terminal)
                .And()
                .Double(out reward)
                .And()
                .Observation(out observation)
                .And()
                .Action(out action);

            return new RewardObservationActionTerminal()
            {
                Terminal = terminal,
                Reward = reward,
                Observation = observation,
                Action = action
            };
        }

        public virtual void RLCleanup()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLCleanup)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLCleanup);
        }

        public virtual string RLAgentMessage(string message)
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLAgentMessage)
                .And()
                .SizeOfState()
                .AndSizeOfString(message)
                .And()
                .String(message)
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLAgentMessage);

            return rlGlueConnection.Receive().String();
        }

        public virtual string RLEnvMessage(string message)
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLEnvironmentMessage)
                .And()
                .SizeOfState()
                .AndSizeOfString(message)
                .And()
                .String(message)
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLEnvironmentMessage);

            return rlGlueConnection.Receive().String();
        }

        public virtual double RLReturn()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLReturn)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLReturn);

            return rlGlueConnection.Receive().Double();
        }

        public virtual int RLNumSteps()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLNumSteps)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLNumSteps);

            return rlGlueConnection.Receive().Integer();
        }

        public virtual int RLNumEpisodes()
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLNumEpisodes)
                .And()
                .SizeOfState()
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLNumEpisodes);

            return rlGlueConnection.Receive().Integer();
        }

        public virtual int RLEpisode(int numSteps)
        {
            rlGlueConnection
                .Send()
                .State(RlGlueConnection.ConnectionState.RLEpisode)
                .And()
                .SizeOfState()
                .AndSizeOfInteger()
                .And()
                .Integer(numSteps)
                .Flush();
            ReceiveAndVerifyState(RlGlueConnection.ConnectionState.RLEpisode);

            return rlGlueConnection.Receive().Integer();
        }

        private void ReceiveAndVerifyState(RlGlueConnection.ConnectionState state)
        {
            RlGlueConnection.ConnectionState glueState;

            rlGlueConnection.Receive()
                .State(out glueState)
                .And()
                .DiscardInteger();

            if (glueState != state)
            {
                throw new InvalidDataException("Synchronization lost");
            }
        }
    }
}
