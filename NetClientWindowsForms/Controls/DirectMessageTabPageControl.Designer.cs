namespace NetClientWindowsForms.Controls
{
    partial class DirectMessageTabPageControl
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
            this.richTextBoxDirectMessageReceive = new System.Windows.Forms.RichTextBox();
            this.textBoxDirectMessageSend = new System.Windows.Forms.TextBox();
            this.buttonSendDirectMessage = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBoxDirectMessageReceive
            // 
            this.richTextBoxDirectMessageReceive.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxDirectMessageReceive.Dock = System.Windows.Forms.DockStyle.Top;
            this.richTextBoxDirectMessageReceive.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxDirectMessageReceive.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxDirectMessageReceive.Name = "richTextBoxDirectMessageReceive";
            this.richTextBoxDirectMessageReceive.ReadOnly = true;
            this.richTextBoxDirectMessageReceive.Size = new System.Drawing.Size(464, 338);
            this.richTextBoxDirectMessageReceive.TabIndex = 0;
            this.richTextBoxDirectMessageReceive.Text = "";
            // 
            // textBoxDirectMessageSend
            // 
            this.textBoxDirectMessageSend.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDirectMessageSend.Location = new System.Drawing.Point(3, 347);
            this.textBoxDirectMessageSend.Multiline = true;
            this.textBoxDirectMessageSend.Name = "textBoxDirectMessageSend";
            this.textBoxDirectMessageSend.Size = new System.Drawing.Size(352, 28);
            this.textBoxDirectMessageSend.TabIndex = 1;
            this.textBoxDirectMessageSend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxDirectMessageSend_KeyDown);
            // 
            // buttonSendDirectMessage
            // 
            this.buttonSendDirectMessage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSendDirectMessage.Font = new System.Drawing.Font("Trebuchet MS", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSendDirectMessage.Location = new System.Drawing.Point(361, 347);
            this.buttonSendDirectMessage.Name = "buttonSendDirectMessage";
            this.buttonSendDirectMessage.Size = new System.Drawing.Size(100, 28);
            this.buttonSendDirectMessage.TabIndex = 2;
            this.buttonSendDirectMessage.Text = "Send";
            this.buttonSendDirectMessage.UseVisualStyleBackColor = true;
            this.buttonSendDirectMessage.Click += new System.EventHandler(this.buttonSendDirectMessage_Click);
            // 
            // DirectMessageTabPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.buttonSendDirectMessage);
            this.Controls.Add(this.textBoxDirectMessageSend);
            this.Controls.Add(this.richTextBoxDirectMessageReceive);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DirectMessageTabPageControl";
            this.Size = new System.Drawing.Size(460, 360);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxDirectMessageReceive;
        private System.Windows.Forms.TextBox textBoxDirectMessageSend;
        private System.Windows.Forms.Button buttonSendDirectMessage;
    }
}
