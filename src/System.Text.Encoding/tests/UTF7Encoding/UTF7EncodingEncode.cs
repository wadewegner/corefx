// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Xunit;

namespace System.Text.Tests
{
    public class UTF7EncodingEncode
    {
        public static IEnumerable<object[]> Encode_Basic_TestData()
        {
            yield return new object[] { "\t\n\rXYZabc123", 0, 12, new byte[] { 9, 10, 13, 88, 89, 90, 97, 98, 99, 49, 50, 51 } };

            yield return new object[] { "\u03a0\u03a3", 0, 2, new byte[] { 43, 65, 54, 65, 68, 111, 119, 45 } };
            
            string chars2 = "UTF7 Encoding Example";
            yield return new object[] { chars2, 1, 2, new byte[] { 84, 70 } };

            // Surrogate pairs
            yield return new object[] { "\uD800\uDC00", 0, 2, new byte[] { 43, 50, 65, 68, 99, 65, 65, 45 } };
            yield return new object[] { "a\uD800\uDC00b", 0, 4, new byte[] { 97, 43, 50, 65, 68, 99, 65, 65, 45, 98 } };

            yield return new object[] { "+", 0, 1, new byte[] { 43, 45 } };

            yield return new object[] { string.Empty, 0, 0, new byte[0] };
        }

        [Theory]
        [MemberData(nameof(Encode_Basic_TestData))]
        public void Encode(string source, int index, int count, byte[] expected)
        {
            Encode(true, source, index, count, expected);
            Encode(false, source, index, count, expected);
        }

        public static IEnumerable<object[]> Encode_Advanced_TestData()
        {
            string optionalChars1 = "!\"#$%&*;<=>@[]^_`{|}";
            byte[] optionalFalseBytes = new byte[] 
            {
                43, 65, 67, 69, 65, 73, 103, 65,
                106, 65, 67, 81, 65, 74, 81, 65,
                109, 65, 67, 111, 65, 79, 119, 65,
                56, 65, 68, 48, 65, 80, 103, 66, 65,
                65, 70, 115, 65, 88, 81, 66, 101, 65,
                70, 56, 65, 89, 65, 66, 55, 65,
                72, 119, 65, 102, 81, 45
            };
            byte[] optionalTrueBytes = new byte[]
            {
                33, 34, 35, 36, 37, 38, 42, 59, 60, 61, 62,
                64, 91, 93, 94, 95, 96, 123, 124, 125
            };

            yield return new object[] { false, optionalChars1, 0, optionalChars1.Length, optionalFalseBytes };
            yield return new object[] { true, optionalChars1, 0, optionalChars1.Length, optionalTrueBytes };
            
            yield return new object[] { false, "\u0023\u0025\u03a0\u03a3", 1, 2, new byte[] { 43, 65, 67, 85, 68, 111, 65, 45 } };
            yield return new object[] { true, "\u0023\u0025\u03a0\u03a3", 1, 2, new byte[] { 37, 43, 65, 54, 65, 45 } };
        }

        [Theory]
        [MemberData(nameof(Encode_Advanced_TestData))]
        public void Encode(bool allowOptionals, string source, int index, int count, byte[] expected)
        {
            EncodingHelpers.Encode(new UTF7Encoding(allowOptionals), source, index, count, expected);
        }

        [Fact]
        public void Encode_InvalidUnicode()
        {
            // TODO: add into Encode_TestData once #7166 is fixed
            Encode("\uD800", 0, 1, new byte[] { 43, 50, 65, 65, 45 }); // Lone high surrogate
            Encode("\uDC00", 0, 1, new byte[] { 43, 51, 65, 65, 45 }); // Lone low surrogate
            Encode("\uD800\uDC00", 0, 1, new byte[] { 43, 50, 65, 65, 45 }); // Surrogate pair out of range
            Encode("\uD800\uDC00", 1, 1, new byte[] { 43, 51, 65, 65, 45 }); // Surrogate pair out of range

            Encode("\uD800\uD800", 0, 2, new byte[] { 43, 50, 65, 68, 89, 65, 65, 45 }); // High, high
            Encode("\uDC00\uD800", 0, 2, new byte[] { 43, 51, 65, 68, 89, 65, 65, 45 }); // Low, high
            Encode("\uDC00\uDC00", 0, 2, new byte[] { 43, 51, 65, 68, 99, 65, 65, 45 }); // Low, low

            // High BMP non-chars
            Encode("\uFFFD", 0, 1, new byte[] { 43, 47, 47, 48, 45 });
            Encode("\uFFFE", 0, 1, new byte[] { 43, 47, 47, 52, 45 });
            Encode("\uFFFF", 0, 1, new byte[] { 43, 47, 47, 56, 45 });
        }
    }
}
