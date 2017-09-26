using StagWare.ExtensionMethods;
using System;
using Xunit;

namespace StagWare.FanControl.Configurations.Tests.ExtensionMethods
{
    public class StringExtensionMethodsTests
    {
        public class GetLongestCommonSubstring
        {
            [Theory]
            [InlineData("1234567#-34fm", "uue4567sdfof", "4567")]
            [InlineData("kdgbdg1234567mgm", "1234567", "1234567")]
            [InlineData("1234567", "12345", "12345")]
            [InlineData("123456789", "123456789", "123456789")]
            [InlineData("", "", "")]
            [InlineData("", "123", "")]
            [InlineData("123", "", "")]
            [InlineData("123", "2", "2")]
            public void FindsLongestSubstring(string s1, string s2, string result)
            {
                Assert.Equal(result, s1.GetLongestCommonSubstring(s2));
            }

            [Fact]
            public void ThrowsIfStringIsNull()
            {
                Assert.Throws<ArgumentNullException>(() => "".GetLongestCommonSubstring(null));
            }
        }
    }
}
