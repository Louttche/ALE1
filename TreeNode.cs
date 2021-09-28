using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ALE1_Katerina
{
    public class TreeNode
    {
        public int ID { get; set; }
        public char Value { get; set; }
        public TreeNode Left_child { get; set; }
        public TreeNode Right_child { get; set; }

        //public TreeNode Parent { get; set; }

        //Drawing parameters
        //public float X_coord { get; set; }
        //public float Y_coord { get; set; }
        //public float Radius { get; set; }

        public TreeNode(int id, char value, TreeNode left_child, TreeNode right_child/*, TreeNode parent*/)
        {
            this.ID = id;
            this.Value = value;
            this.Left_child = left_child;
            this.Right_child = right_child;
            //this.Parent = parent;
        }

        public void DrawNode(int x, int y, int r, System.Drawing.Color c, System.Drawing.Graphics g)
        {
            Pen myPen = new Pen(c, 4);

            // Circle
            Rectangle myRectangle = new Rectangle(x, y, r, r);
            g.DrawEllipse(myPen, myRectangle);

            // Text/Char
            Font myFont = new System.Drawing.Font("Helvetica", 14, FontStyle.Italic);
            Brush myBrush = new SolidBrush(System.Drawing.Color.Red);
            g.DrawString(this.Value.ToString(), myFont, myBrush, myRectangle.X + 8, myRectangle.Y + 5);

            // Check for children and draw them

        }

        public void AddChild(TreeNode child)
        {
            if (this.Left_child == null)
                this.Left_child = child;
            else if (this.Right_child == null)
                this.Right_child = child;
        }
    }
}
