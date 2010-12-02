namespace Connect_4_3D
{
    partial class Options
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
            this.label1 = new System.Windows.Forms.Label();
            this.Box_AI_Difficulty = new System.Windows.Forms.ComboBox();
            this.Button_Save = new System.Windows.Forms.Button();
            this.Button_Cancel = new System.Windows.Forms.Button();
            this.Box_Antialiasing = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Box_Anisotropic = new System.Windows.Forms.ComboBox();
            this.Check_Skybox = new System.Windows.Forms.CheckBox();
            this.Check_VSynch = new System.Windows.Forms.CheckBox();
            this.Text_Hostport = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Check_shaders = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "AI Difficulty";
            // 
            // Box_AI_Difficulty
            // 
            this.Box_AI_Difficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Box_AI_Difficulty.FormattingEnabled = true;
            this.Box_AI_Difficulty.Location = new System.Drawing.Point(78, 69);
            this.Box_AI_Difficulty.Name = "Box_AI_Difficulty";
            this.Box_AI_Difficulty.Size = new System.Drawing.Size(121, 21);
            this.Box_AI_Difficulty.TabIndex = 2;
            // 
            // Button_Save
            // 
            this.Button_Save.Location = new System.Drawing.Point(18, 187);
            this.Button_Save.Name = "Button_Save";
            this.Button_Save.Size = new System.Drawing.Size(75, 23);
            this.Button_Save.TabIndex = 7;
            this.Button_Save.Text = "Save";
            this.Button_Save.UseVisualStyleBackColor = true;
            this.Button_Save.Click += new System.EventHandler(this.Button_Save_Click);
            // 
            // Button_Cancel
            // 
            this.Button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Button_Cancel.Location = new System.Drawing.Point(238, 187);
            this.Button_Cancel.Name = "Button_Cancel";
            this.Button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.Button_Cancel.TabIndex = 8;
            this.Button_Cancel.Text = "Cancel";
            this.Button_Cancel.UseVisualStyleBackColor = true;
            this.Button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
            // 
            // Box_Antialiasing
            // 
            this.Box_Antialiasing.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Box_Antialiasing.FormattingEnabled = true;
            this.Box_Antialiasing.Location = new System.Drawing.Point(78, 15);
            this.Box_Antialiasing.Name = "Box_Antialiasing";
            this.Box_Antialiasing.Size = new System.Drawing.Size(121, 21);
            this.Box_Antialiasing.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Anti-aliasing";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 29);
            this.label3.TabIndex = 7;
            this.label3.Text = "Anisotropic Filtering";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Box_Anisotropic
            // 
            this.Box_Anisotropic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Box_Anisotropic.FormattingEnabled = true;
            this.Box_Anisotropic.Location = new System.Drawing.Point(78, 42);
            this.Box_Anisotropic.Name = "Box_Anisotropic";
            this.Box_Anisotropic.Size = new System.Drawing.Size(121, 21);
            this.Box_Anisotropic.TabIndex = 1;
            // 
            // Check_Skybox
            // 
            this.Check_Skybox.AutoSize = true;
            this.Check_Skybox.Location = new System.Drawing.Point(214, 17);
            this.Check_Skybox.Name = "Check_Skybox";
            this.Check_Skybox.Size = new System.Drawing.Size(99, 17);
            this.Check_Skybox.TabIndex = 4;
            this.Check_Skybox.Text = "Render Skybox";
            this.Check_Skybox.UseVisualStyleBackColor = true;
            // 
            // Check_VSynch
            // 
            this.Check_VSynch.AutoSize = true;
            this.Check_VSynch.Location = new System.Drawing.Point(214, 44);
            this.Check_VSynch.Name = "Check_VSynch";
            this.Check_VSynch.Size = new System.Drawing.Size(66, 17);
            this.Check_VSynch.TabIndex = 5;
            this.Check_VSynch.Text = "V-Synch";
            this.Check_VSynch.UseVisualStyleBackColor = true;
            // 
            // Text_Hostport
            // 
            this.Text_Hostport.Location = new System.Drawing.Point(78, 106);
            this.Text_Hostport.MaxLength = 5;
            this.Text_Hostport.Name = "Text_Hostport";
            this.Text_Hostport.Size = new System.Drawing.Size(121, 20);
            this.Text_Hostport.TabIndex = 3;
            this.Text_Hostport.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Text_Hostport_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 109);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Host Port";
            // 
            // Check_shaders
            // 
            this.Check_shaders.AutoSize = true;
            this.Check_shaders.Location = new System.Drawing.Point(214, 73);
            this.Check_shaders.Name = "Check_shaders";
            this.Check_shaders.Size = new System.Drawing.Size(87, 17);
            this.Check_shaders.TabIndex = 6;
            this.Check_shaders.Text = "Use Shaders";
            this.Check_shaders.UseVisualStyleBackColor = true;
            // 
            // Options
            // 
            this.AcceptButton = this.Button_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Button_Cancel;
            this.ClientSize = new System.Drawing.Size(321, 223);
            this.Controls.Add(this.Check_shaders);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Text_Hostport);
            this.Controls.Add(this.Check_VSynch);
            this.Controls.Add(this.Check_Skybox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Box_Anisotropic);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Box_Antialiasing);
            this.Controls.Add(this.Button_Cancel);
            this.Controls.Add(this.Button_Save);
            this.Controls.Add(this.Box_AI_Difficulty);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Options";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox Box_AI_Difficulty;
        private System.Windows.Forms.Button Button_Save;
        private System.Windows.Forms.Button Button_Cancel;
        private System.Windows.Forms.ComboBox Box_Antialiasing;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox Box_Anisotropic;
        private System.Windows.Forms.CheckBox Check_Skybox;
        private System.Windows.Forms.CheckBox Check_VSynch;
        private System.Windows.Forms.TextBox Text_Hostport;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox Check_shaders;
    }
}