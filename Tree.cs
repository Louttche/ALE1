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
    public class Tree
    {
        private Size size;

        private Graphics g;
        public float zoom = 1f;
        private Point left_most_child_pos;
        private Point right_most_child_pos;
        private bool drawn;
        private int panel_offset;

        private Form1 form;

        public Tree(Form1 active_form)
        {
            this.form = active_form;

            this.drawn = false;
            this.left_most_child_pos.X = 0;
            this.right_most_child_pos.X = 0;

            // Connect paint event to UI
            this.form.ui_pb_tree.Paint += new PaintEventHandler(Draw_Tree);
            this.form.ui_pb_tree.Refresh();
        }

        public void Draw_Tree(object sender, PaintEventArgs e)
        {
            Console.WriteLine("panel size: " + form.ui_pb_tree.Size);
            Console.WriteLine("panel location: " + form.ui_pb_tree.Location);

            // Don't draw nandified formula (overflow)
            if (this.form.nodeManager.nodes[0].Value == '%')
                return;

            g = e.Graphics;
            g.Clear(Color.White);
            g.ScaleTransform(this.zoom, this.zoom);

            // Set top center values
            int radius = 40;
            int x_coord_init = (this.form.ui_pb_tree.Width / 2) - radius; // -radius because the point is at top left
            int y_coord_init = 20;
            this.left_most_child_pos = new Point(x_coord_init, y_coord_init);
            this.right_most_child_pos = new Point(x_coord_init, y_coord_init);

            DrawTreeChild((Operator)this.form.nodeManager.nodes[0], x_coord_init, y_coord_init, radius, g);

            //if (this.drawn == false)
            //{
            //    this.form.ui_pb_tree.Width += Math.Abs(this.form.ui_pb_tree.Location.X - this.left_most_child_pos.X)
            //        + Math.Abs((this.form.ui_pb_tree.Location.X + this.form.ui_pb_tree.Width) - this.right_most_child_pos.X); // Left | Right

            //    if (this.left_most_child_pos.Y > this.right_most_child_pos.Y)
            //        this.form.ui_pb_tree.Height += Math.Abs(this.left_most_child_pos.Y);
            //    else
            //        this.form.ui_pb_tree.Height += Math.Abs(this.right_most_child_pos.Y);

            //    this.drawn = true;
            //}
        }

        private void DrawTreeChild(Operator o, int x, int y, int radius, Graphics g, double x_offset = 0, bool debug = false)
        {
            Color color = Color.Black;
            Pen linePen = new Pen(Color.Black, 3);

            if (debug == true) { Console.WriteLine("Drawing " + o.Value + "\tat: (" + x + "," + y + ")"); }
            // Draw operator
            o.DrawNode(x, y, radius, color, g);

            // Set up offsets for this node
            if (x_offset == 0)
            {
                //double nr_of_levels = Math.Log(this.operators.Count(), 2);
                //x_offset = nr_of_levels * (nr_of_levels / 10);

                x_offset = Math.Pow(2, this.form.nodeManager.operators.Count() - 2);
            }
            else
                x_offset = x_offset - (x_offset / 2);

            this.panel_offset = Convert.ToInt32(x_offset);
            double y_offset = radius * 2;

            // Draw children
            if (o.Left_child != null)
            {
                int x_child = (int)(x - x_offset * radius);
                int y_child = (int)(y + y_offset);

                // Set this is the left-most child if it is so far
                if (x_child < this.left_most_child_pos.X)
                    this.left_most_child_pos.X = x_child;
                if (y_child > this.left_most_child_pos.Y)
                    this.left_most_child_pos.Y = y_child;

                // Draw line for left child
                g.DrawLine(linePen, x + (radius / 2), y + radius, x_child + (radius / 2), y_child);

                if (o.Left_child is Operator)
                    // If child is operator, call this method again
                    DrawTreeChild((Operator)o.Left_child, x_child, y_child, radius, g, x_offset);
                else
                {
                    // Draw left operant child
                    o.Left_child.DrawNode(x_child, y_child, radius, color, g, false);
                    if (debug == true) { Console.WriteLine("Drawing " + o.Left_child.Value + "\tat: (" + x_child + "," + y_child + ")"); }
                }
            }

            if (o.Right_child != null)
            {
                int x_child = (int)(x + x_offset * radius);
                int y_child = (int)(y + y_offset);

                // Set this is the left-most child if it is so far
                if (x_child > this.right_most_child_pos.X)
                    this.right_most_child_pos.X = x_child;
                if (y_child > this.right_most_child_pos.Y)
                    this.right_most_child_pos.Y = y_child;

                // Draw line for right child
                g.DrawLine(linePen, x + (radius / 2), y + radius, x_child + (radius / 2), y_child);

                if (o.Right_child is Operator)
                    // If child is operator, call this method again
                    DrawTreeChild((Operator)o.Right_child, x_child, y_child, radius, g, x_offset);
                else
                {
                    // Draw right operant child
                    o.Right_child.DrawNode(x_child, y_child, radius, color, g, false);
                    if (debug == true) { Console.WriteLine("Drawing " + o.Right_child.Value + "\tat: (" + x_child + "," + y_child + ")"); }
                }
            }
        }
    }
}
