namespace NetServerWindowsForms.Controls
{
    partial class ServerStateControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerStateControl));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.saveConfigurationToolStripMenuItemSaveConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItemLoadConfiguration = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButtonServerStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonServerStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButtonClearLog = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonLogToFile = new System.Windows.Forms.ToolStripButton();
            this.richTextBoxServerLog = new System.Windows.Forms.RichTextBox();
            this.propertyGridServerConfig = new System.Windows.Forms.PropertyGrid();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripButtonServerStart,
            this.toolStripButtonServerStop,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.toolStripButtonClearLog,
            this.toolStripButtonLogToFile});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(659, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveConfigurationToolStripMenuItemSaveConfig,
            this.loadConfigurationToolStripMenuItemLoadConfiguration});
            this.toolStripDropDownButton1.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(59, 22);
            this.toolStripDropDownButton1.Text = "Config";
            // 
            // saveConfigurationToolStripMenuItemSaveConfig
            // 
            this.saveConfigurationToolStripMenuItemSaveConfig.Name = "saveConfigurationToolStripMenuItemSaveConfig";
            this.saveConfigurationToolStripMenuItemSaveConfig.Size = new System.Drawing.Size(188, 22);
            this.saveConfigurationToolStripMenuItemSaveConfig.Text = "Save Configuration";
            // 
            // loadConfigurationToolStripMenuItemLoadConfiguration
            // 
            this.loadConfigurationToolStripMenuItemLoadConfiguration.Name = "loadConfigurationToolStripMenuItemLoadConfiguration";
            this.loadConfigurationToolStripMenuItemLoadConfiguration.Size = new System.Drawing.Size(188, 22);
            this.loadConfigurationToolStripMenuItemLoadConfiguration.Text = "Load Configuration";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("Trebuchet MS", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(91, 22);
            this.toolStripLabel1.Text = "Server State";
            // 
            // toolStripButtonServerStart
            // 
            this.toolStripButtonServerStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonServerStart.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonServerStart.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonServerStart.Image")));
            this.toolStripButtonServerStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonServerStart.Name = "toolStripButtonServerStart";
            this.toolStripButtonServerStart.Size = new System.Drawing.Size(41, 22);
            this.toolStripButtonServerStart.Text = "Start";
            this.toolStripButtonServerStart.Click += new System.EventHandler(this.toolStripButtonServerStart_Click);
            // 
            // toolStripButtonServerStop
            // 
            this.toolStripButtonServerStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonServerStop.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonServerStop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonServerStop.Image")));
            this.toolStripButtonServerStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonServerStop.Name = "toolStripButtonServerStop";
            this.toolStripButtonServerStop.Size = new System.Drawing.Size(38, 22);
            this.toolStripButtonServerStop.Text = "Stop";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Font = new System.Drawing.Font("Trebuchet MS", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(80, 22);
            this.toolStripLabel2.Text = "Server Log";
            // 
            // toolStripButtonClearLog
            // 
            this.toolStripButtonClearLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonClearLog.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonClearLog.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClearLog.Image")));
            this.toolStripButtonClearLog.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClearLog.Name = "toolStripButtonClearLog";
            this.toolStripButtonClearLog.Size = new System.Drawing.Size(42, 22);
            this.toolStripButtonClearLog.Text = "Clear";
            // 
            // toolStripButtonLogToFile
            // 
            this.toolStripButtonLogToFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonLogToFile.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonLogToFile.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonLogToFile.Image")));
            this.toolStripButtonLogToFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLogToFile.Name = "toolStripButtonLogToFile";
            this.toolStripButtonLogToFile.Size = new System.Drawing.Size(75, 22);
            this.toolStripButtonLogToFile.Text = "Log To File";
            // 
            // richTextBoxServerLog
            // 
            this.richTextBoxServerLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxServerLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxServerLog.Font = new System.Drawing.Font("Trebuchet MS", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxServerLog.Location = new System.Drawing.Point(202, 25);
            this.richTextBoxServerLog.Name = "richTextBoxServerLog";
            this.richTextBoxServerLog.ReadOnly = true;
            this.richTextBoxServerLog.Size = new System.Drawing.Size(459, 369);
            this.richTextBoxServerLog.TabIndex = 1;
            this.richTextBoxServerLog.Text = "";
            // 
            // propertyGridServerConfig
            // 
            this.propertyGridServerConfig.Dock = System.Windows.Forms.DockStyle.Left;
            this.propertyGridServerConfig.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyGridServerConfig.Location = new System.Drawing.Point(0, 25);
            this.propertyGridServerConfig.Name = "propertyGridServerConfig";
            this.propertyGridServerConfig.Size = new System.Drawing.Size(199, 367);
            this.propertyGridServerConfig.TabIndex = 2;
            // 
            // ServerStateControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.propertyGridServerConfig);
            this.Controls.Add(this.richTextBoxServerLog);
            this.Controls.Add(this.toolStrip1);
            this.Name = "ServerStateControl";
            this.Size = new System.Drawing.Size(659, 392);
            this.Load += new System.EventHandler(this.ServerStateControl_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButtonServerStart;
        private System.Windows.Forms.ToolStripButton toolStripButtonServerStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearLog;
        private System.Windows.Forms.ToolStripButton toolStripButtonLogToFile;
        private System.Windows.Forms.RichTextBox richTextBoxServerLog;
        private System.Windows.Forms.PropertyGrid propertyGridServerConfig;
        private System.Windows.Forms.ToolStripMenuItem saveConfigurationToolStripMenuItemSaveConfig;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItemLoadConfiguration;
    }
}
