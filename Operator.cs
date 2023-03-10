using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE1_Katerina
{
    public class Operator : INode
    {
        public int ID { get; set; }
        public char Value { get; set; }
        public INode Left_child { get; set; }
        public INode Right_child { get; set; }
        public int Parent_ID { get; set; }
        public float X_coord { get; set; }
        public float Y_coord { get; set; }
        public float Radius { get; set; }
        public bool Truth_value { get; set; }
        public bool Drawn { get; set; }

        public Operator(int id, char value, INode left_child = null, INode right_child = null, int parent_id = -1)
        {
            this.ID = id;
            this.Value = value;
            this.Left_child = left_child;
            this.Right_child = right_child;
            this.Parent_ID = parent_id;
            this.Drawn = false;
        }

        public void AddChild(INode child)
        {
            if (child != null)
            {
                if (this.Left_child == null)
                    this.Left_child = child;
                else if (this.Right_child == null)
                    this.Right_child = child;
            }
        }

        public void DrawNode(int x, int y, int r, System.Drawing.Color c, System.Drawing.Graphics g, bool debug = false)
        {
            if (debug) { Console.WriteLine($"Node {this.Value} drawn at: {x},{y}"); }

            this.X_coord = x;
            this.Y_coord = y;
            this.Radius = r;

            // Update parent's coords

            Pen myPen = new Pen(c, 4);

            // Circle
            Rectangle myRectangle = new Rectangle(x, y, r, r);
            g.DrawEllipse(myPen, myRectangle);

            // Text/Char
            Font myFont = new System.Drawing.Font("Helvetica", 14, FontStyle.Italic);
            Brush myBrush = new SolidBrush(System.Drawing.Color.Red);
            g.DrawString(this.Value.ToString(), myFont, myBrush, myRectangle.X + 10, myRectangle.Y + 10);

            // mark as drawn
            this.Drawn = true;
        }
    }
}
