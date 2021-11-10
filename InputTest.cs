using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE1_Katerina
{
    public class InputTest
    {
        public string formula_infix;
        public string formula_prefix;
        public string binary_top;
        public string binary_bottom;
        public string hashcode_top;
        public string hashcode_bottom;
        public string simplify;

        public bool bin_test;
        public bool hash_test;
        public bool simp_test;

        private Form1 form;

        public InputTest(Form1 form, string formula_infix, string formula_prefix, string binary_bottom, string hashcode_bottom, string binary_top, string hashcode_top, string simplify) {
            this.formula_infix = formula_infix;
            this.formula_prefix = formula_prefix;
            this.binary_bottom = binary_bottom;
            this.hashcode_bottom = hashcode_bottom;
            this.binary_top = binary_top;
            this.hashcode_top = hashcode_top;
            this.simplify = simplify;

            this.bin_test = false;
            this.hash_test = false;
            this.simp_test = false;

            this.form = form;
        }

        public bool Test()
        {
            //Console.WriteLine($"\nTesting {formula_infix}...");

            try
            {
                this.form.ResetFormControls();

                string formula = this.formula_prefix.Replace(" ", "");      // --> remove spaces

                // Test Parse
                this.form.nodeManager = new NodeManager(this.form);
                this.form.nodeManager.AddNode(formula);                     // Parse formula and store nodes
                this.form.nodeManager.ConvertAsciiToInfix();

                // Test Truth Table (Binary)
                this.form.active_truth_table = new TruthTable(this.form);   // Creates a Truth Table
                this.form.active_truth_table.SimplifyTable();               // Simplify Table

                this.form.nodeManager.Convert2Hex(this.form.nodeManager.formula_binary);  // Converts the Binary output of the formula to Hexadecimal

                if (this.form.nodeManager.formula_binary == this.binary_bottom || this.form.nodeManager.formula_binary == this.binary_top)
                    this.bin_test = true;

                if (this.form.nodeManager.formula_hex == this.hashcode_bottom || this.form.nodeManager.formula_hex == this.hashcode_top)
                    this.hash_test = true;

                if (this.simplify == "none")
                {
                    if (!this.form.active_truth_table.can_simplify)
                        this.simp_test = true;
                }
                else
                {
                    if (this.form.active_truth_table.can_simplify)
                        this.simp_test = true;
                }

                return true;
            }
            catch (Exception)
            {
                Console.WriteLine($"\nCan't process this yet.");
                return false;
            }
        }
    }
}
