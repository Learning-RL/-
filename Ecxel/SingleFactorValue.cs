using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecxel
{
    class SingleFactorValue:Basis
    {

        public List<FactorData> FWdataList;
        public int[] fwQunjian = new int[2];
        public float[] FWTmax_min = new float[2]; // 风温最大值和最小值
        public int best_qujian_fw;
        public List<int> labellist = new List<int> { -1, 0, 1 };
        public int roll_t; // 因素滚动
        public float roll_l; // 因素滚动步长
        public float[] cg; // 趋势分区值
        public bool DengFenNum;
        public float best_cg;
        public bool TrendFenqu; // 趋势分区


        public float[] pinghua_a = new float[2]; // 平滑指数a范围
        public float pinghua_l; // 平滑指数a滚动步长
        public float[] trend_b = new float[2]; // 趋势指数b范围
        public float trend_bl; // 趋势指数滚动步长
        public int[] qujian_z = new int[2]; // 区间数z范围
        public float[] err = new float[2]; // 误差区间
        public int roll_target; // 目标滚动
        public float roll_targetl; // 目标滚动步长
        public bool NumberDengFen = true;


        public float best_a; // 最好的平滑指数a
        public float best_b; // 最好的趋势指数b
        public int best_qushu; // 最好的分区数
        public float chancaMin = 0; // 残差最小值
        public float chancaMax = 0; // 残差最大值


        // 值分区构造函数
        public SingleFactorValue(int train_index, int test_index, int[] relevance_k, int[] valid_n, int[] statistics_m, int stat_ml, int[] stat_start, float deletBaifen, int[] fwQunjian, int roll_t, float roll_l, bool DengFenNum, float[] pinghua_a, float pinghua_l, float[] trend_b, float trend_bl, int[] qujian_z, float[] err, int roll_target, float roll_targetl, bool NumberDengFen)
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
            this.roll_t = roll_t;
            this.roll_l = roll_l;
            this.TrendFenqu = false;
            this.DengFenNum = DengFenNum;
            
            this.pinghua_a = pinghua_a;
            this.pinghua_l = pinghua_l;
            this.trend_b = trend_b;
            this.trend_bl = trend_bl;
            this.qujian_z = qujian_z;
            this.err = err;
            this.roll_target = roll_target;
            this.roll_targetl = roll_targetl;
            this.NumberDengFen = NumberDengFen;

        }
        // 趋势分区
        public SingleFactorValue(int train_index, int test_index, int[] relevance_k, int[] valid_n, int[] statistics_m, int stat_ml, int[] stat_start, float deletBaifen, float[] cg,float[] pinghua_a, float pinghua_l, float[] trend_b, float trend_bl, int[] qujian_z, float[] err, int roll_target, float roll_targetl, bool NumberDengFen)
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
            this.TrendFenqu = true;

            this.pinghua_a = pinghua_a;
            this.pinghua_l = pinghua_l;
            this.trend_b = trend_b;
            this.trend_bl = trend_bl;
            this.qujian_z = qujian_z;
            this.err = err;
            this.roll_target = roll_target;
            this.roll_targetl = roll_targetl;
            this.NumberDengFen = NumberDengFen;

        }

        // 在给定的平滑指数和趋势指数下计算二次平滑值和相应的残差
        public void PinghuaCalculate(float pinghua_a, float trend_b)
        {
            int Num = listData.Count;
            float[] trendarry = new float[Num]; // 存放趋势值
            trendarry[0] = 0;
            listData[0].SecPinghua = listData[0].Rhmsi;
            listData[0].SecPinghuaErr = 0;
            for (int i = 1; i < Num; i++)
            {
                //趋势值
                trendarry[i] = trend_b * (listData[i].Rhmsi - listData[i - 1].Rhmsi) + (1 - trend_b) * trendarry[i - 1];
                //二次平滑值
                listData[i].SecPinghua = pinghua_a * listData[i - 1].Rhmsi + (1 - pinghua_a) * (trendarry[i - 1] + listData[i - 1].SecPinghua);
                //二次平滑值残差
                listData[i].SecPinghuaErr = listData[i].Rhmsi - listData[i].SecPinghua;
                if (chancaMin > listData[i].SecPinghuaErr)
                {
                    chancaMin = listData[i].SecPinghuaErr;
                }
                if (chancaMax < listData[i].SecPinghuaErr)
                {
                    chancaMax = listData[i].SecPinghuaErr;
                }
            }
        }

        // 给定的区间数下，获取每个区间的分界值
        public float[,] Divide(int qujian_z)
        {
            lablelist.Clear(); // 清空标签表
            float[] err = new float[listData.Count];
            float[,] qujianRange = new float[qujian_z, 2]; // 各个区的值范围[a1,a2),[a2,a3)....
            float temp_err = 0;
            for (int i = 0; i < listData.Count; i++)
            {
                err[i] = listData[i].SecPinghuaErr;
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


            if (NumberDengFen == true)
            {
                //获取分区的分界值的下标
                int num = listData.Count / qujian_z;

                // 获取区间的分界值
                qujianRange[0, 0] = -100000;
                qujianRange[0, 1] = err[num];
                lablelist.Add(1);
                for (int i = 1; i < qujian_z - 1; i++)
                {
                    qujianRange[i, 0] = err[num * i];
                    qujianRange[i, 1] = err[num * (i + 1)];

                    lablelist.Add(i + 1);
                }

                lablelist.Add(qujian_z);
                qujianRange[qujian_z - 1, 0] = err[num * (qujian_z - 1)];
                qujianRange[qujian_z - 1, 1] = 100000;
            }
            else
            {
                float jiange = (err[err.Length - 1] - err[0]) / qujian_z;
                qujianRange[0, 0] = -100000;
                qujianRange[0, 1] = err[0] + jiange;
                lablelist.Add(1);
                for (int i = 1; i < qujian_z - 1; i++)
                {
                    qujianRange[i, 0] = err[0] + jiange * i;
                    qujianRange[i, 1] = err[0] + jiange * (i + 1);
                    lablelist.Add(i + 1);

                }
                lablelist.Add(qujian_z);

                qujianRange[qujian_z - 1, 0] = err[0] + jiange * (qujian_z - 1);
                qujianRange[qujian_z - 1, 1] = 100000;
            }

            return qujianRange;

        }

        // 给定的区间数,和每个区间的分界值，对数据进行分区打标签
        public void Dividefen(int qujian, float[,] linjiezhi)
        {

            for (int i = 0; i < listData.Count; i++)
            {

                for (int j = 0; j < linjiezhi.GetLength(0); j++)
                {
                    if (listData[i].SecPinghuaErr >= linjiezhi[j, 0] && listData[i].SecPinghuaErr < linjiezhi[j, 1])
                    {
                        listData[i].Label = lablelist[j]; // 打上标签

                        break;
                    }
                }

            }
        }

        // 两个区间取交集
        public float[] Qiujiaoji(float[] qiujiao_a, float[] qiujiao_b)
        {
            float a = 0;
            float b = 0;
            float[] jiaoji = new float[4];

            // 无交集情况
            if (qiujiao_a[0] >= qiujiao_b[1])
            {
                jiaoji[0] = a;
                jiaoji[1] = b;
                jiaoji[2] = 0;
                jiaoji[3] = 0;
            } // 无交集情况
            else if (qiujiao_a[1] <= qiujiao_b[0])
            {
                jiaoji[0] = a;
                jiaoji[1] = b;
                jiaoji[2] = 0;
                jiaoji[3] = 0;
            }
            else // 有交集
            {
                a = qiujiao_a[0];
                b = qiujiao_a[1];
                if (a < qiujiao_b[0])
                {
                    a = qiujiao_b[0];
                }
                if (b > qiujiao_b[1])
                {
                    b = qiujiao_b[1];
                }

                jiaoji[0] = a;
                jiaoji[1] = b;
                jiaoji[2] = qiujiao_a[2] + qiujiao_b[2];
                jiaoji[3] = qiujiao_a[3] + qiujiao_b[3];
            }

            return jiaoji;
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

                Tqujian[0, 0] = -10000;
                Tqujian[0, 1] = FWTmax_min[1] + T;

                for (int i = 1; i < qujianNum - 1; i++)
                {
                    Tqujian[i, 0] = FWTmax_min[1] + i * T;
                    Tqujian[i, 1] = FWTmax_min[1] + T * (i + 1);
                }
                Tqujian[qujianNum - 1, 0] = FWTmax_min[1] + (qujianNum - 1) * T;
                Tqujian[qujianNum - 1, 1] = 100000;
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

            // 判断有效值是否超过现在的个数
            if (valid_nn >= Prow.Count-1)
            {
                valid_nn = Prow.Count - 1;
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

        // [第N行标签，可信度] 
        public float[,] AllRowsLabelOnce(int start_index, int end_index, int relev_k, int valid_nn, List<int> label, int stata_m, int sta_st)
        {
            float[,] prows = new float[end_index - start_index + 1, 2];

            Parallel.For(start_index, end_index + 1, x =>
            {
                float[] prow = RowCaculate_fw(x, relev_k, valid_nn, label, stata_m, sta_st);
                prows[x - start_index, 0] = prow[0]; // 标签
                prows[x - start_index, 1] = prow[1];// 可信度

            });

            return prows;
        }

        // 固定区间数、关联时长k、有效值n、统计时长m、滚动次数rollNum，滚动步长rollValue
        public float[,] GetRowsPredict(int index_st, int index_ed, int qujian_z, float[,] fenqulinjiezhi, int k, int n, int m, int rollNum, float rollValue, int m_st)
        {

            // [0]=第一次滚动 [0][0]=第一次固定标签为1的具体包含的值 
            List<float[,]> bigLableValue = new List<float[,]>();// [[[0,203,1,32]],] 不同滚动窗口时，分区标签，对应的区间 
            //List<List<float>> labeview = new List<List<float>>(); // 一
            List<float[,]> RowsPlabel = new List<float[,]>();// 各种窗口得到的N行值
            float[,] BestValue = new float[index_ed - index_st + 1, 2]; // 存放每行预测的最好的值，和可信度

            # region  滚动窗口
            // 没滚动前先算一次
            bigLableValue.Add((float[,])fenqulinjiezhi.Clone()); // 保存第一次计算的分区区间值
            Dividefen(qujian_z, fenqulinjiezhi);
            //rowsPlabe = RowsCaculate(index_st, index_ed, k, n, qujian_z, m); // 计算得到该种分区条件下,预测的标签值,对应的可信度,
            RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
            float[,] fenqulinjiezhistart = (float[,])fenqulinjiezhi.Clone();

            // 正向滚动计算
            for (int i = 1; i <= rollNum; i++)
            {

                for (int j = 0; j < fenqulinjiezhi.GetLength(0); j++)
                {
                    fenqulinjiezhi[j, 0] += rollValue;
                    fenqulinjiezhi[j, 1] += rollValue;
                }

                bigLableValue.Add((float[,])fenqulinjiezhi.Clone()); // 保存分区区间
                Dividefen(qujian_z, fenqulinjiezhi);
                RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
            }
            // 反向滚动计算
            for (int i = 1; i <= rollNum; i++)
            {

                for (int j = 0; j < fenqulinjiezhistart.GetLength(0); j++)
                {
                    fenqulinjiezhistart[j, 0] -= rollValue;
                    fenqulinjiezhistart[j, 1] -= rollValue;
                }
                bigLableValue.Add((float[,])fenqulinjiezhistart.Clone()); // 保存分区区间
                Dividefen(qujian_z, fenqulinjiezhistart);
                RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
            }
            # endregion
            int ax = 1;

            Parallel.For(0, index_ed - index_st + 1, i =>
            {
                List<float[]> predict = new List<float[]>(); // 存放所有有交集的预测值,交集个数,最大可信度[[pre,n,q],[],[]]

                #region 计算第i行的预测值的交集
                // 各种窗口下的标签的具体值取交集 j控制滚动是第几次

                for (int j = 0; j < bigLableValue.Count - 1; j++)
                {
                    float[] Labelv = new float[4]; //[左区间,右区间,可信度]
                    float[] LabelvNext = new float[4];
                    Labelv[0] = bigLableValue[j][(int)RowsPlabel[j][i, 0] - 1, 0]; // 左区间
                    Labelv[1] = bigLableValue[j][(int)RowsPlabel[j][i, 0] - 1, 1]; // 右区间
                    Labelv[2] = RowsPlabel[j][i, 1];  // 可信度
                    Labelv[3] = 1; // 交集个数
                    for (int y = j + 1; y < bigLableValue.Count; y++)
                    {
                        LabelvNext[0] = bigLableValue[y][(int)RowsPlabel[y][i, 0] - 1, 0];
                        LabelvNext[1] = bigLableValue[y][(int)RowsPlabel[y][i, 0] - 1, 1];
                        LabelvNext[2] = RowsPlabel[y][i, 1];
                        LabelvNext[3] = 1;
                        LabelvNext = Qiujiaoji(Labelv, LabelvNext);
                        if (LabelvNext[3] != 0)
                        {
                            predict.Add((float[])LabelvNext.Clone());
                        }

                    }
                }
                #endregion


                # region 得到该行最佳的具体预测值和对应的可信度
                // 没有交集，取可信度最高的值
                if (predict.Count == 0)
                {
                    List<int> indexlist = new List<int>(); // 存放可信度最高的几个下标
                    float idexbest = RowsPlabel[0][i, 1]; // 获取可信度最高值
                    for (int r = 1; r < RowsPlabel.Count; r++)
                    {
                        if (idexbest < RowsPlabel[r][i, 1])
                        {
                            idexbest = RowsPlabel[r][i, 1];
                        }
                    }
                    for (int r = 0; r < RowsPlabel.Count; r++)  // 获取可信度最高值对应的所有下标
                    {
                        if (RowsPlabel[r][i, 1] == idexbest)
                        {
                            indexlist.Add(r);
                        }
                    }

                    List<float[,]> predict_max = new List<float[,]>();
                    float left = 0;
                    float right = 0;
                    float midel = 0;
                    for (int r = 0; r < indexlist.Count; r++)  // 获取对应的区间
                    {
                        left = bigLableValue[r][(int)RowsPlabel[r][i, 0] - 1, 0];
                        right = bigLableValue[r][(int)RowsPlabel[r][i, 0] - 1, 1];
                        if (left < chancaMin)
                        {
                            left = chancaMin;
                        }
                        else if (right > chancaMax)
                        {
                            right = chancaMax;
                        }
                        midel += (right + left) / 2;

                    }

                    BestValue[i, 0] = midel / indexlist.Count;
                    BestValue[i, 1] = idexbest;

                }
                else //有交集,取最多交集的值
                {
                    List<float[]> predict_only = new List<float[]>(); // 去除重复区间得到的交集区间集合
                    while (predict.Count != 0)
                    {
                        // 先对上次取交集得到的区间，去除相同区间。
                        predict_only.Clear();
                        for (int pre = 0; pre < predict.Count - 1; pre++)
                        {
                            int index = predict_only.FindIndex(pr => pr[0] == predict[pre][0] && pr[1] == predict[pre][1]);
                            if (index == -1)
                            {
                                for (int pred = pre + 1; pred < predict.Count; pred++)
                                {
                                    if (predict[pred][0] == predict[pre][0] && predict[pred][1] == predict[pre][1])
                                    {
                                        predict[pre][2] += predict[pred][2];
                                        predict[pre][3] += predict[pred][3];
                                    }


                                }
                                predict_only.Add(predict[pre]);
                            }
                        }
                        int indexlast = predict_only.FindIndex(p => p[0] == predict[predict.Count - 1][0] && p[1] == predict[predict.Count - 1][1]);
                        if (indexlast == -1)
                        {
                            predict_only.Add(predict[predict.Count - 1]);
                        }

                        predict.Clear();
                        // 去重后的区间取交集
                        float[] jiaojiqujian = new float[4];
                        for (int px = 0; px < predict_only.Count - 1; px++)
                        {
                            for (int py = px + 1; py < predict_only.Count; py++)
                            {
                                jiaojiqujian = Qiujiaoji(predict_only[px], predict_only[py]);
                                if (jiaojiqujian[2] != 0)
                                {
                                    predict.Add((float[])jiaojiqujian.Clone());
                                }
                            }
                        }

                    }

                    // 取最大的交集个数的区间
                    float pinlv_max = predict_only[0][3];
                    for (int ji = 1; ji < predict_only.Count; ji++)
                    {
                        if (pinlv_max < predict_only[ji][3])
                        {
                            pinlv_max = predict_only[ji][3];
                        }
                    }
                    List<float[]> qujian_max = predict_only.FindAll(p => p[3] == pinlv_max);
                    float[] pingjuzhi = new float[3];
                    pingjuzhi[0] = 0;
                    pingjuzhi[1] = 0;
                    pingjuzhi[2] = 0;
                    foreach (float[] predictqujian in qujian_max)
                    {

                        if (predictqujian[0] < chancaMin)
                        {
                            predictqujian[0] = chancaMin;
                        }
                        else if (predictqujian[1] > chancaMax)
                        {
                            predictqujian[1] = chancaMax;
                        }
                        pingjuzhi[0] += (predictqujian[0] + predictqujian[1]) / 2;
                        pingjuzhi[1] += predictqujian[2];
                        pingjuzhi[2] += predictqujian[3];
                    }

                    BestValue[i, 0] = pingjuzhi[0] / qujian_max.Count();
                    BestValue[i, 1] = pingjuzhi[1] / pingjuzhi[2];

                }
                # endregion
            });
            return BestValue;
        }

        // 迭代获取最优的关联时长k和有效值个数n,统计时长m,统计起点mst,返回最好的预测值和对应的可信度和该组数据命中率
        public float[,] GetbestKN_fw(int start_index, int end_index, int relev_kst, int relev_ked, int valid_nst, int valid_ned, List<int> label, int stast_m1, int stast_m2, int stast_ml, int stast_mst1, int stast_mst2, float cg1, float cg2, float a_st, float a_ed, float a_l, float b_st, float b_ed, float b_l, int quhu_st, int qushu_ed, int gundong, float gundong_l)
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
            int a_num = (int)((a_ed - a_st) / a_l);
            int b_num = (int)((b_ed - b_st) / b_l);
            float[,] bestpre = new float[end_index - start_index + 1, 2];


            best_k = relev_kst;// 最好的关联时长k
            best_n = valid_nst;// 最好的有效值个数n
            best_m = stast_m1;// 最好的统计时长m
            best_mst = stast_mst1;// 最好的统计时长起点
            best_a = a_st; // 最好的平滑指数a
            best_b = b_st; // 最好的趋势指数b
            best_qushu = quhu_st; // 最好的分区数
            best_cg = cg1;

            QiuFWMax_Min(); //   求出风温的最大和最小值
            for (int i = 0; i <= a_num; i++) // 平滑指数a
            {
                float a = i * a_l + a_st;
                for (int j = 0; j <= b_num; j++) // 趋势指数b
                {
                    float b = j * b_l + b_st;
                    // 计算平滑指数a,趋势指数b下，二次平滑值，和残差
                    PinghuaCalculate(a, b);
                    for (float fwqu = cg1; fwqu <= cg2; fwqu++)
                    {
                        label_fw_trend(fwqu);
                    
                        for (int z = quhu_st; z <= qushu_ed; z++) // 区间数z
                        {
                            float[,] fenqulinjiezhi = new float[z, 1]; // 分区临界值
                            fenqulinjiezhi = Divide(z); // 获取到该分区数下的区间临界值      
                            int m_num = (stast_m2 - stast_m1) / stast_ml;
                            for (int m = 0; m <= m_num; m++)  //统计时长m
                            {
                                int sta_m = stast_m1 + m * stast_ml;
                                for (int mst = stast_mst1; mst <= stast_mst2; mst++)
                                {
                                    for (int k = relev_kst; k <= relev_ked; k++) // 关联时长k
                                    {
                                        for (int n = valid_nst; n <= valid_ned; n++) // 有效值n
                                        {
                                            // 第一次时，直接当做最好的结果
                                            if (i == 0 && j == 0 && z == quhu_st && m == 0 && mst == stast_mst1 && k == relev_kst && n == valid_nst && fwqu == cg1 )
                                            {
                                                bestpre = GetRowsPredict(start_index, end_index, quhu_st, (float[,])fenqulinjiezhi.Clone(), relev_kst, valid_nst, stast_m1, gundong, gundong_l, stast_mst1);
                                                TestRight = CalculateRight(start_index, err[0], err[1], bestpre);
                                            }
                                            else
                                            {
                                                float[,] rowsPredict = GetRowsPredict(start_index, end_index, z, (float[,])fenqulinjiezhi.Clone(), k, n, sta_m, gundong, gundong_l, mst);
                                                float right = CalculateRight(start_index, err[0], err[1], rowsPredict); // 命中率
                                                if (right > TestRight)
                                                {
                                                    bestpre = rowsPredict;
                                                    TestRight = right;
                                                    best_n = n;
                                                    best_k = k;
                                                    best_m = sta_m;
                                                    best_mst = mst;
                                                    best_qushu = z;
                                                    best_b = b;
                                                    best_a = a;
                                                    best_cg = fwqu;
                                                }
                                            }

                                        }
                                    }

                                }


                            }
                        }
                  }
                }
            }
            return bestpre;

        }

        // 计算验证集
        public float[,] GetYanzheng(int index_st, int index_ed, float a, float b, int z, int gundong, float gundong_l, int stat_m, int relevk, int vali_n, int best_mst,float best_trend)
        {
            float[,] yanzhenPre = new float[index_ed - index_st + 1, 2];
            PinghuaCalculate(a, b);
            float[,] fenqulinjiezhi = new float[z, 1]; // 分区临界值
            fenqulinjiezhi = Divide(z); // 获取到该分区数下的区间临界值      
            label_fw_trend(best_trend);
            yanzhenPre = GetRowsPredict(index_st, index_ed, z, (float[,])fenqulinjiezhi.Clone(), relevk, vali_n, stat_m, gundong, gundong_l, best_mst);

            return yanzhenPre;
        }
        
        // 计算预测值的命中率
        public float CalculateRight(int index_st, float err_st, float err_ed, float[,] predict_data)
        {
            int rightn = 0;
            for (int i = 0; i < predict_data.GetLength(0); i++)
            {
                float er = listData[index_st + i].Rhmsi - (predict_data[i, 0] + listData[index_st + i].SecPinghua);
                if (er >= err_st && er <= err_ed)
                {
                    rightn++;
                }
            }
            float right = (float)rightn / predict_data.GetLength(0);
            return right;
        }


        // 固定区间数、关联时长k、有效值n、统计时长m、滚动次数rollNum，滚动步长rollValue
        public float[,] GetRowsPredict(int index_st, int index_ed, int qujian_z, float[,] fenqulinjiezhi, int k, int n, int m, int rollNum, float rollValue, int m_st,float[,] fenquFac,int roll_fc,float roll_fcl)
        {

            // [0]=第一次滚动 [0][0]=第一次固定标签为1的具体包含的值 
            List<float[,]> bigLableValue = new List<float[,]>();// [[[0,203,1,32]],] 不同滚动窗口时，分区标签，对应的区间 
            //List<List<float>> labeview = new List<List<float>>(); // 一
            List<float[,]> RowsPlabel = new List<float[,]>();// 各种窗口得到的N行值
            float[,] BestValue = new float[index_ed - index_st + 1, 2]; // 存放每行预测的最好的值，和可信度

            # region  滚动窗口     
            float[,] fenquFcOri = (float[,])fenquFac.Clone();
            // 滚动因素，正向滚动
            for (int fcro = 0; fcro <= roll_fc; fcro++)
            {
                float[,] fenqulinjiezhi1 = (float[,])fenqulinjiezhi.Clone();
                float[,] fenqulinjiezhistart = (float[,])fenqulinjiezhi.Clone();
                if (fcro > 0)
                {
                    for (int fcj = 0; fcj < fenquFac.GetLength(0); fcj++)
                    {
                        fenquFac[fcj, 0] += roll_fcl;
                        fenquFac[fcj, 1] += roll_fcl;
                    }
                }
                label_fw_value(fenquFac); // 因素打标签

                bigLableValue.Add((float[,])fenqulinjiezhi1.Clone()); // 保存第一次计算的分区区间值
                Dividefen(qujian_z, fenqulinjiezhi1);
                //rowsPlabe = RowsCaculate(index_st, index_ed, k, n, qujian_z, m); // 计算得到该种分区条件下,预测的标签值,对应的可信度,
                RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
                
                // 正向滚动计算

                for (int i = 1; i <= rollNum; i++)
                {

                    for (int j = 0; j < fenqulinjiezhi1.GetLength(0); j++)
                    {
                        fenqulinjiezhi1[j, 0] += rollValue;
                        fenqulinjiezhi1[j, 1] += rollValue;
                    }

                    bigLableValue.Add((float[,])fenqulinjiezhi1.Clone()); // 保存分区区间
                    Dividefen(qujian_z, fenqulinjiezhi1);
                    RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
                }
                // 反向滚动计算
                for (int i = 1; i <= rollNum; i++)
                {

                    for (int j = 0; j < fenqulinjiezhistart.GetLength(0); j++)
                    {
                        fenqulinjiezhistart[j, 0] -= rollValue;
                        fenqulinjiezhistart[j, 1] -= rollValue;
                    }
                    bigLableValue.Add((float[,])fenqulinjiezhistart.Clone()); // 保存分区区间
                    Dividefen(qujian_z, fenqulinjiezhistart);
                    RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
                }

            }

            // 滚动因素，反向滚动
            for (int fcro = 1; fcro <= roll_fc; fcro++)
            {
                float[,] fenqulinjiezhi1 = (float[,])fenqulinjiezhi.Clone();
                float[,] fenqulinjiezhistart = (float[,])fenqulinjiezhi.Clone();
                for (int fcj = 0; fcj < fenquFac.GetLength(0); fcj++)
                {
                    fenquFcOri[fcj, 0] -= roll_fcl;
                    fenquFcOri[fcj, 1] -= roll_fcl;
                }
                label_fw_value(fenquFcOri); // 因素打标签
                bigLableValue.Add((float[,])fenqulinjiezhi1.Clone()); // 保存第一次计算的分区区间值
                Dividefen(qujian_z, fenqulinjiezhi);
                //rowsPlabe = RowsCaculate(index_st, index_ed, k, n, qujian_z, m); // 计算得到该种分区条件下,预测的标签值,对应的可信度,
                RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));

                // 正向滚动计算
                for (int i = 1; i <= rollNum; i++)
                {

                    for (int j = 0; j < fenqulinjiezhi1.GetLength(0); j++)
                    {
                        fenqulinjiezhi1[j, 0] += rollValue;
                        fenqulinjiezhi1[j, 1] += rollValue;
                    }

                    bigLableValue.Add((float[,])fenqulinjiezhi1.Clone()); // 保存分区区间
                    Dividefen(qujian_z, fenqulinjiezhi1);
                    RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
                }
                // 反向滚动计算
                for (int i = 1; i <= rollNum; i++)
                {

                    for (int j = 0; j < fenqulinjiezhistart.GetLength(0); j++)
                    {
                        fenqulinjiezhistart[j, 0] -= rollValue;
                        fenqulinjiezhistart[j, 1] -= rollValue;
                    }
                    bigLableValue.Add((float[,])fenqulinjiezhistart.Clone()); // 保存分区区间
                    Dividefen(qujian_z, fenqulinjiezhistart);
                    RowsPlabel.Add(AllRowsLabelOnce(index_st, index_ed, k, n, lablelist, m, m_st));
                }

            }
           
            # endregion
            int ax = 1;

            Parallel.For(0, index_ed - index_st + 1, i =>
            {
                List<float[]> predict = new List<float[]>(); // 存放所有有交集的预测值,交集个数,最大可信度[[pre,n,q],[],[]]

                #region 计算第i行的预测值的交集
                // 各种窗口下的标签的具体值取交集 j控制滚动是第几次

                for (int j = 0; j < bigLableValue.Count - 1; j++)
                {
                    float[] Labelv = new float[4]; //[左区间,右区间,可信度]
                    float[] LabelvNext = new float[4];
                    Labelv[0] = bigLableValue[j][(int)RowsPlabel[j][i, 0] - 1, 0]; // 左区间
                    Labelv[1] = bigLableValue[j][(int)RowsPlabel[j][i, 0] - 1, 1]; // 右区间
                    Labelv[2] = RowsPlabel[j][i, 1];  // 可信度
                    Labelv[3] = 1;
                    for (int y = j + 1; y < bigLableValue.Count; y++)
                    {
                        LabelvNext[0] = bigLableValue[y][(int)RowsPlabel[y][i, 0] - 1, 0];
                        LabelvNext[1] = bigLableValue[y][(int)RowsPlabel[y][i, 0] - 1, 1];
                        LabelvNext[2] = RowsPlabel[y][i, 1];
                        LabelvNext[3] = 1;
                        LabelvNext = Qiujiaoji(Labelv, LabelvNext);
                        if (LabelvNext[3] != 0)
                        {
                            predict.Add((float[])LabelvNext.Clone());
                        }

                    }
                }
                #endregion


                # region 得到该行最佳的具体预测值和对应的可信度
                // 没有交集，取可信度最高的值
                if (predict.Count == 0)
                {
                    List<int> indexlist = new List<int>(); // 存放可信度最高的几个下标
                    float idexbest = RowsPlabel[0][i, 1]; // 获取可信度最高值
                    for (int r = 1; r < RowsPlabel.Count; r++)
                    {
                        if (idexbest < RowsPlabel[r][i, 1])
                        {
                            idexbest = RowsPlabel[r][i, 1];
                        }
                    }
                    for (int r = 0; r < RowsPlabel.Count; r++)  // 获取可信度最高值对应的所有下标
                    {
                        if (RowsPlabel[r][i, 1] == idexbest)
                        {
                            indexlist.Add(r);
                        }
                    }

                    List<float[,]> predict_max = new List<float[,]>();
                    float left = 0;
                    float right = 0;
                    float midel = 0;
                    for (int r = 0; r < indexlist.Count; r++)  // 获取对应的区间
                    {
                        left = bigLableValue[r][(int)RowsPlabel[r][i, 0] - 1, 0];
                        right = bigLableValue[r][(int)RowsPlabel[r][i, 0] - 1, 1];
                        if (left < chancaMin)
                        {
                            left = chancaMin;
                        }
                        else if (right > chancaMax)
                        {
                            right = chancaMax;
                        }
                        midel += (right + left) / 2;

                    }

                    BestValue[i, 0] = midel / indexlist.Count;
                    BestValue[i, 1] = idexbest;

                }
                else //有交集,取最多交集的值
                {
                    List<float[]> predict_only = new List<float[]>();
                    while (predict.Count != 0)
                    {
                        predict_only.Clear();
                        for (int pre = 0; pre < predict.Count - 1; pre++)
                        {
                            int index = predict_only.FindIndex(pr => pr[0] == predict[pre][0] && pr[1] == predict[pre][1]);
                            if (index == -1)
                            {
                                for (int pred = pre + 1; pred < predict.Count; pred++)
                                {
                                    if (predict[pred][0] == predict[pre][0] && predict[pred][1] == predict[pre][1])
                                    {
                                        predict[pre][2] += predict[pred][2];
                                        predict[pre][3] += predict[pred][3];
                                    }


                                }
                                predict_only.Add(predict[pre]);
                            }
                        }
                        int indexlast = predict_only.FindIndex(p => p[0] == predict[predict.Count - 1][0] && p[1] == predict[predict.Count - 1][1]);
                        if (indexlast == -1)
                        {
                            predict_only.Add(predict[predict.Count - 1]);
                        }

                        predict.Clear();
                        // 取交集
                        float[] jiaojiqujian = new float[4];
                        for (int px = 0; px < predict_only.Count - 1; px++)
                        {
                            for (int py = px + 1; py < predict_only.Count; py++)
                            {
                                jiaojiqujian = Qiujiaoji(predict_only[px], predict_only[py]);
                                if (jiaojiqujian[2] != 0)
                                {
                                    predict.Add((float[])jiaojiqujian.Clone());
                                }
                            }
                        }

                    }

                    // 取最大的交集个数的区间
                    float pinlv_max = predict_only[0][3];
                    for (int ji = 1; ji < predict_only.Count; ji++)
                    {
                        if (pinlv_max < predict_only[ji][3])
                        {
                            pinlv_max = predict_only[ji][3];
                        }
                    }
                    List<float[]> qujian_max = predict_only.FindAll(p => p[3] == pinlv_max);
                    float[] pingjuzhi = new float[3];
                    pingjuzhi[0] = 0;
                    pingjuzhi[1] = 0;
                    pingjuzhi[2] = 0;
                    foreach (float[] predictqujian in qujian_max)
                    {

                        if (predictqujian[0] < chancaMin)
                        {
                            predictqujian[0] = chancaMin;
                        }
                        else if (predictqujian[1] > chancaMax)
                        {
                            predictqujian[1] = chancaMax;
                        }
                        pingjuzhi[0] += (predictqujian[0] + predictqujian[1]) / 2;
                        pingjuzhi[1] += predictqujian[2];
                        pingjuzhi[2] += predictqujian[3];
                    }

                    BestValue[i, 0] = pingjuzhi[0] / qujian_max.Count();
                    BestValue[i, 1] = pingjuzhi[1] / pingjuzhi[2];

                }
                # endregion
            });
            return BestValue;
        }

        // 迭代获取最优的关联时长k和有效值个数n,统计时长m,统计起点mst,返回最好的预测值和对应的可信度和该组数据命中率
        public float[,] GetbestKN_fw(int start_index, int end_index, int relev_kst, int relev_ked, int valid_nst, int valid_ned, List<int> label, int stast_m1, int stast_m2, int stast_ml, int stast_mst1, int stast_mst2,float a_st, float a_ed, float a_l, float b_st, float b_ed, float b_l, int quhu_st, int qushu_ed, int gundong, float gundong_l,int roll_fc,float roll_fcl,int qushu_fc1,int qushu_fc2)
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
            int a_num = (int)((a_ed - a_st) / a_l);
            int b_num = (int)((b_ed - b_st) / b_l);
            float[,] bestpre = new float[end_index - start_index + 1, 2];


            best_k = relev_kst;// 最好的关联时长k
            best_n = valid_nst;// 最好的有效值个数n
            best_m = stast_m1;// 最好的统计时长m
            best_mst = stast_mst1;// 最好的统计时长起点
            best_a = a_st; // 最好的平滑指数a
            best_b = b_st; // 最好的趋势指数b
            best_qushu = quhu_st; // 最好的分区数
            best_qujian_fw = qushu_fc1;


            QiuFWMax_Min(); //   求出风温的最大和最小值
            for (int i = 0; i <= a_num; i++) // 平滑指数a
            {
                float a = i * a_l + a_st;
                for (int j = 0; j <= b_num; j++) // 趋势指数b
                {
                    float b = j * b_l + b_st;
                    // 计算平滑指数a,趋势指数b下，二次平滑值，和残差
                    PinghuaCalculate(a, b);
                    for (int fwqu = qushu_fc1; fwqu <= qushu_fc2; fwqu++)
                    {
                        float[,] fenqulinjiezhi_fc = Qiufenquzhi(fwqu);

                        for (int z = quhu_st; z <= qushu_ed; z++) // 区间数z
                        {
                            float[,] fenqulinjiezhi = new float[z, 1]; // 分区临界值
                            fenqulinjiezhi = Divide(z); // 获取到该分区数下的区间临界值      
                            int m_num = (stast_m2 - stast_m1) / stast_ml;
                            for (int m = 0; m <= m_num; m++)  //统计时长m
                            {
                                int sta_m = stast_m1 + m * stast_ml;
                                for (int mst = stast_mst1; mst <= stast_mst2; mst++)
                                {
                                    for (int k = relev_kst; k <= relev_ked; k++) // 关联时长k
                                    {
                                        for (int n = valid_nst; n <= valid_ned; n++) // 有效值n
                                        {
                                            // 第一次时，直接当做最好的结果
                                            if (i == 0 && j == 0 && z == quhu_st && m == 0 && mst == stast_mst1 && k == relev_kst && n == valid_nst && fwqu == qushu_fc1)
                                            {
                                                bestpre = GetRowsPredict(start_index, end_index, quhu_st, (float[,])fenqulinjiezhi.Clone(), relev_kst, valid_nst, stast_m1, gundong, gundong_l, stast_mst1,(float[,])fenqulinjiezhi_fc.Clone(),roll_fc,roll_fcl);
                                                TestRight = CalculateRight(start_index, err[0], err[1], bestpre);
                                            }
                                            else
                                            {
                                                float[,] rowsPredict = GetRowsPredict(start_index, end_index, z, (float[,])fenqulinjiezhi.Clone(), k, n, sta_m, gundong, gundong_l, mst, (float[,])fenqulinjiezhi_fc.Clone(), roll_fc, roll_fcl);
                                                float right = CalculateRight(start_index, err[0], err[1], rowsPredict); // 命中率
                                                if (right > TestRight)
                                                {
                                                    bestpre = rowsPredict;
                                                    TestRight = right;
                                                    best_n = n;
                                                    best_k = k;
                                                    best_m = sta_m;
                                                    best_mst = mst;
                                                    best_qushu = z;
                                                    best_b = b;
                                                    best_a = a;
                                                    best_qujian_fw = fwqu;
                                                }
                                            }

                                        }
                                    }

                                }


                            }
                        }
                    }
                }
            }
            return bestpre;

        }

        // 计算验证集
        public float[,] GetYanzheng(int index_st, int index_ed, float a, float b, int z, int gundong, float gundong_l, int stat_m, int relevk, int vali_n, int best_mst, int z_fc)
        {
            float[,] yanzhenPre = new float[index_ed - index_st + 1, 2];
            PinghuaCalculate(a, b);
            float[,] fenqulinjiezhi = new float[z, 1]; // 分区临界值
            fenqulinjiezhi = Divide(z); // 获取到该分区数下的区间临界值      
            float[,] fenqulinjiezhi_fc = Qiufenquzhi(z_fc);
            yanzhenPre = GetRowsPredict(index_st, index_ed, z, (float[,])fenqulinjiezhi.Clone(), relevk, vali_n, stat_m, gundong, gundong_l, best_mst,fenqulinjiezhi_fc,roll_t,roll_l);
            return yanzhenPre;
        }


        public void SingleFactorCalculate()
        {
           
            FindIndexFW();
            float[,] bestTestlist;
            float[,] verifylist;
            if (TrendFenqu)
            {
                bestTestlist = GetbestKN_fw(train_index + 1, test_index, relevance_k[0], relevance_k[1], valid_n[0], valid_n[1], labellist, statistics_m[0], statistics_m[1], stat_ml, stat_start[0], stat_start[1], cg[0], cg[1],pinghua_a[0],pinghua_a[1],pinghua_l,trend_b[0],trend_b[1],trend_bl,qujian_z[0],qujian_z[1],roll_target,roll_targetl);
                verifylist = GetYanzheng(test_index + 1, listData.Count - 1, best_a,best_b,best_qushu,roll_target,roll_targetl,best_m,best_k,best_n,best_mst,best_cg);
            }
            else
            {
                bestTestlist = GetbestKN_fw(train_index + 1, test_index, relevance_k[0], relevance_k[1], valid_n[0], valid_n[1], labellist, statistics_m[0], statistics_m[1], stat_ml, stat_start[0], stat_start[1], pinghua_a[0], pinghua_a[1], pinghua_l, trend_b[0], trend_b[1], trend_bl, qujian_z[0], qujian_z[1], roll_target, roll_targetl, roll_t, roll_l, fwQunjian[0],fwQunjian[1]);
                verifylist = GetYanzheng(test_index + 1, listData.Count - 1, best_a, best_b, best_qushu, roll_target, roll_targetl, best_m, best_k, best_n, best_mst,best_qujian_fw);
            }

            VerifyRight = CalculateRight(test_index + 1, err[0], err[1], verifylist);
           
            int n = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                listData[i].YuceZhenZhi = bestTestlist[n, 0] + listData[i].SecPinghua;
                listData[i].Trust_v = (float)Math.Round(bestTestlist[n, 1], 4);
                n++;
            }
            n = 0;
            for (int i = test_index + 1; i < listData.Count; i++)
            {
                listData[i].YuceZhenZhi = verifylist[n, 0] + listData[i].SecPinghua;
                listData[i].Trust_v = (float)Math.Round(verifylist[n, 1], 4);
                n++;
            }


        }
    }
}
