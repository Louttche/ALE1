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

namespace ALE1_Katerina
{
    public partial class Form1 : Form
    {
        public string formula = "";
        public int formula_index = 0;
        Stack<INode> parent_stack = new Stack<INode>();

        public List<TreeNode> tree_nodes = new List<TreeNode>();
        public List<INode> formula_nodes = new List<INode>(); // using interface + inheritance

        public List<char> ascii_operators = new List<char>() {'~', '>', '=', '&', '|'};
        //public List<char> ascii_operators = new List<char>() {'(' , ')', ','};

        public List<Operator> operators = new List<Operator>();
        public List<Operant> operants = new List<Operant>();

        //public System.Drawing.Graphics graphicsObj;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            // Clear existing lists of variables and operators
            this.operants.Clear();
            this.operators.Clear();
            this.formula_nodes.Clear();
            this.parent_stack.Clear();

            table_truth.RowStyles.Clear();
            table_truth.ColumnStyles.Clear();
            table_truth.Controls.Clear();

            // Get formula from text box //
            this.formula = tb_formula.Text.Replace(" ", ""); // --> remove spaces
            formula_index = 0;

            // recursive method initializes all nodes with their children/parent
            AddNode(this.formula);
            Draw_Truth_Table(this.operants);

            // DEBUG
            //_nodesDebug();

            // Connect paint event to UI
            tc_1.TabPages[0].Paint += new PaintEventHandler(Draw_Tree);
            tc_1.TabPages[0].Refresh();
            //panel_tree.Paint += new PaintEventHandler(Draw_Tree);
            //panel_tree.Refresh();

            // Display variables
            ShowVariables(this.operants);
        }

        private void AddNode(string formula)
        {

            INode n;
            int id_index = this.formula_nodes.Count + 1;

            for (int i = formula_index; i < formula.Length; i = formula_index)
            {
                id_index = this.formula_nodes.Count + 1;
                char c = formula[i];
                //Console.WriteLine(c + " : " + formula_index + " : " + i);

                switch (c)
                {
                    case '(': // Add children
                        formula_index++;
                        AddNode(formula);
                        break;
                    case ')':
                        // Last parent has no more children to add so remove from stack
                        if (this.parent_stack.Count > 0)
                            this.parent_stack.Pop();
                        return; // to exit from child method (of recursion)
                    case ',':
                        // TODO: Indicates that next char is next child (for now pass works)
                        break;
                    default: // Operants/Operators

                        // Add the nodes to their respective list
                        if (ascii_operators.Contains(c)) {
                            n = new Operator(id_index, c, null, null,
                                this.parent_stack.Count > 0 ? this.parent_stack.Peek().ID : -1);
                            this.operators.Add((Operator)n);
                        }
                        else {
                            n = new Operant(id_index, c,
                                this.parent_stack.Count > 0 ? this.parent_stack.Peek().ID : -1);

                            // Don't add if same operant already exists.
                            if (this.operants.Any(x => x.Value == n.Value) == false)
                                this.operants.Add((Operant)n);
                        }

                        // Add this node as child to parent in stack
                        if (this.parent_stack.Count > 0)
                        {
                            foreach (Operator o in this.formula_nodes.Where(x => x.GetType() == typeof(Operator)))
                            {
                                if (o.ID == this.parent_stack.Peek().ID)
                                    o.AddChild(n);

                                // TODO: Also update the operator in the operators list
                            }
                        }

                        // Add this in parent stack if operator
                        if (n.GetType() == typeof(Operator))
                            this.parent_stack.Push(n);

                        // Add it to the general list of nodes
                        if (n.GetType() != typeof(INode))
                            this.formula_nodes.Add(n);

                        break;
                }

                formula_index++;
            }
        }

