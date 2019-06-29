using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecxel
{
    class TargetTrend:Basis
    {
        public List<int> labellist = new List<int> { -1, 0, 1 };
        public float jieding;


        public TargetTrend(int train_index, int test_index, int[] relevance_k, int[] valid_n, int[] statistics_m, int stat_ml, int[] stat_start, float deletBaifen,float jieding)
        {
            this.train_index = train_index;
            this.test_index = test_index;
            this.relevance_k = relevance_k;
            this.valid_n = valid_n;
            this.statistics_m = statistics_m;
            this.stat_ml = stat_ml;
            this.stat_start = stat_start;
            this.deletBaifen = deletBaifen;
            this.jieding = jieding;
        }

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
        // 固定条件下，多行计算的预测结果
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
                prow = RowCaculate(i, relev_k, valid_nn, label, stata_m, sta_st, deletBaifen);
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

        // 迭代获取最优的关联时长k和有效值个数n,统计时长m,统计起点mst,返回最好的预测值和对应的可信度和该组数据命中率
        public List<float[,]> GetbestKN(int start_index, int end_index, int relev_kst, int relev_ked, int valid_nst, int valid_ned, List<int> label, int stast_m1, int stast_m2, int stast_ml, int stast_mst1, int stast_mst2)
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

            for (int i = relev_kst; i <= relev_ked; i++)
            {
                maxn = (i + 1) * i / 2;
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
                            if (i == relev_kst && j == start_n && m == 0 && mt == stast_mst1)
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
                                }
                            }

                        }
                    }
                }
            }

            return bestLabelQ;

        }
        // 趋势计算
        public void TrendCalculate()
        {
            Lable();
            List<float[,]> bestTestlist = GetbestKN(train_index + 1, test_index, relevance_k[0], relevance_k[1], valid_n[0], valid_n[1], labellist, statistics_m[0], statistics_m[1], stat_ml, stat_start[0], stat_start[1]);
            List<float[,]> verifylist = RowsCaculate_t(test_index + 1, listData.Count - 1, best_k, best_n, labellist, best_m, best_mst);

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
