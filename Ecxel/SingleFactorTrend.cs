using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecxel
{
    class SingleFactorTrend:Basis
    {
        //public ObservableCollection<FactorData> FWdataList;
        public List<FactorData> FWdataList;
        public int[] fwQunjian = new int[2];
        public float[] FWTmax_min = new float[2]; // 风温最大值和最小值
        public int best_qujian_fw;
        public List<int> labellist = new List<int> { -1, 0, 1 };
        public float jieding;
        public int roll_t;
        public float roll_l;
        public float[] cg; // 趋势分区值
        public bool DengFenNum;
        public float best_cg;
        public bool TrendFenqu; // 趋势分区

        // 值分区构造函数
        public SingleFactorTrend(int train_index, int test_index, int[] relevance_k, int[] valid_n, int[] statistics_m, int stat_ml, int[] stat_start, float deletBaifen, int[] fwQunjian, float jieding,int roll_t,float roll_l,bool DengFenNum)
        {
            this.train_index = train_index;
            this.test_index = test_index;
            this.relevance_k = relevance_k;
            this.valid_n = valid_n;
            this.statistics_m = statistics_m;
            this.stat_ml = stat_ml;
            this.stat_start = stat_start;
            this.deletBaifen = deletBaifen;
            this.fwQunjian = fwQunjian;
            this.jieding = jieding;
            this.roll_t = roll_t;
            this.roll_l = roll_l;
            this.TrendFenqu = false;
            this.DengFenNum = DengFenNum;
        }
        // 趋势分区
        public SingleFactorTrend(int train_index, int test_index, int[] relevance_k, int[] valid_n, int[] statistics_m, int stat_ml, int[] stat_start, float deletBaifen, float[] cg, float jieding)
        {
            this.train_index = train_index;
            this.test_index = test_index;
            this.relevance_k = relevance_k;
            this.valid_n = valid_n;
            this.statistics_m = statistics_m;
            this.stat_ml = stat_ml;
            this.stat_start = stat_start;
            this.deletBaifen = deletBaifen;
            this.cg = cg;
            this.jieding = jieding;
            this.TrendFenqu = true;

        }
        


        // 

        // 打标签
        public void Lable()
        {
            for (int i = 0; i < listData.Count - 1; i++)
            {
                if (listData[i + 1].Rhmsi - listData[i].Rhmsi < -jieding)
                {
                    listData[i + 1].Label = labellist[0];
                }
                else if (listData[i + 1].Rhmsi - listData[i].Rhmsi > jieding)
                {
                    listData[i + 1].Label = labellist[2];
                }
                else
                {
                    listData[i + 1].Label = labellist[1];
                }
            }

        }
        // 求因素的最大最小值
        public void QiuFWMax_Min()
        {
            FWTmax_min[0] = FWdataList[0].Temperature; // 最大值
            FWTmax_min[1] = FWdataList[0].Temperature; // 最小值

            for (int i = 1; i < FWdataList.Count; i++)
            {
                if (FWdataList[i].Temperature > FWTmax_min[0])
                {
                    FWTmax_min[0] = FWdataList[i].Temperature;
                }
                else if (FWdataList[i].Temperature < FWTmax_min[1])
                {
                    FWTmax_min[1] = FWdataList[i].Temperature;
                }
            }
        }
        // 计算某种条件下的概率值，返回最大的概率值,以及对应的标签[p,lable]
        public List<int> PCaculate_fw(int current_index, int inval, int contiu, List<int> label, int statistics_m, int statistics_mst)
        {
            //存储各个标签对应的数量
            List<int> nlist = new List<int>();
            // 存储预测标签以及对应的概率值
            float[,] label_p = new float[label.Count, 2];
            // 存储条件值
            List<int> tglist = new List<int>();

            // 在条件一样时，各种标签对应的个数,也等于需要计算的概率p个数
            for (int j = 0; j < label.Count; j++)
            {
                nlist.Add(0);
                label_p[j, 0] = label[j];

            }
            // 统计的起点和终点
            int startindex = current_index - statistics_m;
            if (startindex < 0)
            {
                startindex = 0;
            }
            int lastindex = current_index - 1;



            //条件值
            for (int i = 0; i < contiu; i++)
            {
                int index = listData[current_index].IndexFW - inval - statistics_mst - i;
                tglist.Add(FWdataList[index].Label);
            }

            // 统计在目标条件下，各个标签的个数
            for (int i = startindex; i <= lastindex; i++)
            {
                int number = tglist.Count;
                int fwindex = listData[i].IndexFW - inval - statistics_mst;
                for (int j = 0; j < tglist.Count; j++)
                {
                    if (FWdataList[fwindex - j].Label == tglist[j])
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
                        if (listData[i].Label == label[labi])
                        {
                            nlist[labi]++;
                            break;
                        }

                    }
                }

            }

            // 计算概率
            //for (int i = 0; i < label_p.GetLength(0); i++)
            //{
            //    label_p[i, 1] = (float)nlist[i] / (nlist.Sum());
            //}
            //float[] pmax = new float[2];
            //pmax[0] = p.Max();
            //for (int maxi = 0; maxi < p.Count; maxi++)
            //{
            //    if (pmax[0] == p[maxi])
            //    {
            //        pmax[1] = (float)label[maxi];
            //        break;
            //    }
            //}
            return nlist;

        }
        // 因素打标签
        public void label_fw_value(float[,] fenquzhi)
        {
           

            // 打标签
            for (int i = 0; i < FWdataList.Count; i++)
            {
                for (int j = 0; j < fenquzhi.GetLength(0); j++)
                {
                    if (FWdataList[i].Temperature >= fenquzhi[j, 0] && FWdataList[i].Temperature < fenquzhi[j, 1])
                    {
                        FWdataList[i].Label = j + 1;
                        break;
                    }
                }
            }
        }

        // 根据区间数求各个区间的值范围
        public float[,] Qiufenquzhi(int qujianNum)
        {

            float[,] Tqujian = new float[qujianNum, 2]; // 区间分区值范围
            // 数量等分
            if (DengFenNum)
            {
                float[] err = new float[FWdataList.Count];
                float temp_err = 0;
                for (int i = 0; i < FWdataList.Count; i++)
                {
                    err[i] = FWdataList[i].Temperature;
                }

                // 先进行排序
                for (int i = 0; i < err.Length; i++)
                {
                    for (int j = i + 1; j < err.Length; j++)
                    {
                        if (err[i] > err[j])
                        {
                            temp_err = err[i];
                            err[i] = err[j];
                            err[j] = temp_err;
                        }
                    }
                }

                //获取分区的分界值的下标
                int num = FWdataList.Count / qujianNum;

                // 获取区间的分界值
                Tqujian[0, 0] = -100000;
                Tqujian[0, 1] = err[num];
            
                for (int i = 1; i < qujianNum - 1; i++)
                {
                    Tqujian[i, 0] = err[num * i];
                    Tqujian[i, 1] = err[num * (i + 1)];

                    lablelist.Add(i + 1);
                }

                
                Tqujian[qujianNum - 1, 0] = err[num * (qujianNum - 1)];
                Tqujian[qujianNum - 1, 1] = 100000;


            }
            else  // 值等分
            {
                float T = (FWTmax_min[0] - FWTmax_min[1]) / qujianNum;

                Tqujian[0, 0] = -100000;
                Tqujian[0, 1] = FWTmax_min[1] + T;

                for (int i = 1; i < qujianNum - 1; i++)
                {
                    Tqujian[i, 0] = FWTmax_min[1] + i * T;
                    Tqujian[i, 1] = FWTmax_min[1] + T * (i + 1);
                }
                Tqujian[qujianNum - 1, 0] = FWTmax_min[1] + (qujianNum - 1) * T;
                Tqujian[qujianNum - 1, 1] = 1000000;
            }
          
            
            

            
            
            
            return Tqujian;
        }

        // 根据趋势变化分区打标签
        public void label_fw_trend(float cgv)
        {
            FWdataList[0].Label = 0;
            float temputre = 0;
            for (int i = 0; i < FWdataList.Count - 1; i++)
            {
                temputre = FWdataList[i + 1].Temperature - FWdataList[i].Temperature;
                if (temputre > cgv)
                {
                    FWdataList[i + 1].Label = 1;
                }
                else if (temputre < -cgv)
                {
                    FWdataList[i + 1].Label = -1;
                }
                else
                {
                    FWdataList[i + 1].Label = 0;
                }
            }
        }

        // 寻找目标时间所对应的因素时间
        public void FindIndexFW()
        {
            int index = 0;
            for (int i = 0; i < FWdataList.Count; i++)
            {
                if (listData[index].POPtime <= FWdataList[i].Time)
                {
                    listData[index].IndexFW = i - 1;
                    index++;
                    if (index == listData.Count)
                    {

                        break;
                    }
                }
            }
        }

        // 计算固定的k,n下,某一行最大概率的标签,以及对应的可信度[lable,q]
        public float[] RowCaculate_fw(int current_index, int relev_k, int valid_nn, List<int> label, int statistics_m, int statistics_mst)
        {

            List<float[]> Prow = new List<float[]>();
            List<List<int>> numberList = new List<List<int>>();
            Prow.Add(new float[2] { 0, 2 });
            for (int j = 0; j < relev_k; j++)
            {
                for (int i = 1; i <= relev_k - j; i++)
                {
                    numberList.Add(PCaculate_fw(current_index, j, i, label, statistics_m, statistics_mst));

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

        // 计算固定的k,n下，多行各自的预测标签,和命中率
        public List<float[,]> RowsCaculate_t(int start_index, int end_index, int relev_k, int valid_nn, List<int> label, int stata_m, int sta_st)
        {
            // 存储一段数据的预测值,可信度，和命中率 [[lable,q],[rightrate]]
            // 其中其中 [lable,q] 是个多行2列数组[n,2],[rightrate]是[1,1] 这个两个组成一个list
            List<float[,]> pRowsList = new List<float[,]>();
            // 记录每行的预测标签和其可信度
            float[,] prows = new float[end_index - start_index + 1, 2];
            float[] prow = new float[2];
            int right = 0;
            int index = 0;
            float[,] rightRate = new float[1, 1]; //存放命中率
            for (int i = start_index; i <= end_index; i++)
            {
                prow = RowCaculate_fw(i, relev_k, valid_nn, label, stata_m, sta_st);
                //判断是否命中
                if (prow[0] == (float)listData[i].Label)
                {
                    right++;
                }
                prows[index, 0] = prow[0]; // 标签
                prows[index, 1] = prow[1];// 可信度
                index++;
            }
            rightRate[0, 0] = (float)right / index;
            pRowsList.Add(prows);
            pRowsList.Add(rightRate);
            return pRowsList;
        }

        // 计算固定的k,n下，多行各自的预测标签,和命中率(值滚动分区)
        public List<float[,]> RowsCaculate_t(int start_index, int end_index, int relev_k, int valid_nn, List<int> label, int stata_m, int sta_st,float[,] qujianzhi)
        {
            // 存储一段数据的预测值,可信度，和命中率 [[lable,q],[rightrate]]
            // 其中其中 [lable,q] 是个多行2列数组[n,2],[rightrate]是[1,1] 这个两个组成一个list
            List<float[,]> pRowsList = new List<float[,]>();
            // 记录每行的预测标签和其可信度
            //float[,] prows = new float[end_index - start_index + 1, 2];
            float[,] Labelpre = new float[end_index - start_index+1, 2];
            
            //float[,] rightRate = new float[1, 1]; //存放命中率
            float[,] qujianzhi_Copy = (float[,])qujianzhi.Clone();
            label_fw_value(qujianzhi);
            pRowsList.Add(AllRowsLabelOnce(start_index, end_index, relev_k, valid_nn, label, stata_m, sta_st)); // 滚动前计算一次

            // 正向滚动计算
            for (int i = 1; i <= roll_t; i++)
            {

                for (int j = 0; j < qujianzhi.GetLength(0); j++)
                {
                    qujianzhi[j, 0] += roll_l;
                    qujianzhi[j, 1] += roll_l;
                }
                label_fw_value(qujianzhi);
                pRowsList.Add(AllRowsLabelOnce(start_index, end_index, relev_k, valid_nn, label, stata_m, sta_st)); 
                
            }
            // 反向滚动计算
            for (int i = 1; i <= roll_t; i++)
            {

                for (int j = 0; j < qujianzhi_Copy.GetLength(0); j++)
                {
                    qujianzhi_Copy[j, 0] -= roll_l;
                    qujianzhi_Copy[j, 1] -= roll_l;
                }
                label_fw_value(qujianzhi_Copy);
                pRowsList.Add(AllRowsLabelOnce(start_index, end_index, relev_k, valid_nn, label, stata_m, sta_st)); 
               
            }

            // 求交集
            for (int i = 0; i <= end_index - start_index; i++)
            {
                List<float[]> jiaoji_list = new List<float[]> { new float[] { -1,0, 0 },new float[] {0,0, 0 },new float[] { 1,0, 0 } };
                foreach (float[,] rowLabel in pRowsList)
                {
                    for (int j = 0; j < label.Count; j++)
                    {
                        if (rowLabel[i, 0] == label[j])
                        {
                            jiaoji_list[j][1] += 1;  // 个数
                            jiaoji_list[j][2] += rowLabel[i, 1]; // 可信度
                        }
                    }
                }

                // 求交集最多的那个值
                jiaoji_list.Sort((a,b) => a[1].CompareTo(b[1]));
                Labelpre[i, 0] = jiaoji_list[2][0];
                Labelpre[i, 1] = jiaoji_list[2][2] / jiaoji_list[2][1];
                

            }

            // 计算命中率
            int right = 0;
            for (int i = start_index; i <= end_index; i++)
            {
                if (listData[i].Label== Labelpre[i - start_index, 0])
                {
                    right++;
                }
            }

            List<float[,]> labelYuce = new List<float[,]>();
            labelYuce.Add(Labelpre);
            float[,] right_arry = new float[1, 1];
            right_arry[0,0] = ((float)right) / Labelpre.GetLength(0);
            labelYuce.Add(right_arry);

            return labelYuce;
        }


        // [第N行标签，可信度] 
        public float[,] AllRowsLabelOnce(int start_index, int end_index, int relev_k, int valid_nn, List<int> label, int stata_m, int sta_st)
        {
            float[,] prows = new float[end_index - start_index + 1, 2];
            
            Parallel.For(start_index, end_index + 1, x =>
            {
                float[] prow = RowCaculate_fw(x, relev_k, valid_nn, label, stata_m, sta_st);
                prows[x-start_index, 0] = prow[0]; // 标签
                prows[x-start_index, 1] = prow[1];// 可信度
               
            });

            return prows;
        }

        // 迭代获取最优的关联时长k和有效值个数n,统计时长m,统计起点mst,返回最好的预测值和对应的可信度和该组数据命中率
        public List<float[,]> GetbestKN_fw(int start_index, int end_index, int relev_kst, int relev_ked, int valid_nst, int valid_ned, List<int> label, int stast_m1, int stast_m2, int stast_ml, int stast_mst1, int stast_mst2, float cg1, float cg2)
        {
            // 存储各k n 下，所对应的一段数据的预测值,和可信度，和命中率 [[[lable,q],[rightrate],[k,n]],[[lable,q],[rightrate],[k,n]]..]
            // 其中 [lable,q] 是个多行2列数组,[rightrate]是[1,1],[k,n]是[1,2],三个构成一个list

            // 存储一个k和n下的预测值和可信度,[[lable,q],[rightrate],[k,n]]
            List<float[,]> LabelQ = new List<float[,]>();
            List<float[,]> bestLabelQ = new List<float[,]>();


            float[,] kn = new float[1, 2];
            int maxn = 0;
            int end_n;
            int start_n;
            best_k = relev_kst;
            best_n = valid_nst;
            best_m = stast_m1;
            best_mst = stast_mst1;
            best_cg = cg1;
            QiuFWMax_Min(); //   求出风温的最大和最小值
            for (float fwqu = cg1; fwqu <= cg2; fwqu++)
            {

                label_fw_trend(fwqu);
                for (int i = relev_kst; i <= relev_ked; i++)
                {
                    maxn = ((i + 1) * i / 2)*3;
                    end_n = valid_ned;
                    start_n = valid_nst;
                    if (valid_ned > maxn)
                    {
                        end_n = maxn;
                    }
                    if (valid_nst > maxn)
                    {
                        start_n = maxn;

                    }
                    for (int j = start_n; j <= end_n; j++)
                    {
                        int mn = (stast_m2 - stast_m1) / stast_ml;
                        for (int m = 0; m <= mn; m++)
                        {
                            int stm = stast_m1 + m * stast_ml;
                            for (int mt = stast_mst1; mt <= stast_mst2; mt++)
                            {
                                if (fwqu == cg1 && i == relev_kst && j == start_n && m == 0 && mt == stast_mst1)
                                {
                                    bestLabelQ = RowsCaculate_t(start_index, end_index, relev_kst, start_n, label, stast_m1, stast_mst1);
                                }
                                else
                                {
                                    LabelQ = RowsCaculate_t(start_index, end_index, i, j, label, stm, mt);
                                    if (bestLabelQ[1][0, 0] < LabelQ[1][0, 0])
                                    {
                                        bestLabelQ = LabelQ;
                                        best_k = i;
                                        best_n = j;
                                        best_m = stm;
                                        best_mst = mt;
                                        best_cg = fwqu;
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return bestLabelQ;

        }

        // 值分区
        // 迭代获取最优的关联时长k和有效值个数n,统计时长m,统计起点mst,返回最好的预测值和对应的可信度和该组数据命中率
        public List<float[,]> GetbestKN_fw(int start_index, int end_index, int relev_kst, int relev_ked, int valid_nst, int valid_ned, List<int> label, int stast_m1, int stast_m2, int stast_ml, int stast_mst1, int stast_mst2, int qujian1,int qujian2)
        {
            // 存储各k n 下，所对应的一段数据的预测值,和可信度，和命中率 [[[lable,q],[rightrate],[k,n]],[[lable,q],[rightrate],[k,n]]..]
            // 其中 [lable,q] 是个多行2列数组,[rightrate]是[1,1],[k,n]是[1,2],三个构成一个list

            // 存储一个k和n下的预测值和可信度,[[lable,q],[rightrate],[k,n]]
            List<float[,]> LabelQ = new List<float[,]>();
            List<float[,]> bestLabelQ = new List<float[,]>();


            float[,] kn = new float[1, 2];
            int maxn = 0;
            int end_n;
            int start_n;
            best_k = relev_kst;
            best_n = valid_nst;
            best_m = stast_m1;
            best_mst = stast_mst1;
            best_qujian_fw = qujian1;
            QiuFWMax_Min(); //   求出风温的最大和最小值
            for (int fwqu = qujian1; fwqu <= qujian2; fwqu++)
            {

                float[,] qujianzhi= Qiufenquzhi(fwqu);
                for (int i = relev_kst; i <= relev_ked; i++)
                {
                    maxn =3*((i + 1) * i / 2);
                    end_n = valid_ned;
                    start_n = valid_nst;
                    if (valid_ned > maxn)
                    {
                        end_n = maxn;
                    }
                    if (valid_nst > maxn)
                    {
                        start_n = maxn;

                    }
                    for (int j = start_n; j <= end_n; j++)
                    {
                        int mn = (stast_m2 - stast_m1) / stast_ml;
                        for (int m = 0; m <= mn; m++)
                        {
                            int stm = stast_m1 + m * stast_ml;
                            for (int mt = stast_mst1; mt <= stast_mst2; mt++)
                            {
                                if (fwqu == qujian1 && i == relev_kst && j == start_n && m == 0 && mt == stast_mst1)
                                {
                                    bestLabelQ = RowsCaculate_t(start_index, end_index, relev_kst, start_n, label, stast_m1, stast_mst1,(float[,])qujianzhi.Clone());
                                }
                                else
                                {
                                    LabelQ = RowsCaculate_t(start_index, end_index, i, j, label, stm, mt,(float[,])qujianzhi.Clone());
                                    if (bestLabelQ[1][0, 0] < LabelQ[1][0, 0])
                                    {
                                        bestLabelQ = LabelQ;
                                        best_k = i;
                                        best_n = j;
                                        best_m = stm;
                                        best_mst = mt;
                                        best_qujian_fw = fwqu;
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return bestLabelQ;

        }

        // 趋势分区求测试集预测值
        public List<float[,]> RowsCaculate_fw(int start_index, int end_index, int relev_k, int valid_nn, List<int> label, int stata_m, int sta_st, float best_cg)
        {
            // 存储一段数据的预测值,可信度，和命中率 [[lable,q],[rightrate]]
            // 其中其中 [lable,q] 是个多行2列数组[n,2],[rightrate]是[1,1] 这个两个组成一个list
            label_fw_trend(best_cg);
            List<float[,]> pRowsList = new List<float[,]>();
            // 记录每行的预测标签和其可信度
            float[,] prows = new float[end_index - start_index + 1, 2];
            float[] prow = new float[2];
            int right = 0;
            int index = 0;
            float[,] rightRate = new float[1, 1]; //存放命中率
            for (int i = start_index; i <= end_index; i++)
            {
                prow = RowCaculate_fw(i, relev_k, valid_nn, label, stata_m, sta_st);
                //判断是否命中
                if (prow[0] == (float)listData[i].Label)
                {
                    right++;
                }
                prows[index, 0] = prow[0]; // 标签
                prows[index, 1] = prow[1];// 可信度
                index++;
            }
            rightRate[0, 0] = (float)right / index;
            pRowsList.Add(prows);
            pRowsList.Add(rightRate);
            return pRowsList;
        }

        public void SingleFactorCalculate()
        {
            Lable();
            FindIndexFW();
            List<float[,]> bestTestlist;
            List<float[,]> verifylist;
            if (TrendFenqu)
            {
                bestTestlist = GetbestKN_fw(train_index + 1, test_index, relevance_k[0], relevance_k[1], valid_n[0], valid_n[1], labellist, statistics_m[0], statistics_m[1], stat_ml, stat_start[0], stat_start[1], cg[0], cg[1]);
                verifylist = RowsCaculate_fw(test_index + 1, listData.Count - 1, best_k, best_n, labellist, best_m, best_mst, best_qujian_fw);
            }
            else
            {
                bestTestlist = GetbestKN_fw(train_index + 1, test_index, relevance_k[0], relevance_k[1], valid_n[0], valid_n[1], labellist, statistics_m[0], statistics_m[1], stat_ml, stat_start[0], stat_start[1], fwQunjian[0], fwQunjian[1]);
                float[,] fenquzhi =Qiufenquzhi(best_qujian_fw);
                verifylist = RowsCaculate_t(test_index + 1, listData.Count - 1, best_k, best_n, labellist, best_m, best_mst, fenquzhi);
            }


            int n = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                listData[i].LabelYuCe = (int)bestTestlist[0][n, 0];
                listData[i].Trust_t = (float)Math.Round(bestTestlist[0][n, 1], 4);
                n++;
            }
            n = 0;
            for (int i = test_index + 1; i < listData.Count; i++)
            {
                listData[i].LabelYuCe = (int)verifylist[0][n, 0];
                listData[i].Trust_t = (float)Math.Round(verifylist[0][n, 1], 4);
                n++;
            }

            TestRight = bestTestlist[1][0, 0];
            VerifyRight = verifylist[1][0, 0];

        }
    }
}
