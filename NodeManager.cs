using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE1_Katerina
{
    public class NodeManager
    {
        private Form1 form;
        public string formula = "";
        public string formula_infix = "";
        public string formula_binary = "";
        public List<INode> nodes = new List<INode>();
        public List<Operator> operators = new List<Operator>();
        public List<Operant> operants = new List<Operant>();

        private int formula_index = 0;
        private Stack<Operator> parent_stack = new Stack<Operator>();

        public Dictionary<char, string> infix_notations = new Dictionary<char, string> { // ascii : notation
            { '~', Char.ConvertFromUtf32(172) },
            { '>', Char.ConvertFromUtf32(8658) },
            { '=', Char.ConvertFromUtf32(8660) },
            { '&', Char.ConvertFromUtf32(8743) },
            { '|', Char.ConvertFromUtf32(8744) },
            { '%', Char.ConvertFromUtf32(8892) }
        };

        public NodeManager(Form1 active_form)
        {
            this.form = active_form;

            // Clear existing lists of variables and operators
            this.operants.Clear();
            this.operators.Clear();
            this.nodes.Clear();
            this.parent_stack.Clear();
            this.formula_binary = " ";
            this.formula_infix = "";
            formula_index = 0;
        }

        public void AddNode(string formula)
        {
            INode n;
            int id_index = this.nodes.Count + 1;

            for (int i = formula_index; i < formula.Length; i = formula_index)
            {
                id_index = this.nodes.Count + 1;
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
                        break;
                    default: // Operants/Operators

                        // Add the nodes to their respective list
                        if (this.infix_notations.Keys.Contains(c))
                        {
                            n = new Operator(id_index, c, null, null,
                                this.parent_stack.Count > 0 ? this.parent_stack.Peek().ID : -1);
                            this.operators.Add((Operator)n);
                        }
                        else
                        {
                            n = new Operant(id_index, c, this.parent_stack.Count > 0 ? this.parent_stack.Peek().ID : -1);

                            // Don't add if same operant already exists.
                            if (this.operants.Any(x => x.Value == n.Value) == false)
                                this.operants.Add((Operant)n);
                        }

                        // Add this node as child to parent in stack
                        if (this.parent_stack.Count > 0)
                        {
                            foreach (Operator o in this.nodes.Where(x => x.GetType() == typeof(Operator)))
                            {
                                if (o.ID == this.parent_stack.Peek().ID)
                                    o.AddChild(n);
                            }
                        }

                        // Add this in parent stack if operator
                        if (n.GetType() == typeof(Operator))
                            this.parent_stack.Push((Operator)n);

                        // Add it to the general list of nodes
                        if (n.GetType() != typeof(INode))
                            this.nodes.Add(n);

                        break;
                }

                formula_index++;
            }
        }

        public void ConvertAsciiToInfix()
        {
            Get_Notation((Operator)this.nodes[0]); // First node in formula list is always the head parent
            this.form.ui_lbl_notation.Text = formula_infix;
        }

        private void Get_Notation(Operator parent, int parent_pos = 0)
        {
            // if not root node, and not negation, add child with () around it
            if (parent.ID != operators[0].ID && parent.Value != '~')
            {
                this.formula_infix = this.formula_infix.Insert(parent_pos, ")");
                this.formula_infix = this.formula_infix.Insert(parent_pos, this.infix_notations[parent.Value]);
                this.formula_infix = this.formula_infix.Insert(parent_pos, "(");
                parent_pos++;
            }
            else
                this.formula_infix = this.formula_infix.Insert(parent_pos, this.infix_notations[parent.Value]);

            // LEFT CHILD
            if (parent.Left_child != null)
            {
                // if is operator
                if (parent.Left_child.GetType() == typeof(Operator))
                {
                    if (parent.Value == '~')
                        parent_pos++;

                    Get_Notation((Operator)parent.Left_child, parent_pos);
                }
                // if is variable
                else
                {
                    if (parent.Value == '~')
                    {
                        parent_pos += 2;
                        this.formula_infix = this.formula_infix.Insert(parent_pos - 1, ")");
                        this.formula_infix = this.formula_infix.Insert(parent_pos - 1, "(");
                    }

                    this.formula_infix = this.formula_infix.Insert(parent_pos, parent.Left_child.Value.ToString());
                    parent_pos++;
                }
            }

            // RIGHT CHILD
            if (parent.Right_child != null)
            {

                // if root node's right child, reset position
                if (parent.ID == this.nodes[0].ID)
                    parent_pos = this.formula_infix.Length - 1;

                // if is operator
                if (parent.Right_child.GetType() == typeof(Operator))
                {
                    Get_Notation((Operator)parent.Right_child, parent_pos + 1);
                }
                // if is variable
                else
                {
                    this.formula_infix = this.formula_infix.Insert(parent_pos + 1, parent.Right_child.Value.ToString());
                }
            }
        }

        public void Convert2Hex(string binary_s)
        {
            binary_s = FillInZeroes(binary_s);

            this.form.ui_lbl_hex.Text = "";
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
                    this.form.ui_lbl_hex.Text += each_sum.ToString("X");

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

        public void ShowVariables(List<Operant> vars)
        {
            string var_string = "";

            foreach (Operant o in vars)
            {
                var_string += o.Value + ", ";
            }

            // cut the last 2 chars (", ") from string
            this.form.ui_lbl_vars.Text = var_string.Substring(0, var_string.Length - 2);
        }

        public void Nandify()
        {
            string nand_formula = NandifyNode((Operator)this.nodes[0]);
            if (nand_formula != null)
                this.form.ui_lbl_nand.Text = nand_formula;
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
                        return $"%({NandifyNode((Operator)node.Left_child)},{NandifyNode((Operator)node.Left_child)})";
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
    }
}
