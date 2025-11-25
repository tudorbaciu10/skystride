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
            this.uiPanel = new System.Windows.Forms.Panel();
            this.lstEntities = new System.Windows.Forms.ListBox();
            this.lblPos = new System.Windows.Forms.Label();
            this.numPosX = new System.Windows.Forms.NumericUpDown();
            this.numPosY = new System.Windows.Forms.NumericUpDown();
            this.numPosZ = new System.Windows.Forms.NumericUpDown();
            this.btnAddCube = new System.Windows.Forms.Button();
            this.btnAddSphere = new System.Windows.Forms.Button();
            this.btnAddPlane = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numPosX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosZ)).BeginInit();
            this.uiPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // glControlMapEditor
            // 
            this.glControlMapEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControlMapEditor.BackColor = System.Drawing.Color.Black;
            this.glControlMapEditor.Location = new System.Drawing.Point(0, 0);
            this.glControlMapEditor.Margin = new System.Windows.Forms.Padding(4);
            this.glControlMapEditor.Name = "glControlMapEditor";
            this.glControlMapEditor.Size = new System.Drawing.Size(796, 445);
            this.glControlMapEditor.TabIndex = 0;
            this.glControlMapEditor.VSync = false;
            this.glControlMapEditor.Load += new System.EventHandler(this.glControlMapEditor_Load);
            // 
            // uiPanel
            // 
            this.uiPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uiPanel.Controls.Add(this.btnDelete);
            this.uiPanel.Controls.Add(this.btnAddPlane);
            this.uiPanel.Controls.Add(this.btnAddSphere);
            this.uiPanel.Controls.Add(this.btnAddCube);
            this.uiPanel.Controls.Add(this.numPosZ);
            this.uiPanel.Controls.Add(this.numPosY);
            this.uiPanel.Controls.Add(this.numPosX);
            this.uiPanel.Controls.Add(this.lblPos);
            this.uiPanel.Controls.Add(this.lstEntities);
            this.uiPanel.Location = new System.Drawing.Point(806, 10);
            this.uiPanel.Name = "uiPanel";
            this.uiPanel.Size = new System.Drawing.Size(293, 430);
            this.uiPanel.TabIndex = 1;
            // 
            // lstEntities
            // 
            this.lstEntities.FormattingEnabled = true;
            this.lstEntities.ItemHeight = 16;
            this.lstEntities.Location = new System.Drawing.Point(0, 0);
            this.lstEntities.Name = "lstEntities";
            this.lstEntities.Size = new System.Drawing.Size(293, 196);
            this.lstEntities.TabIndex = 0;
            this.lstEntities.SelectedIndexChanged += new System.EventHandler(this.LstEntities_SelectedIndexChanged);
            // 
            // lblPos
            // 
            this.lblPos.AutoSize = true;
            this.lblPos.Location = new System.Drawing.Point(0, 210);
            this.lblPos.Name = "lblPos";
            this.lblPos.Size = new System.Drawing.Size(125, 17);
            this.lblPos.TabIndex = 1;
            this.lblPos.Text = "Position (X, Y, Z):";
            // 
            // numPosX
            // 
            this.numPosX.DecimalPlaces = 2;
            this.numPosX.Location = new System.Drawing.Point(0, 235);
            this.numPosX.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numPosX.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.numPosX.Name = "numPosX";
            this.numPosX.Size = new System.Drawing.Size(60, 22);
            this.numPosX.TabIndex = 2;
            this.numPosX.ValueChanged += new System.EventHandler(this.NumPos_ValueChanged);
            // 
            // numPosY
            // 
            this.numPosY.DecimalPlaces = 2;
            this.numPosY.Location = new System.Drawing.Point(70, 235);
            this.numPosY.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numPosY.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.numPosY.Name = "numPosY";
            this.numPosY.Size = new System.Drawing.Size(60, 22);
            this.numPosY.TabIndex = 3;
            this.numPosY.ValueChanged += new System.EventHandler(this.NumPos_ValueChanged);
            // 
            // numPosZ
            // 
            this.numPosZ.DecimalPlaces = 2;
            this.numPosZ.Location = new System.Drawing.Point(140, 235);
            this.numPosZ.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numPosZ.Minimum = new decimal(new int[] {
            10000,
            0,
            0,
            -2147483648});
            this.numPosZ.Name = "numPosZ";
            this.numPosZ.Size = new System.Drawing.Size(60, 22);
            this.numPosZ.TabIndex = 4;
            this.numPosZ.ValueChanged += new System.EventHandler(this.NumPos_ValueChanged);
            // 
            // btnAddCube
            // 
            this.btnAddCube.Location = new System.Drawing.Point(0, 265);
            this.btnAddCube.Name = "btnAddCube";
            this.btnAddCube.Size = new System.Drawing.Size(293, 23);
            this.btnAddCube.TabIndex = 5;
            this.btnAddCube.Text = "Add Cube";
            this.btnAddCube.UseVisualStyleBackColor = true;
            this.btnAddCube.Click += new System.EventHandler(this.BtnAddCube_Click);
            // 
            // btnAddSphere
            // 
            this.btnAddSphere.Location = new System.Drawing.Point(0, 295);
            this.btnAddSphere.Name = "btnAddSphere";
            this.btnAddSphere.Size = new System.Drawing.Size(293, 23);
            this.btnAddSphere.TabIndex = 6;
            this.btnAddSphere.Text = "Add Sphere";
            this.btnAddSphere.UseVisualStyleBackColor = true;
            this.btnAddSphere.Click += new System.EventHandler(this.BtnAddSphere_Click);
            // 
            // btnAddPlane
            // 
            this.btnAddPlane.Location = new System.Drawing.Point(0, 325);
            this.btnAddPlane.Name = "btnAddPlane";
            this.btnAddPlane.Size = new System.Drawing.Size(293, 23);
            this.btnAddPlane.TabIndex = 7;
            this.btnAddPlane.Text = "Add Plane";
            this.btnAddPlane.UseVisualStyleBackColor = true;
            this.btnAddPlane.Click += new System.EventHandler(this.BtnAddPlane_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(0, 355);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(293, 23);
            this.btnDelete.TabIndex = 8;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // MapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1109, 450);
            this.Controls.Add(this.uiPanel);
            this.Controls.Add(this.glControlMapEditor);
            this.Name = "MapEditor";
            this.Text = "MapEditor";
            ((System.ComponentModel.ISupportInitialize)(this.numPosX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosZ)).EndInit();
            this.uiPanel.ResumeLayout(false);
            this.uiPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glControlMapEditor;
        private System.Windows.Forms.Panel uiPanel;
        private System.Windows.Forms.ListBox lstEntities;
        private System.Windows.Forms.Label lblPos;
        private System.Windows.Forms.NumericUpDown numPosX;
        private System.Windows.Forms.NumericUpDown numPosY;
        private System.Windows.Forms.NumericUpDown numPosZ;
        private System.Windows.Forms.Button btnAddCube;
        private System.Windows.Forms.Button btnAddSphere;
        private System.Windows.Forms.Button btnAddPlane;
        private System.Windows.Forms.Button btnDelete;
    }
}