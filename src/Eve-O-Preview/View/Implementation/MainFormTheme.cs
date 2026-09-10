using EveOPreview.Services.Interop;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EveOPreview.View
{
	internal static class MainFormTheme
	{
		public static readonly Color Background = Color.FromArgb(45, 45, 48);
		public static readonly Color Surface = Color.FromArgb(37, 37, 38);
		public static readonly Color Input = Color.FromArgb(63, 63, 70);
		public static readonly Color Foreground = Color.FromArgb(241, 241, 241);
		public static readonly Color DisabledForeground = Color.FromArgb(130, 130, 130);
		public static readonly Color Border = Color.FromArgb(80, 80, 80);
		public static readonly Color TabStrip = Color.FromArgb(37, 37, 38);
		public static readonly Color TabSelected = Color.FromArgb(62, 62, 66);
		public static readonly Color TabUnselected = Color.FromArgb(45, 45, 48);
		public static readonly Color TabAccent = Color.FromArgb(0, 122, 204);
		public static readonly Color CaptureHighlight = Color.FromArgb(78, 65, 36);
		public static readonly Color Button = Color.FromArgb(55, 55, 58);
		public static readonly Color Link = Color.FromArgb(86, 156, 214);
		public static readonly Color Accent = Color.FromArgb(0, 122, 204);

		public static void Apply(Control root)
		{
			ApplyRecursive(root);
		}

		public static void RefreshEnabledAppearance(Control root)
		{
			RefreshEnabledRecursive(root);
		}

		public static void DrawTab(TabControl control, DrawItemEventArgs e)
		{
			TabPage page = control.TabPages[e.Index];
			Rectangle bounds = control.GetTabRect(e.Index);
			bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			Color backgroundColor = selected ? TabSelected : TabUnselected;

			bounds.Inflate(1, 1);

			using (Brush backgroundBrush = new SolidBrush(backgroundColor))
			{
				e.Graphics.FillRectangle(backgroundBrush, bounds);
			}

			if (selected)
			{
				using (Brush accentBrush = new SolidBrush(TabAccent))
				{
					e.Graphics.FillRectangle(accentBrush, bounds.Right - 3, bounds.Top + 6, 3, bounds.Height - 12);
				}
			}

			Color textColor = selected ? Foreground : Color.FromArgb(200, 200, 200);
			TextRenderer.DrawText(
				e.Graphics,
				page.Text,
				control.Font,
				Rectangle.Inflate(bounds, -4, 0),
				textColor,
				TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
		}

		private static void ApplyRecursive(Control control)
		{
			switch (control)
			{
				case Form form:
					form.BackColor = Background;
					form.ForeColor = Foreground;
					EnableDarkTitleBar(form);
					break;
				case TabControl tabControl:
					SetupTabControl(tabControl);
					break;
				case TabPage tabPage:
					tabPage.BackColor = Surface;
					tabPage.ForeColor = Foreground;
					tabPage.UseVisualStyleBackColor = false;
					break;
				case Panel panel:
					SetupPanel(panel);
					break;
				case GroupBox groupBox:
					SetupGroupBox(groupBox);
					break;
				case LinkLabel linkLabel:
					linkLabel.BackColor = Color.Transparent;
					linkLabel.ForeColor = Foreground;
					linkLabel.LinkColor = Link;
					linkLabel.ActiveLinkColor = Color.FromArgb(120, 180, 230);
					linkLabel.VisitedLinkColor = Color.FromArgb(180, 140, 220);
					break;
				case Label label:
					label.ForeColor = label.Enabled ? Foreground : DisabledForeground;
					label.BackColor = Color.Transparent;
					break;
				case CheckBox checkBox:
					SetupCheckBox(checkBox);
					break;
				case RadioButton radioButton:
					SetupRadioButton(radioButton);
					break;
				case TextBox textBox:
					textBox.BackColor = Input;
					textBox.ForeColor = Foreground;
					textBox.BorderStyle = BorderStyle.FixedSingle;
					DisableVisualTheme(textBox);
					break;
				case ComboBox comboBox:
					SetupComboBox(comboBox);
					break;
				case NumericUpDown numericUpDown:
					numericUpDown.BackColor = Input;
					numericUpDown.ForeColor = Foreground;
					numericUpDown.BorderStyle = BorderStyle.FixedSingle;
					DisableVisualTheme(numericUpDown);
					break;
				case TrackBar trackBar:
					trackBar.BackColor = Surface;
					DisableVisualTheme(trackBar);
					break;
				case DarkCheckedListBox darkCheckedListBox:
					SetupDarkCheckedListBox(darkCheckedListBox);
					break;
				case CheckedListBox checkedListBox:
					SetupDarkCheckedListBox(checkedListBox);
					break;
				case ListBox listBox:
					listBox.BackColor = Input;
					listBox.ForeColor = Foreground;
					listBox.BorderStyle = BorderStyle.FixedSingle;
					DisableVisualTheme(listBox);
					break;
				case Button button:
					button.UseVisualStyleBackColor = false;
					button.BackColor = Button;
					button.ForeColor = Foreground;
					button.FlatStyle = FlatStyle.Flat;
					button.FlatAppearance.BorderColor = Border;
					button.FlatAppearance.BorderSize = 1;
					DisableVisualTheme(button);
					break;
			}

			foreach (Control child in control.Controls)
			{
				ApplyRecursive(child);
			}
		}

		private static void RefreshEnabledRecursive(Control control)
		{
			switch (control)
			{
				case Label label:
					label.ForeColor = label.Enabled ? Foreground : DisabledForeground;
					break;
				case CheckBox checkBox:
					checkBox.ForeColor = checkBox.Enabled ? Foreground : DisabledForeground;
					checkBox.Invalidate();
					break;
				case RadioButton radioButton:
					radioButton.ForeColor = radioButton.Enabled ? Foreground : DisabledForeground;
					break;
				case GroupBox groupBox:
					groupBox.ForeColor = groupBox.Enabled ? Foreground : DisabledForeground;
					break;
			}

			foreach (Control child in control.Controls)
			{
				RefreshEnabledRecursive(child);
			}
		}

		private static void SetupComboBox(ComboBox comboBox)
		{
			comboBox.BackColor = Input;
			comboBox.ForeColor = Foreground;
			comboBox.FlatStyle = FlatStyle.Flat;
			DisableVisualTheme(comboBox);
		}

		public static void DrawCheckedListItem(CheckedListBox listBox, DrawItemEventArgs e)
		{
			if (e.Index < 0)
			{
				return;
			}

			bool isChecked = listBox.GetItemChecked(e.Index);
			bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			Color backColor = selected ? TabSelected : Input;
			Color textColor = Foreground;

			using (Brush backBrush = new SolidBrush(backColor))
			{
				e.Graphics.FillRectangle(backBrush, e.Bounds);
			}

			const int boxSize = 16;
			int boxY = e.Bounds.Top + ((e.Bounds.Height - boxSize) / 2);
			Rectangle box = new Rectangle(e.Bounds.Left + 4, boxY, boxSize, boxSize);

			using (Pen borderPen = new Pen(Border))
			{
				e.Graphics.DrawRectangle(borderPen, box.X, box.Y, box.Width - 1, box.Height - 1);

				if (isChecked)
				{
					using (Brush fillBrush = new SolidBrush(Accent))
					{
						e.Graphics.FillRectangle(fillBrush, box.X + 3, box.Y + 3, box.Width - 6, box.Height - 6);
					}
				}
			}

			string text = GetCheckedListItemText(listBox, e.Index);
			Rectangle textBounds = new Rectangle(box.Right + 6, e.Bounds.Top, e.Bounds.Width - box.Right - 10, e.Bounds.Height);
			TextRenderer.DrawText(
				e.Graphics,
				text,
				listBox.Font,
				textBounds,
				textColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
		}

		private static void SetupDarkCheckedListBox(CheckedListBox checkedListBox)
		{
			if (checkedListBox is DarkCheckedListBox darkCheckedListBox)
			{
				darkCheckedListBox.ItemHeight = darkCheckedListBox.Font.Height + 8;
			}
			else
			{
				checkedListBox.DrawMode = DrawMode.OwnerDrawFixed;
				checkedListBox.ItemHeight = checkedListBox.Font.Height + 8;
			}

			checkedListBox.BackColor = Input;
			checkedListBox.ForeColor = Foreground;
			checkedListBox.BorderStyle = BorderStyle.FixedSingle;
			DisableVisualTheme(checkedListBox);
		}

		private static void SetupTabControl(TabControl tabControl)
		{
			tabControl.BackColor = TabStrip;
			tabControl.ForeColor = Foreground;
			tabControl.Padding = new Point(8, 3);
			tabControl.Paint += TabControl_Paint;
			DisableVisualTheme(tabControl);
		}

		private static void TabControl_Paint(object sender, PaintEventArgs e)
		{
			if (sender is not TabControl tabControl)
			{
				return;
			}

			Rectangle display = tabControl.DisplayRectangle;
			using (Brush brush = new SolidBrush(TabStrip))
			{
				e.Graphics.FillRectangle(brush, tabControl.ClientRectangle);

				e.Graphics.FillRectangle(brush, 0, 0, display.X, tabControl.Height);
				e.Graphics.FillRectangle(brush, 0, 0, tabControl.Width, display.Y);
				e.Graphics.FillRectangle(brush, display.Right, 0, tabControl.Width - display.Right, tabControl.Height);
				e.Graphics.FillRectangle(brush, 0, display.Bottom, tabControl.Width, tabControl.Height - display.Bottom);
			}

			using (Pen seamPen = new Pen(Surface))
			{
				e.Graphics.DrawLine(seamPen, display.X - 1, display.Y - 1, display.X - 1, display.Bottom);
				e.Graphics.DrawLine(seamPen, display.X - 1, display.Y - 1, display.Right, display.Y - 1);
				e.Graphics.DrawLine(seamPen, display.Right, display.Y - 1, display.Right, display.Bottom);
				e.Graphics.DrawLine(seamPen, display.X - 1, display.Bottom, display.Right, display.Bottom);
			}
		}

		private static void SetupPanel(Panel panel)
		{
			if (panel.BackColor != Color.Transparent)
			{
				panel.BackColor = Surface;
			}

			panel.ForeColor = Foreground;

			if (panel.AutoScroll)
			{
				panel.HorizontalScroll.Enabled = false;
				panel.HorizontalScroll.Visible = false;
			}

			if (panel.BorderStyle == BorderStyle.FixedSingle)
			{
				panel.BorderStyle = BorderStyle.None;
				panel.Paint += BorderedPanel_Paint;
			}
		}

		private static void BorderedPanel_Paint(object sender, PaintEventArgs e)
		{
			if (sender is not Panel panel)
			{
				return;
			}

			using (Pen borderPen = new Pen(Border))
			{
				e.Graphics.DrawRectangle(borderPen, 0, 0, panel.Width - 1, panel.Height - 1);
			}
		}

		internal static string GetCheckedListItemText(CheckedListBox listBox, int index)
		{
			if (listBox == null || index < 0 || index >= listBox.Items.Count)
			{
				return string.Empty;
			}

			object item = listBox.Items[index];
			if (item is IThumbnailDescription description && !string.IsNullOrEmpty(description.Title))
			{
				return description.Title;
			}

			string text = listBox.GetItemText(item);
			if (!string.IsNullOrEmpty(text) && text != item?.GetType().FullName && text != item?.GetType().Name)
			{
				return text;
			}

			return item?.ToString() ?? string.Empty;
		}

		private static void SetupGroupBox(GroupBox groupBox)
		{
			groupBox.BackColor = Surface;
			groupBox.ForeColor = Foreground;
			groupBox.Paint += GroupBox_Paint;
			DisableVisualTheme(groupBox);
		}

		private static void GroupBox_Paint(object sender, PaintEventArgs e)
		{
			if (sender is not GroupBox groupBox)
			{
				return;
			}

			Size textSize = TextRenderer.MeasureText(groupBox.Text, groupBox.Font);
			const int headerLineY = 11;
			int textY = headerLineY - (textSize.Height / 2);
			int textX = 12;
			int textEnd = textX + textSize.Width + 6;
			Color textColor = groupBox.Enabled ? Foreground : DisabledForeground;

			e.Graphics.Clear(groupBox.BackColor);

			using (Pen borderPen = new Pen(Border))
			{
				e.Graphics.DrawLine(borderPen, 0, headerLineY, textX, headerLineY);
				e.Graphics.DrawLine(borderPen, textEnd, headerLineY, groupBox.Width - 1, headerLineY);
				e.Graphics.DrawLine(borderPen, 0, headerLineY, 0, groupBox.Height - 1);
				e.Graphics.DrawLine(borderPen, groupBox.Width - 1, headerLineY, groupBox.Width - 1, groupBox.Height - 1);
				e.Graphics.DrawLine(borderPen, 0, groupBox.Height - 1, groupBox.Width - 1, groupBox.Height - 1);
			}

			TextRenderer.DrawText(
				e.Graphics,
				groupBox.Text,
				groupBox.Font,
				new Point(textX, Math.Max(2, textY)),
				textColor,
				TextFormatFlags.Left);
		}

		private static void SetupCheckBox(CheckBox checkBox)
		{
			checkBox.UseVisualStyleBackColor = false;
			checkBox.FlatStyle = FlatStyle.Flat;
			checkBox.FlatAppearance.BorderSize = 0;
			checkBox.BackColor = Surface;
			checkBox.ForeColor = checkBox.Enabled ? Foreground : DisabledForeground;
			checkBox.Paint += CheckBox_Paint;
			checkBox.EnabledChanged += ThemedControl_EnabledChanged;
			DisableVisualTheme(checkBox);
		}

		private static void SetupRadioButton(RadioButton radioButton)
		{
			radioButton.UseVisualStyleBackColor = false;
			radioButton.FlatStyle = FlatStyle.Flat;
			radioButton.FlatAppearance.BorderSize = 0;
			radioButton.BackColor = Surface;
			radioButton.ForeColor = radioButton.Enabled ? Foreground : DisabledForeground;
			radioButton.Paint += RadioButton_Paint;
			radioButton.EnabledChanged += ThemedControl_EnabledChanged;
			DisableVisualTheme(radioButton);
		}

		private static void ThemedControl_EnabledChanged(object sender, EventArgs e)
		{
			if (sender is CheckBox checkBox)
			{
				checkBox.ForeColor = checkBox.Enabled ? Foreground : DisabledForeground;
				checkBox.Invalidate();
			}
			else if (sender is RadioButton radioButton)
			{
				radioButton.ForeColor = radioButton.Enabled ? Foreground : DisabledForeground;
				radioButton.Invalidate();
			}
		}

		private static void CheckBox_Paint(object sender, PaintEventArgs e)
		{
			if (sender is not CheckBox checkBox)
			{
				return;
			}

			Color backColor = checkBox.Parent?.BackColor ?? Surface;
			e.Graphics.Clear(backColor);

			const int boxSize = 16;
			int boxY = (checkBox.ClientSize.Height - boxSize) / 2;
			Rectangle box = new Rectangle(0, boxY, boxSize, boxSize);
			Color textColor = checkBox.Enabled ? Foreground : DisabledForeground;

			using (Pen borderPen = new Pen(Border))
			{
				e.Graphics.DrawRectangle(borderPen, box.X, box.Y, box.Width - 1, box.Height - 1);

				if (checkBox.Checked)
				{
					using (Brush fillBrush = new SolidBrush(Accent))
					{
						e.Graphics.FillRectangle(fillBrush, box.X + 3, box.Y + 3, box.Width - 6, box.Height - 6);
					}
				}
			}

			TextRenderer.DrawText(
				e.Graphics,
				checkBox.Text,
				checkBox.Font,
				new Rectangle(boxSize + 6, 0, checkBox.Width - boxSize - 6, checkBox.Height),
				textColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
		}

		private static void RadioButton_Paint(object sender, PaintEventArgs e)
		{
			if (sender is not RadioButton radioButton)
			{
				return;
			}

			Color backColor = radioButton.Parent?.BackColor ?? Surface;
			e.Graphics.Clear(backColor);

			const int dotSize = 14;
			int dotY = (radioButton.ClientSize.Height - dotSize) / 2;
			Rectangle dot = new Rectangle(0, dotY, dotSize, dotSize);
			Color textColor = radioButton.Enabled ? Foreground : DisabledForeground;

			using (Pen borderPen = new Pen(Border))
			{
				e.Graphics.DrawEllipse(borderPen, dot);

				if (radioButton.Checked)
				{
					using (Brush fillBrush = new SolidBrush(Accent))
					{
						e.Graphics.FillEllipse(fillBrush, dot.X + 4, dot.Y + 4, dot.Width - 8, dot.Height - 8);
					}
				}
			}

			TextRenderer.DrawText(
				e.Graphics,
				radioButton.Text,
				radioButton.Font,
				new Rectangle(dotSize + 6, 0, radioButton.Width - dotSize - 6, radioButton.Height),
				textColor,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
		}

		private static void DisableVisualTheme(Control control)
		{
			void ApplyTheme()
			{
				if (control.IsHandleCreated)
				{
					UxThemeNativeMethods.SetWindowTheme(control.Handle, string.Empty, string.Empty);
				}
			}

			control.HandleCreated += (_, _) => ApplyTheme();
			ApplyTheme();
		}

		private static void EnableDarkTitleBar(Form form)
		{
			void ApplyDarkMode()
			{
				if (!form.IsHandleCreated)
				{
					return;
				}

				const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
				int useDarkMode = 1;
				DwmNativeMethods.DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
			}

			form.HandleCreated += (_, _) => ApplyDarkMode();
			ApplyDarkMode();
		}
	}
}
