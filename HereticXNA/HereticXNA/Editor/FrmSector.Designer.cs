namespace HereticXNA
{
	partial class FrmSector
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
			this.picFloor = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.picCeil = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.picSector = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.picFloor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picCeil)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.picSector)).BeginInit();
			this.SuspendLayout();
			// 
			// picFloor
			// 
			this.picFloor.BackColor = System.Drawing.Color.Black;
			this.picFloor.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.picFloor.Location = new System.Drawing.Point(12, 28);
			this.picFloor.Name = "picFloor";
			this.picFloor.Size = new System.Drawing.Size(96, 96);
			this.picFloor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.picFloor.TabIndex = 0;
			this.picFloor.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Floor texture";
			// 
			// picCeil
			// 
			this.picCeil.BackColor = System.Drawing.Color.Black;
			this.picCeil.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.picCeil.Location = new System.Drawing.Point(114, 28);
			this.picCeil.Name = "picCeil";
			this.picCeil.Size = new System.Drawing.Size(96, 96);
			this.picCeil.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.picCeil.TabIndex = 0;
			this.picCeil.TabStop = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(114, 12);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(59, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Ceil texture";
			// 
			// picSector
			// 
			this.picSector.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.picSector.BackColor = System.Drawing.Color.Black;
			this.picSector.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.picSector.Location = new System.Drawing.Point(50, 130);
			this.picSector.Name = "picSector";
			this.picSector.Size = new System.Drawing.Size(140, 140);
			this.picSector.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.picSector.TabIndex = 0;
			this.picSector.TabStop = false;
			// 
			// FrmSector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(248, 277);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.picCeil);
			this.Controls.Add(this.picSector);
			this.Controls.Add(this.picFloor);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FrmSector";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Sector";
			((System.ComponentModel.ISupportInitialize)(this.picFloor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picCeil)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.picSector)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox picFloor;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox picCeil;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox picSector;
	}
}