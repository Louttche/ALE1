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
    public class TruthTable
    {
        private Form1 form;

        public List<string> truth_rows = new List<string>();
        public List<string> simplified_rows = new List<string>();
        private Dictionary<int, List<string>> nr_of_ones_groups = new Dictionary<int, List<string>>();

        public TruthTable(Form1 active_form)
        {
            this.form = active_form;

            this.nr_of_ones_groups.Clear();
            this.simplified_rows.Clear();
            this.truth_rows.Clear();

            this.DrawTruthTable();
        }

        public void DrawTruthTable()
        {
            List<Operant> variables = this.form.nodeManager.operants;
            int numOfVariables = variables.Count();
            int numOfColumns = numOfVariables + 1;
            double numOfRows = Math.Pow(2, numOfVariables); // number of rows for the truth values - without the variable/formula row
            Dictionary<char, double> each_var_numOfZeroes = new Dictionary<char, double>();
            Dictionary<char, bool> changeTruthValue = new Dictionary<char, bool>();

            // Simplification
            string temp_row = "";

            form.ui_table_truth.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            form.ui_table_truth.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            form.ui_table_truth.ColumnCount = numOfColumns;
            form.ui_table_truth.RowCount = (int)numOfRows + 1;

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
                            IEnumerable<INode> ns = this.form.nodeManager.nodes.Where(n => n.Value == variables[col].Value);
                            foreach (INode n in ns)
                                n.Truth_value = false;

                            temp_table_value.Text = variables[col].Value.ToString();
                            temp_table_value.Font = new Font(Label.DefaultFont, FontStyle.Bold);
                            this.form.ui_table_truth.Controls.Add(temp_table_value, col, row);

                        }
                        // Draw zeroes/ones / truth values
                        else
                        {
                            // Draw zero/one
                            string one_zero = variables[col].Truth_value == true ? 1.ToString() : 0.ToString();  //truthValue == true ? 1.ToString() : 0.ToString();
                            temp_table_value.Text = one_zero;
                            this.form.ui_table_truth.Controls.Add(temp_table_value, col, row);

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
                            temp_table_value.Text = this.form.nodeManager.formula;
                            temp_table_value.AutoSize = true;
                            this.form.ui_table_truth.Controls.Add(temp_table_value, col, row);
                        }
                        else
                        { // Calculate and Draw truth value
                            string result = Get_Result(this.form.nodeManager.operators[0]).ToString();
                            temp_table_value.Text = result;
                            this.form.ui_table_truth.Controls.Add(temp_table_value, col, row);

                            // For simplification: add result to row string list
                            temp_row += result;

                            // Add the result to the front of the binary list
                            this.form.nodeManager.formula_binary = result + this.form.nodeManager.formula_binary;

                            // switch to zeroes/ones depending on variable
                            foreach (Operant operant_var in variables)
                            {
                                if (changeTruthValue[operant_var.Value] == true)
                                {
                                    // Find all nodes with the same operant and set their truth value
                                    IEnumerable<INode> ns = this.form.nodeManager.nodes.Where(n => n.Value == operant_var.Value);
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
            this.form.nodeManager.formula_binary = this.form.nodeManager.formula_binary.Replace(" ", "");
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

        public void SimplifyTable()
        {
            /* Using Quine-McCluskey's Technique */

            // Rows with result 0 SHOULD NOT be simplified
            List<string> simplified_truth_rows = new List<string>();

            // Initialise groups dictionary (based on nr of 1's)
            for (int i = 0; i <= this.form.nodeManager.operants.Count(); i++)
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
                }
            }

            /** STEP 2
             *  Compare Groups and Simplify pairs */
            Simplify_CompareGroups(this.nr_of_ones_groups);

            this.DrawSimpleTable();
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
                        for (int truth_value_index = 0; truth_value_index < this.form.nodeManager.operants.Count(); truth_value_index++)
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
                            if (truth_value_index + 1 == this.form.nodeManager.operants.Count())
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
                    if ((old_simplified.Count != this.simplified_rows.Count) || (old_simplified[i] != this.simplified_rows[i]))
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
            List<string> zero_rows = new List<string>();
            zero_rows.AddRange(this.truth_rows.Where(s => s.Last() == '0'));

            if (this.simplified_rows.Count > 0)
            {
                this.form.ui_table_simple.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
                this.form.ui_table_simple.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.form.ui_table_simple.ColumnCount = this.form.nodeManager.operants.Count() + 1;
                this.form.ui_table_simple.RowCount = this.simplified_rows.Count() + zero_rows.Count() + 1; // 1 for the Variables and 1 for for zeroes row

                for (int row = 0; row < this.form.ui_table_simple.RowCount; row++)
                {
                    for (int col = 0; col < this.form.ui_table_simple.ColumnCount; col++)
                    {
                        Label temp_table_value = new Label();

                        if (row == 0)
                        {
                            temp_table_value.Font = new Font(Label.DefaultFont, FontStyle.Bold);
                            if (col == this.form.nodeManager.operants.Count())
                            {// last column: show formula
                                temp_table_value.AutoSize = true;
                                temp_table_value.Text = this.form.nodeManager.formula;
                            }
                            else // Show variable
                                temp_table_value.Text = this.form.nodeManager.operants[col].Value.ToString();
                        }
                        else
                        {
                            if (row <= zero_rows.Count())
                                temp_table_value.Text = zero_rows[row - 1][col].ToString();
                            else if (col < this.form.nodeManager.operants.Count()) // all but last column
                                temp_table_value.Text = this.simplified_rows[row - (zero_rows.Count() + 1)][col].ToString();

                            // last columns
                            else if (row < zero_rows.Count())
                                temp_table_value.Text = "0";
                            else
                                temp_table_value.Text = "1";
                        }

                        this.form.ui_table_simple.Controls.Add(temp_table_value, col, row);
                    }
                }
            }
            else
                this.form.ui_lbl_norm_simp.Text = "Doesn't Simplify";
        }

        public void Normalize()
        {
            string result = "";

            // Normalize the whole truth table
            foreach (string t_row in this.truth_rows.Where(s => s.EndsWith("1")))
            {
                result += "(";

                for (int i = 0; i < this.form.nodeManager.operants.Count(); i++) // value for each variable (don't include result)
                {
                    if (t_row[i] == '0')
                        result += this.form.nodeManager.infix_notations['~'];

                    result += this.form.nodeManager.operants[i].Value;

                    // if its not the last variable, add an AND char at the end
                    if (i + 1 < this.form.nodeManager.operants.Count())
                        result += this.form.nodeManager.infix_notations['&'];
                }

                result += ")" + this.form.nodeManager.infix_notations['|'];
            }
            this.form.ui_lbl_norm.Text = result.Substring(0, result.Length - 1); // remove last character that is the extra '|'


            // Normalize the simplified truth table
            result = "";
            foreach (string s_row in this.simplified_rows)
            {
                result += "(";
                for (int i = 0; i < s_row.Length; i++)
                {
                    if (s_row[i] == '0')
                        result += this.form.nodeManager.infix_notations['~'];
                    else if (s_row[i] == '*')
                        continue;

                    result += this.form.nodeManager.operants[i].Value;

                    if (i + 1 < s_row.Length)
                        result += this.form.nodeManager.infix_notations['&'];
                }

                if (result.Last() == Convert.ToChar(this.form.nodeManager.infix_notations['&']))
                    result = result.Substring(0, result.Length - 1);

                result += ")" + this.form.nodeManager.infix_notations['|'];
            }
            if (result.Length > 0)
                this.form.ui_lbl_norm_simp.Text = result.Substring(0, result.Length - 1); // remove last character that is the extra '|'

        }

    }
}
