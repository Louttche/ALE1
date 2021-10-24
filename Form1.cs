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
        Stack<Operator> parent_stack = new Stack<Operator>();

        public List<TreeNode> tree_nodes = new List<TreeNode>();
        public List<INode> formula_nodes = new List<INode>();

        //public List<char> ascii_operators = new List<char>() { '~', '>', '=', '&', '|' };
        public Dictionary<char, string> infix_notations = new Dictionary<char, string> { // ascii : notation
            { '~', Char.ConvertFromUtf32(172) },
            { '>', Char.ConvertFromUtf32(8658) },
            { '=', Char.ConvertFromUtf32(8660) },
            { '&', Char.ConvertFromUtf32(8743) },
            { '|', Char.ConvertFromUtf32(8744) },
            { '%', Char.ConvertFromUtf32(8892) }
        };
        public string infix_formula = "";
        public bool right_node = false;

        public List<Operator> operators = new List<Operator>();
        public List<Operant> operants = new List<Operant>();

        // For Simplification
        public List<string> truth_rows = new List<string>();
        Dictionary<int, List<string>> nr_of_ones_groups = new Dictionary<int, List<string>>();
        List<string> nr_of_zeros = new List<string>();
        List<string> simplified_rows = new List<string>();

        public string formula_binary = "";

        public Form1()
        {
            InitializeComponent();
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

            this.nr_of_zeros.Clear();

            table_simple.RowStyles.Clear();
            table_simple.ColumnStyles.Clear();
            table_simple.Controls.Clear();

            this.nr_of_ones_groups.Clear();
            this.simplified_rows.Clear();
            this.truth_rows.Clear();

            this.formula_binary = " ";
            this.right_node = false;
            this.infix_formula = "";

            // Get formula from text box //
            this.formula = tb_formula.Text.Replace(" ", ""); // --> remove spaces
            formula_index = 0;

            AddNode(this.formula);              // recursive method initializes all nodes with their children/parent
            ConvertAsciiToInfix();
            DrawTruthTable(this.operants);      // creates and draws truth table values
            Simplify();                         // simplifies truth tables
            DrawSimpleTable();                  // draws table for simplified values
            Normalize();                        // normalizes the truth table and its simplified form
            Nandify();                          // nandifies nodes

            lbl_binary.Text = " ";
            lbl_binary.Text = this.formula_binary;
            Convert2Hex(this.formula_binary);       // shows the hexadecimal form of the formula

            // Connect paint event to UI
            panel_tree.Paint += new PaintEventHandler(Draw_Tree);
            panel_tree.Refresh();

            // Display variables
            ShowVariables(this.operants);

            // DEBUG
            _nodesDebug(false, false, false); // nodes | tree | simplification
        }

        private void AddNode(string formula)
        {
            INode n;
            int id_index = this.formula_nodes.Count + 1;

            for (int i = formula_index; i < formula.Length; i = formula_index)
            {
                id_index = this.formula_nodes.Count + 1;
                char c = formula[i];

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
                        if (this.infix_notations.Keys.Contains(c)) {
                            n = new Operator(id_index, c, this, null, null,
                                this.parent_stack.Count > 0 ? this.parent_stack.Peek().ID : -1);
                            this.operators.Add((Operator)n);
                        }
                        else {
                            n = new Operant(id_index, c, this,
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
                            this.parent_stack.Push((Operator) n);

                        // Add it to the general list of nodes
                        if (n.GetType() != typeof(INode))
                            this.formula_nodes.Add(n);

                        break;
                }

                formula_index++;
            }
        }

        public void Draw_Tree(object sender, PaintEventArgs e)
        {
            // Don't draw nandified formula (overflow)
            if (formula_nodes[0].Value == '%')
                return;

            var g = e.Graphics;
            System.Drawing.Color color = System.Drawing.Color.Black;

            // TODO: Find a way to resize if tree goes out of bounds
            //panel_tree.Width = 350;

            // Set top center values
            int x_coord_init = (panel_tree.Width / 2) - 30;
            int y_coord_init = 20;
            int radius = 40;

            DrawTreeChild((Operator) this.formula_nodes[0], x_coord_init, y_coord_init, radius, g);
        }

        private void DrawTreeChild(Operator o, int x, int y, int radius, Graphics g, double x_offset = 0, bool debug = false)
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

        private void ConvertAsciiToInfix()
        {
            Get_Notation((Operator) this.formula_nodes[0]); // First node in formula list is always the head parent
            lbl_notation.Text = infix_formula;
        }

        private void Get_Notation(Operator parent, int parent_pos = 0)
        {
            // if not root node, and not negation, add child with () around it
            if (parent.ID != operators[0].ID && parent.Value != '~')
            {
                this.infix_formula = this.infix_formula.Insert(parent_pos, ")");
                this.infix_formula = this.infix_formula.Insert(parent_pos, this.infix_notations[parent.Value]);
                this.infix_formula = this.infix_formula.Insert(parent_pos, "(");
                parent_pos++;
            } else
                this.infix_formula = this.infix_formula.Insert(parent_pos, this.infix_notations[parent.Value]);

            // LEFT CHILD
            if (parent.Left_child != null) {
                // if is operator
                if (parent.Left_child.GetType() == typeof(Operator)) {
                    if (parent.Value == '~')
                        parent_pos++;

                    Get_Notation((Operator)parent.Left_child, parent_pos);
                }
                // if is variable
                else {
                    if (parent.Value == '~')
                    {
                        parent_pos += 2;
                        this.infix_formula = this.infix_formula.Insert(parent_pos - 1, ")");
                        this.infix_formula = this.infix_formula.Insert(parent_pos - 1, "(");
                    }

                    this.infix_formula = this.infix_formula.Insert(parent_pos, parent.Left_child.Value.ToString());
                    parent_pos++;
                }
            }

            // RIGHT CHILD
            if (parent.Right_child != null) {

                // if root node's right child, reset position
                if (parent.ID == this.formula_nodes[0].ID)
                    parent_pos = this.infix_formula.Length - 1;

                // if is operator
                if (parent.Right_child.GetType() == typeof(Operator))
                {
                    Get_Notation((Operator)parent.Right_child, parent_pos + 1);
                }
                // if is variable
                else
                {
                    this.infix_formula = this.infix_formula.Insert(parent_pos + 1, parent.Right_child.Value.ToString());
                }
            }
        }

        private void DrawTruthTable(List<Operant> variables)
        {
            int numOfVariables = variables.Count();
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

                            // Find all nodes with the same operant and set their truth value
                            IEnumerable<INode> ns = formula_nodes.Where(n => n.Value == variables[col].Value);
                            foreach (INode n in ns)
                                n.Truth_value = false;

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
                            temp_table_value.AutoSize = true;
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

            //Remove the spaces from the binary string
            this.formula_binary = this.formula_binary.Replace(" ", "");
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
                    case '%':
                        if (!(result_left == 1 && result_right == 1))
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

        private void Convert2Hex(string binary_s)
        {
            binary_s = FillInZeroes(binary_s);

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

                    // Reset sum and pos_value
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
                {
                    Console.WriteLine("lalal");
                    binary_s = "00" + binary_s;
                }
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
            Simplify_CompareGroups(this.nr_of_ones_groups);
            
        }

        private void Simplify_CompareGroups(Dictionary<int, List<string>> groups)
        {
            // Append all of dictionary's values (List<string>) to one list
            List<string> old_simplified = new List<string>();
            foreach (KeyValuePair<int, List<string>> group in groups)
            {
                if (group.Value.Count() > 0)
                    old_simplified.AddRange(group.Value);
            }

            Dictionary<int, List<string>> simplified_groups = new Dictionary<int, List<string>>(); // After simplification groups

            KeyValuePair<int, List<string>> last_group = groups.Last();

            foreach (KeyValuePair<int, List<string>> group in groups)
            {
                // Initialize blank table/dictionary
                simplified_groups.Add(group.Key, new List<string>());

                // Discard last group, can't compare it with anything more
                if (last_group.Equals(group))
                    break;

                // Main row we're comparing in main group
                foreach (string s_row in group.Value)
                {
                    // Compare with each row of the group that comes next
                    foreach (string other_row in groups[group.Key + 1])
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
                            }
                            else // it matches
                                simplified_row += s_row[truth_value_index];

                            // If its the last pair, add the row to simplified groups
                            if (truth_value_index + 1 == this.operants.Count())
                            {
                                if (!this.simplified_rows.Contains(simplified_row))
                                {
                                    this.simplified_rows.Add(simplified_row);
                                    simplified_groups[group.Key].Add(simplified_row);
                                }
                            }
                        }
                    }
                }
            }

            // if the current table matches the previous table (no more simplification can be done)
            if (this.simplified_rows.Count >= 1)
            {
                for (int i = 0; i < this.simplified_rows.Count; i++)
                {
                    if ((old_simplified.Count != this.simplified_rows.Count) || (old_simplified[i] != this.simplified_rows[i]) )
                    {
                        // Does not match so simplify more
                        this.simplified_rows.Clear();
                        this.Simplify_CompareGroups(simplified_groups);
                        break;
                    }
                }
            }
            // if empty but has simplified before, set previous result as result
            else if ((this.simplified_rows.Count < 1) && (old_simplified.Last().Any(s => s == '*')))
                this.simplified_rows = old_simplified;
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
                            temp_table_value.Font = new Font(Label.DefaultFont, FontStyle.Bold);
                            if (col == this.operants.Count()) {// last column: show formula
                                temp_table_value.AutoSize = true;
                                temp_table_value.Text = this.formula;
                            }
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
            else
                lbl_norm_simp.Text = "Doesn't Simplify";
        }

        private void Normalize()
        {
            string result = "";

            // Normalize the whole truth table
            foreach (string t_row in this.truth_rows.Where(s => s.EndsWith("1")))
            {
                result += "(";

                for (int i = 0; i < this.operants.Count(); i++) // value for each variable (don't include result)
                {
                    if (t_row[i] == '0')
                        result += this.infix_notations['~'];

                    result += this.operants[i].Value;

                    // if its not the last variable, add an AND char at the end
                    if (i + 1 < this.operants.Count())
                        result += this.infix_notations['&'];
                }

                result += ")" + this.infix_notations['|'];
            }
            lbl_norm.Text = result.Substring(0, result.Length - 1); // remove last character that is the extra '|'


            // Normalize the simplified truth table
            result = "";
            foreach (string s_row in this.simplified_rows)
            {
                result += "(";
                for (int i = 0; i < s_row.Length; i++)
                {
                    if (s_row[i] == '0')
                        result += this.infix_notations['~'];
                    else if (s_row[i] == '*')
                        continue;
                    
                    result += this.operants[i].Value;

                    if (i + 1 < s_row.Length)
                        result += this.infix_notations['&'];
                }

                if (result.Last() == Convert.ToChar(this.infix_notations['&']))
                    result = result.Substring(0, result.Length - 1);

                result += ")" + this.infix_notations['|'];
            }
            if (result.Length > 0)
                lbl_norm_simp.Text = result.Substring(0, result.Length - 1); // remove last character that is the extra '|'

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

        public void Nandify()
        {
            string nand_formula = NandifyNode((Operator) this.formula_nodes[0]);
            if (nand_formula != null)
                lbl_nand.Text = nand_formula;
        }

        private string NandifyNode(Operator node)
        {
            string l_value = "";
            string r_value = "";

            // Recurse children if they are operators to nandify their children
            switch (node.Value)
            {
                case '~':
                    if (node.Left_child is Operant)
                        return $"%({node.Left_child.Value},{node.Left_child.Value})";
                    else // is Operator
                        return $"%({NandifyNode((Operator) node.Left_child)},{NandifyNode((Operator) node.Left_child)})";
                case '|':
                    l_value = node.Left_child is Operant ? node.Left_child.Value.ToString() : NandifyNode((Operator)node.Left_child);
                    r_value = node.Right_child is Operant ? node.Right_child.Value.ToString() : NandifyNode((Operator)node.Right_child);
                    return $"%(%({l_value},{l_value}),%({r_value},{r_value}))";
                case '&':
                    l_value = node.Left_child is Operant ? node.Left_child.Value.ToString() : NandifyNode((Operator)node.Left_child);
                    r_value = node.Right_child is Operant ? node.Right_child.Value.ToString() : NandifyNode((Operator)node.Right_child);
                    return $"%(%({l_value},{r_value}),%({l_value},{r_value}))";
                case '>':
                    l_value = node.Left_child is Operant ? node.Left_child.Value.ToString() : NandifyNode((Operator)node.Left_child);
                    r_value = node.Right_child is Operant ? node.Right_child.Value.ToString() : NandifyNode((Operator)node.Right_child);
                    return $"%({l_value},%({r_value},{r_value}))";
                case '=':
                    l_value = node.Left_child is Operant ? node.Left_child.Value.ToString() : NandifyNode((Operator)node.Left_child);
                    r_value = node.Right_child is Operant ? node.Right_child.Value.ToString() : NandifyNode((Operator)node.Right_child);
                    return $"%(%(%({l_value},{l_value}),%({r_value},{r_value})),%({l_value},{r_value}))";
                default:
                    break;
            }

            return null;
        }

        // DEBUG
        private void _nodesDebug(bool debugNodes = true, bool debugTree = true, bool debugSimplification = true)
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

            //if (debugTree)
            //{
            //    Console.WriteLine("DRAW TREE");
            //    foreach (Operant n in this.operants)
            //        Console.WriteLine($"Node '{n.Value}' drawn at (x, y) : ({n.X_coord}, {n.Y_coord})");
            //    foreach (Operator n in this.operators)
            //        Console.WriteLine($"Node '{n.Value}' drawn at (x, y) : ({n.X_coord}, {n.Y_coord})");
            //}

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

        private void tb_formula_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                btn_submit.PerformClick();
        }
    }
}