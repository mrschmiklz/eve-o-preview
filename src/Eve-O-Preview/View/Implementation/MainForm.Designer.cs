using System.Drawing;
using System.Windows.Forms;

namespace EveOPreview.View
{
	partial class MainForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			ToolStripMenuItem RestoreWindowMenuItem;
			ToolStripMenuItem ExitMenuItem;
			ToolStripMenuItem TitleMenuItem;
			ToolStripSeparator SeparatorMenuItem;
			Label AnimationStyleLabel;
			TableLayoutPanel MainLayout;
			TableLayoutPanel BindingsLayout;
			TableLayoutPanel AnimationLayout;
			TableLayoutPanel ClientsLayout;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));

			MainPanel = new Panel();
			FooterPanel = new Panel();
			MinimizeToTrayCheckBox = new CheckBox();
			ClientCycleBindingsGroupBox = new GroupBox();
			ClientCycleBindingHintLabel = new Label();
			CycleForwardBindingLabel = new Label();
			CycleForwardRecordButton = new Button();
			CycleForwardBindingTextBox = new TextBox();
			CycleBackwardBindingLabel = new Label();
			CycleBackwardRecordButton = new Button();
			CycleBackwardBindingTextBox = new TextBox();
			MinimizeInactiveClientsCheckBox = new CheckBox();
			AnimationStyleCombo = new ComboBox();
			HideCaptionOnClientsCheckBox = new CheckBox();
			ClientsGroupBox = new GroupBox();
			ThumbnailsListLabel = new Label();
			ThumbnailsList = new DarkCheckedListBox();
			VersionLabel = new Label();
			DocumentationLink = new LinkLabel();
			NotifyIcon = new NotifyIcon(components);
			TrayMenu = new ContextMenuStrip(components);
			RestoreWindowMenuItem = new ToolStripMenuItem();
			ExitMenuItem = new ToolStripMenuItem();
			TitleMenuItem = new ToolStripMenuItem();
			SeparatorMenuItem = new ToolStripSeparator();
			AnimationStyleLabel = new Label();
			MainLayout = new TableLayoutPanel();
			BindingsLayout = new TableLayoutPanel();
			AnimationLayout = new TableLayoutPanel();
			ClientsLayout = new TableLayoutPanel();

			MainPanel.SuspendLayout();
			MainLayout.SuspendLayout();
			ClientCycleBindingsGroupBox.SuspendLayout();
			BindingsLayout.SuspendLayout();
			AnimationLayout.SuspendLayout();
			ClientsGroupBox.SuspendLayout();
			ClientsLayout.SuspendLayout();
			FooterPanel.SuspendLayout();
			TrayMenu.SuspendLayout();
			SuspendLayout();

			RestoreWindowMenuItem.Name = "RestoreWindowMenuItem";
			RestoreWindowMenuItem.Size = new Size(153, 22);
			RestoreWindowMenuItem.Text = "Restore";
			RestoreWindowMenuItem.Click += RestoreMainForm_Handler;

			ExitMenuItem.Name = "ExitMenuItem";
			ExitMenuItem.Size = new Size(153, 22);
			ExitMenuItem.Text = "Exit";
			ExitMenuItem.Click += ExitMenuItemClick_Handler;

			TitleMenuItem.Enabled = false;
			TitleMenuItem.Name = "TitleMenuItem";
			TitleMenuItem.Size = new Size(153, 22);
			TitleMenuItem.Text = "EVE-O-Preview";

			SeparatorMenuItem.Name = "SeparatorMenuItem";
			SeparatorMenuItem.Size = new Size(150, 6);

			MainPanel.Controls.Add(MainLayout);
			MainPanel.Dock = DockStyle.Fill;
			MainPanel.Location = new Point(0, 0);
			MainPanel.Name = "MainPanel";
			MainPanel.Padding = new Padding(12, 14, 12, 10);
			MainPanel.Size = new Size(416, 408);
			MainPanel.TabIndex = 0;

			MainLayout.BackColor = Color.Transparent;
			MainLayout.ColumnCount = 1;
			MainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			MainLayout.Controls.Add(MinimizeToTrayCheckBox, 0, 0);
			MainLayout.Controls.Add(ClientCycleBindingsGroupBox, 0, 1);
			MainLayout.Controls.Add(MinimizeInactiveClientsCheckBox, 0, 2);
			MainLayout.Controls.Add(AnimationLayout, 0, 3);
			MainLayout.Controls.Add(HideCaptionOnClientsCheckBox, 0, 4);
			MainLayout.Controls.Add(ClientsGroupBox, 0, 5);
			MainLayout.Dock = DockStyle.Fill;
			MainLayout.Location = new Point(12, 14);
			MainLayout.Name = "MainLayout";
			MainLayout.RowCount = 6;
			MainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			MainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 172F));
			MainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			MainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			MainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			MainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			MainLayout.TabIndex = 0;

			MinimizeToTrayCheckBox.AutoSize = true;
			MinimizeToTrayCheckBox.Dock = DockStyle.Fill;
			MinimizeToTrayCheckBox.Margin = new Padding(0, 0, 0, 10);
			MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
			MinimizeToTrayCheckBox.Size = new Size(141, 19);
			MinimizeToTrayCheckBox.TabIndex = 0;
			MinimizeToTrayCheckBox.Text = "Minimize to system tray";
			MinimizeToTrayCheckBox.UseVisualStyleBackColor = true;
			MinimizeToTrayCheckBox.CheckedChanged += OptionChanged_Handler;

			ClientCycleBindingsGroupBox.Controls.Add(BindingsLayout);
			ClientCycleBindingsGroupBox.Dock = DockStyle.Fill;
			ClientCycleBindingsGroupBox.Margin = new Padding(0, 0, 0, 10);
			ClientCycleBindingsGroupBox.Name = "ClientCycleBindingsGroupBox";
			ClientCycleBindingsGroupBox.Padding = new Padding(10, 16, 10, 8);
			ClientCycleBindingsGroupBox.TabIndex = 1;
			ClientCycleBindingsGroupBox.TabStop = false;
			ClientCycleBindingsGroupBox.Text = "Mouse and keyboard bindings";

			BindingsLayout.BackColor = Color.Transparent;
			BindingsLayout.ColumnCount = 3;
			BindingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			BindingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32F));
			BindingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			BindingsLayout.Controls.Add(ClientCycleBindingHintLabel, 0, 0);
			BindingsLayout.Controls.Add(CycleForwardBindingLabel, 0, 1);
			BindingsLayout.Controls.Add(CycleForwardRecordButton, 1, 1);
			BindingsLayout.Controls.Add(CycleForwardBindingTextBox, 2, 1);
			BindingsLayout.Controls.Add(CycleBackwardBindingLabel, 0, 2);
			BindingsLayout.Controls.Add(CycleBackwardRecordButton, 1, 2);
			BindingsLayout.Controls.Add(CycleBackwardBindingTextBox, 2, 2);
			BindingsLayout.Dock = DockStyle.Fill;
			BindingsLayout.Name = "BindingsLayout";
			BindingsLayout.RowCount = 3;
			BindingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			BindingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			BindingsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			BindingsLayout.TabIndex = 0;

			ClientCycleBindingHintLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			ClientCycleBindingHintLabel.AutoSize = true;
			BindingsLayout.SetColumnSpan(ClientCycleBindingHintLabel, 3);
			ClientCycleBindingHintLabel.Margin = new Padding(0, 4, 0, 8);
			ClientCycleBindingHintLabel.MaximumSize = new Size(360, 0);
			ClientCycleBindingHintLabel.Name = "ClientCycleBindingHintLabel";
			ClientCycleBindingHintLabel.Text = "Click the box, then press a key or mouse button. Defaults: Mouse 4 = next client, Mouse 5 = minimize all.";

			CycleForwardBindingLabel.Anchor = AnchorStyles.Left;
			CycleForwardBindingLabel.AutoSize = true;
			CycleForwardBindingLabel.Margin = new Padding(0, 0, 8, 0);
			CycleForwardBindingLabel.Name = "CycleForwardBindingLabel";
			CycleForwardBindingLabel.Text = "Cycle to next client";
			CycleForwardBindingLabel.TextAlign = ContentAlignment.MiddleLeft;

			CycleForwardRecordButton.Dock = DockStyle.Fill;
			CycleForwardRecordButton.Margin = new Padding(0, 2, 6, 2);
			CycleForwardRecordButton.Name = "CycleForwardRecordButton";
			CycleForwardRecordButton.TabIndex = 2;
			CycleForwardRecordButton.Text = "...";
			CycleForwardRecordButton.UseVisualStyleBackColor = true;
			CycleForwardRecordButton.Click += CycleForwardRecordButton_Click;

			CycleForwardBindingTextBox.Cursor = Cursors.Hand;
			CycleForwardBindingTextBox.Dock = DockStyle.Fill;
			CycleForwardBindingTextBox.Margin = new Padding(0, 2, 0, 2);
			CycleForwardBindingTextBox.Name = "CycleForwardBindingTextBox";
			CycleForwardBindingTextBox.ReadOnly = true;
			CycleForwardBindingTextBox.TabIndex = 3;
			CycleForwardBindingTextBox.TabStop = false;
			CycleForwardBindingTextBox.Click += CycleForwardRecordButton_Click;

			CycleBackwardBindingLabel.Anchor = AnchorStyles.Left;
			CycleBackwardBindingLabel.AutoSize = true;
			CycleBackwardBindingLabel.Margin = new Padding(0, 0, 8, 0);
			CycleBackwardBindingLabel.Name = "CycleBackwardBindingLabel";
			CycleBackwardBindingLabel.Text = "Minimize all clients";
			CycleBackwardBindingLabel.TextAlign = ContentAlignment.MiddleLeft;

			CycleBackwardRecordButton.Dock = DockStyle.Fill;
			CycleBackwardRecordButton.Margin = new Padding(0, 2, 6, 2);
			CycleBackwardRecordButton.Name = "CycleBackwardRecordButton";
			CycleBackwardRecordButton.TabIndex = 4;
			CycleBackwardRecordButton.Text = "...";
			CycleBackwardRecordButton.UseVisualStyleBackColor = true;
			CycleBackwardRecordButton.Click += CycleBackwardRecordButton_Click;

			CycleBackwardBindingTextBox.Cursor = Cursors.Hand;
			CycleBackwardBindingTextBox.Dock = DockStyle.Fill;
			CycleBackwardBindingTextBox.Margin = new Padding(0, 2, 0, 2);
			CycleBackwardBindingTextBox.Name = "CycleBackwardBindingTextBox";
			CycleBackwardBindingTextBox.ReadOnly = true;
			CycleBackwardBindingTextBox.TabIndex = 5;
			CycleBackwardBindingTextBox.TabStop = false;
			CycleBackwardBindingTextBox.Click += CycleBackwardRecordButton_Click;

			MinimizeInactiveClientsCheckBox.AutoSize = true;
			MinimizeInactiveClientsCheckBox.Dock = DockStyle.Fill;
			MinimizeInactiveClientsCheckBox.Margin = new Padding(0, 0, 0, 10);
			MinimizeInactiveClientsCheckBox.Name = "MinimizeInactiveClientsCheckBox";
			MinimizeInactiveClientsCheckBox.Size = new Size(168, 19);
			MinimizeInactiveClientsCheckBox.TabIndex = 2;
			MinimizeInactiveClientsCheckBox.Text = "Minimize inactive EVE clients";
			MinimizeInactiveClientsCheckBox.UseVisualStyleBackColor = true;
			MinimizeInactiveClientsCheckBox.CheckedChanged += OptionChanged_Handler;

			AnimationLayout.AutoSize = true;
			AnimationLayout.BackColor = Color.Transparent;
			AnimationLayout.ColumnCount = 2;
			AnimationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			AnimationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			AnimationLayout.Controls.Add(AnimationStyleLabel, 0, 0);
			AnimationLayout.Controls.Add(AnimationStyleCombo, 1, 0);
			AnimationLayout.Dock = DockStyle.Fill;
			AnimationLayout.Margin = new Padding(0, 0, 0, 10);
			AnimationLayout.Name = "AnimationLayout";
			AnimationLayout.RowCount = 1;
			AnimationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			AnimationLayout.TabIndex = 3;

			AnimationStyleLabel.Anchor = AnchorStyles.Left;
			AnimationStyleLabel.AutoSize = true;
			AnimationStyleLabel.Margin = new Padding(0, 0, 8, 0);
			AnimationStyleLabel.Name = "AnimationStyleLabel";
			AnimationStyleLabel.Text = "Animation style";
			AnimationStyleLabel.TextAlign = ContentAlignment.MiddleLeft;

			AnimationStyleCombo.Anchor = AnchorStyles.Left;
			AnimationStyleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			AnimationStyleCombo.FormattingEnabled = true;
			AnimationStyleCombo.Margin = new Padding(0, 1, 0, 1);
			AnimationStyleCombo.Name = "AnimationStyleCombo";
			AnimationStyleCombo.Size = new Size(180, 23);
			AnimationStyleCombo.TabIndex = 3;
			AnimationStyleCombo.SelectedIndexChanged += OptionChanged_Handler;

			HideCaptionOnClientsCheckBox.AutoSize = true;
			HideCaptionOnClientsCheckBox.Dock = DockStyle.Fill;
			HideCaptionOnClientsCheckBox.Margin = new Padding(0, 0, 0, 10);
			HideCaptionOnClientsCheckBox.Name = "HideCaptionOnClientsCheckBox";
			HideCaptionOnClientsCheckBox.Size = new Size(168, 19);
			HideCaptionOnClientsCheckBox.TabIndex = 4;
			HideCaptionOnClientsCheckBox.Text = "Hide caption bar on clients";
			HideCaptionOnClientsCheckBox.UseVisualStyleBackColor = true;
			HideCaptionOnClientsCheckBox.CheckedChanged += OptionChanged_Handler;

			ClientsGroupBox.Controls.Add(ClientsLayout);
			ClientsGroupBox.Dock = DockStyle.Fill;
			ClientsGroupBox.Margin = new Padding(0);
			ClientsGroupBox.Name = "ClientsGroupBox";
			ClientsGroupBox.Padding = new Padding(10, 16, 10, 8);
			ClientsGroupBox.TabIndex = 5;
			ClientsGroupBox.TabStop = false;
			ClientsGroupBox.Text = "EVE clients";

			ClientsLayout.BackColor = Color.Transparent;
			ClientsLayout.ColumnCount = 1;
			ClientsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			ClientsLayout.Controls.Add(ThumbnailsListLabel, 0, 0);
			ClientsLayout.Controls.Add(ThumbnailsList, 0, 1);
			ClientsLayout.Dock = DockStyle.Fill;
			ClientsLayout.Name = "ClientsLayout";
			ClientsLayout.RowCount = 2;
			ClientsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			ClientsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			ClientsLayout.TabIndex = 0;

			ThumbnailsListLabel.AutoSize = true;
			ThumbnailsListLabel.Dock = DockStyle.Fill;
			ThumbnailsListLabel.Margin = new Padding(0, 4, 0, 6);
			ThumbnailsListLabel.Name = "ThumbnailsListLabel";
			ThumbnailsListLabel.Text = "Check a client to exclude it from cycling";

			ThumbnailsList.CheckOnClick = true;
			ThumbnailsList.Dock = DockStyle.Fill;
			ThumbnailsList.FormattingEnabled = true;
			ThumbnailsList.IntegralHeight = false;
			ThumbnailsList.Margin = new Padding(0);
			ThumbnailsList.Name = "ThumbnailsList";
			ThumbnailsList.TabIndex = 0;
			ThumbnailsList.ItemCheck += ThumbnailsList_ItemCheck_Handler;

			FooterPanel.Controls.Add(DocumentationLink);
			FooterPanel.Controls.Add(VersionLabel);
			FooterPanel.Dock = DockStyle.Bottom;
			FooterPanel.Location = new Point(0, 408);
			FooterPanel.Name = "FooterPanel";
			FooterPanel.Padding = new Padding(12, 6, 12, 10);
			FooterPanel.Size = new Size(416, 56);
			FooterPanel.TabIndex = 1;

			VersionLabel.AutoSize = true;
			VersionLabel.Location = new Point(12, 8);
			VersionLabel.Name = "VersionLabel";
			VersionLabel.Size = new Size(42, 15);
			VersionLabel.Text = "1.0.0";

			DocumentationLink.AutoSize = true;
			DocumentationLink.Location = new Point(12, 26);
			DocumentationLink.Name = "DocumentationLink";
			DocumentationLink.Size = new Size(120, 15);
			DocumentationLink.TabStop = true;
			DocumentationLink.Text = "Forum thread";
			DocumentationLink.LinkClicked += DocumentationLinkClicked_Handler;

			NotifyIcon.ContextMenuStrip = TrayMenu;
			NotifyIcon.Icon = (Icon)resources.GetObject("NotifyIcon.Icon");
			NotifyIcon.Text = "EVE-O-Preview";
			NotifyIcon.Visible = true;
			NotifyIcon.MouseDoubleClick += RestoreMainForm_Handler;

			TrayMenu.ImageScalingSize = new Size(24, 24);
			TrayMenu.Items.AddRange(new ToolStripItem[] { TitleMenuItem, RestoreWindowMenuItem, SeparatorMenuItem, ExitMenuItem });
			TrayMenu.Name = "TrayMenu";
			TrayMenu.Size = new Size(154, 76);

			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(45, 45, 48);
			ClientSize = new Size(416, 464);
			Controls.Add(MainPanel);
			Controls.Add(FooterPanel);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			Icon = (Icon)resources.GetObject("$this.Icon");
			MaximizeBox = false;
			MinimumSize = new Size(416, 464);
			Name = "MainForm";
			Text = "EVE-O-Preview";
			TopMost = true;
			FormClosing += MainFormClosing_Handler;
			Resize += MainFormResize_Handler;

			MainLayout.ResumeLayout(false);
			MainLayout.PerformLayout();
			BindingsLayout.ResumeLayout(false);
			BindingsLayout.PerformLayout();
			AnimationLayout.ResumeLayout(false);
			AnimationLayout.PerformLayout();
			ClientsLayout.ResumeLayout(false);
			ClientsLayout.PerformLayout();
			MainPanel.ResumeLayout(false);
			ClientCycleBindingsGroupBox.ResumeLayout(false);
			ClientsGroupBox.ResumeLayout(false);
			FooterPanel.ResumeLayout(false);
			FooterPanel.PerformLayout();
			TrayMenu.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private Panel MainPanel;
		private Panel FooterPanel;
		private CheckBox MinimizeToTrayCheckBox;
		private GroupBox ClientCycleBindingsGroupBox;
		private Label ClientCycleBindingHintLabel;
		private Label CycleForwardBindingLabel;
		private Button CycleForwardRecordButton;
		private TextBox CycleForwardBindingTextBox;
		private Label CycleBackwardBindingLabel;
		private Button CycleBackwardRecordButton;
		private TextBox CycleBackwardBindingTextBox;
		private CheckBox MinimizeInactiveClientsCheckBox;
		private ComboBox AnimationStyleCombo;
		private CheckBox HideCaptionOnClientsCheckBox;
		private GroupBox ClientsGroupBox;
		private Label ThumbnailsListLabel;
		private DarkCheckedListBox ThumbnailsList;
		private Label VersionLabel;
		private LinkLabel DocumentationLink;
		private NotifyIcon NotifyIcon;
		private ContextMenuStrip TrayMenu;
	}
}
