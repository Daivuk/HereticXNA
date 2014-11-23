namespace HereticXNA
{
	partial class FrmMobj
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
			this.lblMobType = new System.Windows.Forms.Label();
			this.picMobSprite = new System.Windows.Forms.PictureBox();
			this.sldTopIllumination = new System.Windows.Forms.TrackBar();
			this.label1 = new System.Windows.Forms.Label();
			this.sldBottomIllumination = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.sldHue = new System.Windows.Forms.TrackBar();
			this.label4 = new System.Windows.Forms.Label();
			this.sldSaturation = new System.Windows.Forms.TrackBar();
			this.label5 = new System.Windows.Forms.Label();
			this.btnRemoveColor = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.txtMultiplier = new System.Windows.Forms.TextBox();
			this.sldRadius = new System.Windows.Forms.TrackBar();
			this.label6 = new System.Windows.Forms.Label();
			this.cboLightType = new System.Windows.Forms.ComboBox();
			this.label8 = new System.Windows.Forms.Label();
			this.chkCastShadow = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.picMobSprite)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldTopIllumination)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldBottomIllumination)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldHue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldSaturation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldRadius)).BeginInit();
			this.SuspendLayout();
			// 
			// lblMobType
			// 
			this.lblMobType.AutoSize = true;
			this.lblMobType.Location = new System.Drawing.Point(52, 9);
			this.lblMobType.Name = "lblMobType";
			this.lblMobType.Size = new System.Drawing.Size(62, 13);
			this.lblMobType.TabIndex = 0;
			this.lblMobType.Text = "lblMobType";
			// 
			// picMobSprite
			// 
			this.picMobSprite.BackColor = System.Drawing.Color.Black;
			this.picMobSprite.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.picMobSprite.Location = new System.Drawing.Point(12, 25);
			this.picMobSprite.Name = "picMobSprite";
			this.picMobSprite.Size = new System.Drawing.Size(96, 96);
			this.picMobSprite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.picMobSprite.TabIndex = 1;
			this.picMobSprite.TabStop = false;
			// 
			// sldTopIllumination
			// 
			this.sldTopIllumination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldTopIllumination.LargeChange = 50;
			this.sldTopIllumination.Location = new System.Drawing.Point(12, 140);
			this.sldTopIllumination.Maximum = 100;
			this.sldTopIllumination.Name = "sldTopIllumination";
			this.sldTopIllumination.Size = new System.Drawing.Size(224, 45);
			this.sldTopIllumination.TabIndex = 2;
			this.sldTopIllumination.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldTopIllumination.Scroll += new System.EventHandler(this.sldTopIllumination_Scroll);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 124);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Top Illumination";
			// 
			// sldBottomIllumination
			// 
			this.sldBottomIllumination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldBottomIllumination.LargeChange = 50;
			this.sldBottomIllumination.Location = new System.Drawing.Point(12, 188);
			this.sldBottomIllumination.Maximum = 100;
			this.sldBottomIllumination.Name = "sldBottomIllumination";
			this.sldBottomIllumination.Size = new System.Drawing.Size(224, 45);
			this.sldBottomIllumination.TabIndex = 2;
			this.sldBottomIllumination.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldBottomIllumination.Scroll += new System.EventHandler(this.sldBottomIllumination_Scroll);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 172);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(95, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Bottom Illumination";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(34, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Type:";
			// 
			// sldHue
			// 
			this.sldHue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldHue.LargeChange = 45;
			this.sldHue.Location = new System.Drawing.Point(12, 287);
			this.sldHue.Maximum = 360;
			this.sldHue.Name = "sldHue";
			this.sldHue.Size = new System.Drawing.Size(224, 45);
			this.sldHue.TabIndex = 2;
			this.sldHue.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldHue.Scroll += new System.EventHandler(this.sldHue_Scroll);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 268);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(27, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Hue";
			// 
			// sldSaturation
			// 
			this.sldSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldSaturation.LargeChange = 45;
			this.sldSaturation.Location = new System.Drawing.Point(12, 338);
			this.sldSaturation.Maximum = 100;
			this.sldSaturation.Name = "sldSaturation";
			this.sldSaturation.Size = new System.Drawing.Size(224, 45);
			this.sldSaturation.TabIndex = 2;
			this.sldSaturation.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldSaturation.Scroll += new System.EventHandler(this.sldSaturation_Scroll);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 319);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 13);
			this.label5.TabIndex = 3;
			this.label5.Text = "Saturation";
			// 
			// btnRemoveColor
			// 
			this.btnRemoveColor.Location = new System.Drawing.Point(111, 25);
			this.btnRemoveColor.Name = "btnRemoveColor";
			this.btnRemoveColor.Size = new System.Drawing.Size(94, 23);
			this.btnRemoveColor.TabIndex = 4;
			this.btnRemoveColor.Text = "Add Light";
			this.btnRemoveColor.UseVisualStyleBackColor = true;
			this.btnRemoveColor.Click += new System.EventHandler(this.btnRemoveColor_Click);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(12, 366);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 13);
			this.label7.TabIndex = 3;
			this.label7.Text = "Brightness";
			// 
			// txtMultiplier
			// 
			this.txtMultiplier.Location = new System.Drawing.Point(74, 363);
			this.txtMultiplier.Name = "txtMultiplier";
			this.txtMultiplier.Size = new System.Drawing.Size(48, 20);
			this.txtMultiplier.TabIndex = 6;
			this.txtMultiplier.Text = "1";
			this.txtMultiplier.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.txtMultiplier.TextChanged += new System.EventHandler(this.txtMultiplier_TextChanged);
			// 
			// sldRadius
			// 
			this.sldRadius.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldRadius.Location = new System.Drawing.Point(12, 236);
			this.sldRadius.Maximum = 1000;
			this.sldRadius.Name = "sldRadius";
			this.sldRadius.Size = new System.Drawing.Size(224, 45);
			this.sldRadius.TabIndex = 2;
			this.sldRadius.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldRadius.Scroll += new System.EventHandler(this.sldRadius_Scroll);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 220);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 13);
			this.label6.TabIndex = 3;
			this.label6.Text = "Radius";
			// 
			// cboLightType
			// 
			this.cboLightType.FormattingEnabled = true;
			this.cboLightType.Items.AddRange(new object[] {
            "None",
            "Candle",
            "CandleLight",
            "Strobe",
            "Pulse",
            "Slow Pulse"});
			this.cboLightType.Location = new System.Drawing.Point(111, 67);
			this.cboLightType.Name = "cboLightType";
			this.cboLightType.Size = new System.Drawing.Size(121, 21);
			this.cboLightType.TabIndex = 7;
			this.cboLightType.Text = "None";
			this.cboLightType.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(108, 51);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(60, 13);
			this.label8.TabIndex = 8;
			this.label8.Text = "Light Type:";
			// 
			// chkCastShadow
			// 
			this.chkCastShadow.AutoSize = true;
			this.chkCastShadow.Location = new System.Drawing.Point(115, 95);
			this.chkCastShadow.Name = "chkCastShadow";
			this.chkCastShadow.Size = new System.Drawing.Size(89, 17);
			this.chkCastShadow.TabIndex = 9;
			this.chkCastShadow.Text = "Cast Shadow";
			this.chkCastShadow.UseVisualStyleBackColor = true;
			this.chkCastShadow.CheckedChanged += new System.EventHandler(this.chkCastShadow_CheckedChanged);
			// 
			// FrmMobj
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 397);
			this.Controls.Add(this.chkCastShadow);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.cboLightType);
			this.Controls.Add(this.txtMultiplier);
			this.Controls.Add(this.btnRemoveColor);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.sldSaturation);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.sldHue);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.sldRadius);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.sldBottomIllumination);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.sldTopIllumination);
			this.Controls.Add(this.picMobSprite);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblMobType);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FrmMobj";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Map Object Type";
			((System.ComponentModel.ISupportInitialize)(this.picMobSprite)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldTopIllumination)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldBottomIllumination)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldHue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldSaturation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldRadius)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblMobType;
		private System.Windows.Forms.PictureBox picMobSprite;
		private System.Windows.Forms.TrackBar sldTopIllumination;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TrackBar sldBottomIllumination;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TrackBar sldHue;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TrackBar sldSaturation;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnRemoveColor;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txtMultiplier;
		private System.Windows.Forms.TrackBar sldRadius;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cboLightType;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.CheckBox chkCastShadow;
	}
}