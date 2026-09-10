using EveOPreview.View;
using System.Windows.Forms;
using Xunit;

namespace EveOPreview.Tests
{
	public class ThumbnailDescriptionTests
	{
		[Fact]
		public void ToString_ReturnsTitle()
		{
			ThumbnailDescription description = new ThumbnailDescription("My Client", false);

			Assert.Equal("My Client", description.ToString());
		}

		[Fact]
		public void GetCheckedListItemText_UsesTitleInsteadOfTypeName()
		{
			using CheckedListBox listBox = new CheckedListBox();
			listBox.DisplayMember = "Title";
			listBox.Items.Add(new ThumbnailDescription("Pilot One", false));

			Assert.Equal("Pilot One", MainFormTheme.GetCheckedListItemText(listBox, 0));
		}

		[Fact]
		public void GetCheckedListItemText_FallsBackToToStringWithoutDisplayMember()
		{
			using CheckedListBox listBox = new CheckedListBox();
			listBox.Items.Add(new ThumbnailDescription("Pilot Two", true));

			Assert.Equal("Pilot Two", MainFormTheme.GetCheckedListItemText(listBox, 0));
		}

		[Fact]
		public void GetCheckedListItemText_ReturnsEmptyForInvalidIndex()
		{
			using CheckedListBox listBox = new CheckedListBox();

			Assert.Equal(string.Empty, MainFormTheme.GetCheckedListItemText(listBox, 0));
			Assert.Equal(string.Empty, MainFormTheme.GetCheckedListItemText(null, 0));
		}
	}
}
