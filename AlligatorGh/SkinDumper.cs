using System;
using System.Reflection;
using Grasshopper.GUI.Canvas;

namespace AlligatorGh {
    public class SkinDumper {
        public static void Dump() {
            var fields = typeof(GH_Skin).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var f in fields) {
                Console.WriteLine("FIELD: " + f.Name + " : " + f.FieldType.Name);
            }
        }
    }
}
