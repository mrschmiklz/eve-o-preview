using EveOPreview.UI.Hotkeys;
using System.Windows.Forms;
using Xunit;

namespace EveOPreview.Tests
{
	public class InputBindingHelperTests
	{
		[Fact]
		public void ParseKeyboardBinding_ReturnsF14()
		{
			Assert.Equal(Keys.F14, InputBindingHelper.ParseKeyboardBinding("F14"));
		}

		[Fact]
		public void ParseKeyboardBinding_IgnoresMouseBindings()
		{
			Assert.Equal(Keys.None, InputBindingHelper.ParseKeyboardBinding("MouseXButton1"));
		}

		[Fact]
		public void ParseKeyboardBinding_IgnoresBlankBindings()
		{
			Assert.Equal(Keys.None, InputBindingHelper.ParseKeyboardBinding(""));
			Assert.Equal(Keys.None, InputBindingHelper.ParseKeyboardBinding("   "));
		}

		[Theory]
		[InlineData("MouseXButton1", "Mouse Button 4 (Rear)")]
		[InlineData("MouseXButton2", "Mouse Button 5 (Front)")]
		[InlineData("MouseMiddle", "Middle Click")]
		[InlineData("", "Not set")]
		public void ToDisplayString_FormatsKnownBindings(string binding, string expected)
		{
			Assert.Equal(expected, InputBindingHelper.ToDisplayString(binding));
		}

		[Theory]
		[InlineData("MouseXButton1")]
		[InlineData("MouseXButton2")]
		[InlineData("MouseMiddle")]
		public void TryParseMouseBinding_ParsesKnownMouseBindings(string binding)
		{
			Assert.True(InputBindingHelper.TryParseMouseBinding(binding, out _));
		}

		[Fact]
		public void TryParseMouseBinding_RejectsKeyboardBindings()
		{
			Assert.False(InputBindingHelper.TryParseMouseBinding("F14", out _));
		}

		[Fact]
		public void FormatKeyboardBinding_RoundTripsF14()
		{
			Assert.Equal("F14", InputBindingHelper.FormatKeyboardBinding(Keys.F14));
		}
	}
}
