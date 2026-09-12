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

			MainPanel.SuspendLayout();
			ClientCycleBindingsGroupBox.SuspendLayout();
			ClientsGroupBox.SuspendLayout();
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

			MainPanel.Controls.Add(ClientsGroupBox);
			MainPanel.Controls.Add(HideCaptionOnClientsCheckBox);
			MainPanel.Controls.Add(AnimationStyleLabel);
			MainPanel.Controls.Add(AnimationStyleCombo);
			MainPanel.Controls.Add(MinimizeInactiveClientsCheckBox);
			MainPanel.Controls.Add(ClientCycleBindingsGroupBox);
			MainPanel.Controls.Add(MinimizeToTrayCheckBox);
			MainPanel.Dock = DockStyle.Fill;
			MainPanel.Location = new Point(0, 0);
			MainPanel.Name = "MainPanel";
			MainPanel.Padding = new Padding(12, 14, 12, 10);
			MainPanel.Size = new Size(416, 408);
			MainPanel.TabIndex = 0;

			MinimizeToTrayCheckBox.AutoSize = true;
			MinimizeToTrayCheckBox.Location = new Point(12, 14);
			MinimizeToTrayCheckBox.Name = "MinimizeToTrayCheckBox";
			MinimizeToTrayCheckBox.Size = new Size(141, 19);
			MinimizeToTrayCheckBox.TabIndex = 0;
			MinimizeToTrayCheckBox.Text = "Minimize to system tray";
			MinimizeToTrayCheckBox.UseVisualStyleBackColor = true;
			MinimizeToTrayCheckBox.CheckedChanged += OptionChanged_Handler;

			ClientCycleBindingsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			ClientCycleBindingsGroupBox.Controls.Add(CycleBackwardBindingTextBox);
			ClientCycleBindingsGroupBox.Controls.Add(CycleBackwardRecordButton);
			ClientCycleBindingsGroupBox.Controls.Add(CycleBackwardBindingLabel);
			ClientCycleBindingsGroupBox.Controls.Add(CycleForwardBindingTextBox);
			ClientCycleBindingsGroupBox.Controls.Add(CycleForwardRecordButton);
			ClientCycleBindingsGroupBox.Controls.Add(CycleForwardBindingLabel);
			ClientCycleBindingsGroupBox.Controls.Add(ClientCycleBindingHintLabel);
			ClientCycleBindingsGroupBox.Location = new Point(12, 44);
			ClientCycleBindingsGroupBox.Name = "ClientCycleBindingsGroupBox";
			ClientCycleBindingsGroupBox.Size = new Size(392, 142);
			ClientCycleBindingsGroupBox.TabIndex = 1;
			ClientCycleBindingsGroupBox.TabStop = false;
			ClientCycleBindingsGroupBox.Text = "Mouse and keyboard bindings";

			ClientCycleBindingHintLabel.AutoSize = true;
			ClientCycleBindingHintLabel.Location = new Point(10, 28);
			ClientCycleBindingHintLabel.MaximumSize = new Size(370, 0);
			ClientCycleBindingHintLabel.Name = "ClientCycleBindingHintLabel";
			ClientCycleBindingHintLabel.Text = "Click the box, then press a key or mouse button. Defaults: Mouse 4 = next client, Mouse 5 = minimize all.";

			CycleForwardBindingLabel.AutoSize = true;
			CycleForwardBindingLabel.Location = new Point(10, 66);
			CycleForwardBindingLabel.Name = "CycleForwardBindingLabel";
			CycleForwardBindingLabel.Size = new Size(118, 15);
			CycleForwardBindingLabel.Text = "Cycle to next client";

			CycleForwardRecordButton.Location = new Point(136, 62);
			CycleForwardRecordButton.Name = "CycleForwardRecordButton";
			CycleForwardRecordButton.Size = new Size(24, 24);
			CycleForwardRecordButton.TabIndex = 2;
			CycleForwardRecordButton.UseVisualStyleBackColor = true;
			CycleForwardRecordButton.Click += CycleForwardRecordButton_Click;

			CycleForwardBindingTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			CycleForwardBindingTextBox.Location = new Point(168, 63);
			CycleForwardBindingTextBox.Name = "CycleForwardBindingTextBox";
			CycleForwardBindingTextBox.ReadOnly = true;
			CycleForwardBindingTextBox.Size = new Size(214, 23);
			CycleForwardBindingTextBox.TabIndex = 3;
			CycleForwardBindingTextBox.TabStop = false;

			CycleBackwardBindingLabel.AutoSize = true;
			CycleBackwardBindingLabel.Location = new Point(10, 98);
			CycleBackwardBindingLabel.Name = "CycleBackwardBindingLabel";
			CycleBackwardBindingLabel.Size = new Size(114, 15);
			CycleBackwardBindingLabel.Text = "Minimize all clients";

			CycleBackwardRecordButton.Location = new Point(136, 94);
			CycleBackwardRecordButton.Name = "CycleBackwardRecordButton";
			CycleBackwardRecordButton.Size = new Size(24, 24);
			CycleBackwardRecordButton.TabIndex = 4;
			CycleBackwardRecordButton.UseVisualStyleBackColor = true;
			CycleBackwardRecordButton.Click += CycleBackwardRecordButton_Click;

			CycleBackwardBindingTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			CycleBackwardBindingTextBox.Location = new Point(168, 95);
			CycleBackwardBindingTextBox.Name = "CycleBackwardBindingTextBox";
			CycleBackwardBindingTextBox.ReadOnly = true;
			CycleBackwardBindingTextBox.Size = new Size(214, 23);
			CycleBackwardBindingTextBox.TabIndex = 5;
			CycleBackwardBindingTextBox.TabStop = false;

			MinimizeInactiveClientsCheckBox.AutoSize = true;
			MinimizeInactiveClientsCheckBox.Location = new Point(12, 198);
			MinimizeInactiveClientsCheckBox.Name = "MinimizeInactiveClientsCheckBox";
			MinimizeInactiveClientsCheckBox.Size = new Size(168, 19);
			MinimizeInactiveClientsCheckBox.TabIndex = 2;
			MinimizeInactiveClientsCheckBox.Text = "Minimize inactive EVE clients";
			MinimizeInactiveClientsCheckBox.UseVisualStyleBackColor = true;
			MinimizeInactiveClientsCheckBox.CheckedChanged += OptionChanged_Handler;

			AnimationStyleLabel.AutoSize = true;
			AnimationStyleLabel.Location = new Point(12, 228);
			AnimationStyleLabel.Name = "AnimationStyleLabel";
			AnimationStyleLabel.Size = new Size(96, 15);
			AnimationStyleLabel.Text = "Animation style";

			AnimationStyleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
			AnimationStyleCombo.FormattingEnabled = true;
			AnimationStyleCombo.Location = new Point(120, 224);
			AnimationStyleCombo.Name = "AnimationStyleCombo";
			AnimationStyleCombo.Size = new Size(180, 23);
			AnimationStyleCombo.TabIndex = 3;
			AnimationStyleCombo.SelectedIndexChanged += OptionChanged_Handler;

			HideCaptionOnClientsCheckBox.AutoSize = true;
			HideCaptionOnClientsCheckBox.Location = new Point(12, 258);
			HideCaptionOnClientsCheckBox.Name = "HideCaptionOnClientsCheckBox";
			HideCaptionOnClientsCheckBox.Size = new Size(168, 19);
			HideCaptionOnClientsCheckBox.TabIndex = 4;
			HideCaptionOnClientsCheckBox.Text = "Hide caption bar on clients";
			HideCaptionOnClientsCheckBox.UseVisualStyleBackColor = true;
			HideCaptionOnClientsCheckBox.CheckedChanged += OptionChanged_Handler;

			ClientsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			ClientsGroupBox.Controls.Add(ThumbnailsList);
			ClientsGroupBox.Controls.Add(ThumbnailsListLabel);
			ClientsGroupBox.Location = new Point(12, 288);
			ClientsGroupBox.Name = "ClientsGroupBox";
			ClientsGroupBox.Size = new Size(392, 112);
			ClientsGroupBox.TabIndex = 5;
			ClientsGroupBox.TabStop = false;
			ClientsGroupBox.Text = "EVE clients";

			ThumbnailsListLabel.AutoSize = true;
			ThumbnailsListLabel.Location = new Point(10, 28);
			ThumbnailsListLabel.Name = "ThumbnailsListLabel";
			ThumbnailsListLabel.Size = new Size(248, 15);
			ThumbnailsListLabel.Text = "Check a client to keep it open (never auto-minimize)";

			ThumbnailsList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			ThumbnailsList.CheckOnClick = true;
			ThumbnailsList.FormattingEnabled = true;
			ThumbnailsList.IntegralHeight = false;
			ThumbnailsList.Location = new Point(10, 48);
			ThumbnailsList.Name = "ThumbnailsList";
			ThumbnailsList.Size = new Size(372, 54);
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

			MainPanel.ResumeLayout(false);
			MainPanel.PerformLayout();
			ClientCycleBindingsGroupBox.ResumeLayout(false);
			ClientCycleBindingsGroupBox.PerformLayout();
			ClientsGroupBox.ResumeLayout(false);
			ClientsGroupBox.PerformLayout();
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
