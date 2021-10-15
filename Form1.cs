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

        // For simplification
        public List<string> truth_rows = new List<string>();
        Dictionary<int, List<string>> nr_of_ones_groups = new Dictionary<int, List<string>>();

        List<string> nr_of_zeros = new List<string>();
        List<string> simplified_rows = new List<string>();

        public string formula_binary = "";

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

            table_simple.RowStyles.Clear();
            table_simple.ColumnStyles.Clear();
            table_simple.Controls.Clear();

            this.nr_of_ones_groups.Clear();
            this.simplified_rows.Clear();

            this.formula_binary = " ";

            // Get formula from text box //
            this.formula = tb_formula.Text.Replace(" ", ""); // --> remove spaces
            formula_index = 0;

            AddNode(this.formula);              // recursive method initializes all nodes with their children/parent
            Draw_Truth_Table(this.operants);    // creates and draws truth table values
            Simplify();                         // simplifies truth tables
            DrawSimpleTable();                  // draws table for simplified values

            lbl_binary.Text = " ";
            lbl_binary.Text = this.formula_binary;
            FillInZeroes(this.formula_binary);
            ShowHex(this.formula_binary);
            

            // DEBUG
            _nodesDebug(false, true); // nodes | simplification

            // Connect paint event to UI
            panel_tree.Paint += new PaintEventHandler(Draw_Tree);
            panel_tree.Refresh();

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
            double numOfRows = Math.Pow(2, numOfVariables); // number of rows for the truth values - without the variable/formula row
            Dictionary<char, double> each_var_numOfZeroes = new Dictionary<char, double>();
            Dictionary<char, bool> changeTruthValue = new Dictionary<char, bool>();

            // Simplification
            string temp_row = "";

            table_truth.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            table_truth.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            table_truth.ColumnCount = numOfColumns;
            table_truth.RowCount = (int)numOfRows + 1;

            for (int row = 0; row < numOfRows + 1; row++)
            {
                temp_row = "";
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

                            //variables[col].Truth_value = false; // DEFAULT
                            // Find all nodes with the same operant and set their truth value
                            IEnumerable<INode> ns = formula_nodes.Where(n => n.Value == variables[col].Value);
                            foreach (INode n in ns)
                                n.Truth_value = false;
                                //Console.WriteLine($"{n.Value}'s truth: {n.Truth_value}");

                            temp_table_value.Text = variables[col].Value.ToString();
                            temp_table_value.Font = new Font(Label.DefaultFont, FontStyle.Bold);
                            table_truth.Controls.Add(temp_table_value, col, row);

                        }
                        // Draw zeroes/ones / truth values
                        else
                        {
                            // Draw zero/one
                            string one_zero = variables[col].Truth_value == true ? 1.ToString() : 0.ToString();  //truthValue == true ? 1.ToString() : 0.ToString();
                            temp_table_value.Text = one_zero;
                            table_truth.Controls.Add(temp_table_value, col, row);

                            // For simplification: add 0/1 to row string list
                            temp_row += one_zero;

                            // Set which variable's truth values need to switch to one/zero after the calculation
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
                        else { // Calculate and Draw truth value
                            string result = Get_Result(operators[0]).ToString();
                            temp_table_value.Text = result;
                            table_truth.Controls.Add(temp_table_value, col, row);

                            // For simplification: add result to row string list
                            temp_row += result;

                            // Add the result to the front of the binary list
                            this.formula_binary = result + this.formula_binary;

                            // switch to zeroes/ones depending on variable
                            foreach (Operant operant_var in variables)
                            {
                                if (changeTruthValue[operant_var.Value] == true)
                                {
                                    //operant_var.Truth_value = !operant_var.Truth_value; // DEFAULT (doesnt work when there is more than 1 of the same variables)
                                    // Find all nodes with the same operant and set their truth value
                                    IEnumerable<INode> ns = formula_nodes.Where(n => n.Value == operant_var.Value);
                                    foreach (INode n in ns)
                                        n.Truth_value = !n.Truth_value;
                                }
                            }                                
                        }
                    }

                    // End of Column
                    
                    if (temp_row.Length == numOfColumns)
                        this.truth_rows.Add(temp_row);
                }
                // End of Row
            }
        }

        private int Get_Result(Operator o)
        {
            //Console.WriteLine($"Getting result for: {o.Value}");
            int result = -1;
            int result_left = -1;
            int result_right = -1;

            if (o.Left_child != null)
            {
                //Console.WriteLine($"Getting result for left child: {o.Left_child.Value}");
                if (o.Left_child.GetType() == typeof(Operator))
                    result_left = Get_Result((Operator)o.Left_child);
                else
                    result_left = o.Left_child.Truth_value == true ? 1 : 0;

                //Console.WriteLine($"\tleft result: {result_left}");
            }

            if (o.Right_child != null)
            {
                //Console.WriteLine($"Getting result for right child: {o.Right_child.Value}");
                if (o.Right_child.GetType() == typeof(Operator))
                    result_right = Get_Result((Operator)o.Right_child);
                else
                    result_right = o.Right_child.Truth_value == true ? 1 : 0;

                //Console.WriteLine($"\tright result: {result_right}");
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

            //Console.WriteLine($"final result: {result}");
            return result;
        }

        private void Draw_Tree(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            System.Drawing.Color color = System.Drawing.Color.Black;

            // TODO: Find a way to resize if tree goes out of bounds
            //panel_tree.Width = 350;

            // Set top center values
            int x_coord_init = (panel_tree.Width / 2) - 30;
            int y_coord_init = 20;
            int radius = 40;

            DrawTreeChild(operators[0], x_coord_init, y_coord_init, radius, g);
        }

        private void Simplify()
        {
            /* Using Quine-McCluskey's Technique */

            // Rows with result 0 SHOULD NOT be simplified
            List<string> simplified_truth_rows = new List<string>();

            // Initialise groups dictionary (based on nr of 1's)
            for (int i = 0; i <= this.operants.Count(); i++)
                this.nr_of_ones_groups.Add(i, new List<string>());

            /** STEP 1
            *  Separate rows in groups depending on the nr of ones they have, no 1's, one 1, two 1's ... */

            foreach (string s_row in this.truth_rows)
            {
                if (s_row.Last() == '1') // Don't simplify rows with result = 0
                {
                    string trimmed_row = s_row.Substring(0, s_row.Length - 1); // Remove result from end
                    simplified_truth_rows.Add(trimmed_row);

                    int nr_of_ones = trimmed_row.Count(one => one == '1');
                    this.nr_of_ones_groups[nr_of_ones].Add(trimmed_row);
                } else
                    nr_of_zeros.Add(s_row); // Save the zeroes to draw them on table later
            }

            /** STEP 2
             *  Compare Groups and Simplify pairs */

            KeyValuePair<int, List<string>> last_group = this.nr_of_ones_groups.Last();
            foreach (KeyValuePair<int, List<string>> group in this.nr_of_ones_groups)
            {
                // Discard last group, can't compare it with anything more
                if (last_group.Equals(group))
                    break;

                // Main row we're comparing in main group
                foreach (string s_row in group.Value)
                {
                    // Compare with each row of the group that comes next
                    foreach (string other_row in this.nr_of_ones_groups[group.Key + 1])
                    {
                        // Compare each character
                        int differences = 0;
                        string simplified_row = "";
                        for (int truth_value_index = 0; truth_value_index < this.operants.Count(); truth_value_index++)
                        {
                            if (s_row[truth_value_index] != other_row[truth_value_index])
                            {
                                differences++;
                                if (differences > 1)
                                    break; // Ignore rows with more than 1 differences

                                simplified_row += '*';
                            } else // it matches
                                simplified_row += s_row[truth_value_index];

                            // If its the last pair, add the row to simplified groups
                            if (truth_value_index + 1 == this.operants.Count())
                            {
                                this.simplified_rows.Add(simplified_row);
                            }  
                        }
                    }
                }
            }            
        }

        private void DrawSimpleTable()
        {
            if (this.simplified_rows.Count > 0)
            {
                table_simple.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
                table_simple.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                table_simple.ColumnCount = this.operants.Count() + 1;
                table_simple.RowCount = this.simplified_rows.Count() + this.nr_of_zeros.Count() + 1; // 1 for the Variables and 1 for for zeroes row

                for (int row = 0; row < table_simple.RowCount; row++)
                {
                    for (int col = 0; col < table_simple.ColumnCount; col++)
                    {
                        Label temp_table_value = new Label();

                        if (row == 0)
                        {
                            if (col == this.operants.Count()) // last column: show formula
                                temp_table_value.Text = this.formula;
                            else // Show variable
                                temp_table_value.Text = this.operants[col].Value.ToString();
                        }
                        else
                        {
                            if (row <= this.nr_of_zeros.Count())
                                temp_table_value.Text = this.nr_of_zeros[row - 1][col].ToString();
                            else if (col < this.operants.Count()) // all but last column
                                temp_table_value.Text = this.simplified_rows[row - (this.nr_of_zeros.Count() + 1)][col].ToString();

                            // last columns
                            else if (row < this.nr_of_zeros.Count())
                                temp_table_value.Text = "0";
                            else
                                temp_table_value.Text = "1";
                        }

                        table_simple.Controls.Add(temp_table_value, col, row);
                    }
                }
            }
        }

        private void DrawTreeChild(Operator o, int x, int y, int radius, Graphics g, double x_offset = 0, bool debug = false) //double nrOfoffsets = -1
        {
            System.Drawing.Color color = System.Drawing.Color.Black;

            if (debug == true) { Console.WriteLine("Drawing " + o.Value + "\tat: (" + x + "," + y + ")"); }
            // TODO: Set radius according to node char (in the DrawNode method)
            // Draw operator
            o.DrawNode(x, y, radius, color, g);

            // Set up offsets for this node
            if (x_offset == 0)
                x_offset = Math.Pow(2, operators.Count() - 1); // Correlates to the number of levels the tree will have
            else
                x_offset = x_offset - (x_offset / 2);

            double y_offset = radius * 2;
            //double line_length = Math.Sqrt(Math.Pow(line_x - radius, 2) + Math.Pow(line_y - radius, 2)); // Pythagoras

            // Draw children
            if (o.Left_child != null)
            {
                int x_child = (int)(x - x_offset * radius);
                int y_child = (int)(y + y_offset);

                if (o.Left_child is Operator)
                    // If child is operator, call this method again
                    DrawTreeChild((Operator)o.Left_child, x_child, y_child, radius, g, x_offset);
                else
                {
                    // Draw left operant child
                    o.Left_child.DrawNode(x_child, y_child, radius, color, g);
                    if (debug == true) { Console.WriteLine("Drawing " + o.Left_child.Value + "\tat: (" + x_child + "," + y_child + ")"); }
                }
            }

            if (o.Right_child != null)
            {
                int x_child = (int)(x + x_offset * radius);
                int y_child = (int)(y + y_offset);

                if (o.Right_child is Operator)
                    // If child is operator, call this method again
                    DrawTreeChild((Operator)o.Right_child, x_child, y_child, radius, g, x_offset);
                else
                {
                    // Draw right operant child
                    o.Right_child.DrawNode(x_child, y_child, radius, color, g);
                    if (debug == true) { Console.WriteLine("Drawing " + o.Right_child.Value + "\tat: (" + x_child + "," + y_child + ")"); }
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

        private void ShowHex(string binary_s)
        {
            lbl_hex.Text = "";
            double pos_value = 8; // 8, 4, 2, 1
            int each_sum = 0;
            for (int i = 1; i <= binary_s.Length; i++)
            {
                // Add to sum
                if (binary_s[i - 1] == '1')
                    each_sum += (int)pos_value;

                if (i % 4 == 0)
                {
                    // Convert and add the sum of the current '4' binary digits into hex
                    //if (each_sum > 0) // Don't show 0 hex values
                    lbl_hex.Text += each_sum.ToString("X");

                    // Reset sum and j
                    each_sum = 0;
                    pos_value = 8;
                }
                else
                    pos_value = pos_value / 2;
            }
        }

        private string FillInZeroes(string binary_s)
        {
            int s_length = binary_s.Length;
            // If length is 'missing' 0s for hex, add them in front of string //
            if (s_length % 4 != 0)
            {
                // add 2 0's if length is even
                if (s_length % 2 == 0)
                    binary_s = "00" + binary_s;
                // add 1/3 0's if odd
                else
                {
                    // if the remainder is 1 or bigger than 3, add 3 0's
                    if ((s_length % 4 > 3) || (s_length % 4 == 1))
                        binary_s = "000" + binary_s;
                    // else add 1 0's
                    else
                        binary_s = '0' + binary_s;
                }
            }

            return binary_s;
        }

        // DEBUG
        private void _nodesDebug(bool debugNodes = true, bool debugSimplification = true)
        {

            Console.WriteLine("\n\nDEBUG\n");

            if (debugNodes)
            {
                Console.WriteLine("INITIALIZED NODES");
                foreach (Operator x in this.formula_nodes.Where(x => x.GetType() == typeof(Operator)))
                {
                    Console.WriteLine("\nNode {0}\n", x.Value);
                    if (x.Left_child != null)
                        Console.WriteLine("\tLC: {0}", x.Left_child.Value);
                    if (x.Right_child != null)
                        Console.WriteLine("\tRC: {0}", x.Right_child.Value);
                }
            }

            if (debugSimplification)
            {
                Console.WriteLine("\nSIMPLIFICATION");

                Console.WriteLine("\nSTEP 1: Groups Based on nr of 1's");
                foreach (KeyValuePair<int, List<string>> entry in this.nr_of_ones_groups)
                {
                    Console.WriteLine($"Group {entry.Key}");
                    foreach (string val in this.nr_of_ones_groups[entry.Key])
                        Console.WriteLine(val);
                }

                Console.WriteLine("\nSTEP 2: Simplified Groups");
                foreach (string simplified_row in this.simplified_rows)
                    Console.WriteLine(simplified_row);
            }
        }
    }
}