        private void InitializeTreeNodes(string formula)
        {
            int i = 0;
            int parent_id = -1;
            TreeNode n = null;

            foreach (char s in formula)
            {
                if (s == ')')
                    parent_id = -1;
                else if (s == '(')
                    parent_id = i - 1;
                else if (s == ',')
                    ;
                else // is operant/operator
                {
                    n = new TreeNode(i, s, null, null);
                    if (parent_id >= 0) // Has parent/root
                    {
                        foreach (TreeNode p in tree_nodes)
                        {
                            if (p.ID == parent_id)
                                p.AddChild(n);
                        }
                    }

                    if (n != null)
                        tree_nodes.Add(n);
                    i++;
                }
            }
        }              

        private void Draw_Truth_Table(List<Operant> variables)
        {
            int numOfVariables = variables.Count();
            Console.WriteLine("Number of  vars this time: " + numOfVariables);
            int numOfColumns = numOfVariables + 1;
            double numOfRows = Math.Pow(2, numOfVariables); //number of rows for the truth values - without the variable/formula row
            Dictionary<char, double> each_var_numOfZeroes = new Dictionary<char, double>();
            Dictionary<char, bool> changeTruthValue = new Dictionary<char, bool>();
            //bool changeTruthValue = false;

            table_truth.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            table_truth.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table_truth.ColumnCount = numOfColumns;
            table_truth.RowCount = (int)numOfRows + 1;

            for (int row = 0; row < numOfRows + 1; row++)
            {
                int variablesLeft = numOfVariables;
                for (int col = 0; col < numOfColumns; col++)
                {
                    // Draw Variable e.g. 'A' or its zeroes/ones / truth values
                    if (variablesLeft > 0) //i < numOfColumns - 1
                    {

                        Label temp_table_value = new Label();
                        // Draw Variable e.g. 'A'
                        if (row == 0)
                        {
                            // Set the number of zeroes/ones for that variable using a dictionary as reference
                            each_var_numOfZeroes.Add(variables[col].Value, Math.Pow(2, variablesLeft - 1));
                            // Add to dictionary that references whether this variable or not needs to switch zeroes/ones
                            changeTruthValue.Add(variables[col].Value, false);

                            variables[col].Truth_value = false;

                            temp_table_value.Text = variables[col].Value.ToString();
                            temp_table_value.Font = new Font(Label.DefaultFont, FontStyle.Bold);
                            table_truth.Controls.Add(temp_table_value, col, row);
                        }
                        // Draw zeroes/ones / truth values
                        else
                        {
                            // Draw zero/one
                            temp_table_value.Text = variables[col].Truth_value == true ? 1.ToString() : 0.ToString();  //truthValue == true ? 1.ToString() : 0.ToString();
                            table_truth.Controls.Add(temp_table_value, col, row);

                            // Switch to one/zero for the corresponding var using the dictionary
                            double numOfZeroes;
                            if (each_var_numOfZeroes.TryGetValue(variables[col].Value, out numOfZeroes))
                            {
                                if (row % numOfZeroes == 0)
                                    changeTruthValue[variables[col].Value] = true;
                                else
                                    changeTruthValue[variables[col].Value] = false;
                            }
                        }

                        variablesLeft -= 1;
                    }
                    else // Draw Formula or its truth results
                    {
                        Label temp_table_value = new Label();

                        // Draw Formula
                        if (row == 0)
                        {
                            temp_table_value.Font = new Font(Label.DefaultFont, FontStyle.Bold);
                            temp_table_value.Text = this.formula;
                            table_truth.Controls.Add(temp_table_value, col, row);
                        }
                        // TODO: Fix it
                        else { // Calculate and Draw truth value
                            temp_table_value.Text = Get_Result(operators[0]).ToString();
                            table_truth.Controls.Add(temp_table_value, col, row);

                            // switch to zeroes/ones depending on variable
                            foreach (Operant operant_var in variables)
                            {
                                if (changeTruthValue[operant_var.Value] == true)
                                    operant_var.Truth_value = !operant_var.Truth_value;
                            }                                
                        }
                    }
                }
            }
        }

