namespace ParseAws
{
    partial class AWSLogParser
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
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.buttonConsolidateLogs = new System.Windows.Forms.Button();
            this.labelPath = new System.Windows.Forms.Label();
            this.buttonRun = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(23, 30);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(124, 23);
            this.buttonChooseFolder.TabIndex = 0;
            this.buttonChooseFolder.Text = "Choose Folder";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // buttonConsolidateLogs
            // 
            this.buttonConsolidateLogs.Location = new System.Drawing.Point(23, 200);
            this.buttonConsolidateLogs.Name = "buttonConsolidateLogs";
            this.buttonConsolidateLogs.Size = new System.Drawing.Size(124, 23);
            this.buttonConsolidateLogs.TabIndex = 1;
            this.buttonConsolidateLogs.Text = "Consolidate Logs";
            this.buttonConsolidateLogs.UseVisualStyleBackColor = true;
            this.buttonConsolidateLogs.Click += new System.EventHandler(this.buttonConsolidateLogs_Click);
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(170, 40);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(35, 13);
            this.labelPath.TabIndex = 2;
            this.labelPath.Text = "label1";
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(23, 75);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(124, 23);
            this.buttonRun.TabIndex = 3;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(170, 85);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(35, 13);
            this.labelStatus.TabIndex = 4;
            this.labelStatus.Text = "label1";
            // 
            // AWSLogParser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 262);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.labelPath);
            this.Controls.Add(this.buttonConsolidateLogs);
            this.Controls.Add(this.buttonChooseFolder);
            this.Name = "AWSLogParser";
            this.Text = "AWS Log Parser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.Button buttonConsolidateLogs;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Label labelStatus;
    }
}

