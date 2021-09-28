
namespace ALE1_Katerina
{
    partial class Form1
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
            this.btn_submit = new System.Windows.Forms.Button();
            this.tb_formula = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_vars = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbl_binary = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbl_hex = new System.Windows.Forms.Label();
            this.tc_1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.table_truth = new System.Windows.Forms.TableLayoutPanel();
            this.tc_1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_submit
            // 
            this.btn_submit.Location = new System.Drawing.Point(257, 20);
            this.btn_submit.Name = "btn_submit";
            this.btn_submit.Size = new System.Drawing.Size(77, 37);
            this.btn_submit.TabIndex = 0;
            this.btn_submit.Text = "Submit";
            this.btn_submit.UseVisualStyleBackColor = true;
            this.btn_submit.Click += new System.EventHandler(this.btn_submit_Click);
            // 
            // tb_formula
            // 
            this.tb_formula.Location = new System.Drawing.Point(36, 27);
            this.tb_formula.Name = "tb_formula";
            this.tb_formula.Size = new System.Drawing.Size(204, 22);
            this.tb_formula.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Variables";
            // 
            // lbl_vars
            // 
            this.lbl_vars.AutoSize = true;
            this.lbl_vars.Location = new System.Drawing.Point(288, 94);
            this.lbl_vars.Name = "lbl_vars";
            this.lbl_vars.Size = new System.Drawing.Size(46, 17);
            this.lbl_vars.TabIndex = 8;
            this.lbl_vars.Text = "label3";
            this.lbl_vars.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Binary";
            // 
            // lbl_binary
            // 
            this.lbl_binary.AutoSize = true;
            this.lbl_binary.Location = new System.Drawing.Point(288, 140);
            this.lbl_binary.Name = "lbl_binary";
            this.lbl_binary.Size = new System.Drawing.Size(46, 17);
            this.lbl_binary.TabIndex = 10;
            this.lbl_binary.Text = "label3";
            this.lbl_binary.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 186);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 17);
            this.label4.TabIndex = 11;
            this.label4.Text = "Hex";
            // 
            // lbl_hex
            // 
            this.lbl_hex.AutoSize = true;
            this.lbl_hex.Location = new System.Drawing.Point(288, 186);
            this.lbl_hex.Name = "lbl_hex";
            this.lbl_hex.Size = new System.Drawing.Size(46, 17);
            this.lbl_hex.TabIndex = 12;
            this.lbl_hex.Text = "label3";
            this.lbl_hex.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tc_1
            // 
            this.tc_1.Controls.Add(this.tabPage1);
            this.tc_1.Controls.Add(this.tabPage2);
            this.tc_1.Location = new System.Drawing.Point(364, 12);
            this.tc_1.Name = "tc_1";
            this.tc_1.SelectedIndex = 0;
            this.tc_1.Size = new System.Drawing.Size(422, 517);
            this.tc_1.TabIndex = 13;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(416, 499);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Tree";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.table_truth);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(414, 488);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Truth Table";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // table_truth
            // 
            this.table_truth.BackColor = System.Drawing.Color.Transparent;
            this.table_truth.ColumnCount = 3;
            this.table_truth.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.table_truth.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.table_truth.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.table_truth.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.table_truth.Location = new System.Drawing.Point(18, 19);
            this.table_truth.Name = "table_truth";
            this.table_truth.RowCount = 2;
            this.table_truth.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table_truth.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.table_truth.Size = new System.Drawing.Size(379, 456);
            this.table_truth.TabIndex = 14;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 544);
            this.Controls.Add(this.tc_1);
            this.Controls.Add(this.lbl_hex);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbl_binary);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbl_vars);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_formula);
            this.Controls.Add(this.btn_submit);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tc_1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_submit;
        private System.Windows.Forms.TextBox tb_formula;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_vars;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbl_binary;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbl_hex;
        private System.Windows.Forms.TabControl tc_1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel table_truth;
    }
}

