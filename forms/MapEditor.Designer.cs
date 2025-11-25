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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpInspector = new System.Windows.Forms.GroupBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAddPlane = new System.Windows.Forms.Button();
            this.btnAddSphere = new System.Windows.Forms.Button();
            this.btnAddCube = new System.Windows.Forms.Button();
            this.grpTransform = new System.Windows.Forms.GroupBox();
            this.numPosZ = new System.Windows.Forms.NumericUpDown();
            this.numPosY = new System.Windows.Forms.NumericUpDown();
            this.numPosX = new System.Windows.Forms.NumericUpDown();
            this.lblPos = new System.Windows.Forms.Label();
            this.lstEntities = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpInspector.SuspendLayout();
            this.grpTransform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPosZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosX)).BeginInit();
            this.SuspendLayout();
            // 
            // glControlMapEditor
            // 
            this.glControlMapEditor.BackColor = System.Drawing.Color.Black;
            this.glControlMapEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlMapEditor.Location = new System.Drawing.Point(0, 0);
            this.glControlMapEditor.Margin = new System.Windows.Forms.Padding(4);
            this.glControlMapEditor.Name = "glControlMapEditor";
            this.glControlMapEditor.Size = new System.Drawing.Size(800, 450);
            this.glControlMapEditor.TabIndex = 0;
            this.glControlMapEditor.VSync = false;
            this.glControlMapEditor.Load += new System.EventHandler(this.glControlMapEditor_Load);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.glControlMapEditor);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(38)))));
            this.splitContainer1.Panel2.Controls.Add(this.grpInspector);
            this.splitContainer1.Panel2.Controls.Add(this.lstEntities);
            this.splitContainer1.Size = new System.Drawing.Size(1109, 450);
            this.splitContainer1.SplitterDistance = 800;
            this.splitContainer1.TabIndex = 1;
            // 
            // grpInspector
            // 
            this.grpInspector.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInspector.Controls.Add(this.btnDelete);
            this.grpInspector.Controls.Add(this.btnAddPlane);
            this.grpInspector.Controls.Add(this.btnAddSphere);
            this.grpInspector.Controls.Add(this.btnAddCube);
            this.grpInspector.Controls.Add(this.grpTransform);
            this.grpInspector.ForeColor = System.Drawing.Color.White;
            this.grpInspector.Location = new System.Drawing.Point(3, 205);
            this.grpInspector.Name = "grpInspector";
            this.grpInspector.Size = new System.Drawing.Size(299, 242);
            this.grpInspector.TabIndex = 1;
            this.grpInspector.TabStop = false;
            this.grpInspector.Text = "Inspector";
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(6, 180);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(287, 30);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // btnAddPlane
            // 
            this.btnAddPlane.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddPlane.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.btnAddPlane.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnAddPlane.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddPlane.ForeColor = System.Drawing.Color.White;
            this.btnAddPlane.Location = new System.Drawing.Point(6, 144);
            this.btnAddPlane.Name = "btnAddPlane";
            this.btnAddPlane.Size = new System.Drawing.Size(287, 30);
            this.btnAddPlane.TabIndex = 3;
            this.btnAddPlane.Text = "Add Plane";
            this.btnAddPlane.UseVisualStyleBackColor = false;
            this.btnAddPlane.Click += new System.EventHandler(this.BtnAddPlane_Click);
            // 
            // btnAddSphere
            // 
            this.btnAddSphere.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSphere.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.btnAddSphere.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnAddSphere.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddSphere.ForeColor = System.Drawing.Color.White;
            this.btnAddSphere.Location = new System.Drawing.Point(6, 108);
            this.btnAddSphere.Name = "btnAddSphere";
            this.btnAddSphere.Size = new System.Drawing.Size(287, 30);
            this.btnAddSphere.TabIndex = 2;
            this.btnAddSphere.Text = "Add Sphere";
            this.btnAddSphere.UseVisualStyleBackColor = false;
            this.btnAddSphere.Click += new System.EventHandler(this.BtnAddSphere_Click);
            // 
            // btnAddCube
            // 
            this.btnAddCube.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddCube.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.btnAddCube.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnAddCube.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddCube.ForeColor = System.Drawing.Color.White;
            this.btnAddCube.Location = new System.Drawing.Point(6, 72);
            this.btnAddCube.Name = "btnAddCube";
            this.btnAddCube.Size = new System.Drawing.Size(287, 30);
            this.btnAddCube.TabIndex = 1;
            this.btnAddCube.Text = "Add Cube";
            this.btnAddCube.UseVisualStyleBackColor = false;
            this.btnAddCube.Click += new System.EventHandler(this.BtnAddCube_Click);
            // 
            // grpTransform
            // 
            this.grpTransform.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTransform.Controls.Add(this.numPosZ);
            this.grpTransform.Controls.Add(this.numPosY);
            this.grpTransform.Controls.Add(this.numPosX);
            this.grpTransform.Controls.Add(this.lblPos);
            this.grpTransform.ForeColor = System.Drawing.Color.White;
            this.grpTransform.Location = new System.Drawing.Point(6, 21);
            this.grpTransform.Name = "grpTransform";
            this.grpTransform.Size = new System.Drawing.Size(287, 45);
            this.grpTransform.TabIndex = 0;
            this.grpTransform.TabStop = false;
            this.grpTransform.Text = "Transform";
            // 
            // numPosZ
            // 
            this.numPosZ.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.numPosZ.DecimalPlaces = 2;
            this.numPosZ.ForeColor = System.Drawing.Color.White;
            this.numPosZ.Location = new System.Drawing.Point(200, 17);
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
            this.numPosZ.TabIndex = 3;
            this.numPosZ.ValueChanged += new System.EventHandler(this.NumPos_ValueChanged);
            // 
            // numPosY
            // 
            this.numPosY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.numPosY.DecimalPlaces = 2;
            this.numPosY.ForeColor = System.Drawing.Color.White;
            this.numPosY.Location = new System.Drawing.Point(134, 17);
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
            this.numPosY.TabIndex = 2;
            this.numPosY.ValueChanged += new System.EventHandler(this.NumPos_ValueChanged);
            // 
            // numPosX
            // 
            this.numPosX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.numPosX.DecimalPlaces = 2;
            this.numPosX.ForeColor = System.Drawing.Color.White;
            this.numPosX.Location = new System.Drawing.Point(68, 17);
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
            this.numPosX.TabIndex = 1;
            this.numPosX.ValueChanged += new System.EventHandler(this.NumPos_ValueChanged);
            // 
            // lblPos
            // 
            this.lblPos.AutoSize = true;
            this.lblPos.Location = new System.Drawing.Point(6, 19);
            this.lblPos.Name = "lblPos";
            this.lblPos.Size = new System.Drawing.Size(56, 17);
            this.lblPos.TabIndex = 0;
            this.lblPos.Text = "Pos:";
            // 
            // lstEntities
            // 
            this.lstEntities.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lstEntities.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstEntities.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstEntities.ForeColor = System.Drawing.Color.White;
            this.lstEntities.FormattingEnabled = true;
            this.lstEntities.ItemHeight = 16;
            this.lstEntities.Location = new System.Drawing.Point(0, 0);
            this.lstEntities.Name = "lstEntities";
            this.lstEntities.Size = new System.Drawing.Size(305, 194);
            this.lstEntities.TabIndex = 0;
            this.lstEntities.SelectedIndexChanged += new System.EventHandler(this.LstEntities_SelectedIndexChanged);
            // 
            // MapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(1109, 450);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MapEditor";
            this.Text = "MapEditor";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.grpInspector.ResumeLayout(false);
            this.grpTransform.ResumeLayout(false);
            this.grpTransform.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPosZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPosX)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glControlMapEditor;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstEntities;
        private System.Windows.Forms.GroupBox grpInspector;
        private System.Windows.Forms.GroupBox grpTransform;
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