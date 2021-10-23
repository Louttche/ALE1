using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE1_Katerina
{
    public class Operant : INode
    {
        public int ID { get; set; }
        public char Value { get; set; }
        public int Parent_ID { get; set; }
        public float X_coord { get; set; }
        public float Y_coord { get; set; }
        public float Radius { get; set; }
        public bool Truth_value { get; set; }
        public bool Drawn { get; set; }

        public Form1 Form;

        public Operant(int id, char value, Form1 form, int parent_id = -1)
        {
            this.ID = id;
            this.Value = value;
            this.Parent_ID = parent_id;
            this.Drawn = false;
            this.Form = form;
        }

        public void DrawNode(int x, int y, int r, Color c, Graphics g)
        {
            // Update node's location in form list
            //foreach (INode fn in this.Form.formula_nodes)
            //{
            //    if (fn.ID == this.ID)
            //    {
            //        fn.X_coord = x;
            //        fn.Y_coord = y;
            //        fn.Radius = r;
            //        break;
            //    }
            //}

            this.X_coord = x;
            this.Y_coord = y;
            this.Radius = r;

            Pen myPen = new Pen(c, 4);

            // Circle
            Rectangle myRectangle = new Rectangle(x, y, r, r);
            g.DrawEllipse(myPen, myRectangle);

            // Text/Char
            Font myFont = new System.Drawing.Font("Helvetica", 14, FontStyle.Italic);
            Brush myBrush = new SolidBrush(System.Drawing.Color.Red);
            g.DrawString(this.Value.ToString(), myFont, myBrush, myRectangle.X + 8, myRectangle.Y + 5);

            // mark as drawn
            this.Drawn = true;
        }

        public void AddParent(int parent_id)
        {
            this.Parent_ID = parent_id;
        }
    }
}
