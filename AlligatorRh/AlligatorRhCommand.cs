using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

namespace AlligatorRh
{
    public class AlligatorRhCommand : Command
    {
        public AlligatorRhCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static AlligatorRhCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "paradosCircle";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            double radius = 10;

            GetNumber gn = new GetNumber();
            gn.SetCommandPrompt("Enter radius");

            gn.SetDefaultNumber(radius);

            gn.SetLowerLimit(0, false);

            var result = gn.Get();

            if (result != Rhino.Input.GetResult.Number)
                return Result.Cancel;

            radius = gn.Number();
            AlligatorCore.ParadosGenerator pardusGenerator = new(radius);
            foreach (var cir in pardusGenerator.pardoscircle)
            {
                RhinoDoc.ActiveDoc.Objects.Add(cir);

            }
            doc.Views.Redraw();

            return Result.Success;
        }
    }
}
