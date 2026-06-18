namespace OpenCV_TemplateMatching
{
    partial class Form1
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
            this.pictureBox_Source = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.Load_Template1 = new System.Windows.Forms.Button();
            this.Load_Template2 = new System.Windows.Forms.Button();
            this.Load_Template3 = new System.Windows.Forms.Button();
            this.Load_Source = new System.Windows.Forms.Button();
            this.Process = new System.Windows.Forms.Button();
            this.Clear_Template = new System.Windows.Forms.Button();
            this.Load_Template_All = new System.Windows.Forms.Button();
            this.Save_Bbox = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Source)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox_Source
            // 
            this.pictureBox_Source.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_Source.Location = new System.Drawing.Point(12, 12);
            this.pictureBox_Source.Name = "pictureBox_Source";
            this.pictureBox_Source.Size = new System.Drawing.Size(784, 630);
            this.pictureBox_Source.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Source.TabIndex = 0;
            this.pictureBox_Source.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(1028, 19);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(176, 63);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1025, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Template1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1025, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Template 2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1025, 217);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Template 3";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(1028, 128);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(176, 63);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 5;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox3.Location = new System.Drawing.Point(1028, 233);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(176, 63);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 6;
            this.pictureBox3.TabStop = false;
            // 
            // Load_Template1
            // 
            this.Load_Template1.Location = new System.Drawing.Point(947, 47);
            this.Load_Template1.Name = "Load_Template1";
            this.Load_Template1.Size = new System.Drawing.Size(75, 35);
            this.Load_Template1.TabIndex = 7;
            this.Load_Template1.Text = "Load";
            this.Load_Template1.UseVisualStyleBackColor = true;
            this.Load_Template1.Click += new System.EventHandler(this.Load_Template1_Click);
            // 
            // Load_Template2
            // 
            this.Load_Template2.Location = new System.Drawing.Point(947, 156);
            this.Load_Template2.Name = "Load_Template2";
            this.Load_Template2.Size = new System.Drawing.Size(75, 35);
            this.Load_Template2.TabIndex = 8;
            this.Load_Template2.Text = "Load";
            this.Load_Template2.UseVisualStyleBackColor = true;
            this.Load_Template2.Click += new System.EventHandler(this.Load_Template2_Click);
            // 
            // Load_Template3
            // 
            this.Load_Template3.Location = new System.Drawing.Point(947, 261);
            this.Load_Template3.Name = "Load_Template3";
            this.Load_Template3.Size = new System.Drawing.Size(75, 35);
            this.Load_Template3.TabIndex = 9;
            this.Load_Template3.Text = "Load";
            this.Load_Template3.UseVisualStyleBackColor = true;
            this.Load_Template3.Click += new System.EventHandler(this.Load_Template3_Click);
            // 
            // Load_Source
            // 
            this.Load_Source.Location = new System.Drawing.Point(802, 3);
            this.Load_Source.Name = "Load_Source";
            this.Load_Source.Size = new System.Drawing.Size(75, 35);
            this.Load_Source.TabIndex = 10;
            this.Load_Source.Text = "Load";
            this.Load_Source.UseVisualStyleBackColor = true;
            this.Load_Source.Click += new System.EventHandler(this.Load_Source_Click);
            // 
            // Process
            // 
            this.Process.Location = new System.Drawing.Point(802, 60);
            this.Process.Name = "Process";
            this.Process.Size = new System.Drawing.Size(75, 35);
            this.Process.TabIndex = 11;
            this.Process.Text = "Process";
            this.Process.UseVisualStyleBackColor = true;
            this.Process.Click += new System.EventHandler(this.Process_Click);
            // 
            // Clear_Template
            // 
            this.Clear_Template.Location = new System.Drawing.Point(1129, 394);
            this.Clear_Template.Name = "Clear_Template";
            this.Clear_Template.Size = new System.Drawing.Size(75, 35);
            this.Clear_Template.TabIndex = 12;
            this.Clear_Template.Text = "Clear Template";
            this.Clear_Template.UseVisualStyleBackColor = true;
            this.Clear_Template.Click += new System.EventHandler(this.Clear_Template_Click);
            // 
            // Load_Template_All
            // 
            this.Load_Template_All.Location = new System.Drawing.Point(947, 394);
            this.Load_Template_All.Name = "Load_Template_All";
            this.Load_Template_All.Size = new System.Drawing.Size(75, 35);
            this.Load_Template_All.TabIndex = 13;
            this.Load_Template_All.Text = "Load Template All";
            this.Load_Template_All.UseVisualStyleBackColor = true;
            this.Load_Template_All.Click += new System.EventHandler(this.Load_Template_All_Click);
            // 
            // Save_Bbox
            // 
            this.Save_Bbox.Location = new System.Drawing.Point(947, 478);
            this.Save_Bbox.Name = "Save_Bbox";
            this.Save_Bbox.Size = new System.Drawing.Size(75, 35);
            this.Save_Bbox.TabIndex = 14;
            this.Save_Bbox.Text = "Save Bbox";
            this.Save_Bbox.UseVisualStyleBackColor = true;
            this.Save_Bbox.Click += new System.EventHandler(this.Save_Bbox_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1216, 659);
            this.Controls.Add(this.Save_Bbox);
            this.Controls.Add(this.Load_Template_All);
            this.Controls.Add(this.Clear_Template);
            this.Controls.Add(this.Process);
            this.Controls.Add(this.Load_Source);
            this.Controls.Add(this.Load_Template3);
            this.Controls.Add(this.Load_Template2);
            this.Controls.Add(this.Load_Template1);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBox_Source);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Source)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox_Source;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.Button Load_Template1;
        private System.Windows.Forms.Button Load_Template2;
        private System.Windows.Forms.Button Load_Template3;
        private System.Windows.Forms.Button Load_Source;
        private System.Windows.Forms.Button Process;
        private System.Windows.Forms.Button Clear_Template;
        private System.Windows.Forms.Button Load_Template_All;
        private System.Windows.Forms.Button Save_Bbox;
    }
}

