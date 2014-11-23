namespace HereticXNA
{
	partial class FrmFloor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.sldSelfIllumination = new System.Windows.Forms.TrackBar();
			this.label7 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.sldSaturation = new System.Windows.Forms.TrackBar();
			this.label4 = new System.Windows.Forms.Label();
			this.sldHue = new System.Windows.Forms.TrackBar();
			this.label6 = new System.Windows.Forms.Label();
			this.sldDistance = new System.Windows.Forms.TrackBar();
			this.btnAddLight = new System.Windows.Forms.Button();
			this.sldSpread = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.sldBrightness = new System.Windows.Forms.TrackBar();
			this.cboType = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.sldSelfIllumination)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldSaturation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldHue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldDistance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldSpread)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldBrightness)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Self Illumination";
			// 
			// sldSelfIllumination
			// 
			this.sldSelfIllumination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldSelfIllumination.LargeChange = 50;
			this.sldSelfIllumination.Location = new System.Drawing.Point(8, 25);
			this.sldSelfIllumination.Maximum = 100;
			this.sldSelfIllumination.Name = "sldSelfIllumination";
			this.sldSelfIllumination.Size = new System.Drawing.Size(234, 45);
			this.sldSelfIllumination.TabIndex = 4;
			this.sldSelfIllumination.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldSelfIllumination.Scroll += new System.EventHandler(this.sldSelfIllumination_Scroll);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 277);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 13);
			this.label7.TabIndex = 13;
			this.label7.Text = "Brightness";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(8, 230);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Saturation";
			// 
			// sldSaturation
			// 
			this.sldSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldSaturation.LargeChange = 45;
			this.sldSaturation.Location = new System.Drawing.Point(8, 249);
			this.sldSaturation.Maximum = 100;
			this.sldSaturation.Name = "sldSaturation";
			this.sldSaturation.Size = new System.Drawing.Size(234, 45);
			this.sldSaturation.TabIndex = 9;
			this.sldSaturation.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldSaturation.Value = 50;
			this.sldSaturation.Scroll += new System.EventHandler(this.sldSaturation_Scroll);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 179);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(27, 13);
			this.label4.TabIndex = 11;
			this.label4.Text = "Hue";
			// 
			// sldHue
			// 
			this.sldHue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldHue.LargeChange = 45;
			this.sldHue.Location = new System.Drawing.Point(8, 198);
			this.sldHue.Maximum = 360;
			this.sldHue.Name = "sldHue";
			this.sldHue.Size = new System.Drawing.Size(234, 45);
			this.sldHue.TabIndex = 8;
			this.sldHue.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldHue.Scroll += new System.EventHandler(this.sldHue_Scroll);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(8, 83);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(49, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "Distance";
			// 
			// sldDistance
			// 
			this.sldDistance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldDistance.Location = new System.Drawing.Point(8, 99);
			this.sldDistance.Maximum = 1000;
			this.sldDistance.Name = "sldDistance";
			this.sldDistance.Size = new System.Drawing.Size(234, 45);
			this.sldDistance.TabIndex = 7;
			this.sldDistance.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldDistance.Scroll += new System.EventHandler(this.sldDistance_Scroll);
			// 
			// btnAddLight
			// 
			this.btnAddLight.Location = new System.Drawing.Point(12, 54);
			this.btnAddLight.Name = "btnAddLight";
			this.btnAddLight.Size = new System.Drawing.Size(94, 23);
			this.btnAddLight.TabIndex = 15;
			this.btnAddLight.Text = "Add Light";
			this.btnAddLight.UseVisualStyleBackColor = true;
			this.btnAddLight.Click += new System.EventHandler(this.btnAddLight_Click);
			// 
			// sldSpread
			// 
			this.sldSpread.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldSpread.Location = new System.Drawing.Point(8, 147);
			this.sldSpread.Maximum = 30;
			this.sldSpread.Minimum = 1;
			this.sldSpread.Name = "sldSpread";
			this.sldSpread.Size = new System.Drawing.Size(234, 45);
			this.sldSpread.TabIndex = 7;
			this.sldSpread.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldSpread.Value = 1;
			this.sldSpread.Scroll += new System.EventHandler(this.sldSpread_Scroll);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 131);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(41, 13);
			this.label2.TabIndex = 10;
			this.label2.Text = "Spread";
			// 
			// sldBrightness
			// 
			this.sldBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldBrightness.LargeChange = 45;
			this.sldBrightness.Location = new System.Drawing.Point(8, 300);
			this.sldBrightness.Maximum = 1000;
			this.sldBrightness.Name = "sldBrightness";
			this.sldBrightness.Size = new System.Drawing.Size(234, 45);
			this.sldBrightness.TabIndex = 16;
			this.sldBrightness.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldBrightness.Value = 100;
			this.sldBrightness.Scroll += new System.EventHandler(this.sldBrightness_Scroll);
			// 
			// cboType
			// 
			this.cboType.FormattingEnabled = true;
			this.cboType.Items.AddRange(new object[] {
            "Floor",
            "Ceiling"});
			this.cboType.Location = new System.Drawing.Point(121, -2);
			this.cboType.Name = "cboType";
			this.cboType.Size = new System.Drawing.Size(121, 21);
			this.cboType.TabIndex = 17;
			this.cboType.Text = "Floor";
			this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Location = new System.Drawing.Point(38, 320);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(9, 13);
			this.label3.TabIndex = 18;
			this.label3.Text = "|";
			// 
			// FrmFloor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 374);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.cboType);
			this.Controls.Add(this.sldBrightness);
			this.Controls.Add(this.btnAddLight);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.sldSaturation);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.sldHue);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.sldSpread);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.sldDistance);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.sldSelfIllumination);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FrmFloor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Flat Type";
			((System.ComponentModel.ISupportInitialize)(this.sldSelfIllumination)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldSaturation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldHue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldDistance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldSpread)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldBrightness)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TrackBar sldSelfIllumination;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TrackBar sldSaturation;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TrackBar sldHue;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TrackBar sldDistance;
		private System.Windows.Forms.Button btnAddLight;
		private System.Windows.Forms.TrackBar sldSpread;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TrackBar sldBrightness;
		private System.Windows.Forms.ComboBox cboType;
		private System.Windows.Forms.Label label3;
	}
}