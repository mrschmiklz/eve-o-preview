using System.Drawing;
using System.Windows.Forms;

namespace EveOPreview.View
{
	/// <summary>
	/// CheckedListBox that never calls the base visual-styles checkbox renderer.
	/// </summary>
	internal sealed class DarkCheckedListBox : CheckedListBox
	{
		public DarkCheckedListBox()
		{
			this.DrawMode = DrawMode.OwnerDrawFixed;
			this.ItemHeight = this.Font.Height + 8;
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			MainFormTheme.DrawCheckedListItem(this, e);
		}
	}
}