        private int Get_Result(Operator o)
        {
            int result = -1;
            int result_left = -1;
            int result_right = -1;

            if (o.Left_child != null)
            {
                if (o.Left_child.GetType() == typeof(Operator))
                    result_left = Get_Result((Operator)o.Left_child);
                else
                    result_left = o.Left_child.Truth_value == true ? 1 : 0;
            }

            if (o.Right_child != null)
            {
                if (o.Right_child.GetType() == typeof(Operator))
                    result_right = Get_Result((Operator)o.Right_child);
                else
                    result_right = o.Right_child.Truth_value == true ? 1 : 0;
            }

            if ((result_left > -1) || (result_right > -1))
            {
                switch (o.Value)
                {
                    // ~ always has only 1 child
                    case '~':
                        result = result_left == 0 ? 1 : 0;
                        break;
                    case '>':
                        if ((result_left == 1) && (result_right == 0))
                            result = 0;
                        else
                            result = 1;
                        break;
                    case '=':
                        if (result_left == result_right)
                            result = 1;
                        else
                            result = 0;
                        break;
                    case '&':
                        if ((result_left == 1) && (result_right == 1))
                            result = 1;
                        else
                            result = 0;
                        break;
                    case '|':
                        if ((result_left == 1) || (result_right == 1))
                            result = 1;
                        else
                            result = 0;
                        break;
                    default:
                        break;
                }
            }
            
            return result;
        }

        private void Draw_Tree(object sender, PaintEventArgs e) // TODO
        {
            var g = e.Graphics;
            System.Drawing.Color color = System.Drawing.Color.Black;
            List<int> drawn_nodes_id = new List<int>();

            // Get panel dimensions
            float max_width = tc_1.Width;
            float max_height = tc_1.Height;

            // Initialize to top center
            int x_coord_init = (tc_1.Width / 2) - 30;
            int y_coord_init = 20;
            int radius = 40;

            int x_coord_curr = x_coord_init;
            int y_coord_curr = y_coord_init;

            int x_child_offset = radius * 2;
            int y_child_offset = 20 + (radius * 2);

            foreach (INode n in this.formula_nodes)
            {
                // if not drawn yet
                if (n.Drawn == false)
                {
                    n.DrawNode(x_coord_init, y_coord_init, radius, color, g);
                    drawn_nodes_id.Add(n.ID);
                }

                // draw children
                if (n is Operator)
                {
                    if (n.GetType().GetProperty("Left_child") != null)
                    {
                        x_coord_curr = x_coord_init - x_child_offset;
                        y_coord_curr = y_coord_init + y_child_offset;
                        //n.left_child.DrawNode(x_coord_curr, y_coord_curr, radius, color, g);
                    }
                    if (n.GetType().GetProperty("Right_child") != null)
                    {
                        x_coord_curr = x_coord_init + x_child_offset;
                        y_coord_curr = y_coord_init + y_child_offset;
                        //DrawNode()
                    }
                }
            }
        }

        private void ShowVariables(List<Operant> vars){
            string var_string = "";

            foreach (Operant o in vars)
            {
                var_string += o.Value + ", ";
            }

            // cut the last 2 chars (", ") from string
            lbl_vars.Text = var_string.Substring(0, var_string.Length - 2);
        }

        private void ShowBinary()
        {
            ;
        }

        private void ShowHex()
        {
            ;
        }

        // DEBUG
        private void _nodesDebug()
        {
            foreach (Operator x in this.formula_nodes.Where(x => x.GetType() == typeof(Operator)))
            {
                Console.WriteLine("\nNode {0}\n", x.Value);
                if (x.Left_child != null)
                    Console.WriteLine("\tLC: {0}", x.Left_child.Value);
                if (x.Right_child != null)
                    Console.WriteLine("\tRC: {0}", x.Right_child.Value);
            }
        }
    }
}
