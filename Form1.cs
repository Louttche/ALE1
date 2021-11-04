using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using More.Windows.Forms;

namespace ALE1_Katerina
{
    public partial class Form1 : Form
    {
        // UI Components
        public PictureBox ui_pb_tree;
        private int initialPanelWidth;
        private Point initialPanelLocation;

        public TableLayoutPanel ui_table_truth;
        public TableLayoutPanel ui_table_simple;
        public Label ui_lbl_norm_simp;
        public Label ui_lbl_norm;
        public Label ui_lbl_notation;
        public Label ui_lbl_hex;
        public Label ui_lbl_vars;
        public Label ui_lbl_nand;
        public ComboBox ui_cb_input;

        // Fields
        public NodeManager nodeManager;
        public Tree active_tree;
        public TruthTable active_truth_table;

        public Form1()
        {
            InitializeComponent();

            // Initialize ui components
            ui_pb_tree = pb_tree;
            this.initialPanelWidth = pb_tree.Width;
            this.initialPanelLocation = pb_tree.Location;

            ui_table_truth = table_truth;
            ui_table_simple = table_simple;
            ui_lbl_norm_simp = lbl_norm_simp;
            ui_lbl_norm = lbl_norm;
            ui_lbl_notation = lbl_notation;
            ui_lbl_hex = lbl_hex;
            ui_lbl_vars = lbl_vars;
            ui_lbl_nand = lbl_nand;
            ui_cb_input = cb_input;
        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            if (cb_input.Text.Length <= 0)
                return;

            this.nodeManager = new NodeManager(this);

            // UI Tree - Reset
            pb_tree.Controls.Clear();
            pb_tree.Width = this.initialPanelWidth;
            pb_tree.Location = this.initialPanelLocation;

            // UI Tables - Reset
            table_truth.RowStyles.Clear();
            table_truth.ColumnStyles.Clear();
            table_truth.Controls.Clear();

            table_simple.RowStyles.Clear();
            table_simple.ColumnStyles.Clear();
            table_simple.Controls.Clear();

            try
            {
                string formula = cb_input.Text.Replace(" ", "");    // --> remove spaces
                this.nodeManager.AddNode(formula);                  // Parse formula and store nodes
                this.nodeManager.ConvertAsciiToInfix();

                // Ignore tree for nandified formula (TODO)
                if (!this.nodeManager.formula.StartsWith("%"))
                    this.active_tree = new Tree(this);              // Creates and adds a paint event for a tree
                else
                    pb_tree.Controls.Clear();

                this.active_truth_table = new TruthTable(this);     // Creates a Truth Table
                this.active_truth_table.SimplifyTable();            // Simplify Table
                this.active_truth_table.Normalize();                // Normalizes Truth + Simple Table
                this.nodeManager.Nandify();                         // Nandifies the formula

                lbl_binary.Text = " ";
                lbl_binary.Text = this.nodeManager.formula_binary;

                this.nodeManager.Convert2Hex(this.nodeManager.formula_binary);  // Converts the Binary output of the formula to Hexadecimal
                this.nodeManager.ShowVariables(this.nodeManager.operants);      // Shows the operants separately
            }
            catch (Exception)
            {
                throw;
            }
        }

        //UI Events
        private void tb_formula_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                btn_submit.PerformClick();
        }

        private void tb_tree_zoom_Scroll(object sender, EventArgs e)
        {
            active_tree.zoom = tb_tree_zoom.Value / 10f;
            pb_tree.Invalidate();
        }
    }
}