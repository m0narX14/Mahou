﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
namespace Mahou
{
	public partial class MahouForm : Form
	{
		#region Variables
		// Hotkeys, HKC => HotKey Convert
		public Hotkey Mainhk, ExitHk, HKCLast, HKCSelection, HKCLine, HKSymIgn;
		bool messagebox;
		// Temporary modifiers
		string tempCLMods = "None", tempCSMods = "None", tempCLineMods = "None";
		// Temporary keys
		int tempCLKey, tempCSKey, tempCLineKey;
		// Temporary locales
		Locales.Locale tempLoc1 = new Locales.Locale { Lang = "dummy", uId = 0 },
					   tempLoc2 = new Locales.Locale { Lang = "dummy", uId = 0	};
		public TrayIcon icon;
		public Update update = new Update();
		public Timer ICheck = new Timer();
		public LangDisplay langDisplay = new LangDisplay();
		MoreConfigs moreConfigs = new MoreConfigs();
		#endregion
		public MahouForm()
		{
			InitializeComponent();
			ICheck.Tick += (_, __) => {
				if (ICheckings.IsICursor())
					langDisplay.ShowInactiveTopmost();
				else
					langDisplay.HideWnd();
				langDisplay.Location = new Point(Cursor.Position.X + MMain.MyConfs.ReadInt("TTipUI", "xpos"),
					Cursor.Position.Y + MMain.MyConfs.ReadInt("TTipUI", "ypos"));
				switch (Locales.GetCurrentLocale()) {
					case 1041:
						langDisplay.ChangeLD("Jp");
						break;
					case 1028:
					case 3076:
						langDisplay.ChangeLD("Ch");
						break;
					case 1049:
						langDisplay.ChangeLD("Ru");
						break;
					case 1033:
						langDisplay.ChangeLD("En");
						break;
					case 1055:
						langDisplay.ChangeLD("Tu");
						break;
					case 1058:
						langDisplay.ChangeLD("Ua");
						break;
					case 2074:
					case 3098:
						langDisplay.ChangeLD("Se");
						break;
					case 1040:
					case 2064:
						langDisplay.ChangeLD("It");
						break;
					case 1036:
					case 5132:
						langDisplay.ChangeLD("Fr");
						break;
					case 3082:
					case 1034:
						langDisplay.ChangeLD("Sp");
						break;
					case 1031:
					case 5127:
					case 3079:
					case 4103:
					case 2055:
						langDisplay.ChangeLD("De");
						break;
					case 1045:
						langDisplay.ChangeLD("Po");
						break;
				}
			};			
			KMHook.doublekey.Tick += (_, __) => {
				if (KMHook.hklOK)
					KMHook.hklOK = false;
				if (KMHook.hksOK)
					KMHook.hksOK = false;
				if (KMHook.hklineOK)
					KMHook.hklineOK = false;
				if (KMHook.hkSIOK)
					KMHook.hkSIOK = false;
				KMHook.doublekey.Stop();
				//Console.WriteLine("Timeout!");
			};
			langDisplay.ChangeColors(ColorTranslator.FromHtml(MMain.MyConfs.Read("Functions", "DLForeColor")),
				ColorTranslator.FromHtml(MMain.MyConfs.Read("Functions", "DLBackColor")));
			langDisplay.ChangeSizes((Font)moreConfigs.fcv.ConvertFromString(MMain.MyConfs.Read("TTipUI", "Font")), 
				MMain.MyConfs.ReadInt("TTipUI", "Height"), 
				MMain.MyConfs.ReadInt("TTipUI", "Width"));
			ICheck.Interval = MMain.MyConfs.ReadInt("Functions", "DLRefreshRate");
			if (MMain.MyConfs.ReadBool("Functions", "DisplayLang"))
				ICheck.Start();
			icon = new TrayIcon(MMain.MyConfs.ReadBool("Functions", "IconVisibility"));
			icon.Exit += exitToolStripMenuItem_Click;
			icon.ShowHide += showHideToolStripMenuItem_Click;
			RefreshIconAll();
			InitializeHotkeys();
			//Background startup check for updates
			var uche = new System.Threading.Thread(update.StartupCheck);
			uche.Name = "Startup Check";
			uche.Start();
		}
		#region Form Events
		void MahouForm_Load(object sender, EventArgs e)
		{
			Text += " " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
			tempRestore();
			RefreshControlsData();
			EnableIF();
			RemoveAddCtrls();
			RefreshLanguage();
		}
		void MahouForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing) {
				e.Cancel = true;
				ToggleVisibility();
			}
			tempRestore();
		}
		void MahouForm_VisibleChanged(object sender, EventArgs e)
		{
			IfNotExist();
			RefreshControlsData();
		}
		void MahouForm_Activated(object sender, EventArgs e)
		{
			RefreshLocales();
		}
		void MahouForm_Deactivate(object sender, EventArgs e)
		{
			RefreshLocales();
		}
		#endregion
		#region Textboxes(Hotkeyboxes)
		void tbCLHK_KeyDown(object sender, KeyEventArgs e)// Catch hotkey for Convert Last action
		{
			if (MMain.MyConfs.ReadBool("DoubleKey", "Use")) {
				tbCLHK.Text = OemReadable(Remake(e.KeyCode));
				tempCLMods = "None";
				tempCLKey = (int)e.KeyCode;				
			} else {
				tbCLHK.Text = OemReadable((e.Modifiers.ToString().Replace(",", " +") + " + " +
				Remake(e.KeyCode)).Replace("None + ", ""));
				tempCLMods = e.Modifiers.ToString().Replace(",", " +");
				switch ((int)e.KeyCode) {
					case 16:
					case 17:
					case 18:
						tempCLKey = 0;
						break;
					default:
						tempCLKey = (int)e.KeyCode;
						break;
				}
			}
		}
		void tbCLineHK_KeyDown(object sender, KeyEventArgs e) // Catch hotkey for Convert Line action
		{
			if (MMain.MyConfs.ReadBool("DoubleKey", "Use")) {
				tbCLineHK.Text = OemReadable(Remake(e.KeyCode));
				tempCLineMods = "None";
				tempCLineKey = (int)e.KeyCode;				
			} else {
				tbCLineHK.Text = OemReadable((e.Modifiers.ToString().Replace(",", " +") + " + " +
				Remake(e.KeyCode)).Replace("None + ", ""));
				tempCLineMods = e.Modifiers.ToString().Replace(",", " +");
				switch ((int)e.KeyCode) {
					case 16:
					case 17:
					case 18:
						tempCLineKey = 0;
						break;
					default:
						tempCLineKey = (int)e.KeyCode;
						break;
				}
			}
		}
		void tbCSHK_KeyDown(object sender, KeyEventArgs e)// Catch hotkey for Convert Selection action
		{
			if (MMain.MyConfs.ReadBool("DoubleKey", "Use")) {
				tbCSHK.Text = OemReadable(Remake(e.KeyCode));
				tempCSMods = "None";
				tempCSKey = (int)e.KeyCode;				
			} else {
				tbCSHK.Text = OemReadable((e.Modifiers.ToString().Replace(",", " +") + " + " +
				Remake(e.KeyCode)).Replace("None + ", ""));
				tempCSMods = e.Modifiers.ToString().Replace(",", " +");
				switch ((int)e.KeyCode) {
					case 16:
					case 17:
					case 18:
						tempCSKey = 0;
						break;
					default:
						tempCSKey = (int)e.KeyCode;
						break;
				}
			}
		}
		#endregion
		#region Comboboxes
		void cbLangOne_SelectedIndexChanged(object sender, EventArgs e)
		{
			foreach (Locales.Locale lc in MMain.locales) {
				if (cbLangOne.Text == lc.Lang + "(" + lc.uId + ")") {
					tempLoc1 = new Locales.Locale {
						Lang = lc.Lang,
						uId = lc.uId
					};
				}
			}
		}
		void cbLangTwo_SelectedIndexChanged(object sender, EventArgs e)
		{
			foreach (Locales.Locale lc in MMain.locales) {
				if (cbLangTwo.Text == lc.Lang + "(" + lc.uId + ")") {
					tempLoc2 = new Locales.Locale {
						Lang = lc.Lang,
						uId = lc.uId
					};
				}
			}
		}
		#endregion
		#region Checkboxes
		void cbCycleMode_CheckedChanged(object sender, EventArgs e)
		{
			EnableIF();
		}
		void cbCLActive_CheckedChanged(object sender, EventArgs e)
		{
			tbCLHK.Enabled = cbCLActive.Checked;
		}
		void cbCSActive_CheckedChanged(object sender, EventArgs e)
		{
			tbCSHK.Enabled = cbCSActive.Checked;
		}
		void cbCLineActive_CheckedChanged(object sender, EventArgs e)
		{
			tbCLineHK.Enabled = cbCLineActive.Checked;
		}
		void cbUseEmulate_CheckedChanged(object sender, EventArgs e)
		{
			cbELSType.Enabled = cbUseEmulate.Checked;
		}
		#endregion
		#region Buttons & link
		void btnApply_Click(object sender, EventArgs e)
		{
			Apply();
		}
		void btnCancel_Click(object sender, EventArgs e)
		{
			messagebox = false;
			RefreshControlsData();
			tempRestore();
			ToggleVisibility();
		}
		void btnOK_Click(object sender, EventArgs e)
		{
			Apply();
			ToggleVisibility();
		}
		void btnHelp_Click(object sender, EventArgs e)
		{
			messagebox = true;
			MessageBox.Show(MMain.Msgs[2], MMain.Msgs[3], MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		void GitHubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/BladeMight/Mahou");
		}
		void btnUpd_Click(object sender, EventArgs e)
		{
			update.ShowDialog();
		}
		void btnDDD_Click(object sender, EventArgs e)
		{
			moreConfigs.ShowDialog();
		}
		void btnLangChange_Click(object sender, EventArgs e)
		{
			if (MMain.MyConfs.Read("Locales", "LANGUAGE") == "RU") {
				MMain.MyConfs.Write("Locales", "LANGUAGE", "EN");
				btnLangChange.Text = "EN";
			} else if (MMain.MyConfs.Read("Locales", "LANGUAGE") == "EN") {
				MMain.MyConfs.Write("Locales", "LANGUAGE", "RU");
				btnLangChange.Text = "RU";
			}
			MMain.InitLanguage();
			RefreshLanguage();
		}
		#endregion
		#region Tray Events
		void mhouIcon_DoubleClick(object sender, EventArgs e)
		{
			ToggleVisibility();
		}
		void showHideToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToggleVisibility();
		}
		void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExitProgram();
		}
		#endregion
		#region WndProc & Functions
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == MMain.ao) { // ao = Already Opened
				ToggleVisibility();
			}
			base.WndProc(ref m);
		}
		public string Remake(Keys k, bool oninit = false) //Make readable some special keys
		{
			if (MMain.MyConfs.ReadBool("DoubleKey", "Use") || oninit) {
				switch (k) {
					case Keys.ShiftKey:
						return "Shift";
					case Keys.Menu:
						return "Alt";
					case Keys.ControlKey:
						return "Control";
				}
			}
			switch (k) {
				case Keys.Cancel:
					return k.ToString().Replace("Cancel", "Pause");
				case Keys.Scroll:
					return k.ToString().Replace("Cancel", "Scroll");
				case Keys.ShiftKey:
				case Keys.Menu:
				case Keys.ControlKey:
				case Keys.LWin:
				case Keys.RWin:
					return "";
				case Keys.D0:
				case Keys.D1:
				case Keys.D2:
				case Keys.D3:
				case Keys.D4:
				case Keys.D5:
				case Keys.D6:
				case Keys.D7:
				case Keys.D8:
				case Keys.D9:
					return k.ToString().Replace("D", "");
				default:
					return k.ToString();
			}
		}
		public string OemReadable(string inpt) //Make readable Oem Keys
		{
			return inpt
                  .Replace("Oemtilde", "`")
                  .Replace("OemMinus", "-")
                  .Replace("Oemplus", "+")
                  .Replace("OemBackslash", "\\")
                  .Replace("Oem5", "\\")
                  .Replace("OemOpenBrackets", "{")
                  .Replace("OemCloseBrackets", "}")
                  .Replace("Oem6", "}")
                  .Replace("OemSemicolon", ";")
                  .Replace("Oem1", ";")
                  .Replace("OemQuotes", "\"")
                  .Replace("Oem7", "\"")
                  .Replace("OemPeriod", ".")
                  .Replace("Oemcomma", ",")
                  .Replace("OemQuestion", "/");
		}
		void tempRestore() //Restores temporary variables from settings
		{
			//This creates(silently) new config file if existed one disappeared o_O
			IfNotExist();
			// Restores temps
			tempCLKey = MMain.MyConfs.ReadInt("Hotkeys", "HKCLKey");
			tempCSKey = MMain.MyConfs.ReadInt("Hotkeys", "HKCSKey");
			tempCLineKey = MMain.MyConfs.ReadInt("Hotkeys", "HKCLineKey");
			tempCLMods = MMain.MyConfs.Read("Hotkeys", "HKCLMods");
			tempCSMods = MMain.MyConfs.Read("Hotkeys", "HKCSMods");
			tempCLineMods = MMain.MyConfs.Read("Hotkeys", "HKCLineMods");
			tempLoc1 = tempLoc2 = new Locales.Locale {
				Lang = "dummy",
				uId = 0
			};
		}
		public void InitializeHotkeys() //Initializes all hotkeys
		{
			Mainhk = new Hotkey((int)Keys.Insert, new bool[]{ true, true, true });
			ExitHk = new Hotkey((int)Keys.F12, new bool[]{ true, true, true });
			HKCLast = new Hotkey(MMain.MyConfs.ReadInt("Hotkeys", "HKCLKey"), 
				Hotkey.GetMods(MMain.MyConfs.Read("Hotkeys", "HKCLMods")));
			HKCSelection = new Hotkey(MMain.MyConfs.ReadInt("Hotkeys", "HKCSKey"), 
				Hotkey.GetMods(MMain.MyConfs.Read("Hotkeys", "HKCSMods")));
			HKCLine = new Hotkey(MMain.MyConfs.ReadInt("Hotkeys", "HKCLineKey"), 
				Hotkey.GetMods(MMain.MyConfs.Read("Hotkeys", "HKCLineMods")));
			HKSymIgn = new Hotkey(MMain.MyConfs.ReadInt("Hotkeys", "HKSymIgnKey"), 
				Hotkey.GetMods(MMain.MyConfs.Read("Hotkeys", "HKSymIgnMods")));
		}
		public void IfNotExist()
		{
			if (!System.IO.File.Exists(System.IO.Path.Combine(Mahou.Update.nPath, "Mahou.ini"))) {
				MMain.MyConfs = new Configs();
				tempRestore();
			}
		}
		void Apply() //Saves current selections to settings
		{
			IfNotExist();
			if (tempCLKey != 0 && tempCLineKey != 0) {
				if (tempCLineKey == tempCLKey && tempCLMods == tempCLineMods) {
					messagebox = true;
					MessageBox.Show(MMain.Msgs[4], MMain.Msgs[5], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				} else
					messagebox = false;
			}

			if (!string.IsNullOrEmpty(tempCLMods) && tempCLKey != 0)
				MMain.MyConfs.Write("Hotkeys", "HKCLMods", tempCLMods);

			if (tempCLKey != 0) {
				MMain.MyConfs.Write("Hotkeys", "HKCLKey", tempCLKey.ToString());
				messagebox = false;
			} else {
				messagebox = true;
				MessageBox.Show(MMain.Msgs[6], MMain.Msgs[5], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			if (!string.IsNullOrEmpty(tempCSMods) && tempCSKey != 0)
				MMain.MyConfs.Write("Hotkeys", "HKCSMods", tempCSMods);

			if (tempCSKey != 0) {
				MMain.MyConfs.Write("Hotkeys", "HKCSKey", tempCSKey.ToString());
				messagebox = false;
			} else {
				messagebox = true;
				MessageBox.Show(MMain.Msgs[7], MMain.Msgs[5], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			if (!string.IsNullOrEmpty(tempCLineMods) && tempCLineKey != 0)
				MMain.MyConfs.Write("Hotkeys", "HKCLineMods", tempCLineMods);

			if (tempCLineKey != 0) {
				MMain.MyConfs.Write("Hotkeys", "HKCLineKey", tempCLineKey.ToString());
				messagebox = false;
			} else {
				messagebox = true;
				MessageBox.Show(MMain.Msgs[8], MMain.Msgs[5], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			if (cbAutorun.Checked) {
				CreateShortcut();
			} else {
				DeleteShortcut();
			}
			MMain.MyConfs.Write("Hotkeys", "OnlyKeyLayoutSwicth", cbSwitchLayoutKeys.Text);
			MMain.MyConfs.Write("Functions", "CycleMode", cbCycleMode.Checked.ToString());
			MMain.MyConfs.Write("Functions", "IconVisibility", cbTrayIcon.Checked.ToString());
			MMain.MyConfs.Write("Functions", "BlockCTRL", cbBlockC.Checked.ToString());
			MMain.MyConfs.Write("Functions", "CSSwitch", cbCSSwitch.Checked.ToString());
			MMain.MyConfs.Write("Functions", "EmulateLayoutSwitch", cbUseEmulate.Checked.ToString());
			MMain.MyConfs.Write("Functions", "ELSType", cbELSType.SelectedIndex.ToString());
			MMain.MyConfs.Write("Functions", "RePress", cbRePress.Checked.ToString());
			MMain.MyConfs.Write("Functions", "EatOneSpace", cbEatOneSpace.Checked.ToString());
			MMain.MyConfs.Write("Functions", "ReSelect", cbResel.Checked.ToString());
			MMain.MyConfs.Write("EnabledHotkeys", "HKCLEnabled", cbCLActive.Checked.ToString());
			MMain.MyConfs.Write("EnabledHotkeys", "HKCSEnabled", cbCSActive.Checked.ToString());
			MMain.MyConfs.Write("EnabledHotkeys", "HKCLineEnabled", cbCLineActive.Checked.ToString());
			if (tempLoc1.Lang != "dummy" || tempLoc1.uId != 0) {
				MMain.MyConfs.Write("Locales", "locale1Lang", tempLoc1.Lang);
				MMain.MyConfs.Write("Locales", "locale1uId", tempLoc1.uId.ToString());
			}
			if (tempLoc2.Lang != "dummy" || tempLoc2.uId != 0) {
				MMain.MyConfs.Write("Locales", "locale2Lang", tempLoc2.Lang);
				MMain.MyConfs.Write("Locales", "locale2uId", tempLoc2.uId.ToString());
			}
			RefreshControlsData();
			InitializeHotkeys();
		}
		void EnableIF() //Enables controls IF...
		{
			if (!cbCycleMode.Checked) {
				gbSBL.Enabled = true;
				cbUseEmulate.Enabled = cbELSType.Enabled = false;
			} else {
				gbSBL.Enabled = false;
				cbUseEmulate.Enabled = true;
				cbELSType.Enabled &= cbUseEmulate.Checked;
			}
		}
		public void ToggleVisibility() //Toggles visibility of main window
		{
			if (Visible != false)
				Visible = moreConfigs.Visible = update.Visible = false;
			else {
				TopMost = true;
				Visible = true;
				System.Threading.Thread.Sleep(5);
				TopMost = false;
			}
			Refresh();
		}
		public void RemoveAddCtrls() //Removes or adds ctrls to "Switch layout by key" items
		{
			if (MMain.MyConfs.ReadBool("ExtCtrls", "UseExtCtrls")) {
				cbSwitchLayoutKeys.SelectedIndex = 0;
				cbSwitchLayoutKeys.Items.Remove("Left Control");
				cbSwitchLayoutKeys.Items.Remove("Right Control");
			} else {
				cbSwitchLayoutKeys.Items.Remove("None");
				if (!cbSwitchLayoutKeys.Items.Contains("Left Control"))
					cbSwitchLayoutKeys.Items.Add("Left Control");
				if (!cbSwitchLayoutKeys.Items.Contains("Right Control"))
					cbSwitchLayoutKeys.Items.Add("Right Control");
				cbSwitchLayoutKeys.Items.Add("None");
			}
		}
		void RefreshControlsData() //Refresh all controls state from configs
		{
			RefreshLocales();
			RefreshIconAll();
			cbSwitchLayoutKeys.Text = MMain.MyConfs.Read("Hotkeys", "OnlyKeyLayoutSwicth");
			cbTrayIcon.Checked = MMain.MyConfs.ReadBool("Functions", "IconVisibility");
			cbCycleMode.Checked = MMain.MyConfs.ReadBool("Functions", "CycleMode");
			cbBlockC.Checked = MMain.MyConfs.ReadBool("Functions", "BlockCTRL");
			cbCSSwitch.Checked = MMain.MyConfs.ReadBool("Functions", "CSSwitch");
			cbUseEmulate.Checked = MMain.MyConfs.ReadBool("Functions", "EmulateLayoutSwitch");
			cbRePress.Checked = MMain.MyConfs.ReadBool("Functions", "RePress");
			cbEatOneSpace.Checked = MMain.MyConfs.ReadBool("Functions", "EatOneSpace");
			cbResel.Checked = MMain.MyConfs.ReadBool("Functions", "ReSelect");
			cbCLActive.Checked = MMain.MyConfs.ReadBool("EnabledHotkeys", "HKCLEnabled");
			cbCLineActive.Checked = MMain.MyConfs.ReadBool("EnabledHotkeys", "HKCLineEnabled");
			cbCSActive.Checked = MMain.MyConfs.ReadBool("EnabledHotkeys", "HKCSEnabled");
			cbELSType.SelectedIndex = MMain.MyConfs.ReadInt("Functions", "ELSType");
			btnLangChange.Text = MMain.MyConfs.Read("Locales", "LANGUAGE");
			if (!messagebox) {
				tbCLHK.Text = OemReadable((MMain.MyConfs.Read("Hotkeys", "HKCLMods").Replace(",", " +") +
				" + " + Remake((Keys)MMain.MyConfs.ReadInt("Hotkeys", "HKCLKey"), true)).Replace("None + ", ""));
				tbCSHK.Text = OemReadable((MMain.MyConfs.Read("Hotkeys", "HKCSMods").Replace(",", " +") +
				" + " + Remake((Keys)MMain.MyConfs.ReadInt("Hotkeys", "HKCSKey"), true)).Replace("None + ", ""));
				tbCLineHK.Text = OemReadable((MMain.MyConfs.Read("Hotkeys", "HKCLineMods").Replace(",", " +") +
				" + " + Remake((Keys)MMain.MyConfs.ReadInt("Hotkeys", "HKCLineKey"), true)).Replace("None + ", ""));
			}
			cbAutorun.Checked = System.IO.File.Exists(System.IO.Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Startup),
				"Mahou.lnk")) ? true : false;
			RefreshLocales();
		}
		void RefreshLocales() //Re-adds existed locales to select boxes
		{
			Locales.IfLessThan2();
			MMain.locales = Locales.AllList();
			cbLangOne.Items.Clear();
			cbLangTwo.Items.Clear();
			MMain.lcnmid.Clear();
			foreach (Locales.Locale lc in MMain.locales) {
				cbLangOne.Items.Add(lc.Lang + "(" + lc.uId + ")");
				cbLangTwo.Items.Add(lc.Lang + "(" + lc.uId + ")");
				MMain.lcnmid.Add(lc.Lang + "(" + lc.uId + ")");
			}
			try {
				cbLangOne.SelectedIndex = MMain.lcnmid.IndexOf(MMain.MyConfs.Read("Locales", "locale1Lang") + "(" + MMain.MyConfs.Read("Locales", "locale1uId") + ")");
				cbLangTwo.SelectedIndex = MMain.lcnmid.IndexOf(MMain.MyConfs.Read("Locales", "locale2Lang") + "(" + MMain.MyConfs.Read("Locales", "locale2uId") + ")");
			} catch {
				MessageBox.Show(MMain.Msgs[9], MMain.Msgs[5], MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				RefreshLocales();
				cbLangOne.SelectedIndex = 0;
				cbLangTwo.SelectedIndex = 1;
			}
		}
		void RefreshLanguage() //Refreshed in realtime all controls text
		{
			GitHubLink.Text = MMain.UI[0];
			cbAutorun.Text = MMain.UI[1];
			btnUpd.Text = MMain.UI[2];
			gbHK.Text = MMain.UI[3];
			cbCLActive.Text = MMain.UI[4] + ":";
			cbCSActive.Text = MMain.UI[5] + ":";
			cbCLineActive.Text = MMain.UI[6] + ":";
			cbCSSwitch.Text = MMain.UI[7];
			cbRePress.Text = MMain.UI[8];
			cbResel.Text = MMain.UI[9];
			lbswithlayout.Text = MMain.UI[10];
			cbBlockC.Text = MMain.UI[11];
			cbTrayIcon.Text = MMain.UI[12];
			cbCycleMode.Text = MMain.UI[13];
			cbUseEmulate.Text = MMain.UI[14];
			gbSBL.Text = MMain.UI[15];
			lbl1lng.Text = MMain.UI[16] + " 1:";
			lbl2lng.Text = MMain.UI[16] + " 2:";
			btnApply.Text = MMain.UI[17];
			btnOK.Text = MMain.UI[18];
			btnCancel.Text = MMain.UI[19];
			btnHelp.Text = MMain.UI[20];
			icon.RefreshText(MMain.UI[44], MMain.UI[42], MMain.UI[43]);
		}
		public void RefreshIconAll() //Refreshes icon's icon and visibility
		{
			if (MMain.MyConfs.ReadBool("Functions", "SymIgnModeEnabled") && MMain.MyConfs.ReadBool("EnabledHotkeys", "HKSymIgnEnabled"))
				Icon = icon.trIcon.Icon = Properties.Resources.MahouSymbolIgnoreMode;
			else
				Icon = icon.trIcon.Icon = Properties.Resources.MahouTrayHD;
			if (MMain.MyConfs.ReadBool("Functions", "IconVisibility")) {
				icon.Show();
				Refresh();
			} else {
				icon.Hide();
				Refresh();
			}
		}
		void CreateShortcut() //Creates startup shortcut v2.0, now not uses com. So whole project not need the Windows SDK :p
		{
			var exelocation = Assembly.GetExecutingAssembly().Location;
			var shortcutLocation = System.IO.Path.Combine(
				                       Environment.GetFolderPath(Environment.SpecialFolder.Startup),
				                       "Mahou.lnk");
			if (System.IO.File.Exists(shortcutLocation))
				return;
			Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
			dynamic shell = Activator.CreateInstance(t);
			try {
				var lnk = shell.CreateShortcut(shortcutLocation);
				try {
					lnk.TargetPath = exelocation;
					lnk.WorkingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					lnk.IconLocation = exelocation + ", 0";
					lnk.Description = "Mahou - Magick layout switcher";
					lnk.Save();
				} finally {
					Marshal.FinalReleaseComObject(lnk);
				}
			} finally {
				Marshal.FinalReleaseComObject(shell);
			}
		}
		void DeleteShortcut() //Deletes startup shortcut
		{
			if (System.IO.File.Exists(System.IO.Path.Combine(
				    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
				    "Mahou.lnk"))) {
				System.IO.File.Delete(System.IO.Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Startup),
					"Mahou.lnk"));
			}
		}
		public void ExitProgram()
		{
			icon.Hide();
			Refresh();
			Application.Exit();
		}
		#endregion
		#region TOOLTIPS!!!
		void cbCycleMode_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbCycleMode.Text;
			HelpTT.Show(MMain.TTips[0], cbCycleMode);
		}
		void tbCLHK_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = tbCLHK.Text;
			HelpTT.Show(MMain.TTips[1], tbCLHK);
		}
		void tbCSHK_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = tbCSHK.Text;
			HelpTT.Show(MMain.TTips[2], tbCSHK);
		}
		void tbCLineHK_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = tbCLineHK.Text;
			HelpTT.Show(MMain.TTips[3], tbCLineHK);
		}
		void GitHubLink_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = GitHubLink.Text;
			HelpTT.Show(MMain.TTips[4], GitHubLink);
		}
		void TrayIconCheckBox_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbTrayIcon.Text;
			HelpTT.Show(MMain.TTips[5], cbTrayIcon);
		}
		void btnUpd_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = btnUpd.Text;
			HelpTT.Show(MMain.TTips[6], btnUpd);
		}
		void cbBlockAC_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbBlockC.Text;
			HelpTT.Show(MMain.TTips[7], cbBlockC);
		}
		void cbSwitchLayoutKeys_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbSwitchLayoutKeys.Text;
			HelpTT.Show(MMain.TTips[8], cbSwitchLayoutKeys);
		}
		void cbUseEmulate_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbUseEmulate.Text;
			HelpTT.Show(MMain.TTips[9], cbUseEmulate);
		}
		void cbUseCycleForCS_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbCSSwitch.Text;
			HelpTT.Show(MMain.TTips[10], cbCSSwitch);
		}
		void gbSBL_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = gbSBL.Text;
			HelpTT.Show(MMain.TTips[11], gbSBL);
		}
		void cbRePress_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbRePress.Text;
			HelpTT.Show(MMain.TTips[12], cbRePress);
		}
		void cbEatOneSpace_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbEatOneSpace.Text;
			HelpTT.Show(MMain.TTips[13], cbEatOneSpace);

		}
		void cbResel_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = cbResel.Text;
			HelpTT.Show(MMain.TTips[14], cbResel);
		}
		void cbELSType_MouseHover(object sender, EventArgs e)
		{
			HelpTT.ToolTipTitle = MMain.TTips[19];
			HelpTT.Show(MMain.TTips[15], cbELSType);
		}
		#endregion
	}
}
