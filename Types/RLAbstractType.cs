/*
 * Ported from Java implementation with some modifications by 
 * Bartosz Papis (bartoszkp@gmail.com)
 */
/* 
 * Copyright (C) 2007, Brian Tanner
 *
 * http://rl-glue-ext.googlecode.com/
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
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
 *  $Revision: 489 $
 *  $Date: 2009-01-31 14:34:21 -0700 (Sat, 31 Jan 2009) $
 *  $Author: brian@tannerpages.com $
 *  $HeadURL: http://rl-glue-ext.googlecode.com/svn/trunk/projects/codecs/Java/src/org/rlcommunity/rlglue/codec/types/RL_abstract_type.java $
 *
 */
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace DotRLGlueCodec.Types
{
    public class RLAbstractType
    {
        public IEnumerable<int> IntArray { get { return this.intArray; } }

        public IEnumerable<double> DoubleArray { get { return this.doubleArray; } }
        
        public IEnumerable<char> CharArray { get { return this.charArray; } }
        
        public int IntCount { get { return this.intArray.Length; } }
        
        public int DoubleCount { get { return this.doubleArray.Length; } }
        
        public int CharCount { get { return this.charArray.Length; } }

        public RLAbstractType(int intCount, int doubleCount, int charCount)
        {
            intArray = new int[intCount];
            doubleArray = new double[doubleCount];
            charArray = new char[charCount];
        }

        public void SetIntArray(int[] intArray)
        {
            this.intArray = intArray;
        }

        public void SetDoubleArray(double[] doubleArray)
        {
            this.doubleArray = doubleArray;
        }

        public void SetCharArray(char[] charArray)
        {
            this.charArray = charArray;
        }

        public virtual int GetInt(int index)
        {
            return GetElement(this.intArray, index);
        }

        public virtual double GetDouble(int index)
        {
            return GetElement(this.doubleArray, index);
        }

        public virtual double GetChar(int index)
        {
            return GetElement(this.charArray, index);
        }

        protected virtual T GetElement<T>(T[] array, int index)
        {
            Contract.Requires(index >= 0 && index < array.Length);

            return array[index];
        }

        public virtual void SetInt(int index, int value)
        {
            SetElement(this.intArray, value, index);
        }

        public virtual void SetDouble(int index, double value)
        {
            SetElement(this.doubleArray, value, index);
        }

        public virtual void SetChar(int index, char value)
        {
            SetElement(this.charArray, value, index);
        }

        protected virtual void SetElement<T>(T[] array, T value, int index)
        {
            Contract.Requires(index >= 0 && index < array.Length);

            array[index] = value;
        }

        private int[] intArray;
        private double[] doubleArray;
        private char[] charArray;
    }
}
