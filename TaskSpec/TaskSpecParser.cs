using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DotRLGlueCodec.TaskSpec
{
    /// <summary>
    /// Does not fully support RL-Glue-3.0.
    /// CHARCOUNT is not supported, and only uniform observations and uniform actions are supported (i.e. only INTS or only DOUBLES).
    /// PROBLEMTYPE is ignored.
    /// </summary>
    public class TaskSpecParser : ITaskSpecParser
    {
        public TaskSpecBase Parse(string taskSpecString)
        {
            IEnumerator<Lexem> tokens = Scan(taskSpecString).GetEnumerator();

            string version = ParseVersion(tokens);
            string problemType = ParseProblemType(tokens);
            double discountFactor = ParseDiscountFactor(tokens);
            ExpectNextToken(tokens, "OBSERVATIONS", context: "TaskSpec");
            RangeParseResult observations = ParseRangeGroup(tokens, "observations");
            ExpectNextToken(tokens, "ACTIONS", context: "TaskSpec");
            RangeParseResult actions = ParseRangeGroup(tokens, "actions");
            ExpectNextToken(tokens, "REWARDS", context: "TaskSpec");
            Tuple<object, object> reward = null;

            try
            {
                reward = ParseRange(tokens, typeof(double), context: "rewards").Single();
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidDataException("Expected single reward range.", e);
            }
            
            string additionalInformation = string.Empty;
            if (OptionalNextToken(tokens, "EXTRA"))
            {
                StringBuilder extraString = new StringBuilder();
                while (tokens.MoveNext())
                {
                    extraString.Append(tokens.Current.Value + tokens.Current.Tail);
                }

                additionalInformation = extraString.ToString();
            }

            return BuildConcreteTaskSpecInstance(observations, actions, reward, discountFactor, additionalInformation);
        }

        private string ParseVersion(IEnumerator<Lexem> tokens)
        {
            ExpectNextToken(tokens, "VERSION", context: "version");
            return ExpectNextToken(tokens, "RL-Glue-3.0", context: "version");
        }

        private string ParseProblemType(IEnumerator<Lexem> tokens)
        {
            ExpectNextToken(tokens, "PROBLEMTYPE", context: "problem type");
            return GetNextToken(tokens, context: "problem type");
        }

        private double ParseDiscountFactor(IEnumerator<Lexem> tokens)
        {
            ExpectNextToken(tokens, "DISCOUNTFACTOR", context: "discount factor");
            string discountFactorString = GetNextToken(tokens, context: "discount factor");

            Exception innerException = null;
            try
            {
                return double.Parse(discountFactorString, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                innerException = e;
            }
            catch (OverflowException e)
            {
                innerException = e;
            }
            catch (ArgumentNullException e)
            {
                innerException = e;
            }

            throw new InvalidDataException("Could not parse discount factor: '" + discountFactorString + "'", innerException);
        }

        private RangeParseResult ParseRangeGroup(IEnumerator<Lexem> tokens, string context)
        {
            Type rangeType = null;
            while (tokens.Current.NextLexem != null && RangeTypes.Contains(tokens.Current.NextLexem.Value))
            {
                rangeType = ParseRangeType(tokens, context: context);
            }

            if (rangeType == null)
            {
                ExpectNextToken(tokens, RangeTypes, context);
            }

            return ParseRangeGroupContents(tokens, rangeType, context);
        }

        private RangeParseResult ParseRangeGroupContents(IEnumerator<Lexem> tokens, Type rangeType, string context)
        {
            List<object> minimumValues = new List<object>();
            List<object> maximumValues = new List<object>();

            var range = ParseRange(tokens, rangeType, context: context);

            minimumValues.AddRange(range.Select(r => r.Item1));
            maximumValues.AddRange(range.Select(r => r.Item2));

            if (tokens.Current.NextLexem != null)
            {
                RangeParseResult nextRange = null;
                if (tokens.Current.NextLexem.Value == "(")
                {
                    nextRange = ParseRangeGroupContents(tokens, rangeType, context);
                }
                else if (RangeTypes.Contains(tokens.Current.NextLexem.Value))
                {
                    nextRange = ParseRangeGroup(tokens, context);

                    if (!nextRange.Type.Equals(rangeType))
                    {
                        throw new InvalidDataException("Non-uniform ranges are not supported. First encountered range: " + TypeToRangeDescription(rangeType) + ", next: " + TypeToRangeDescription(nextRange.Type));
                    }
                }

                if (nextRange != null)
                {
                    minimumValues.AddRange(nextRange.MinimumValues);
                    maximumValues.AddRange(nextRange.MaximumValues);
                }
            }

            return new RangeParseResult() { Type = rangeType, MinimumValues = minimumValues, MaximumValues = maximumValues };
        }

        private Type ParseRangeType(IEnumerator<Lexem> tokens, string context)
        {
            string type = ExpectNextToken(tokens, RangeTypes, context: "observations");

            return type == "INTS" ? typeof(int) : typeof(double);
        }

        private IEnumerable<Tuple<object, object>> ParseRange(IEnumerator<Lexem> tokens, Type rangeType, string context)
        {
            if (tokens.Current.NextLexem != null && RangeTypes.Contains(tokens.Current.NextLexem.Value))
            {
                return Enumerable.Empty<Tuple<object, object>>();
            }

            ExpectNextToken(tokens, "(", context: context + " range");

            TypeConverter converter = TypeDescriptor.GetConverter(rangeType);

            List<Tuple<object, object>> result = new List<Tuple<object, object>>();

            List<string> rangeTokens = new List<string>();
            string token = GetNextToken(tokens, context: context + " range");
            while (token != ")") // scanner guarantees that all parentheses are matched
            {
                rangeTokens.Add(token);
                token = GetNextToken(tokens, context: context + " range");
            }

            if (rangeTokens.Count == 2)
            {
                result.Add(ParseRangeValues(rangeTokens[0], rangeTokens[1], converter, context: context));
            }
            else if (rangeTokens.Count == 3)
            {
                int count = 0;

                Exception innerException = null;
                try
                {
                    count = int.Parse(rangeTokens[0], CultureInfo.InvariantCulture);
                }
                catch (FormatException e)
                {
                    innerException = e;
                }
                catch (OverflowException e)
                {
                    innerException = e;
                }
                catch (ArgumentNullException e)
                {
                    innerException = e;
                }

                if (innerException != null)
                {
                    throw new InvalidDataException("Could not parse range multiplicity value: '" + rangeTokens[0] + "' when parsing " + context);
                }

                var rangeTuple = ParseRangeValues(rangeTokens[1], rangeTokens[2], converter, context: context);
                for (int i = 0; i < count; ++i)
                {
                    result.Add(rangeTuple);
                }
            }

            return result;
        }

        private Tuple<object, object> ParseRangeValues(string minimum, string maximum, TypeConverter converter, string context)
        {
            return new Tuple<object, object>(
                ParseRangeValue(minimum, converter, context, "minimum"),
                ParseRangeValue(maximum, converter, context, "maximum"));
        }

        private object ParseRangeValue(string value, TypeConverter converter, string context, string rangeContext)
        {
            try
            {
                return converter.ConvertFromInvariantString(value);
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Could not parse range " + rangeContext + " value: '" + value + "' when parsing " + context, e);
            }
        }

        private string GetNextToken(IEnumerator<Lexem> tokens, string context)
        {
            if (!tokens.MoveNext())
            {
                throw new InvalidDataException("Unexpected end of input when parsing " + context);
            }

            return tokens.Current.Value;
        }

        private bool OptionalNextToken(IEnumerator<Lexem> tokens, string expected)
        {
            if (!tokens.MoveNext())
            {
                return false;
            }

            return tokens.Current.Value == expected;
        }

        private string ExpectNextToken(IEnumerator<Lexem> tokens, string expected, string context)
        {
            return ExpectNextToken(tokens, new[] { expected }, context: context);
        }

        private string ExpectNextToken(IEnumerator<Lexem> tokens, string[] expected, string context)
        {
            if (!tokens.MoveNext())
            {
                throw new InvalidDataException("Unexpected end of input, expected '" + string.Join("','", expected) + "' when parsing " + context);
            }

            if (!expected.Contains(tokens.Current.Value))
            {
                string messageStart = "Expected: '";
                if (expected.Length > 1)
                {
                    messageStart = "Expected one of: '";
                }

                throw new InvalidDataException(messageStart + string.Join("','", expected) + "', got '" + tokens.Current + "' instead when parsing " + context);
            }

            return tokens.Current.Value;
        }

        private IEnumerable<Lexem> Scan(string input)
        {
            List<Lexem> result = new List<Lexem>();

            StringBuilder accumulator = new StringBuilder();

            string separators = " \r\t\f\n";
            bool insideRange = false;

            for (int i = 0; i < input.Length; ++i)
            {
                if (separators.Contains(input[i]))
                {
                    AddLexemIfNotEmpty(accumulator, result);
                    AppendTail(input[i], result);
                }
                else
                {
                    if (input[i] == '(')
                    {
                        AddLexemIfNotEmpty(accumulator, result);
                        AddLexem("(", result);
                        insideRange = true;
                    }
                    else if (input[i] == ')')
                    {
                        if (!insideRange)
                        {
                            throw new InvalidDataException("Mismatched ')' at position " + (i + 1));
                        }

                        AddLexemIfNotEmpty(accumulator, result);
                        AddLexem(")", result);
                        insideRange = false;
                    }
                    else
                    {
                        accumulator.Append(input[i]);
                    }
                }
            }

            if (insideRange)
            {
                throw new InvalidDataException("Matching ')' not found");
            }

            AddLexemIfNotEmpty(accumulator, result);

            return result;
        }

        private void AppendTail(char character, List<Lexem> result)
        {
            if (result.Any())
            {
                result.Last().Tail += character;
            }
        }

        private void AddLexem(string lexemValue, List<Lexem> result)
        {
            Lexem lexem = new Lexem(lexemValue);
            if (result.Any())
            {
                result.Last().SetNext(lexem);
            }
            
            result.Add(lexem);
        }

        private void AddLexemIfNotEmpty(StringBuilder stringBuilder, List<Lexem> result)
        {
            string lexemValue = stringBuilder.ToString();
            if (lexemValue != string.Empty)
            {
                AddLexem(lexemValue, result);
                stringBuilder.Clear();
            }
        }

        private TaskSpecBase BuildConcreteTaskSpecInstance(
            RangeParseResult observations,
            RangeParseResult actions,
            Tuple<object, object> reward,
            double discountFactor,
            string additionalInformation)
        {
            return (TaskSpecBase)this
                .GetType()
                .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Single(m => m.Name == "BuildConcreteTaskSpecInstance" && m.IsGenericMethod)
                .MakeGenericMethod(observations.Type, actions.Type)
                .Invoke(this, new object[]
                {
                    observations,
                    actions,
                    reward,
                    discountFactor,
                    additionalInformation
                });
        }

        private TaskSpecBase BuildConcreteTaskSpecInstance<TStateSpace, TActionSpace>(
            RangeParseResult observations,
            RangeParseResult actions,
            Tuple<object, object> reward,
            double discountFactor,
            string additionalInformation)
        {
            return (TaskSpecBase)typeof(TaskSpec<,>)
                .MakeGenericType(observations.Type, actions.Type)
                .GetConstructor(new[] 
                { 
                    typeof(IEnumerable<TStateSpace>),
                    typeof(IEnumerable<TStateSpace>),
                    typeof(IEnumerable<TActionSpace>),
                    typeof(IEnumerable<TActionSpace>),
                    typeof(double),
                    typeof(double),
                    typeof(double),
                    typeof(string)
                })
                .Invoke(new object[]
                {
                    new List<TStateSpace>(observations.MinimumValues.Cast<TStateSpace>()),
                    new List<TStateSpace>(observations.MaximumValues.Cast<TStateSpace>()),
                    new List<TActionSpace>(actions.MinimumValues.Cast<TActionSpace>()),
                    new List<TActionSpace>(actions.MaximumValues.Cast<TActionSpace>()),
                    (double)reward.Item1,
                    (double)reward.Item2,
                    discountFactor,
                    additionalInformation
                });
        }

        public static string TypeToRangeDescription(Type rangeType)
        {
            if (rangeType.Equals(typeof(int)))
            {
                return "INTS";
            }
            else if (rangeType.Equals(typeof(double)))
            {
                return "DOUBLES";
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private class Lexem
        {
            public string Value { get; private set; }
            public Lexem NextLexem { get; private set; }
            public string Tail { get; set; }

            public Lexem(string value)
            {
                this.Value = value;
            }

            public void SetNext(Lexem next)
            {
                NextLexem = next;
            }

            public override string ToString()
            {
                return Value;
            }
        }

        private class RangeParseResult
        {
            public Type Type { get; set; }
            public IEnumerable<object> MinimumValues { get; set; }
            public IEnumerable<object> MaximumValues { get; set; }
        }

        private static string[] RangeTypes = new string[] { "INTS", "DOUBLES" };
    }
}
