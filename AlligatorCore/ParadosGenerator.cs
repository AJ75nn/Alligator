using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
namespace AlligatorCore
{
    public class ParadosGenerator
    {
        public List<Curve> pardoscircle = []; 
        public ParadosGenerator( double radius)
        {
            Circle circle = new Circle(Plane.WorldXY, radius);
            pardoscircle.Add(circle.ToNurbsCurve()); 
            var off = circle.ToNurbsCurve().Offset(Plane.WorldXY, 50, 0.001, CurveOffsetCornerStyle.None);
            pardoscircle.AddRange(off);
        }
    }
}
    