using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecxel
{
    class TargetValue:Basis
    {
        public float[] pinghua_a = new float[2]; // 平滑指数a范围
        public float pinghua_l; // 平滑指数a滚动步长
        public float[] trend_b = new float[2]; // 趋势指数b范围
        public float trend_bl; // 趋势指数滚动步长
        public int[] qujian_z = new int[2]; // 区间数z范围
        public float[] err = new float[2]; // 误差区间
        public int roll_t;  // 滚动次数T
        public float roll_l; // 滚动步长
        public bool NumberDengFen = true;

        public float best_a; // 最好的平滑指数a
        public float best_b; // 最好的趋势指数b
        public int best_qushu; // 最好的分区数
        //public float best_testRight; // 最好的测试集命中率
        //public float yanzheng_right; // 验证集命中率
        public float chancaMin = 0; // 残差最小值
        public float chancaMax = 0; // 残差最大值



        public TargetValue(int train_index, int test_index, int[] relevance_k, int[] valid_n, int[] statistics_m, int stat_ml, int[] stat_start, float deleBaifen, float[] pinghua_a, float pinghua_l, float[] trend_b, float trend_bl, int[] qujian_z, float[] err, int roll_t, float roll_l, bool NumberDengFen)
        {
            this.train_index = train_index;
            this.test_index = test_index;
            this.relevance_k = relevance_k;
            this.valid_n = valid_n;
            this.statistics_m = statistics_m;
            this.stat_ml = stat_ml;
            this.stat_start = stat_start;
            this.deletBaifen = deletBaifen;
            this.pinghua_a = pinghua_a;
            this.pinghua_l = pinghua_l;
            this.trend_b = trend_b;
            this.trend_bl = trend_bl;
            this.qujian_z = qujian_z;
            this.err = err;
            this.roll_t = roll_t;
            this.roll_l = roll_l;
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
            RowsPlabel.Add(RowsCaculate(index_st, index_ed, k, n, lablelist, m, m_st, deletBaifen));
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
                RowsPlabel.Add(RowsCaculate(index_st, index_ed, k, n, lablelist, m, m_st, deletBaifen));
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
                RowsPlabel.Add(RowsCaculate(index_st, index_ed, k, n, lablelist, m, m_st, deletBaifen));
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

        // 计算测试集,并求优参数 /计算起始下标、终止下标;平滑指数[a_st,a_ed],步长a_l;趋势指数[b_st,b_ed],步长b_l;区间数范围[qushu_st,qushu_ed];滚动次数gundong,步长gundong_l,统计时长范围[stac_mst,stac_med],步长stc_ml;关联系数[relev_k1,relev_k2];有效值[valid_n1,valid_n2]
        public float[,] GetbestCanshu(int index_st, int index_ed, float a_st, float a_ed, float a_l, float b_st, float b_ed, float b_l, int quhu_st, int qushu_ed, int gundong, float gundong_l, int statc_mst, int stac_med, int stc_ml, int relev_k1, int relev_k2, int valid_n1, int valid_n2, int m_st1, int m_st2)
        {
            int a_num = (int)((a_ed - a_st) / a_l);
            int b_num = (int)((b_ed - b_st) / b_l);
            float[,] bestpre = new float[index_ed - index_st + 1, 2];


            best_k = relev_k1;// 最好的关联时长k
            best_n = valid_n1;// 最好的有效值个数n
            best_m = statc_mst;// 最好的统计时长m
            best_mst = m_st1;// 最好的统计时长起点
            best_a = a_st; // 最好的平滑指数a
            best_b = b_st; // 最好的趋势指数b
            best_qushu = quhu_st; // 最好的分区数

            for (int i = 0; i <= a_num; i++) // 平滑指数a
            {
                float a = i * a_l + a_st;
                for (int j = 0; j <= b_num; j++) // 趋势指数b
                {
                    float b = j * b_l + b_st;
                    // 计算平滑指数a,趋势指数b下，二次平滑值，和残差
                    PinghuaCalculate(a, b);


                    for (int z = quhu_st; z <= qushu_ed; z++) // 区间数z
                    {
                        float[,] fenqulinjiezhi = new float[z, 1]; // 分区临界值
                        fenqulinjiezhi = Divide(z); // 获取到该分区数下的区间临界值      
                        int m_num = (stac_med - statc_mst) / stc_ml;
                        for (int m = 0; m <= m_num; m++)  //统计时长m
                        {
                            int sta_m = statc_mst + m * stc_ml;
                            for (int mst = m_st1; mst <= m_st2; mst++)
                            {
                                for (int k = relev_k1; k <= relev_k2; k++) // 关联时长k
                                {
                                    for (int n = valid_n1; n <= valid_n2; n++) // 有效值n
                                    {
                                        // 第一次时，直接当做最好的结果
                                        if (i == 0 && j == 0 && z == quhu_st && m == 0 && mst == m_st1 && k == relev_k1 && n == valid_n1)
                                        {
                                            bestpre = GetRowsPredict(index_st, index_ed, quhu_st, (float[,])fenqulinjiezhi.Clone(), relev_k1, valid_n1, statc_mst, gundong, gundong_l, m_st1);
                                            TestRight = CalculateRight(index_st, err[0], err[1], bestpre);
                                        }
                                        else
                                        {
                                            float[,] rowsPredict = GetRowsPredict(index_st, index_ed, z, (float[,])fenqulinjiezhi.Clone(), k, n, sta_m, gundong, gundong_l, mst);
                                            float right = CalculateRight(index_st, err[0], err[1], rowsPredict); // 命中率
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
        public float[,] GetYanzheng(int index_st, int index_ed, float a, float b, int z, int gundong, float gundong_l, int stat_m, int relevk, int vali_n, int best_mst)
        {
            float[,] yanzhenPre = new float[index_ed - index_st + 1, 2];
            PinghuaCalculate(a, b);
            float[,] fenqulinjiezhi = new float[z, 1]; // 分区临界值
            fenqulinjiezhi = Divide(z); // 获取到该分区数下的区间临界值      

            yanzhenPre = GetRowsPredict(index_st, index_ed, z, (float[,])fenqulinjiezhi.Clone(), relevk, vali_n, stat_m, gundong, gundong_l, best_mst);
           
            return yanzhenPre;
        }

        // 硅含量预测值计算
        public void ValueCalculate()
        {
            float[,] testpred = GetbestCanshu(train_index + 1, test_index, pinghua_a[0], pinghua_a[1], pinghua_l, trend_b[0], trend_b[1], trend_bl, qujian_z[0], qujian_z[1], roll_t, roll_l, statistics_m[0], statistics_m[1], stat_ml, relevance_k[0], relevance_k[1], valid_n[0], valid_n[1], stat_start[0], stat_start[1]);

            float[,] yanzheng = GetYanzheng(test_index + 1, listData.Count - 1, best_a, best_b, best_qushu, roll_t, roll_l, best_m, best_k, best_n, best_mst);
            VerifyRight = CalculateRight(test_index + 1, err[0], err[1], yanzheng);
            int n = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                listData[i].YuceZhenZhi = testpred[n, 0] + listData[i].SecPinghua;
                listData[i].Trust_v = (float)Math.Round(testpred[n, 1], 4);
                n++;
            }
            n = 0;
            for (int i = test_index + 1; i < listData.Count; i++)
            {
                listData[i].YuceZhenZhi = yanzheng[n, 0] + listData[i].SecPinghua;
                listData[i].Trust_v = (float)Math.Round(yanzheng[n, 1], 4);
                n++;
            }

        }
    }
}
