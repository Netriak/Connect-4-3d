namespace Connect_4_3D
{
    partial class JoinForm
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
            this.Box_IP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Button_JoinGame = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.Box_Port = new System.Windows.Forms.TextBox();
            this.Button_CancelJoin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Box_IP
            // 
            this.Box_IP.Location = new System.Drawing.Point(10, 28);
            this.Box_IP.Name = "Box_IP";
            this.Box_IP.Size = new System.Drawing.Size(75, 20);
            this.Box_IP.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Ip Address";
            // 
            // Button_JoinGame
            // 
            this.Button_JoinGame.Location = new System.Drawing.Point(10, 63);
            this.Button_JoinGame.Name = "Button_JoinGame";
            this.Button_JoinGame.Size = new System.Drawing.Size(75, 23);
            this.Button_JoinGame.TabIndex = 2;
            this.Button_JoinGame.Text = "Join";
            this.Button_JoinGame.UseVisualStyleBackColor = true;
            this.Button_JoinGame.Click += new System.EventHandler(this.Button_JoinGame_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(116, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port";
            // 
            // Box_Port
            // 
            this.Box_Port.Location = new System.Drawing.Point(116, 28);
            this.Box_Port.Name = "Box_Port";
            this.Box_Port.Size = new System.Drawing.Size(75, 20);
            this.Box_Port.TabIndex = 1;
            this.Box_Port.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Box_Port_KeyPress);
            // 
            // Button_CancelJoin
            // 
            this.Button_CancelJoin.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Button_CancelJoin.Location = new System.Drawing.Point(116, 63);
            this.Button_CancelJoin.Name = "Button_CancelJoin";
            this.Button_CancelJoin.Size = new System.Drawing.Size(75, 23);
            this.Button_CancelJoin.TabIndex = 3;
            this.Button_CancelJoin.Text = "Cancel";
            this.Button_CancelJoin.UseVisualStyleBackColor = true;
            this.Button_CancelJoin.Click += new System.EventHandler(this.Button_CancelJoin_Click);
            // 
            // JoinForm
            // 
            this.AcceptButton = this.Button_JoinGame;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Button_CancelJoin;
            this.ClientSize = new System.Drawing.Size(203, 91);
            this.Controls.Add(this.Button_CancelJoin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Box_Port);
            this.Controls.Add(this.Button_JoinGame);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Box_IP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JoinForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Join Game";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Box_IP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Button_JoinGame;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Box_Port;
        private System.Windows.Forms.Button Button_CancelJoin;
    }
}