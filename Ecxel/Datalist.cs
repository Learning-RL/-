using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Ecxel
{
    public class Datalist : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int id; 
        private DateTime poptime; // 出钢时间
        //private string popfinish;
        private float rhmsi;  // 硅含量
        private float secPinghua; // 二次平滑值
        private float secPinghuaErr; //
        //private int number;
        private int label;
        private int labelYuce = 0;
        private float trust_v = 0;
        private float trust_t = 0;
        private float yuceZhenZhi = 0;
        private string consistency = "null";
        private int indexFW; // 该时间硅含量对应风温的下标
        

        public Datalist(Datalist dl)
        {
            id = dl.Id;
            poptime = dl.POPtime;
            rhmsi = dl.Rhmsi;
            //secPinghua = dl.SecPinghua;
            //secPinghuaErr = dl.SecPinghuaErr;
            
        }

        public Datalist() { }
        

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Id"));
                }
            }
        }

        public DateTime POPtime
        {
            get { return poptime; }
            set
            {
                poptime = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("POPtime"));
                }
            }
        }


       

        public float Rhmsi
        {
            get { return rhmsi; }
            set
            {
                rhmsi = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Rhmsi"));
                }
            }
        }

        public float SecPinghua
        {
            get { return secPinghua; }
            set
            {
                secPinghua = value;

            }
        }

        public float SecPinghuaErr
        {
            get { return secPinghuaErr; }
            set
            {
                secPinghuaErr = value;
            }
        }

        public int Label
        {
            get { return label; }
            set
            {
                label = value;

            }
        }

        public int LabelYuCe
        {
            get { return labelYuce; }
            set
            {
                labelYuce = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("LabelYuCe"));
                }
            }
        }

        public float Trust_v
        {
            get { return trust_v; }
            set
            {
                trust_v = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Trust_v"));
                }
            }
        }

        public float Trust_t
        {
            get { return trust_t; }
            set
            {
                trust_t = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Trust_t"));
                }
            }
        }

        public float YuceZhenZhi
        {
            get { return yuceZhenZhi; }
            set
            {
                yuceZhenZhi = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("YuceZhenZhi"));
                }
            }
        }

        public string Consistency
        {
            get { return consistency; }
            set
            {
                consistency = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Consistency"));
                }
            }

        }

        public int IndexFW
        {
            get { return indexFW; }
            set { indexFW = value; }
        }



          //// 正向滚动计算
          //  for (int i = 1; i <= rollNum; i++)
          //  {

          //      for (int j = 0; j < fenqulinjiezhi.GetLength(0); j++)
          //      {
          //          fenqulinjiezhi[j, 0] += rollValue;
          //          fenqulinjiezhi[j, 1] += rollValue;
          //      }

          //      bigLableValue.Add((float[,])fenqulinjiezhi.Clone()); // 保存分区区间
          //      Dividefen(qujian_z, fenqulinjiezhi);
          //      RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
          //  }
          //  // 反向滚动计算
          //  for (int i = 1; i <= rollNum; i++)
          //  {

          //      for (int j = 0; j < fenqulinjiezhistart.GetLength(0); j++)
          //      {
          //          fenqulinjiezhistart[j, 0] -= rollValue;
          //          fenqulinjiezhistart[j, 1] -= rollValue;
          //      }
          //      bigLableValue.Add((float[,])fenqulinjiezhistart.Clone()); // 保存分区区间
          //      Dividefen(qujian_z, fenqulinjiezhistart);
          //      RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
          //  }
  

    }
}
