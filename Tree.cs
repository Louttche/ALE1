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
        public Size size;

        private Graphics g;
        public float zoom = 1f;
        private bool drawn;
        private double left_offset;

        private Form1 form;

        public Tree(Form1 active_form)
        {
            this.form = active_form;
            this.drawn = false;
            this.left_offset = Math.Pow(2, this.form.nodeManager.operators.Count() - 2);

            this.size = this.form.ui_pb_tree.Size;
            // Connect paint event to UI
            this.form.ui_pb_tree.Paint += new PaintEventHandler(Draw_Tree);
            this.form.ui_pb_tree.Refresh();
        }

        public void Draw_Tree(object sender, PaintEventArgs e)
        {
            // Don't draw nandified formula (overflow)
            if (this.form.nodeManager.nodes[0].Value == '%')
                return;

            g = e.Graphics;
            g.Clear(Color.White);
            g.ScaleTransform(this.zoom, this.zoom);

            // Set top center values
            int radius = 40;
            int x_coord_init = (this.form.ui_panel_tree.Width / 2) - radius; // -radius because the point is at top left
            int y_coord_init = 20;

            DrawTreeChild((Operator)this.form.nodeManager.nodes[0], x_coord_init, y_coord_init, radius, g);

            //Resize picturebox so that the full tree can be viewed
            if (this.drawn == false)
            {

                float left_most = x_coord_init;
                float right_most = x_coord_init;
                float bottom_most = y_coord_init;
                foreach (INode n in this.form.nodeManager.nodes)
                {
                    if (n.X_coord < left_most) // left
                        left_most = n.X_coord;
                    if (n.X_coord > right_most) // right
                        right_most = n.X_coord;
                    if (n.Y_coord > bottom_most) // bottom
                        bottom_most = n.Y_coord;
                }

                this.size = new Size(Math.Abs(Convert.ToInt32(right_most - left_most + radius*3)),
                    Math.Abs(Convert.ToInt32(bottom_most + (radius * 2) - y_coord_init)));

                this.form.ui_panel_tree.SetBounds(
                    0,
                    this.form.initialPanelLocation.Y,
                    this.size.Width,
                    this.size.Height);
                this.form.ui_pb_tree.Top = 0;

                // TODO: Update the nodes position to match the final positions on the picture box
                //this.form.nodeManager.UpdateNodesPosition(0, 0);

                this.drawn = true;
            }
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
                x_offset = this.left_offset;
            else
                x_offset = x_offset - (x_offset / 2);

            double y_offset = radius * 2;

            // Draw children
            if (o.Left_child != null)
            {
                int x_child = (int)(x - x_offset * radius);
                int y_child = (int)(y + y_offset);

                //this.form.nodeManager.UpdateNodePosition(o.Left_child, x_child, 0);

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
