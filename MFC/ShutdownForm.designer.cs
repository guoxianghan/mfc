namespace MFC
{
    partial class ShutdownForm
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.threadNameLabel = new System.Windows.Forms.Label();
            this.finishedThreads = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 25);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(268, 23);
            this.progressBar.Step = 1;
            this.progressBar.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Stopping MFC core modules...";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Please wait...";
            // 
            // threadNameLabel
            // 
            this.threadNameLabel.AutoSize = true;
            this.threadNameLabel.Location = new System.Drawing.Point(88, 60);
            this.threadNameLabel.Name = "threadNameLabel";
            this.threadNameLabel.Size = new System.Drawing.Size(0, 13);
            this.threadNameLabel.TabIndex = 3;
            // 
            // finishedThreads
            // 
            this.finishedThreads.FormattingEnabled = true;
            this.finishedThreads.Location = new System.Drawing.Point(12, 86);
            this.finishedThreads.Name = "finishedThreads";
            this.finishedThreads.Size = new System.Drawing.Size(268, 95);
            this.finishedThreads.TabIndex = 4;
            // 
            // ShutdownForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 195);
            this.Controls.Add(this.finishedThreads);
            this.Controls.Add(this.threadNameLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ShutdownForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shutdown";
            this.Activated += new System.EventHandler(this.ShutdownFormActivated);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label threadNameLabel;
        private System.Windows.Forms.ListBox finishedThreads;
    }
}