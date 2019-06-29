using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecxel
{
    class Basis
    {
        public int train_index;
        public int test_index;
        public int[] relevance_k = new int[2]; // 关联时长k范围
        public int[] valid_n = new int[2]; // 有效值n范围
        public List<int> lablelist = new List<int>(); //标签列表
        public int[] statistics_m = new int[2];
        public int stat_ml; // 统计时长滚动步长
        public int[] stat_start = new int[2]; // 统计时长的起点范围
        public float deletBaifen;


        public int best_k;  // 最好的关联时长
        public int best_n;  // 最好的有效值个数
        public int best_m; // 最好的统计时长
        public int best_mst;// 最好的统计时长起点

        public float TestRight;
        public float VerifyRight;

       // public ObservableCollection<Datalist> listData = new ObservableCollection<Datalist>();
        public List<Datalist> listData = new List<Datalist>();

        // 计算某种条件下的概率值，返回最大的概率值,以及对应的标签[p,lable]
        public List<int> PCaculate(int current_index, int inval, int contiu, List<int> label, int statistics_m, int statistics_mst)
        {
            //存储各个标签对应的数量
            List<int> nlist = new List<int>();
            // 存储预测标签以及对应的概率值
            //float[,] label_p = new float[label.Count, 2];
            // 存储条件值
            List<int> tglist = new List<int>();

            // 在条件一样时，各种标签对应的个数,也等于需要计算的概率p个数
            for (int j = 0; j < label.Count; j++)
            {
                nlist.Add(0);
                //label_p[j, 0] = label[j];

            }
            // 统计的起点和终点
            int startindex = current_index - statistics_m - statistics_mst;
            if (startindex < 0)
            {
                startindex = 0;
            }
            int lastindex = current_index - 1 - inval - contiu - statistics_mst;



            //条件值
            for (int i = 0; i < contiu; i++)
            {
                int index = current_index - contiu - inval + i - statistics_mst;
                tglist.Add(listData[index].Label);
            }

            // 统计在目标条件下，各个标签的个数
            for (int i = startindex; i <= lastindex; i++)
            {
                int number = tglist.Count;
                for (int j = 0; j < tglist.Count; j++)
                {
                    if (listData[i + j].Label == tglist[j])
                    {
                        number--;
                    }
                    else
                    {
                        break;
                    }
                }
                if (number == 0)
                {
                    for (int labi = 0; labi < label.Count; labi++)
                    {
                        if (listData[i + inval + contiu].Label == label[labi])
                        {
                            nlist[labi]++;
                            break;
                        }

                    }
                }

            }

            return nlist;

        }
        // 计算固定的k,n下,某一行最大概率的标签,以及对应的可信度[lable,q]
        public float[] RowCaculate(int current_index, int relev_k, int valid_nn, List<int> label, int statistics_m, int statistics_mst, float deletBaifen)
        {


            List<float[]> Prow = new List<float[]>();
            List<List<int>> numberList = new List<List<int>>();
            Prow.Add(new float[2] { 0, 2 });
            for (int j = 0; j < relev_k; j++)
            {
                for (int i = 1; i <= relev_k - j; i++)
                {
                    numberList.Add(PCaculate(current_index, j, i, label, statistics_m, statistics_mst));

                }
            }

            numberList.Sort((a, b) => a.Sum().CompareTo(b.Sum()));

            // 判断有效值是否超过现在的个数

            int deletnumber = (int)(numberList.Count * deletBaifen);
            for (int i = deletnumber; i < numberList.Count; i++)
            {
                for (int j = 0; j < numberList[i].Count; j++)
                {
                    Prow.Add(new float[] { label[j], numberList[i][j] / (float)numberList[i].Sum() });
                }

            }

            Prow.Sort((a, b) => a[1].CompareTo(b[1]));
            Prow.Reverse();
            if (valid_nn >= Prow.Count-1)
            {
                valid_nn = Prow.Count - 2;
            }


            // 存放各个标签的个数,以及对应的最大概率值
            float[,] pn = new float[label.Count, 3];
            // 个数初始化为0
            for (int i = 0; i < label.Count; i++)
            {
                pn[i, 0] = 0; // 个数
                pn[i, 1] = 0; // 概率
                pn[i, 2] = (float)label[i];      // 标签
            }

            int true_n = valid_nn;
            for (int i = valid_nn; i < Prow.Count - 1; i++)
            {
                if (Prow[i][1] > Prow[i + 1][1])
                {
                    break;
                }
                true_n++;
            }


            // 统计
            for (int i = 1; i <= true_n; i++)
            {
                for (int j = 0; j < label.Count; j++)
                {
                    if (Prow[i][0] == (float)label[j])
                    {
                        pn[j, 0]++;
                        break;
                    }
                }
            }

            // 获取各个标签的最大概率值
            for (int i = 0; i < label.Count; i++)
            {
                for (int j = 1; j <= true_n; j++)
                {
                    if (Prow[j][0] == (float)label[i])
                    {
                        pn[i, 1] = Prow[j][1];
                        break;
                    }
                }

            }

            //根据可信度进行排序
            float[] zancun = new float[3];
            for (int i = 0; i < label.Count; i++)
            {
                for (int j = i + 1; j < label.Count; j++)
                {
                    if (pn[i, 0] < pn[j, 0])
                    {
                        zancun[0] = pn[i, 0];
                        zancun[1] = pn[i, 1];
                        zancun[2] = pn[i, 2];
                        pn[i, 0] = pn[j, 0];
                        pn[i, 1] = pn[j, 1];
                        pn[i, 2] = pn[j, 2];
                        pn[j, 0] = zancun[0];
                        pn[j, 1] = zancun[1];
                        pn[j, 2] = zancun[2];
                    }
                }
            }

            //判断最高的可信度是否和第二名相等
            if (pn[0, 0] == pn[1, 0])
            {
                if (pn[0, 1] < pn[1, 1])
                {
                    pn[0, 0] = pn[1, 0];
                    pn[0, 1] = pn[1, 1];
                    pn[0, 2] = pn[1, 2];
                }
            }
            float[] qmax = new float[2];
            qmax[0] = pn[0, 2]; // 可信度最大对应的标签,即可能的预测值
            qmax[1] = pn[0, 0] / true_n; // 可信度;

            return qmax;


        }
        //  // 计算固定的k,n下，多行各自的预测标签可信度
        public float[,] RowsCaculate(int start_index, int end_index, int relev_k, int valid_nn, List<int> label, int statistics_m, int statistics_mst, float deletbaifen)
        {

            // 记录每行的预测标签和其可信度
            float[,] prows = new float[end_index - start_index + 1, 2];
            //List<float[]> prowsss = new List<float[]>();
            //float[,] rightRate = new float[1, 1]; //存放命中率
            // 多线程循环
            Parallel.For(start_index, end_index + 1, x =>
            {

                float[] prow = RowCaculate(x, relev_k, valid_nn, label, statistics_m, statistics_mst, deletbaifen);
                prows[x - start_index, 0] = prow[0];
                prows[x - start_index, 1] = prow[1];
            });


            return prows;
        }

    }
}
