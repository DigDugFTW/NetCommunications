namespace NetClientWindowsForms.Controls
{
    partial class DirectMessageControl
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
            this.tabControlDirectMessage = new System.Windows.Forms.TabControl();
            this.SuspendLayout();
            // 
            // tabControlDirectMessage
            // 
            this.tabControlDirectMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlDirectMessage.Location = new System.Drawing.Point(0, 0);
            this.tabControlDirectMessage.Name = "tabControlDirectMessage";
            this.tabControlDirectMessage.SelectedIndex = 0;
            this.tabControlDirectMessage.Size = new System.Drawing.Size(460, 400);
            this.tabControlDirectMessage.TabIndex = 0;
            // 
            // DirectMessageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tabControlDirectMessage);
            this.Name = "DirectMessageControl";
            this.Size = new System.Drawing.Size(460, 400);
            this.Load += new System.EventHandler(this.DirectMessageControl_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlDirectMessage;
    }
}
