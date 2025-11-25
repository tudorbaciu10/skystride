namespace skystride.forms
{
    partial class MapEditor
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
            this.glControlMapEditor = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // glControlMapEditor
            // 
            this.glControlMapEditor.BackColor = System.Drawing.Color.Black;
            this.glControlMapEditor.Location = new System.Drawing.Point(0, 0);
            this.glControlMapEditor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.glControlMapEditor.Name = "glControlMapEditor";
            this.glControlMapEditor.Size = new System.Drawing.Size(796, 445);
            this.glControlMapEditor.TabIndex = 0;
            this.glControlMapEditor.VSync = false;
            this.glControlMapEditor.Load += new System.EventHandler(this.glControlMapEditor_Load);
            // 
            // MapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1109, 450);
            this.Controls.Add(this.glControlMapEditor);
            this.Name = "MapEditor";
            this.Text = "MapEditor";
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glControlMapEditor;
    }
}