using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALE1_Katerina
{
    public interface INode
    {
        int ID { get; set; }
        char Value { get; set; }
        int Parent_ID { get; set; }
        float X_coord { get; set; }
        float Y_coord { get; set; }
        float Radius { get; set; }
        bool Truth_value { get; set; }
        bool Drawn { get; set; }

        string ToString(); // return it as infix notation

        void DrawNode(int x, int y, int r, System.Drawing.Color c, System.Drawing.Graphics g, bool debug); // sets x, y and r properties
    }
}
