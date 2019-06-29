using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecxel
{
    class FactorData
    {
        private int id;
        private DateTime time;
        private float temperature;
        private int label;


        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        public float Temperature
        {
            get { return temperature; }
            set { temperature = value; }
        }

        public int Label
        {
            get { return label; }
            set { label = value; }
        }
    }
}
