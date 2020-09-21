using System;
using System.Collections.Generic;
using System.Text;

namespace Kazan_Session5_Mobile_21_9
{
    public class GlobalClass
    {
        public class RockType
        {
            public long ID { get; set; }
            public string Name { get; set; }
            public string BackgroundColor { get; set; }
        }

        public class Well
        {
            public long ID { get; set; }
            public long WellTypeID { get; set; }
            public string WellName { get; set; }
            public long GasOilDepth { get; set; }
            public long Capacity { get; set; }
        }
        public class WellLayer
        {
            public long ID { get; set; }
            public long WellID { get; set; }
            public long RockTypeID { get; set; }
            public long StartPoint { get; set; }
            public long EndPoint { get; set; }
        }
        public class WellType
        {
            public long ID { get; set; }
            public string Name { get; set; }
        }


        public class GridView
        {
            public string RockName { get; set; }
            public string BackgroundColour { get; set; }
            public int Start { get; set; }
            public int End { get; set; }
        }

        public class LayerView
        {
            public string RockName { get; set; }
            public long StartPoint { get; set; }
            public long EndPoint { get; set; }
        }

    }
}
