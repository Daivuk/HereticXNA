namespace HereticXNA
{
	partial class FrmGlobalAmbient
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
			this.label7 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.sldSaturation = new System.Windows.Forms.TrackBar();
			this.label4 = new System.Windows.Forms.Label();
			this.sldHue = new System.Windows.Forms.TrackBar();
			this.sldBrightness = new System.Windows.Forms.TrackBar();
			((System.ComponentModel.ISupportInitialize)(this.sldSaturation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldHue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.sldBrightness)).BeginInit();
			this.SuspendLayout();
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 107);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 13);
			this.label7.TabIndex = 10;
			this.label7.Text = "Brightness";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 60);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 13);
			this.label5.TabIndex = 11;
			this.label5.Text = "Saturation";
			// 
			// sldSaturation
			// 
			this.sldSaturation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldSaturation.LargeChange = 45;
			this.sldSaturation.Location = new System.Drawing.Point(6, 79);
			this.sldSaturation.Maximum = 100;
			this.sldSaturation.Name = "sldSaturation";
			this.sldSaturation.Size = new System.Drawing.Size(234, 45);
			this.sldSaturation.TabIndex = 7;
			this.sldSaturation.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldSaturation.Scroll += new System.EventHandler(this.sldSaturation_Scroll);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(27, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Hue";
			// 
			// sldHue
			// 
			this.sldHue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldHue.LargeChange = 45;
			this.sldHue.Location = new System.Drawing.Point(6, 28);
			this.sldHue.Maximum = 360;
			this.sldHue.Name = "sldHue";
			this.sldHue.Size = new System.Drawing.Size(234, 45);
			this.sldHue.TabIndex = 8;
			this.sldHue.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldHue.Scroll += new System.EventHandler(this.sldHue_Scroll);
			// 
			// sldBrightness
			// 
			this.sldBrightness.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.sldBrightness.LargeChange = 45;
			this.sldBrightness.Location = new System.Drawing.Point(6, 130);
			this.sldBrightness.Maximum = 150;
			this.sldBrightness.Name = "sldBrightness";
			this.sldBrightness.Size = new System.Drawing.Size(234, 45);
			this.sldBrightness.TabIndex = 7;
			this.sldBrightness.TickStyle = System.Windows.Forms.TickStyle.None;
			this.sldBrightness.Scroll += new System.EventHandler(this.sldBrightness_Scroll);
			// 
			// FrmGlobalAmbient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 168);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.sldBrightness);
			this.Controls.Add(this.sldSaturation);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.sldHue);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FrmGlobalAmbient";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Global Ambient";
			((System.ComponentModel.ISupportInitialize)(this.sldSaturation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldHue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.sldBrightness)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TrackBar sldSaturation;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TrackBar sldHue;
		private System.Windows.Forms.TrackBar sldBrightness;
	}
}