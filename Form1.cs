using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDataReader;
using System.Data;

//using More.Windows.Forms;

namespace ALE1_Katerina
{
    public partial class Form1 : Form
    {
        // UI Components
        public PictureBox ui_pb_tree;
        public Panel ui_panel_tree;
        public Size initialPanelSize;
        public Point initialPanelLocation;
        public TableLayoutPanel ui_table_truth;
        public TableLayoutPanel ui_table_simple;
        public Label ui_lbl_norm_simp;
        public Label ui_lbl_norm;
        public Label ui_lbl_notation;
        public Label ui_lbl_hex;
        public Label ui_lbl_vars;
        public Label ui_lbl_nand;
        public ComboBox ui_cb_input;

        private Point mDown = Point.Empty;

        // Fields
        public NodeManager nodeManager;
        public Tree active_tree;
        public TruthTable active_truth_table;

        // Testing
        private string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        public List<InputTest> input_tests_2;
        public List<InputTest> input_tests_3;
        public List<InputTest> input_tests_4;

        public Form1()
        {
            InitializeComponent();

            input_tests_2 = new List<InputTest>();
            input_tests_3 = new List<InputTest>();
            input_tests_4 = new List<InputTest>();

            lb_tests_2.Items.Clear();
            lb_tests_3.Items.Clear();
            lb_tests_4.Items.Clear();

            // Initialize ui components
            ui_pb_tree = pb_tree;

            this.initialPanelSize = new Size(panel_tree.Width, panel_tree.Height);
            this.initialPanelLocation = panel_tree.Location;
            ui_panel_tree = panel_tree;
            ui_table_truth = table_truth;
            ui_table_simple = table_simple;
            ui_lbl_norm_simp = lbl_norm_simp;
            ui_lbl_norm = lbl_norm;
            ui_lbl_notation = lbl_notation;
            ui_lbl_hex = lbl_hex;
            ui_lbl_vars = lbl_vars;
            ui_lbl_nand = lbl_nand;
            ui_cb_input = cb_input;

            this.ReadTests();
        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            if (cb_input.Text.Length <= 0)
                return;

            this.ResetFormControls();

            try
            {
                this.nodeManager = new NodeManager(this);
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
            catch (Exception) { throw; }
            finally
            {
                debugAll(false, true, false, false); // Nodes | Tree | Truth Table | Simplification
            }
        }

        public void ResetFormControls()
        {
            // UI Tree - Reset
            panel_tree.Size = this.initialPanelSize;
            panel_tree.Location = this.initialPanelLocation;
            pb_tree.Controls.Clear();
            pb_tree.Width = this.initialPanelSize.Width;
            pb_tree.Location = this.initialPanelLocation;

            // UI Tables - Reset
            table_truth.RowStyles.Clear();
            table_truth.ColumnStyles.Clear();
            table_truth.Controls.Clear();

            table_simple.RowStyles.Clear();
            table_simple.ColumnStyles.Clear();
            table_simple.Controls.Clear();
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
            // TODO: change bounds according to zoom
            pb_tree.Invalidate();
        }

        private void pb_tree_MouseDown(object sender, MouseEventArgs e)
        {
            mDown = e.Location;
        }

        private void pb_tree_MouseMove(object sender, MouseEventArgs e)
        {
            bool move_x = true;
            bool move_y = true;

            lbl_coords.Text = $"({e.X}, {e.Y})";

            if (e.Button.HasFlag(MouseButtons.Left))
            {
                // if trying to go left
                if (e.X > mDown.X)
                {
                    if (pb_tree.Left >= panel_tree.Left)
                        move_x = false;
                }
                // if trying to go right
                if (e.X < mDown.X)
                {
                    if (pb_tree.Right <= initialPanelSize.Width)
                        move_x = false;
                }

                // if trying to go up
                if (e.Y > mDown.Y)
                {
                    if (pb_tree.Top >= 0)
                        move_y = false;
                }

                //if trying to go down
                if (e.Y < mDown.Y)
                {
                    if (pb_tree.Bottom <= initialPanelSize.Height)
                        move_y = false;
                }

                //if (pb_topleft.Y >= panel_topleft.Y)
                if (move_x)
                    pb_tree.Left += e.X - mDown.X;
                if (move_y)
                    pb_tree.Top += e.Y - mDown.Y;
            }
        }

        // DEBUG
        private void debugAll(bool debugNodes = false, bool debugTree = false, bool debugTable = false, bool debugSimple = false)
        {

            if (debugNodes || debugTree || debugTable || debugSimple)
                Console.WriteLine("\nDEBUG\n");

            if (debugNodes)
            {
                Console.WriteLine("INITIALIZED NODES");
                foreach (Operator x in this.nodeManager.nodes.Where(x => x.GetType() == typeof(Operator)))
                {
                    Console.WriteLine("\nNode {0}\n", x.Value);
                    if (x.Left_child != null)
                        Console.WriteLine("\tLC: {0}", x.Left_child.Value);
                    if (x.Right_child != null)
                        Console.WriteLine("\tRC: {0}", x.Right_child.Value);
                }
            }

            if (debugTree)
            {
                Console.WriteLine("DRAW TREE");
                foreach (INode node in this.nodeManager.nodes)
                    Console.WriteLine($"Node {node.Value} drawn at ({node.X_coord}, {node.Y_coord})");
            }

            if (debugSimple)
            {
                Console.WriteLine("\nSIMPLIFICATION");

                Console.WriteLine("\nSTEP 1: Groups Based on nr of 1's");
                foreach (KeyValuePair<int, List<string>> entry in this.active_truth_table.nr_of_ones_groups)
                {
                    Console.WriteLine($"Group {entry.Key}");
                    foreach (string val in this.active_truth_table.nr_of_ones_groups[entry.Key])
                        Console.WriteLine(val);
                }

                Console.WriteLine("\nSTEP 2: Simplified Groups");
                foreach (string simplified_row in this.active_truth_table.simplified_rows)
                    Console.WriteLine(simplified_row);
            }

        }

        private void ReadTests(string filename = "ale1_input.xlsx", bool debug = false)
        {
            string full_path = projectDirectory + "\\" + filename;

            FileStream fStream = File.Open(@full_path, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fStream);
            DataSet resultDataSet = excelDataReader.AsDataSet();
            excelDataReader.Close();

            foreach (DataTable sheet in resultDataSet.Tables)
            {
                // skip if last sheet
                if (sheet.TableName == "Sheet1")
                    break;

                foreach (DataRow row in sheet.Rows)
                {
                    // Convert binary numbers into long ints before turning into string
                    string infix = row["Column0"].ToString();
                    string prefix = row["Column1"].ToString();
                    string bin_bot = row["Column2"].ToString();
                    string hash_bot = row["Column3"].ToString();
                    string bin_top = row["Column4"].ToString();
                    string hash_top = row["Column5"].ToString();
                    string simple = row["Column6"].ToString();

                    switch (sheet.TableName)
                    {
                        case "simple(2operands)":
                            this.input_tests_2.Add(new InputTest(this, infix, prefix, bin_bot, hash_bot, bin_top, hash_top, simple));
                            lb_tests_2.Items.Add(infix);
                            break;
                        case "3operands":
                            this.input_tests_3.Add(new InputTest(this, infix, prefix, bin_bot, hash_bot, bin_top, hash_top, simple));
                            lb_tests_3.Items.Add(infix);
                            break;
                        case "4operands":
                            this.input_tests_4.Add(new InputTest(this, infix, prefix, bin_bot, hash_bot, bin_top, hash_top, simple));
                            lb_tests_4.Items.Add(infix);
                            break;
                        default: // rest sheets have same fields
                            break;
                    }
                }
            }

            // remove first item from lists (column names)
            lb_tests_2.Items.RemoveAt(0);
            lb_tests_3.Items.RemoveAt(0);
            lb_tests_4.Items.RemoveAt(0);

            if (debug == true)
            {
                Console.WriteLine("\n\n2 operand sheet tests:\n");
                foreach (InputTest it in this.input_tests_2)
                {
                    Console.WriteLine($"infix: {it.formula_infix}");
                    Console.WriteLine($"prefix: {it.formula_prefix}");
                    Console.WriteLine($"bin_bot: {it.binary_bottom}");
                    Console.WriteLine($"hash_bot: {it.hashcode_bottom}");
                    Console.WriteLine($"simple: {it.simplify}");
                    Console.WriteLine();
                }

                Console.WriteLine("\n\n3 operand sheet tests:\n");
                foreach (InputTest it in this.input_tests_3)
                {
                    Console.WriteLine($"infix: {it.formula_infix}");
                    Console.WriteLine($"prefix: {it.formula_prefix}");
                    Console.WriteLine($"bin_bot: {it.binary_bottom}");
                    Console.WriteLine($"hash_bot: {it.hashcode_bottom}");
                    Console.WriteLine($"simple: {it.simplify}");
                    Console.WriteLine();
                }

                Console.WriteLine("\n\n4 operand sheet tests:\n");
                foreach (InputTest it in this.input_tests_4)
                {
                    Console.WriteLine($"infix: {it.formula_infix}");
                    Console.WriteLine($"prefix: {it.formula_prefix}");
                    Console.WriteLine($"bin_bot: {it.binary_bottom}");
                    Console.WriteLine($"hash_bot: {it.hashcode_bottom}");
                    Console.WriteLine($"simple: {it.simplify}");
                    Console.WriteLine();
                }
            }
        }

        private void lb_tests_2_SelectedValueChanged(object sender, EventArgs e)
        {
            string selectedItem = lb_tests_2.SelectedItem.ToString();

            InputTest input_test = this.input_tests_2.FirstOrDefault(t => t.formula_infix == selectedItem);
            bool passed = input_test.Test();

            DisplayTestResults(input_test, passed);
        }

        private void lb_tests_3_SelectedValueChanged(object sender, EventArgs e)
        {
            string selectedItem = lb_tests_3.SelectedItem.ToString();

            InputTest input_test = this.input_tests_3.FirstOrDefault(t => t.formula_infix == selectedItem);
            bool passed = input_test.Test();

            DisplayTestResults(input_test, passed);
        }

        private void lb_tests_4_SelectedValueChanged(object sender, EventArgs e)
        {
            string selectedItem = lb_tests_4.SelectedItem.ToString();

            InputTest input_test = this.input_tests_4.FirstOrDefault(t => t.formula_infix == selectedItem);
            bool passed = input_test.Test();

            DisplayTestResults(input_test, passed);
        }

        private void DisplayTestResults(InputTest input_test, bool passed)
        {
            int bin_length = 0;
            int hex_length = 0;
            int simp_length = 0;

            string test_results = "";
            if (passed)
            {
                test_results += "Binary\r\n";
                test_results += $"\tExpected = {input_test.binary_bottom} / {input_test.binary_top}\r\n";
                test_results += $"\tActual = {this.nodeManager.formula_binary}\r\n";
                bin_length = test_results.Length - 3;

                test_results += "Hash\r\n";
                test_results += $"\tExpected = {input_test.hashcode_bottom} / {input_test.hashcode_top}\r\n";
                test_results += $"\tActual = {this.nodeManager.formula_hex}\r\n";
                hex_length = test_results.Length - bin_length - 6;

                test_results += "Simplified\r\n";
                string expected = input_test.simplify == "none" ? "False" : "True";
                test_results += $"\tExpected = {expected}\r\n";
                test_results += $"\tActual = {this.active_truth_table.can_simplify}\r\n";
                simp_length = test_results.Length - (hex_length + bin_length);
            }
            else
                test_results = "Could not process this input.";

            rtb_tests.Text = test_results;

            rtb_tests.SelectionStart = 0;
            rtb_tests.SelectionLength = bin_length;
            if (input_test.bin_test == true)
                rtb_tests.SelectionColor = Color.Green;
            else
                rtb_tests.SelectionColor = Color.Red;

            rtb_tests.SelectionStart = bin_length;
            rtb_tests.SelectionLength = hex_length;
            if (input_test.hash_test == true)
                rtb_tests.SelectionColor = Color.Green;
            else
                rtb_tests.SelectionColor = Color.Red;

            rtb_tests.SelectionStart = hex_length + bin_length;
            rtb_tests.SelectionLength = simp_length;
            if (input_test.simp_test == true)
                rtb_tests.SelectionColor = Color.Green;
            else
                rtb_tests.SelectionColor = Color.Red;
        }
    }
}