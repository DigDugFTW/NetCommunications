namespace NetClientWindowsForms.Controls
{
    partial class ClientCommunicationsControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClientCommunicationsControl));
            this.textBoxChatInput = new System.Windows.Forms.TextBox();
            this.listViewClientChat = new System.Windows.Forms.ListView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelResponseTimeValue = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelReceivedBytes = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelTotalReceivedBytes = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelSentBytes = new System.Windows.Forms.ToolStripStatusLabel();
            this.listViewClients = new System.Windows.Forms.ListView();
            this.contextMenuStripUserActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonShowDirectMessage = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStripUserActions.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxChatInput
            // 
            this.textBoxChatInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxChatInput.Font = new System.Drawing.Font("Trebuchet MS", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxChatInput.Location = new System.Drawing.Point(3, 373);
            this.textBoxChatInput.Multiline = true;
            this.textBoxChatInput.Name = "textBoxChatInput";
            this.textBoxChatInput.Size = new System.Drawing.Size(688, 28);
            this.textBoxChatInput.TabIndex = 1;
            this.textBoxChatInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxChatInput_KeyDown);
            // 
            // listViewClientChat
            // 
            this.listViewClientChat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listViewClientChat.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewClientChat.FullRowSelect = true;
            this.listViewClientChat.GridLines = true;
            this.listViewClientChat.HideSelection = false;
            this.listViewClientChat.Location = new System.Drawing.Point(3, 28);
            this.listViewClientChat.MultiSelect = false;
            this.listViewClientChat.Name = "listViewClientChat";
            this.listViewClientChat.Size = new System.Drawing.Size(688, 339);
            this.listViewClientChat.TabIndex = 2;
            this.listViewClientChat.UseCompatibleStateImageBehavior = false;
            this.listViewClientChat.View = System.Windows.Forms.View.List;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabelResponseTimeValue,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabelReceivedBytes,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabelTotalReceivedBytes,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabelSentBytes});
            this.statusStrip1.Location = new System.Drawing.Point(0, 403);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(857, 23);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(112, 18);
            this.toolStripStatusLabel1.Text = "Response Time ms:";
            // 
            // toolStripStatusLabelResponseTimeValue
            // 
            this.toolStripStatusLabelResponseTimeValue.Name = "toolStripStatusLabelResponseTimeValue";
            this.toolStripStatusLabelResponseTimeValue.Size = new System.Drawing.Size(14, 18);
            this.toolStripStatusLabelResponseTimeValue.Text = "0";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(96, 18);
            this.toolStripStatusLabel2.Text = "Received Bytes:";
            // 
            // toolStripStatusLabelReceivedBytes
            // 
            this.toolStripStatusLabelReceivedBytes.Name = "toolStripStatusLabelReceivedBytes";
            this.toolStripStatusLabelReceivedBytes.Size = new System.Drawing.Size(14, 18);
            this.toolStripStatusLabelReceivedBytes.Text = "0";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(111, 18);
            this.toolStripStatusLabel3.Text = "Total Receive Size:";
            // 
            // toolStripStatusLabelTotalReceivedBytes
            // 
            this.toolStripStatusLabelTotalReceivedBytes.Name = "toolStripStatusLabelTotalReceivedBytes";
            this.toolStripStatusLabelTotalReceivedBytes.Size = new System.Drawing.Size(14, 18);
            this.toolStripStatusLabelTotalReceivedBytes.Text = "0";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(71, 18);
            this.toolStripStatusLabel4.Text = "Sent Bytes:";
            // 
            // toolStripStatusLabelSentBytes
            // 
            this.toolStripStatusLabelSentBytes.Name = "toolStripStatusLabelSentBytes";
            this.toolStripStatusLabelSentBytes.Size = new System.Drawing.Size(14, 18);
            this.toolStripStatusLabelSentBytes.Text = "0";
            // 
            // listViewClients
            // 
            this.listViewClients.ContextMenuStrip = this.contextMenuStripUserActions;
            this.listViewClients.HideSelection = false;
            this.listViewClients.Location = new System.Drawing.Point(697, 28);
            this.listViewClients.Name = "listViewClients";
            this.listViewClients.Size = new System.Drawing.Size(157, 373);
            this.listViewClients.TabIndex = 4;
            this.listViewClients.UseCompatibleStateImageBehavior = false;
            // 
            // contextMenuStripUserActions
            // 
            this.contextMenuStripUserActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dMToolStripMenuItem});
            this.contextMenuStripUserActions.Name = "contextMenuStripUserActions";
            this.contextMenuStripUserActions.Size = new System.Drawing.Size(94, 26);
            // 
            // dMToolStripMenuItem
            // 
            this.dMToolStripMenuItem.Name = "dMToolStripMenuItem";
            this.dMToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.dMToolStripMenuItem.Text = "DM";
            this.dMToolStripMenuItem.Click += new System.EventHandler(this.dMToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonShowDirectMessage});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.toolStrip1.Size = new System.Drawing.Size(857, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonShowDirectMessage
            // 
            this.toolStripButtonShowDirectMessage.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.toolStripButtonShowDirectMessage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonShowDirectMessage.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButtonShowDirectMessage.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonShowDirectMessage.Image")));
            this.toolStripButtonShowDirectMessage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonShowDirectMessage.Name = "toolStripButtonShowDirectMessage";
            this.toolStripButtonShowDirectMessage.Size = new System.Drawing.Size(96, 22);
            this.toolStripButtonShowDirectMessage.Text = "Direct Message";
            this.toolStripButtonShowDirectMessage.Click += new System.EventHandler(this.toolStripButtonShowDirectMessage_Click);
            // 
            // ClientCommunicationsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listViewClients);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.listViewClientChat);
            this.Controls.Add(this.textBoxChatInput);
            this.Name = "ClientCommunicationsControl";
            this.Size = new System.Drawing.Size(857, 426);
            this.Load += new System.EventHandler(this.ClientCommunicationsControl_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStripUserActions.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxChatInput;
        private System.Windows.Forms.ListView listViewClientChat;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelResponseTimeValue;
        private System.Windows.Forms.ListView listViewClients;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripUserActions;
        private System.Windows.Forms.ToolStripMenuItem dMToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowDirectMessage;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelReceivedBytes;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTotalReceivedBytes;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSentBytes;
    }
}
