using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using NPOI.SS.Formula;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace Ecxel
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //ObservableCollection<Datalist> listData = new ObservableCollection<Datalist>(); //目标
        //ObservableCollection<Datalist> listData_factor; //单因素
        List<Datalist> listDataStart = new List<Datalist>();  // 目标数据
        List<Datalist> listData = new List<Datalist>();  // 目标数据
        List<Datalist> listData_factor1 = new List<Datalist>(); // 因素1对应的目标数据
        List<Datalist> listData_factor2 = new List<Datalist>(); // 因素2对应的目标数据
        List<Datalist> listData_factor3 = new List<Datalist>(); // 因素3对应的目标数据
        List<Datalist> listData_factor4 = new List<Datalist>(); // 因素4对应的目标数据
        List<Datalist> listData_factor5 = new List<Datalist>(); // 因素5对应的目标数据
        
        // 具体硅含量预测的参数
        public int train_index; // 训练集与测试集分割线
        public int test_index; // 测试集和验证集分割线
        public float[] pinghua_a = new float[2]; // 平滑指数a范围
        public float pinghua_l; // 平滑指数a滚动步长
        public float[] trend_b = new float[2]; // 趋势指数b范围
        public float trend_bl; // 趋势指数滚动步长
        public int[] qujian_z = new int[2]; // 区间数z范围
        public float[] err = new float[2]; // 误差区间
        public int roll_t;  // 滚动次数T
        public float roll_l; // 滚动步长
        public int[] statistics_m = new int[2]; // 统计时长M范围
        public int stat_ml; // 统计时长滚动步长
        public int[] stat_start = new int[2]; // 统计时长的起点范围
        public int[] relevance_k = new int[2]; // 关联时长k范围
        public int[] valid_n = new int[2]; // 有效值n范围
        public List<int> lablelist = new List<int>() { -1,0,1}; //标签列表
        public int best_k;// 最好的关联时长k
        public int best_n;// 最好的有效值个数n
        public int best_m;// 最好的统计时长m
        public int best_mst;// 最好的统计时长起点
        public float best_a; // 最好的平滑指数a
        public float best_b; // 最好的趋势指数b
        public int best_qushu; // 最好的分区数
        public float best_testRight; // 最好的测试集命中率
        public float yanzheng_right; // 验证集命中率
        public float deletBaifen; // 删除百分比
        public bool DengNumFenQu;
        public bool DengNumFenQU_FC;
        public string FactorName;// 因素名称
        // 趋势预测的参数
        public float jieding_t;
        public List<float[,]> bestTestlist = new List<float[,]>(); // 放最好的测试集预测值,可信度,命中率
        public List<float[,]> verifylist = new List<float[,]>();// 放验证集预测值,可信度,命中率

        // 因素
       // ObservableCollection<FactorData> FWdataList = new ObservableCollection<FactorData>();
        List<FactorData> FWdataList1 = new List<FactorData>(); // 因素1数据
        List<FactorData> FWdataList2 = new List<FactorData>(); // 因素2数据
        List<FactorData> FWdataList3 = new List<FactorData>(); // 因素3数据
        List<FactorData> FWdataList4 = new List<FactorData>(); // 因素4数据
        List<FactorData> FWdataList5 = new List<FactorData>(); // 因素5数据
        public int[] fwQunjian = new int[2]; // 单因素分的区间数
        public int best_qujian_fw; 
        public int rollb_fw;
        public float rollb_fwl;
        public float[] cg = new float[2]; // 趋势分区值
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 目标预测

        #region 硅具体值预测
        // 读入硅数据
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            listDataStart.Clear();
            Microsoft.Win32.OpenFileDialog dialog = new OpenFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fileName = dialog.FileName;
            //int indx = 0; 
            int Rownum = 0; // excel表列数
            if (fileName != "")
            {
                FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
                HSSFWorkbook workbook = new HSSFWorkbook(fileStream); //获取excle数据
                ISheet sheet = workbook.GetSheetAt(0); //根据表名获取表
                IRow row;

                //Rownum = sheet.LastRowNum; // 最后一行
                //DataTable dt = new DataTable();
                //dt.Columns.Add("出铁时间", typeof(String));
                //dt.Columns.Add("出铁结束时间", typeof(String));
                //dt.Columns.Add("R_HMSI",typeof(String));
                //dt.Columns.Add("P_POTTAPPING_TIM", typeof(String));
                //dt.Columns.Add("R_TAPNUMB", typeof(String));


                #region 读入数据
                //String col,colnext;
                //row = sheet.GetRow(2);
                //dt.Rows.Add(new object[] { row.GetCell(1).ToString(), row.GetCell(2).ToString(), row.GetCell(3).ToString(),"1",row.GetCell(5).ToString() });
                //col = row.GetCell(1).ToString();
                // 相邻重复的不读入
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    if (row != null)
                    {
                        //colnext = row.GetCell(1).ToString();
                        //if (col != colnext)
                        //{
                        //dt.Rows.Add(new object[] { colnext, row.GetCell(2).ToString(), row.GetCell(3).ToString(),"1", row.GetCell(5).ToString() });
                        //col = colnext;
                        //}
                        Datalist dl = new Datalist();
                        dl.Id = i;
                        try { dl.POPtime = Convert.ToDateTime(row.GetCell(1).ToString().Replace('/', '-')); }
                        catch
                        {
                            string[] time_str = row.GetCell(1).ToString().Split('/');
                            string new_time = "20" + time_str[2] + "-" + time_str[0] + "-" + time_str[1];
                            dl.POPtime = Convert.ToDateTime(new_time);
                        }
                        dl.Rhmsi = float.Parse(row.GetCell(2).ToString());
                        listDataStart.Add(dl);

                    }
                    else
                    {
                        break;
                    }
                }
                workbook.Close();
                fileStream.Close();
                #endregion

                // 标记重复数据
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //      for (int j = i + 1; j < dt.Rows.Count; j++)
                //      {
                //            if (dt.Rows[i][0].ToString() == dt.Rows[j][0].ToString())
                //            {
                //                 dt.Rows[j][1] = "xx";

                //            }
                //      }
                //}
                listDataStart.ForEach(i=>listData.Add(new Datalist(i)));
                List.ItemsSource = listData; // 给ListView绑定数据源
                //getDatalist(dt);  // 将数据传入 Datalist的对象listData
                //MessageBox.Show(listData.Count.ToString());

            }
            else
            {
                MessageBox.Show("请选择正确的数据文件");
            }

        }
  
        //获取参数的按钮点击事件(硅含量具体预测）
        private void btn_check_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }


                // 平滑指数
                if (pinghua.Text == "")
                {
                    pinghua_a[0] = float.Parse(pinghua1.Text); //左区间
                    pinghua_a[1] = float.Parse(pinghua2.Text); //右区间
                    pinghua_l = float.Parse(pinghual.Text);  //步长
                }
                else
                {
                    pinghua_a[0] = float.Parse(pinghua.Text);
                    pinghua_a[1] = float.Parse(pinghua.Text);
                    pinghua_l = 1;
                }

                // 趋势指数
                if (trend.Text == "")
                {
                    trend_b[0] = float.Parse(trend1.Text);
                    trend_b[1] = float.Parse(trend2.Text);
                    trend_bl = float.Parse(trendL.Text); // 步长
                }
                else
                {
                    trend_b[0] = float.Parse(trend.Text);
                    trend_b[1] = float.Parse(trend.Text);
                    trend_bl = 1;
                }

                // 区间数
                if (qujianNum.Text == "")
                {
                    qujian_z[0] = Int32.Parse(qujianNum1.Text);
                    qujian_z[1] = Int32.Parse(qujianNum2.Text);

                }
                else
                {
                    qujian_z[0] = Int32.Parse(qujianNum.Text);
                    qujian_z[1] = Int32.Parse(qujianNum.Text);
                }

                //滚动次数T
                roll_t = Int32.Parse(rollNum.Text);
                roll_l = float.Parse(rolll.Text);
                // 统计时长M范围
                if (M.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2.Text); // 统计时长m2
                    stat_ml = Int32.Parse(M_ml.Text); // 统计时长步长

                }
                else
                {
                    statistics_m[0] = Int32.Parse(M.Text);
                    statistics_m[1] = Int32.Parse(M.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围
                if (tongjistart.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1.Text);
                    stat_start[1] = Int32.Parse(tongjistart2.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart.Text);
                    stat_start[1] = Int32.Parse(tongjistart.Text);
                }


                // 关联步长
                if (K.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1.Text);
                    relevance_k[1] = Int32.Parse(K2.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K.Text);
                    relevance_k[1] = Int32.Parse(K.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (N.Text == "")
                {
                    valid_n[0] = Int32.Parse(N1.Text);
                    valid_n[1] = Int32.Parse(N2.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(N.Text);
                    valid_n[1] = Int32.Parse(N.Text);
                    //flagN = true;
                }

                if (dengnumberDivide_TG_CK.IsChecked == true)
                {
                    DengNumFenQu = true;
                }
                else
                {
                    DengNumFenQu = false;
                }
                err[0] = -float.Parse(err1.Text);
                err[1] = float.Parse(err1.Text);

                deletBaifen = float.Parse(delet_v.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };

            //MessageBox.Show(pinghua_a1.ToString() + "," + pinghua_a2.ToString());
            //MessageBox.Show(trend_b1.ToString() + "," + trend_b2.ToString());
            //MessageBox.Show(qujian_z1.ToString() + "," + qujian_z2.ToString());

            //MessageBox.Show(roll_t.ToString());
            //MessageBox.Show(statistics_m1.ToString() + "," + statistics_m2.ToString());
            //MessageBox.Show(relevance_k1.ToString() + "," + relevance_k2.ToString());
            //MessageBox.Show(valid_n1.ToString() + "," + valid_n2.ToString());

            //PinghuaCalculate(pinghua_a1, trend_b1);
            //Divide(qujian_z1);

        }

        // 计算（具体值预测）
        private void btn_math_Click(object sender, RoutedEventArgs e)
        {

            TargetValue targetValue = new TargetValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l,DengNumFenQu);
            targetValue.listData = listData;
            targetValue.ValueCalculate();
            TestRight.Text = Math.Round(targetValue.TestRight, 4).ToString();
            VerifyRight.Text = Math.Round(targetValue.VerifyRight, 4).ToString();
            // 最佳参数保存（具体值预测）
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            StreamWriter sw = File.CreateText(path + "具体值预测配置文件.txt");
            sw.Write("训练集：" + (targetValue.train_index + 1).ToString());
            sw.WriteLine();
            sw.Write("测试集：" + (targetValue.test_index - targetValue.train_index).ToString());
            sw.WriteLine();
            sw.Write("平滑指数：" + targetValue.best_a.ToString());
            sw.WriteLine();
            sw.Write("趋势指数：" + targetValue.best_b.ToString());
            sw.WriteLine();
            sw.Write("区间数：" +  targetValue.best_qushu.ToString());
            sw.WriteLine();
            sw.Write("滚动次数：" + targetValue.roll_t.ToString());
            sw.WriteLine();
            sw.Write("滚动步长：" + targetValue.roll_l.ToString());
            sw.WriteLine();
            sw.Write("统计时长：" + targetValue.best_m.ToString());
            sw.WriteLine();
            sw.Write("统计时长起点：" + targetValue.best_mst.ToString());
            sw.WriteLine();
            sw.Write("关联时长：" + targetValue.best_k.ToString());
            sw.WriteLine();
            sw.Write("有效值个数：" + targetValue.best_n.ToString());
            sw.WriteLine();
            sw.Write("误差：±" + err[1]);
            sw.WriteLine();
            sw.Write("删除比例：" + targetValue.deletBaifen.ToString());
            sw.WriteLine();
            if(DengNumFenQu)
            {
                sw.Write("分区模式：等数量");
            }
            else
            {
                sw.Write("分区模式：等值");
            }
            sw.WriteLine();
            sw.Write("*********************************************************");
            sw.WriteLine();
            sw.Write("测试集命中率：" + targetValue.TestRight.ToString());
            sw.WriteLine();
            sw.Write("验证集命中率：" + targetValue.VerifyRight.ToString());
            sw.Close();

        }

        // 数据导出（具体值预测）
        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            #region 写入excel
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fname = dialog.FileName;
            if (fname != "")
            {
                //FileStream fileStream2 = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                HSSFWorkbook workbook1 = new HSSFWorkbook(); //获取excle数据
                //HSSFWorkbook wp = HSSFWorkbook.Create(fileStream2);

                ISheet sheet1;
                try
                {
                    sheet1 = workbook1.CreateSheet("Predict"); //根据表名第一个表
                }
                catch
                {
                    sheet1 = workbook1.GetSheet("Predict");
                }
                IRow row = sheet1.CreateRow(0);
                ICell headCell = row.CreateCell(0);
                headCell.SetCellValue("序号");
                headCell = row.CreateCell(1);
                headCell.SetCellValue("出铁时间");
                headCell = row.CreateCell(2);
                //headCell.SetCellValue("出铁结束时间");
                //headCell = row.CreateCell(3);
                headCell.SetCellValue("铁水中SI含量");
                headCell = row.CreateCell(3);
                headCell.SetCellValue("趋势预测值");
                headCell = row.CreateCell(4);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(5);
                headCell.SetCellValue("具体值预测");
                headCell = row.CreateCell(6);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(7);
                headCell.SetCellValue("一致性");

                HSSFCell[] Cell = new HSSFCell[8];
                int excRow = 1; // 行号
                //int RowNum =sheet1.LastRowNum;
                for (int j = 0; j < listData.Count; j++)
                {
                    row = sheet1.CreateRow(excRow);


                    for (int i = 0; i <= 7; i++)
                    {
                        Cell[i] = (HSSFCell)row.CreateCell(i);
                    }

                    Cell[0].SetCellValue(excRow);
                    Cell[1].SetCellValue(listData[j].POPtime);
                    // Cell[2].SetCellValue(listData[j].POPfinish);
                    Cell[2].SetCellValue(listData[j].Rhmsi);

                    if (j > train_index)
                    {
                        Cell[3].SetCellValue(listData[j].LabelYuCe);
                        Cell[4].SetCellValue(listData[j].Trust_t);
                        Cell[5].SetCellValue(listData[j].YuceZhenZhi);
                        Cell[6].SetCellValue(listData[j].Trust_v);
                        Cell[7].SetCellValue(listData[j].Consistency);
                    }
                    else
                    {
                        Cell[3].SetCellValue("");
                        Cell[4].SetCellValue("");
                        Cell[5].SetCellValue("");
                        Cell[6].SetCellValue("");
                        Cell[7].SetCellValue("");


                    }

                    excRow++;
                }


            
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
                workbook1.Write(fs);
                //fileStream2.Close();
                fs.Close();
                workbook1.Close();

                MessageBox.Show("完成");
            }
            else
            {
                MessageBox.Show("请输入正确的保存文件名");
            }
            #endregion

        }
       
       
        // 读取参数配置文件（具体值预测）
        private void btn_readParm_Click(object sender, RoutedEventArgs e)
        {
            StreamReader sr = new StreamReader("具体值预测配置文件.txt");
            String line;
            line = sr.ReadLine();
            trainN.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            testN.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            pinghua.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            trend.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            qujianNum.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            rollNum.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            rolll.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            M.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            tongjistart.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            K.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            N.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            err1.Text = line.Trim().Split('：')[1].Split('±')[1];
            line = sr.ReadLine();
            delet_v.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            string str = line.Trim().Split('：')[1];
            if (str == "等数量")
            {
                dengnumberDivide_TG_CK.IsChecked = true;
            }
            else
            {
                dengzhiDivide_TG_CK.IsChecked = true;
            }
            sr.Close();




        }

        private void dengnumberDivide_TG_CK_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_TG_CK.IsChecked = false;
        }

        private void dengzhiDivide_TG_CK_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_TG_CK.IsChecked = false;
        }
        #endregion

        #region 硅趋势预测
        // 获取参数设置（趋势预测）
        private void btn_check_t_Click(object sender, RoutedEventArgs e)
        {
          
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号
                jieding_t = float.Parse(Jieding_t.Text); // 界定

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }

                // 统计时长M范围
                if (M_t.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1_t.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2_t.Text); // 统计时长m2
                    stat_ml = Int32.Parse(ML_t.Text); // 统计时长步长

                }
                else
                {
                    statistics_m[0] = Int32.Parse(M_t.Text);
                    statistics_m[1] = Int32.Parse(M_t.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围
                if (tongjistart_t.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_t.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_t.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_t.Text);
                    stat_start[1] = Int32.Parse(tongjistart_t.Text);
                }


                // 关联步长
                if (K_t.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_t.Text);
                    relevance_k[1] = Int32.Parse(K2_t.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_t.Text);
                    relevance_k[1] = Int32.Parse(K_t.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (N_t.Text == "")
                {
                    valid_n[0] = Int32.Parse(N1_t.Text);
                    valid_n[1] = Int32.Parse(N2_t.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(N_t.Text);
                    valid_n[1] = Int32.Parse(N_t.Text);
                    //flagN = true;
                }

                
                deletBaifen = float.Parse(delet_t.Text);
              


            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
           
            //test();
            //MessageBox.Show(train_index_t.ToString());
            //MessageBox.Show(test_index_t.ToString());
            //MessageBox.Show(jieding_t.ToString());
            //MessageBox.Show(statistics_m_t[0].ToString());
            //MessageBox.Show(statistics_m_t[1].ToString());
            //MessageBox.Show(stat_ml_t.ToString());
            //MessageBox.Show(stat_start_t[0].ToString());
            //MessageBox.Show(stat_start_t[1].ToString());
            //MessageBox.Show(relevance_k_t[0].ToString());
            //MessageBox.Show(relevance_k_t[1].ToString());
            //MessageBox.Show(valid_n_t[0].ToString());
            //MessageBox.Show(valid_n_t[1].ToString());
            //MessageBox.Show(fwQunjian[0].ToString());
            //MessageBox.Show(fwQunjian[1].ToString());

        }
        // 计算（趋势预测）
        private void btn_math_t_Click(object sender, RoutedEventArgs e)
        {

            TargetTrend targetTrend = new TargetTrend(train_index,test_index,relevance_k,valid_n,statistics_m,stat_ml,stat_start,deletBaifen,jieding_t);
            targetTrend.listData = listData;
            targetTrend.TrendCalculate();
            TestRight_t.Text = Math.Round(targetTrend.TestRight, 4).ToString();
            VerifyRight_t.Text = Math.Round(targetTrend.VerifyRight, 4).ToString();
            // 最佳参数保存（趋势预测）           
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            StreamWriter sw = File.CreateText(path + "趋势预测配置文件.txt");
            sw.Write("训练集：" + (targetTrend.train_index + 1).ToString());
            sw.WriteLine();
            sw.Write("测试集：" + (targetTrend.test_index - targetTrend.train_index).ToString());
            sw.WriteLine();
            sw.Write("界定值：" + targetTrend.jieding.ToString());
            sw.WriteLine();
            sw.Write("统计时长：" + targetTrend.best_m.ToString());
            sw.WriteLine();
            sw.Write("统计时长起点：" + targetTrend.best_mst.ToString());
            sw.WriteLine();
            sw.Write("关联时长：" + targetTrend.best_k.ToString());
            sw.WriteLine();
            sw.Write("有效值个数：" + targetTrend.best_n.ToString());
            sw.WriteLine();
            sw.Write("删除比例：" + targetTrend.deletBaifen.ToString());
            sw.WriteLine();
            sw.Write("*********************************************************");
            sw.WriteLine();
            sw.Write("测试集命中率：" + targetTrend.TestRight.ToString());
            sw.WriteLine();
            sw.Write("验证集命中率：" + targetTrend.VerifyRight.ToString());
            sw.Close();
        }
        // 数据导出
        private void btn_save_t_Click(object sender, RoutedEventArgs e)
        {
            #region 写入excel
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fname = dialog.FileName;
            if (fname != "")
            {
                // FileStream fileStream2 = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                HSSFWorkbook workbook1 = new HSSFWorkbook(); //获取excle数据
                ISheet sheet1;
                try
                {
                    sheet1 = workbook1.CreateSheet("Predict"); //根据表名第一个表
                }
                catch
                {
                    sheet1 = workbook1.GetSheet("Predict");
                }


                IRow row = sheet1.CreateRow(0);
                ICell headCell = row.CreateCell(0);
                headCell.SetCellValue("序号");
                headCell = row.CreateCell(1);
                headCell.SetCellValue("出铁时间");
                //headCell = row.CreateCell(2);
                //headCell.SetCellValue("出铁结束时间");
                headCell = row.CreateCell(2);
                headCell.SetCellValue("铁水中SI含量");
                headCell = row.CreateCell(3);
                headCell.SetCellValue("趋势预测值");
                headCell = row.CreateCell(4);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(5);
                headCell.SetCellValue("具体值预测");
                headCell = row.CreateCell(6);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(7);
                headCell.SetCellValue("一致性");

                HSSFCell[] Cell = new HSSFCell[8];
                int excRow = 1; // 行号
                //int RowNum =sheet1.LastRowNum;
                for (int j = 0; j < listData.Count; j++)
                {
                    row = sheet1.CreateRow(excRow);


                    for (int i = 0; i <= 7; i++)
                    {
                        Cell[i] = (HSSFCell)row.CreateCell(i);
                    }

                    Cell[0].SetCellValue(excRow);
                    Cell[1].SetCellValue(listData[j].POPtime);
                    //Cell[2].SetCellValue(listData[j].POPfinish);
                    Cell[2].SetCellValue(listData[j].Rhmsi);

                    if (j > train_index)
                    {
                        Cell[3].SetCellValue(listData[j].LabelYuCe);
                        Cell[4].SetCellValue(listData[j].Trust_t);
                        Cell[5].SetCellValue(listData[j].YuceZhenZhi);
                        Cell[6].SetCellValue(listData[j].Trust_v);
                        Cell[7].SetCellValue(listData[j].Consistency);
                    }
                    else
                    {
                        Cell[3].SetCellValue("");
                        Cell[4].SetCellValue("");
                        Cell[5].SetCellValue("");
                        Cell[6].SetCellValue("");
                        Cell[7].SetCellValue("");


                    }

                    excRow++;
                }


            #endregion
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
                workbook1.Write(fs);
                //fileStream2.Close();
                fs.Close();
                workbook1.Close();

                MessageBox.Show("完成");
            }
            else
            {
                MessageBox.Show("请输入正确的保存文件名");
            }



        }
       
     
        // 读取参数配置文件（趋势预测）
        private void btn_readParm_t_Click(object sender, RoutedEventArgs e)
        {
            StreamReader sr = new StreamReader("趋势预测配置文件.txt");
            String line;
            line = sr.ReadLine();
            trainN.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            testN.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            Jieding_t.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            M_t.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            tongjistart_t.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            K_t.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            N_t.Text = line.Trim().Split('：')[1];
            line = sr.ReadLine();
            delet_t.Text = line.Trim().Split('：')[1];
            sr.Close();
        }
        #endregion 

        // 综合分析
        private void btn_Zonghe_Click(object sender, RoutedEventArgs e)
        {
            int testqushi_n = 0;
            int yanzhengqushi_n = 0;
            float qushizhi = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                qushizhi = listData[i].YuceZhenZhi - listData[i - 1].YuceZhenZhi;
                if (qushizhi < -jieding_t && listData[i].LabelYuCe == lablelist[0])
                {
                    testqushi_n++;
                    listData[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData[i].LabelYuCe == lablelist[2])
                {
                    testqushi_n++;
                    listData[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData[i].LabelYuCe == lablelist[1])
                {
                    testqushi_n++;
                    listData[i].Consistency = "Yes";
                }
                else
                {
                    listData[i].Consistency = "No";
                }
            }

            for (int i = test_index + 1; i < listData.Count; i++)
            {
                qushizhi = listData[i].YuceZhenZhi - listData[i - 1].YuceZhenZhi;
                if (qushizhi < -jieding_t && listData[i].LabelYuCe == lablelist[0])
                {
                    yanzhengqushi_n++;
                    listData[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData[i].LabelYuCe == lablelist[2])
                {
                    yanzhengqushi_n++;
                    listData[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData[i].LabelYuCe == lablelist[1])
                {
                    yanzhengqushi_n++;
                    listData[i].Consistency = "Yes";
                }
                else
                {
                    listData[i].Consistency = "No";
                }
            }

            //  MessageBox.Show("训练集一致性：" + ((float)testqushi_n / (test_index - train_index)).ToString());
            // MessageBox.Show("验证集一致性：" + ((float)yanzhengqushi_n / (listData.Count - test_index - 1)).ToString());
            test_yizhi.Text = ((float)testqushi_n / (test_index - train_index)).ToString();
            yanzheng_yizhi.Text = ((float)yanzhengqushi_n / (listData.Count - test_index - 1)).ToString();
        }

        #endregion 


        #region 单因素预测1
       
        #region 单因素趋势预测

        // 读取风温数据表
        private void btn_fw_Click(object sender, RoutedEventArgs e)
        {
           // List<int[]> a = new List<int[]> {new int[]{1,2,3},new int[]{4,5,6}};
            //List<int[]> b = new List<int[]>();
           //a.ForEach(i => b.Add(i));
           // b[0][0] = 0;
            FWdataList1.Clear();
            listData_factor1.Clear();
            listDataStart.ForEach(i => listData_factor1.Add(new Datalist(i)));
            ListFengWen.ItemsSource = listData_factor1;
            Microsoft.Win32.OpenFileDialog dialog = new OpenFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fileNameFW = dialog.FileName;
            //int indx = 0; 
         
            if (fileNameFW != "")
            {
                FileStream fileStream = new FileStream(fileNameFW, FileMode.Open, FileAccess.ReadWrite);
                HSSFWorkbook workbook = new HSSFWorkbook(fileStream); //获取excle数据
                ISheet sheet = workbook.GetSheetAt(0); //根据表名获取表
                IRow row;

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    if (row != null)
                    {
                       
                        FactorData dl = new FactorData();
                        dl.Id = i;
                        try { dl.Time = Convert.ToDateTime(row.GetCell(1).ToString().Replace('/', '-')); }
                        catch
                        {
                            string[] time_str = row.GetCell(1).ToString().Split('/');
                            string new_time = "20"+time_str[2]+"-"+time_str[0]+"-"+time_str[1];
                            dl.Time = Convert.ToDateTime(new_time);
                        }
                        dl.Temperature = float.Parse(row.GetCell(2).ToString());
                        FWdataList1.Add(dl);

                    }
                    else
                    {
                        break;
                    }
                }
                workbook.Close();
                fileStream.Close();
               
            }
        
        }
        
        // 获取参数设置
        private void btn_check_fw_Click(object sender, RoutedEventArgs e)
        {

            K_t.Text = "";
            K1_t.Text = "";
            K2_t.Text = "";
            tongjistart_t.Text = "";
            tongjistart1_t.Text = "";
            tongjistart2_t.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号
                jieding_t = float.Parse(Jieding_t.Text); // 界定

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }

                // 统计时长M范围
                if (M_t.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1_t.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2_t.Text); // 统计时长m2
                    stat_ml = Int32.Parse(ML_t.Text); // 统计时长步长
                }
                else
                {
                    statistics_m[0] = Int32.Parse(M_t.Text);
                    statistics_m[1] = Int32.Parse(M_t.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围
                if (tongjistart_fw.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fw.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fw.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fw.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fw.Text);
                }


                // 关联步长
                if (K_fw.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fw.Text);
                    relevance_k[1] = Int32.Parse(K2_fw.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fw.Text);
                    relevance_k[1] = Int32.Parse(K_fw.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fc.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fc.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fc.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fc.Text);
                    valid_n[1] = Int32.Parse(valid_n_fc.Text);
                    //flagN = true;
                }
                if (value_FC_CK.IsChecked == true)
                {
                    if (qushu_fw.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fw.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fw.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fw.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fw.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fw.Text);
                    rollb_fwl = float.Parse(rollL_fw.Text);

                    if (dengnumberDivide_FC_CK.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if(trend_FC_CK.IsChecked == true)
                {
                    if (change_fw.Text == "")
                    {
                        cg[0] = float.Parse(change1_fw.Text);
                        cg[1] = float.Parse(change2_fw.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fw.Text);
                        cg[1] = float.Parse(change_fw.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }

                FactorName = CBFC.Text;
                deletBaifen = float.Parse(delet_fc.Text);
               
              

            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
            //Lable();
            //MessageBox.Show(train_index_t.ToString());
            //MessageBox.Show(test_index_t.ToString());
            //MessageBox.Show(jieding_t.ToString());
            //MessageBox.Show(statistics_m_t[0].ToString());
            //MessageBox.Show(statistics_m_t[1].ToString());
            //MessageBox.Show(stat_ml_t.ToString());
            //MessageBox.Show(stat_start_t[0].ToString());
            //MessageBox.Show(stat_start_t[1].ToString());
            //MessageBox.Show(relevance_k_t[0].ToString());
            //MessageBox.Show(relevance_k_t[1].ToString());
            //MessageBox.Show(valid_n_t[0].ToString());
            //MessageBox.Show(valid_n_t[1].ToString());
            //MessageBox.Show(fwQunjian[0].ToString());
            //MessageBox.Show(fwQunjian[1].ToString());
        }

        // 计算
        private void btn_math_fw_Click(object sender, RoutedEventArgs e)
        {

            SingleFactorTrend singleFactorTrend;
            if (value_FC_CK.IsChecked == true)
            {
                singleFactorTrend = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, jieding_t, rollb_fw, rollb_fwl, DengNumFenQU_FC);
                
            }
            else
            {
                singleFactorTrend = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen,cg,jieding_t);
                
            }
            singleFactorTrend.FWdataList = FWdataList1;
            singleFactorTrend.listData = listData_factor1;
            singleFactorTrend.SingleFactorCalculate();
            TestRight_fw.Text = singleFactorTrend.TestRight.ToString();
            VerifyRight_fw.Text = singleFactorTrend.VerifyRight.ToString();

            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FC_CK.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName+"趋势预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorTrend.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend.test_index - singleFactorTrend.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorTrend.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName+"趋势预测配置文件(值分区).txt");
                sw.Write("训练集：" + (singleFactorTrend.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend.test_index - singleFactorTrend.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素分区数：" + singleFactorTrend.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorTrend.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorTrend.roll_l.ToString());
                sw.WriteLine();
                if (singleFactorTrend.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend.VerifyRight.ToString());
                sw.Close();
            }
            
        }

        // 写入excel
        private void btn_save_fw_Click(object sender, RoutedEventArgs e)
        {
            #region 写入excel
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fname = dialog.FileName;
            if (fname != "")
            {
                // FileStream fileStream2 = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                HSSFWorkbook workbook1 = new HSSFWorkbook(); //获取excle数据
                ISheet sheet1;
                try
                {
                    sheet1 = workbook1.CreateSheet("Predict"); //根据表名第一个表
                }
                catch
                {
                    sheet1 = workbook1.GetSheet("Predict");
                }


                IRow row = sheet1.CreateRow(0);
                ICell headCell = row.CreateCell(0);
                headCell.SetCellValue("序号");
                headCell = row.CreateCell(1);
                headCell.SetCellValue("出铁时间");
                //headCell = row.CreateCell(2);
                //headCell.SetCellValue("出铁结束时间");
                headCell = row.CreateCell(2);
                headCell.SetCellValue("铁水中SI含量");
                headCell = row.CreateCell(3);
                headCell.SetCellValue("趋势预测值");
                headCell = row.CreateCell(4);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(5);
                headCell.SetCellValue("具体值预测");
                headCell = row.CreateCell(6);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(7);
                headCell.SetCellValue("一致性");

                HSSFCell[] Cell = new HSSFCell[8];
                int excRow = 1; // 行号
                //int RowNum =sheet1.LastRowNum;
                for (int j = 0; j < listData_factor1.Count; j++)
                {
                    row = sheet1.CreateRow(excRow);


                    for (int i = 0; i <= 7; i++)
                    {
                        Cell[i] = (HSSFCell)row.CreateCell(i);
                    }

                    Cell[0].SetCellValue(excRow);
                    Cell[1].SetCellValue(listData_factor1[j].POPtime);
                    //Cell[2].SetCellValue(listData[j].POPfinish);
                    Cell[2].SetCellValue(listData_factor1[j].Rhmsi);

                    if (j > train_index)
                    {
                        Cell[3].SetCellValue(listData_factor1[j].LabelYuCe);
                        Cell[4].SetCellValue(listData_factor1[j].Trust_t);
                        Cell[5].SetCellValue(listData_factor1[j].YuceZhenZhi);
                        Cell[6].SetCellValue(listData_factor1[j].Trust_v);
                        Cell[7].SetCellValue(listData_factor1[j].Consistency);
                    }
                    else
                    {
                        Cell[3].SetCellValue("");
                        Cell[4].SetCellValue("");
                        Cell[5].SetCellValue("");
                        Cell[6].SetCellValue("");
                        Cell[7].SetCellValue("");


                    }

                    excRow++;
                }


            #endregion
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
                workbook1.Write(fs);
                //fileStream2.Close();
                fs.Close();
                workbook1.Close();

                MessageBox.Show("完成");
            }
            else
            {
                MessageBox.Show("请输入正确的保存文件名");
            }
        }

        // 参数配置文件读取
        private void btn_readParm_fw_Click(object sender, RoutedEventArgs e)
        {

            if (CBFC.Text== "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC.Text;
                if (trend_FC_CK.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fc.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    qushu_fw.Text = "";
                    qushu1_fw.Text = "";
                    qushu2_fw.Text = "";
                    roll_fw.Text = "";
                    rollL_fw.Text = "";

                }
                else if (value_FC_CK.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    roll_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fw.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_FC_CK.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_FC_CK.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    delet_fc.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    change_fw.Text = "";
                    change1_fw.Text = "";
                    change2_fw.Text = "";

                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }
           
            
        }

        // 值分区的checked
        private void value_FC_CK_Checked(object sender, RoutedEventArgs e)
        {
            trend_FC_CK.IsChecked = false;
            change_fw.IsEnabled = false;
            change1_fw.IsEnabled = false;
            change2_fw.IsEnabled = false;
            qushu_fw.IsEnabled = true;
            qushu1_fw.IsEnabled = true;
            qushu2_fw.IsEnabled = true;
            roll_fw.IsEnabled = true;
            rollL_fw.IsEnabled = true;
            dengnumberDivide_FC_CK.IsEnabled = true;
            dengzhiDivide_FC_CK.IsEnabled = true;

        }

        private void trend_FC_CK_Checked(object sender, RoutedEventArgs e)
        {
            value_FC_CK.IsChecked = false;
            change_fw.IsEnabled = true;
            change1_fw.IsEnabled = true;
            change2_fw.IsEnabled = true;
            qushu_fw.IsEnabled = false;
            qushu1_fw.IsEnabled = false;
            qushu2_fw.IsEnabled = false;
            roll_fw.IsEnabled = false;
            rollL_fw.IsEnabled = false;

            dengnumberDivide_FC_CK.IsChecked = false;
            dengzhiDivide_FC_CK.IsChecked = false;
            dengnumberDivide_FC_CK.IsEnabled = false;
            dengzhiDivide_FC_CK.IsEnabled = false;


        }

        private void dengnumberDivide_FC_CK_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FC_CK.IsChecked = false;
        }

        private void dengzhiDivide_FC_CK_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FC_CK.IsChecked = false;
        }
        #endregion
      
        #region 单因素具体值预测

        //获取参数
        private void btn_check_fwv_Click(object sender, RoutedEventArgs e)
        {
            tongjistart.Text = "";
            tongjistart1.Text = "";
            tongjistart2.Text = "";
            K.Text = "";
            K1.Text = "";
            K2.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }


                // 平滑指数
                if (pinghua.Text == "")
                {
                    pinghua_a[0] = float.Parse(pinghua1.Text); //左区间
                    pinghua_a[1] = float.Parse(pinghua2.Text); //右区间
                    pinghua_l = float.Parse(pinghual.Text);  //步长
                }
                else
                {
                    pinghua_a[0] = float.Parse(pinghua.Text);
                    pinghua_a[1] = float.Parse(pinghua.Text);
                    pinghua_l = 1;
                }

                // 趋势指数
                if (trend.Text == "")
                {
                    trend_b[0] = float.Parse(trend1.Text);
                    trend_b[1] = float.Parse(trend2.Text);
                    trend_bl = float.Parse(trendL.Text); // 步长
                }
                else
                {
                    trend_b[0] = float.Parse(trend.Text);
                    trend_b[1] = float.Parse(trend.Text);
                    trend_bl = 1;
                }

                // 区间数
                if (qujianNum.Text == "")
                {
                    qujian_z[0] = Int32.Parse(qujianNum1.Text);
                    qujian_z[1] = Int32.Parse(qujianNum2.Text);

                }
                else
                {
                    qujian_z[0] = Int32.Parse(qujianNum.Text);
                    qujian_z[1] = Int32.Parse(qujianNum.Text);
                }

                //滚动次数T
                roll_t = Int32.Parse(rollNum.Text);
                roll_l = float.Parse(rolll.Text);
                // 统计时长M范围
                if (M.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2.Text); // 统计时长m2
                    stat_ml = Int32.Parse(M_ml.Text); // 统计时长步长

                }
                else
                {
                    statistics_m[0] = Int32.Parse(M.Text);
                    statistics_m[1] = Int32.Parse(M.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围（因素）
                if (tongjistart_fwv.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fwv.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fwv.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fwv.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fwv.Text);
                }


                // 关联步长（因素）
                if (K_fwv.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fwv.Text);
                    relevance_k[1] = Int32.Parse(K2_fwv.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fwv.Text);
                    relevance_k[1] = Int32.Parse(K_fwv.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fcv.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fcv.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fcv.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fcv.Text);
                    valid_n[1] = Int32.Parse(valid_n_fcv.Text);
                    //flagN = true;
                }
                if (value_FCv_CK.IsChecked == true)
                {
                    if (qushu_fwv.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fwv.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fwv.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fwv.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fwv.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fwv.Text);
                    rollb_fwl = float.Parse(rollL_fwv.Text);

                    if (dengnumberDivide_FCv_CK.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FCv_CK.IsChecked == true)
                {
                    if (change_fwv.Text == "")
                    {
                        cg[0] = float.Parse(change1_fwv.Text);
                        cg[1] = float.Parse(change2_fwv.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fwv.Text);
                        cg[1] = float.Parse(change_fwv.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }
                if (dengnumberDivide_TG_CK.IsChecked == true)
                {
                    DengNumFenQu = true;
                }
                else
                {
                    DengNumFenQu = false;
                }
                err[0] = -float.Parse(err1.Text);
                err[1] = float.Parse(err1.Text);

                deletBaifen = float.Parse(delet_fcv.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
        }
        // 计算
        private void btn_math_fwv_Click(object sender, RoutedEventArgs e)
        {
            SingleFactorValue singleFactorValue;
            if (value_FCv_CK.IsChecked == true)
            {
                singleFactorValue = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, rollb_fw, rollb_fwl, DengNumFenQU_FC, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }
            else
            {
                singleFactorValue = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen,cg, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }

            singleFactorValue.FWdataList = FWdataList1;
            singleFactorValue.listData = listData_factor1;
            singleFactorValue.SingleFactorCalculate();
            TestRight_fwv.Text = singleFactorValue.TestRight.ToString();
            VerifyRight_fwv.Text = singleFactorValue.VerifyRight.ToString();

            FactorName = CBFC.Text;
            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FCv_CK.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorValue.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue.test_index - singleFactorValue.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorValue.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(值分区).txt");
               
                if (singleFactorValue.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("训练集：" + (singleFactorValue.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue.test_index - singleFactorValue.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素区间数：" + singleFactorValue.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorValue.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorValue.roll_l.ToString());
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue.VerifyRight.ToString());
                sw.Close();
            }

        }
        
        private void value_FCv_CK_Checked(object sender, RoutedEventArgs e)
        {
            trend_FCv_CK.IsChecked = false;
            change_fwv.IsEnabled = false;
            change1_fwv.IsEnabled = false;
            change2_fwv.IsEnabled = false;
            qushu_fwv.IsEnabled = true;
            qushu1_fwv.IsEnabled = true;
            qushu2_fwv.IsEnabled = true;
            roll_fwv.IsEnabled = true;
            rollL_fwv.IsEnabled = true;
            dengnumberDivide_FCv_CK.IsEnabled = true;
            dengzhiDivide_FCv_CK.IsEnabled = true;
        }

        private void trend_FCv_CK_Checked(object sender, RoutedEventArgs e)
        {
            value_FCv_CK.IsChecked = false;
            change_fwv.IsEnabled = true;
            change1_fwv.IsEnabled = true;
            change2_fwv.IsEnabled = true;
            qushu_fwv.IsEnabled = false;
            qushu1_fwv.IsEnabled = false;
            qushu2_fwv.IsEnabled = false;
            roll_fwv.IsEnabled = false;
            rollL_fwv.IsEnabled = false;

            dengnumberDivide_FCv_CK.IsChecked = false;
            dengzhiDivide_FCv_CK.IsChecked = false;
            dengnumberDivide_FCv_CK.IsEnabled = false;
            dengzhiDivide_FCv_CK.IsEnabled = false;
        }

        private void dengnumberDivide_FCv_CK_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FCv_CK.IsChecked = false;
        }

        private void dengzhiDivide_FCv_CK_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FCv_CK.IsChecked = false;
        }
        
        // 读取参数
        private void btn_readParm_fwv_Click(object sender, RoutedEventArgs e)
        {
            if (CBFC.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC.Text;
                if (trend_FCv_CK.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName+"具体值预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fwv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                    qushu_fwv.Text = "";
                    qushu1_fwv.Text = "";
                    qushu2_fwv.Text = "";
                    roll_fwv.Text = "";
                    rollL_fwv.Text = "";


                }
                else if (value_FCv_CK.IsChecked == true)
                {
                    change_fwv.Text = "";
                    change1_fwv.Text = "";
                    change2_fwv.Text = "";
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    string strr = line.Trim().Split('：')[1];
                    if (strr == "等数量")
                    {
                        dengnumberDivide_FCv_CK.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_FCv_CK.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fwv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    roll_fwv.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fwv.Text = line.Trim().Split('：')[1];
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }
        }

        #endregion
        // 综合分析
        private void btn_ZongheFC_Click(object sender, RoutedEventArgs e)
        {
            int testqushi_n = 0;
            int yanzhengqushi_n = 0;
            float qushizhi = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                qushizhi = listData_factor1[i].YuceZhenZhi - listData_factor1[i - 1].YuceZhenZhi;
                if (qushizhi < -jieding_t && listData_factor1[i].LabelYuCe == lablelist[0])
                {
                    testqushi_n++;
                    listData_factor1[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor1[i].LabelYuCe == lablelist[2])
                {
                    testqushi_n++;
                    listData_factor1[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor1[i].LabelYuCe == lablelist[1])
                {
                    testqushi_n++;
                    listData_factor1[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor1[i].Consistency = "No";
                }
            }

            for (int i = test_index + 1; i < listData_factor1.Count; i++)
            {
                qushizhi = listData_factor1[i].YuceZhenZhi - listData_factor1[i - 1].YuceZhenZhi;
                if (qushizhi < -jieding_t && listData_factor1[i].LabelYuCe == lablelist[0])
                {
                    yanzhengqushi_n++;
                    listData_factor1[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor1[i].LabelYuCe == lablelist[2])
                {
                    yanzhengqushi_n++;
                    listData_factor1[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor1[i].LabelYuCe == lablelist[1])
                {
                    yanzhengqushi_n++;
                    listData_factor1[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor1[i].Consistency = "No";
                }
            }

            //  MessageBox.Show("训练集一致性：" + ((float)testqushi_n / (test_index - train_index)).ToString());
            // MessageBox.Show("验证集一致性：" + ((float)yanzhengqushi_n / (listData.Count - test_index - 1)).ToString());
            test_yizhiFC.Text = ((float)testqushi_n / (test_index - train_index)).ToString();
            yanzheng_yizhiFC.Text = ((float)yanzhengqushi_n / (listData_factor1.Count - test_index - 1)).ToString();

        }
        #endregion

        #region 单因素预测2

        #region 单因素趋势预测

        // 读取风温数据表
        private void btn_fw2_Click(object sender, RoutedEventArgs e)
        {
            // List<int[]> a = new List<int[]> {new int[]{1,2,3},new int[]{4,5,6}};
            //List<int[]> b = new List<int[]>();
            //a.ForEach(i => b.Add(i));
            // b[0][0] = 0;
            FWdataList2.Clear();
            listData_factor2.Clear();
            listDataStart.ForEach(i => listData_factor2.Add(new Datalist(i)));
            ListFengWen2.ItemsSource = listData_factor2;
            Microsoft.Win32.OpenFileDialog dialog = new OpenFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fileNameFW = dialog.FileName;
            //int indx = 0; 

            if (fileNameFW != "")
            {
                FileStream fileStream = new FileStream(fileNameFW, FileMode.Open, FileAccess.ReadWrite);
                HSSFWorkbook workbook = new HSSFWorkbook(fileStream); //获取excle数据
                ISheet sheet = workbook.GetSheetAt(0); //根据表名获取表
                IRow row;

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    if (row != null)
                    {

                        FactorData dl = new FactorData();
                        dl.Id = i;
                        try { dl.Time = Convert.ToDateTime(row.GetCell(1).ToString().Replace('/', '-')); }
                        catch
                        {
                            string[] time_str = row.GetCell(1).ToString().Split('/');
                            string new_time = "20" + time_str[2] + "-" + time_str[0] + "-" + time_str[1];
                            dl.Time = Convert.ToDateTime(new_time);
                        }
                        dl.Temperature = float.Parse(row.GetCell(2).ToString());
                        FWdataList2.Add(dl);

                    }
                    else
                    {
                        break;
                    }
                }
                workbook.Close();
                fileStream.Close();

            }

        }

        // 获取参数设置
        private void btn_check_fw2_Click(object sender, RoutedEventArgs e)
        {

            K_t.Text = "";
            K1_t.Text = "";
            K2_t.Text = "";
            tongjistart_t.Text = "";
            tongjistart1_t.Text = "";
            tongjistart2_t.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号
                jieding_t = float.Parse(Jieding_t.Text); // 界定

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }

                // 统计时长M范围
                if (M_t.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1_t.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2_t.Text); // 统计时长m2
                    stat_ml = Int32.Parse(ML_t.Text); // 统计时长步长
                }
                else
                {
                    statistics_m[0] = Int32.Parse(M_t.Text);
                    statistics_m[1] = Int32.Parse(M_t.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围
                if (tongjistart_fw2.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fw2.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fw2.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fw2.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fw2.Text);
                }


                // 关联步长
                if (K_fw2.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fw2.Text);
                    relevance_k[1] = Int32.Parse(K2_fw2.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fw2.Text);
                    relevance_k[1] = Int32.Parse(K_fw2.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fc2.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fc2.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fc2.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fc2.Text);
                    valid_n[1] = Int32.Parse(valid_n_fc2.Text);
                    //flagN = true;
                }
                if (value_FC_CK2.IsChecked == true)
                {
                    if (qushu_fw2.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fw2.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fw2.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fw2.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fw2.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fw2.Text);
                    rollb_fwl = float.Parse(rollL_fw2.Text);

                    if (dengnumberDivide_FC_CK2.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FC_CK2.IsChecked == true)
                {
                    if (change_fw2.Text == "")
                    {
                        cg[0] = float.Parse(change1_fw2.Text);
                        cg[1] = float.Parse(change2_fw2.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fw2.Text);
                        cg[1] = float.Parse(change_fw2.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }

                FactorName = CBFC2.Text;
                deletBaifen = float.Parse(delet_fc2.Text);



            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
            //Lable();
            //MessageBox.Show(train_index_t.ToString());
            //MessageBox.Show(test_index_t.ToString());
            //MessageBox.Show(jieding_t.ToString());
            //MessageBox.Show(statistics_m_t[0].ToString());
            //MessageBox.Show(statistics_m_t[1].ToString());
            //MessageBox.Show(stat_ml_t.ToString());
            //MessageBox.Show(stat_start_t[0].ToString());
            //MessageBox.Show(stat_start_t[1].ToString());
            //MessageBox.Show(relevance_k_t[0].ToString());
            //MessageBox.Show(relevance_k_t[1].ToString());
            //MessageBox.Show(valid_n_t[0].ToString());
            //MessageBox.Show(valid_n_t[1].ToString());
            //MessageBox.Show(fwQunjian[0].ToString());
            //MessageBox.Show(fwQunjian[1].ToString());
        }

        // 计算
        private void btn_math_fw2_Click(object sender, RoutedEventArgs e)
        {

            SingleFactorTrend singleFactorTrend2;
            if (value_FC_CK2.IsChecked == true)
            {
                singleFactorTrend2 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, jieding_t, rollb_fw, rollb_fwl, DengNumFenQU_FC);

            }
            else
            {
                singleFactorTrend2 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, jieding_t);

            }
            singleFactorTrend2.FWdataList = FWdataList2;
            singleFactorTrend2.listData = listData_factor2;
            singleFactorTrend2.SingleFactorCalculate(); // 计算
            TestRight_fw2.Text = singleFactorTrend2.TestRight.ToString();// 测试集命中率
            VerifyRight_fw2.Text = singleFactorTrend2.VerifyRight.ToString(); // 验证集命中率

            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FC_CK2.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorTrend2.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend2.test_index - singleFactorTrend2.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend2.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend2.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend2.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend2.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend2.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorTrend2.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend2.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend2.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend2.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(值分区).txt");
                sw.Write("训练集：" + (singleFactorTrend2.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend2.test_index - singleFactorTrend2.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend2.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend2.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend2.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend2.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend2.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素分区数：" + singleFactorTrend2.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorTrend2.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorTrend2.roll_l.ToString());
                sw.WriteLine();
                if (singleFactorTrend2.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend2.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend2.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend2.VerifyRight.ToString());
                sw.Close();
            }

        }

        // 写入excel
        private void btn_save_fw2_Click(object sender, RoutedEventArgs e)
        {
            #region 写入excel
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fname = dialog.FileName;
            if (fname != "")
            {
                // FileStream fileStream2 = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                HSSFWorkbook workbook1 = new HSSFWorkbook(); //获取excle数据
                ISheet sheet1;
                try
                {
                    sheet1 = workbook1.CreateSheet("Predict"); //根据表名第一个表
                }
                catch
                {
                    sheet1 = workbook1.GetSheet("Predict");
                }


                IRow row = sheet1.CreateRow(0);
                ICell headCell = row.CreateCell(0);
                headCell.SetCellValue("序号");
                headCell = row.CreateCell(1);
                headCell.SetCellValue("出铁时间");
                //headCell = row.CreateCell(2);
                //headCell.SetCellValue("出铁结束时间");
                headCell = row.CreateCell(2);
                headCell.SetCellValue("铁水中SI含量");
                headCell = row.CreateCell(3);
                headCell.SetCellValue("趋势预测值");
                headCell = row.CreateCell(4);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(5);
                headCell.SetCellValue("具体值预测");
                headCell = row.CreateCell(6);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(7);
                headCell.SetCellValue("一致性");

                HSSFCell[] Cell = new HSSFCell[8];
                int excRow = 1; // 行号
                //int RowNum =sheet1.LastRowNum;
                for (int j = 0; j < listData_factor2.Count; j++)
                {
                    row = sheet1.CreateRow(excRow);


                    for (int i = 0; i <= 7; i++)
                    {
                        Cell[i] = (HSSFCell)row.CreateCell(i);
                    }

                    Cell[0].SetCellValue(excRow);
                    Cell[1].SetCellValue(listData_factor2[j].POPtime);
                    //Cell[2].SetCellValue(listData[j].POPfinish);
                    Cell[2].SetCellValue(listData_factor2[j].Rhmsi);

                    if (j > train_index)
                    {
                        Cell[3].SetCellValue(listData_factor2[j].LabelYuCe);
                        Cell[4].SetCellValue(listData_factor2[j].Trust_t);
                        Cell[5].SetCellValue(listData_factor2[j].YuceZhenZhi);
                        Cell[6].SetCellValue(listData_factor2[j].Trust_v);
                        Cell[7].SetCellValue(listData_factor2[j].Consistency);
                    }
                    else
                    {
                        Cell[3].SetCellValue("");
                        Cell[4].SetCellValue("");
                        Cell[5].SetCellValue("");
                        Cell[6].SetCellValue("");
                        Cell[7].SetCellValue("");


                    }

                    excRow++;
                }


            #endregion
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
                workbook1.Write(fs);
                //fileStream2.Close();
                fs.Close();
                workbook1.Close();

                MessageBox.Show("完成");
            }
            else
            {
                MessageBox.Show("请输入正确的保存文件名");
            }
        }

        // 参数配置文件读取
        private void btn_readParm_fw2_Click(object sender, RoutedEventArgs e)
        {

            if (CBFC2.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC2.Text;
                if (trend_FC_CK2.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fc2.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    qushu_fw2.Text = "";
                    qushu1_fw2.Text = "";
                    qushu2_fw2.Text = "";
                    roll_fw2.Text = "";
                    rollL_fw2.Text = "";

                }
                else if (value_FC_CK2.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    roll_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fw2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_FC_CK2.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_FC_CK2.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    delet_fc2.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    change_fw2.Text = "";
                    change1_fw2.Text = "";
                    change2_fw2.Text = "";

                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }


        }

        // 值分区的checked
        private void value_FC_CK2_Checked(object sender, RoutedEventArgs e)
        {
            trend_FC_CK2.IsChecked = false;
            change_fw2.IsEnabled = false;
            change1_fw2.IsEnabled = false;
            change2_fw2.IsEnabled = false;
            qushu_fw2.IsEnabled = true;
            qushu1_fw2.IsEnabled = true;
            qushu2_fw2.IsEnabled = true;
            roll_fw2.IsEnabled = true;
            rollL_fw2.IsEnabled = true;
            dengnumberDivide_FC_CK2.IsEnabled = true;
            dengzhiDivide_FC_CK2.IsEnabled = true;

        }

        private void trend_FC_CK2_Checked(object sender, RoutedEventArgs e)
        {
            value_FC_CK2.IsChecked = false;
            change_fw2.IsEnabled = true;
            change1_fw2.IsEnabled = true;
            change2_fw2.IsEnabled = true;
            qushu_fw2.IsEnabled = false;
            qushu1_fw2.IsEnabled = false;
            qushu2_fw2.IsEnabled = false;
            roll_fw2.IsEnabled = false;
            rollL_fw2.IsEnabled = false;

            dengnumberDivide_FC_CK2.IsChecked = false;
            dengzhiDivide_FC_CK2.IsChecked = false;
            dengnumberDivide_FC_CK2.IsEnabled = false;
            dengzhiDivide_FC_CK2.IsEnabled = false;


        }

        private void dengnumberDivide_FC_CK2_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FC_CK2.IsChecked = false;
        }

        private void dengzhiDivide_FC_CK2_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FC_CK2.IsChecked = false;
        }
        #endregion

        #region 单因素具体值预测

        //获取参数
        private void btn_check_fwv2_Click(object sender, RoutedEventArgs e)
        {
            tongjistart.Text = "";
            tongjistart1.Text = "";
            tongjistart2.Text = "";
            K.Text = "";
            K1.Text = "";
            K2.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }


                // 平滑指数
                if (pinghua.Text == "")
                {
                    pinghua_a[0] = float.Parse(pinghua1.Text); //左区间
                    pinghua_a[1] = float.Parse(pinghua2.Text); //右区间
                    pinghua_l = float.Parse(pinghual.Text);  //步长
                }
                else
                {
                    pinghua_a[0] = float.Parse(pinghua.Text);
                    pinghua_a[1] = float.Parse(pinghua.Text);
                    pinghua_l = 1;
                }

                // 趋势指数
                if (trend.Text == "")
                {
                    trend_b[0] = float.Parse(trend1.Text);
                    trend_b[1] = float.Parse(trend2.Text);
                    trend_bl = float.Parse(trendL.Text); // 步长
                }
                else
                {
                    trend_b[0] = float.Parse(trend.Text);
                    trend_b[1] = float.Parse(trend.Text);
                    trend_bl = 1;
                }

                // 区间数
                if (qujianNum.Text == "")
                {
                    qujian_z[0] = Int32.Parse(qujianNum1.Text);
                    qujian_z[1] = Int32.Parse(qujianNum2.Text);

                }
                else
                {
                    qujian_z[0] = Int32.Parse(qujianNum.Text);
                    qujian_z[1] = Int32.Parse(qujianNum.Text);
                }

                //滚动次数T
                roll_t = Int32.Parse(rollNum.Text);
                roll_l = float.Parse(rolll.Text);
                // 统计时长M范围
                if (M.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2.Text); // 统计时长m2
                    stat_ml = Int32.Parse(M_ml.Text); // 统计时长步长

                }
                else
                {
                    statistics_m[0] = Int32.Parse(M.Text);
                    statistics_m[1] = Int32.Parse(M.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围（因素）
                if (tongjistart_fwv2.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fwv2.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fwv2.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fwv2.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fwv2.Text);
                }


                // 关联步长（因素）
                if (K_fwv2.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fwv2.Text);
                    relevance_k[1] = Int32.Parse(K2_fwv2.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fwv2.Text);
                    relevance_k[1] = Int32.Parse(K_fwv2.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fcv2.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fcv2.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fcv2.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fcv2.Text);
                    valid_n[1] = Int32.Parse(valid_n_fcv2.Text);
                    //flagN = true;
                }
                if (value_FCv_CK2.IsChecked == true)
                {
                    if (qushu_fwv2.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fwv2.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fwv2.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fwv2.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fwv2.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fwv2.Text);
                    rollb_fwl = float.Parse(rollL_fwv2.Text);

                    if (dengnumberDivide_FCv_CK2.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FCv_CK2.IsChecked == true)
                {
                    if (change_fwv2.Text == "")
                    {
                        cg[0] = float.Parse(change1_fwv2.Text);
                        cg[1] = float.Parse(change2_fwv2.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fwv2.Text);
                        cg[1] = float.Parse(change_fwv2.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }
                if (dengnumberDivide_TG_CK.IsChecked == true)
                {
                    DengNumFenQu = true;
                }
                else
                {
                    DengNumFenQu = false;
                }
                err[0] = -float.Parse(err1.Text);
                err[1] = float.Parse(err1.Text);

                deletBaifen = float.Parse(delet_fcv2.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
        }
        // 计算
        private void btn_math_fwv2_Click(object sender, RoutedEventArgs e)
        {
            SingleFactorValue singleFactorValue2;
            if (value_FCv_CK2.IsChecked == true)
            {
                singleFactorValue2 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, rollb_fw, rollb_fwl, DengNumFenQU_FC, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }
            else
            {
                singleFactorValue2 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }

            singleFactorValue2.FWdataList = FWdataList2;
            singleFactorValue2.listData = listData_factor2;
            singleFactorValue2.SingleFactorCalculate();
            TestRight_fwv2.Text = singleFactorValue2.TestRight.ToString();
            VerifyRight_fwv2.Text = singleFactorValue2.VerifyRight.ToString();

            FactorName = CBFC2.Text;
            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FCv_CK2.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorValue2.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue2.test_index - singleFactorValue2.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue2.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue2.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue2.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue2.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorValue2.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue2.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue2.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue2.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue2.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue2.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue2.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue2.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue2.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(值分区).txt");

                if (singleFactorValue2.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("训练集：" + (singleFactorValue2.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue2.test_index - singleFactorValue2.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue2.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue2.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue2.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue2.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素区间数：" + singleFactorValue2.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue2.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue2.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue2.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue2.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue2.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue2.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorValue2.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorValue2.roll_l.ToString());
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue2.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue2.VerifyRight.ToString());
                sw.Close();
            }

        }

        private void value_FCv_CK2_Checked(object sender, RoutedEventArgs e)
        {
            trend_FCv_CK2.IsChecked = false;
            change_fwv2.IsEnabled = false;
            change1_fwv2.IsEnabled = false;
            change2_fwv2.IsEnabled = false;
            qushu_fwv2.IsEnabled = true;
            qushu1_fwv2.IsEnabled = true;
            qushu2_fwv2.IsEnabled = true;
            roll_fwv2.IsEnabled = true;
            rollL_fwv2.IsEnabled = true;
            dengnumberDivide_FCv_CK2.IsEnabled = true;
            dengzhiDivide_FCv_CK2.IsEnabled = true;
        }

        private void trend_FCv_CK2_Checked(object sender, RoutedEventArgs e)
        {
            value_FCv_CK2.IsChecked = false;
            change_fwv2.IsEnabled = true;
            change1_fwv2.IsEnabled = true;
            change2_fwv2.IsEnabled = true;
            qushu_fwv2.IsEnabled = false;
            qushu1_fwv2.IsEnabled = false;
            qushu2_fwv2.IsEnabled = false;
            roll_fwv2.IsEnabled = false;
            rollL_fwv2.IsEnabled = false;

            dengnumberDivide_FCv_CK2.IsChecked = false;
            dengzhiDivide_FCv_CK2.IsChecked = false;
            dengnumberDivide_FCv_CK2.IsEnabled = false;
            dengzhiDivide_FCv_CK2.IsEnabled = false;
        }

        private void dengnumberDivide_FCv_CK2_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FCv_CK2.IsChecked = false;
        }

        private void dengzhiDivide_FCv_CK2_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FCv_CK2.IsChecked = false;
        }

        // 读取参数
        private void btn_readParm_fwv2_Click(object sender, RoutedEventArgs e)
        {
            if (CBFC2.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC2.Text;
                if (trend_FCv_CK2.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fwv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                    qushu_fwv2.Text = "";
                    qushu1_fwv2.Text = "";
                    qushu2_fwv2.Text = "";
                    roll_fwv2.Text = "";
                    rollL_fwv2.Text = "";


                }
                else if (value_FCv_CK2.IsChecked == true)
                {
                    change_fwv2.Text = "";
                    change1_fwv2.Text = "";
                    change2_fwv2.Text = "";
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    string strr = line.Trim().Split('：')[1];
                    if (strr == "等数量")
                    {
                        dengnumberDivide_FCv_CK2.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_FCv_CK2.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fwv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    roll_fwv2.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fwv2.Text = line.Trim().Split('：')[1];
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }
        }

        #endregion
        // 综合分析
        private void btn_ZongheFC2_Click(object sender, RoutedEventArgs e)
        {
            int testqushi_n = 0;
            int yanzhengqushi_n = 0;
            float qushizhi = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                qushizhi = listData_factor2[i].YuceZhenZhi - listData_factor2[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor2[i].LabelYuCe == lablelist[0])
                {
                    testqushi_n++;
                    listData_factor2[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor2[i].LabelYuCe == lablelist[2])
                {
                    testqushi_n++;
                    listData_factor2[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor2[i].LabelYuCe == lablelist[1])
                {
                    testqushi_n++;
                    listData_factor2[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor2[i].Consistency = "No";
                }
            }

            for (int i = test_index + 1; i < listData_factor2.Count; i++)
            {
                qushizhi = listData_factor2[i].YuceZhenZhi - listData_factor2[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor2[i].LabelYuCe == lablelist[0])
                {
                    yanzhengqushi_n++;
                    listData_factor2[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor2[i].LabelYuCe == lablelist[2])
                {
                    yanzhengqushi_n++;
                    listData_factor2[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor2[i].LabelYuCe == lablelist[1])
                {
                    yanzhengqushi_n++;
                    listData_factor2[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor2[i].Consistency = "No";
                }
            }

            //  MessageBox.Show("训练集一致性：" + ((float)testqushi_n / (test_index - train_index)).ToString());
            // MessageBox.Show("验证集一致性：" + ((float)yanzhengqushi_n / (listData.Count - test_index - 1)).ToString());
            test_yizhiFC2.Text = ((float)testqushi_n / (test_index - train_index)).ToString();
            yanzheng_yizhiFC2.Text = ((float)yanzhengqushi_n / (listData_factor2.Count - test_index - 1)).ToString();

        }
        #endregion

        #region 单因素预测3

        #region 单因素趋势预测

        // 读取风温数据表
        private void btn_fw3_Click(object sender, RoutedEventArgs e)
        {
            // List<int[]> a = new List<int[]> {new int[]{1,2,3},new int[]{4,5,6}};
            //List<int[]> b = new List<int[]>();
            //a.ForEach(i => b.Add(i));
            // b[0][0] = 0;
            FWdataList3.Clear();
            listData_factor3.Clear();
            listDataStart.ForEach(i => listData_factor3.Add(new Datalist(i)));
            ListFengWen3.ItemsSource = listData_factor3;
            Microsoft.Win32.OpenFileDialog dialog = new OpenFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fileNameFW = dialog.FileName;
            //int indx = 0; 

            if (fileNameFW != "")
            {
                FileStream fileStream = new FileStream(fileNameFW, FileMode.Open, FileAccess.ReadWrite);
                HSSFWorkbook workbook = new HSSFWorkbook(fileStream); //获取excle数据
                ISheet sheet = workbook.GetSheetAt(0); //根据表名获取表
                IRow row;

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    if (row != null)
                    {

                        FactorData dl = new FactorData();
                        dl.Id = i;
                        try { dl.Time = Convert.ToDateTime(row.GetCell(1).ToString().Replace('/', '-')); }
                        catch
                        {
                            string[] time_str = row.GetCell(1).ToString().Split('/');
                            string new_time = "20" + time_str[2] + "-" + time_str[0] + "-" + time_str[1];
                            dl.Time = Convert.ToDateTime(new_time);
                        }
                        dl.Temperature = float.Parse(row.GetCell(2).ToString());
                        FWdataList3.Add(dl);

                    }
                    else
                    {
                        break;
                    }
                }
                workbook.Close();
                fileStream.Close();

            }

        }

        // 获取参数设置
        private void btn_check_fw3_Click(object sender, RoutedEventArgs e)
        {

            K_t.Text = "";
            K1_t.Text = "";
            K2_t.Text = "";
            tongjistart_t.Text = "";
            tongjistart1_t.Text = "";
            tongjistart2_t.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号
                jieding_t = float.Parse(Jieding_t.Text); // 界定

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }

                // 统计时长M范围
                if (M_t.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1_t.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2_t.Text); // 统计时长m2
                    stat_ml = Int32.Parse(ML_t.Text); // 统计时长步长
                }
                else
                {
                    statistics_m[0] = Int32.Parse(M_t.Text);
                    statistics_m[1] = Int32.Parse(M_t.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围
                if (tongjistart_fw3.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fw3.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fw3.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fw3.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fw3.Text);
                }


                // 关联步长
                if (K_fw3.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fw3.Text);
                    relevance_k[1] = Int32.Parse(K2_fw3.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fw3.Text);
                    relevance_k[1] = Int32.Parse(K_fw3.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fc3.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fc3.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fc3.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fc3.Text);
                    valid_n[1] = Int32.Parse(valid_n_fc3.Text);
                    //flagN = true;
                }
                if (value_FC_CK3.IsChecked == true)
                {
                    if (qushu_fw3.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fw3.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fw3.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fw3.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fw3.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fw3.Text);
                    rollb_fwl = float.Parse(rollL_fw3.Text);

                    if (dengnumberDivide_FC_CK3.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FC_CK3.IsChecked == true)
                {
                    if (change_fw3.Text == "")
                    {
                        cg[0] = float.Parse(change1_fw3.Text);
                        cg[1] = float.Parse(change2_fw3.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fw3.Text);
                        cg[1] = float.Parse(change_fw3.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }

                FactorName = CBFC3.Text;
                deletBaifen = float.Parse(delet_fc3.Text);



            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
            //Lable();
            //MessageBox.Show(train_index_t.ToString());
            //MessageBox.Show(test_index_t.ToString());
            //MessageBox.Show(jieding_t.ToString());
            //MessageBox.Show(statistics_m_t[0].ToString());
            //MessageBox.Show(statistics_m_t[1].ToString());
            //MessageBox.Show(stat_ml_t.ToString());
            //MessageBox.Show(stat_start_t[0].ToString());
            //MessageBox.Show(stat_start_t[1].ToString());
            //MessageBox.Show(relevance_k_t[0].ToString());
            //MessageBox.Show(relevance_k_t[1].ToString());
            //MessageBox.Show(valid_n_t[0].ToString());
            //MessageBox.Show(valid_n_t[1].ToString());
            //MessageBox.Show(fwQunjian[0].ToString());
            //MessageBox.Show(fwQunjian[1].ToString());
        }

        // 计算
        private void btn_math_fw3_Click(object sender, RoutedEventArgs e)
        {

            SingleFactorTrend singleFactorTrend3;
            if (value_FC_CK3.IsChecked == true)
            {
                singleFactorTrend3 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, jieding_t, rollb_fw, rollb_fwl, DengNumFenQU_FC);

            }
            else
            {
                singleFactorTrend3 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, jieding_t);

            }
            singleFactorTrend3.FWdataList = FWdataList3;
            singleFactorTrend3.listData = listData_factor3;
            singleFactorTrend3.SingleFactorCalculate();
            TestRight_fw3.Text = singleFactorTrend3.TestRight.ToString();
            VerifyRight_fw3.Text = singleFactorTrend3.VerifyRight.ToString();

            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FC_CK3.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorTrend3.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend3.test_index - singleFactorTrend3.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend3.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend3.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend3.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend3.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend3.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorTrend3.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend3.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend3.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend3.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(值分区).txt");
                sw.Write("训练集：" + (singleFactorTrend3.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend3.test_index - singleFactorTrend3.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend3.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend3.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend3.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend3.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend3.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素分区数：" + singleFactorTrend3.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorTrend3.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorTrend3.roll_l.ToString());
                sw.WriteLine();
                if (singleFactorTrend3.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend3.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend3.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend3.VerifyRight.ToString());
                sw.Close();
            }

        }

        // 写入excel
        private void btn_save_fw3_Click(object sender, RoutedEventArgs e)
        {
            #region 写入excel
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fname = dialog.FileName;
            if (fname != "")
            {
                // FileStream fileStream2 = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                HSSFWorkbook workbook1 = new HSSFWorkbook(); //获取excle数据
                ISheet sheet1;
                try
                {
                    sheet1 = workbook1.CreateSheet("Predict"); //根据表名第一个表
                }
                catch
                {
                    sheet1 = workbook1.GetSheet("Predict");
                }


                IRow row = sheet1.CreateRow(0);
                ICell headCell = row.CreateCell(0);
                headCell.SetCellValue("序号");
                headCell = row.CreateCell(1);
                headCell.SetCellValue("出铁时间");
                //headCell = row.CreateCell(2);
                //headCell.SetCellValue("出铁结束时间");
                headCell = row.CreateCell(2);
                headCell.SetCellValue("铁水中SI含量");
                headCell = row.CreateCell(3);
                headCell.SetCellValue("趋势预测值");
                headCell = row.CreateCell(4);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(5);
                headCell.SetCellValue("具体值预测");
                headCell = row.CreateCell(6);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(7);
                headCell.SetCellValue("一致性");

                HSSFCell[] Cell = new HSSFCell[8];
                int excRow = 1; // 行号
                //int RowNum =sheet1.LastRowNum;
                for (int j = 0; j < listData_factor3.Count; j++)
                {
                    row = sheet1.CreateRow(excRow);


                    for (int i = 0; i <= 7; i++)
                    {
                        Cell[i] = (HSSFCell)row.CreateCell(i);
                    }

                    Cell[0].SetCellValue(excRow);
                    Cell[1].SetCellValue(listData_factor3[j].POPtime);
                    //Cell[2].SetCellValue(listData[j].POPfinish);
                    Cell[2].SetCellValue(listData_factor3[j].Rhmsi);

                    if (j > train_index)
                    {
                        Cell[3].SetCellValue(listData_factor3[j].LabelYuCe);
                        Cell[4].SetCellValue(listData_factor3[j].Trust_t);
                        Cell[5].SetCellValue(listData_factor3[j].YuceZhenZhi);
                        Cell[6].SetCellValue(listData_factor3[j].Trust_v);
                        Cell[7].SetCellValue(listData_factor3[j].Consistency);
                    }
                    else
                    {
                        Cell[3].SetCellValue("");
                        Cell[4].SetCellValue("");
                        Cell[5].SetCellValue("");
                        Cell[6].SetCellValue("");
                        Cell[7].SetCellValue("");


                    }

                    excRow++;
                }


            #endregion
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
                workbook1.Write(fs);
                //fileStream2.Close();
                fs.Close();
                workbook1.Close();

                MessageBox.Show("完成");
            }
            else
            {
                MessageBox.Show("请输入正确的保存文件名");
            }
        }

        // 参数配置文件读取
        private void btn_readParm_fw3_Click(object sender, RoutedEventArgs e)
        {

            if (CBFC3.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC3.Text;
                if (trend_FC_CK3.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fc3.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    qushu_fw3.Text = "";
                    qushu1_fw3.Text = "";
                    qushu2_fw3.Text = "";
                    roll_fw3.Text = "";
                    rollL_fw3.Text = "";

                }
                else if (value_FC_CK3.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    roll_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fw3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_FC_CK3.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_FC_CK3.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    delet_fc3.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    change_fw3.Text = "";
                    change1_fw3.Text = "";
                    change2_fw3.Text = "";

                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }


        }

        // 值分区的checked
        private void value_FC_CK3_Checked(object sender, RoutedEventArgs e)
        {
            trend_FC_CK3.IsChecked = false;
            change_fw3.IsEnabled = false;
            change1_fw3.IsEnabled = false;
            change2_fw3.IsEnabled = false;
            qushu_fw3.IsEnabled = true;
            qushu1_fw3.IsEnabled = true;
            qushu2_fw3.IsEnabled = true;
            roll_fw3.IsEnabled = true;
            rollL_fw3.IsEnabled = true;
            dengnumberDivide_FC_CK3.IsEnabled = true;
            dengzhiDivide_FC_CK3.IsEnabled = true;

        }

        private void trend_FC_CK3_Checked(object sender, RoutedEventArgs e)
        {
            value_FC_CK3.IsChecked = false;
            change_fw3.IsEnabled = true;
            change1_fw3.IsEnabled = true;
            change2_fw3.IsEnabled = true;
            qushu_fw3.IsEnabled = false;
            qushu1_fw3.IsEnabled = false;
            qushu2_fw3.IsEnabled = false;
            roll_fw3.IsEnabled = false;
            rollL_fw3.IsEnabled = false;

            dengnumberDivide_FC_CK3.IsChecked = false;
            dengzhiDivide_FC_CK3.IsChecked = false;
            dengnumberDivide_FC_CK3.IsEnabled = false;
            dengzhiDivide_FC_CK3.IsEnabled = false;


        }

        private void dengnumberDivide_FC_CK3_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FC_CK3.IsChecked = false;
        }

        private void dengzhiDivide_FC_CK3_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FC_CK3.IsChecked = false;
        }
        #endregion

        #region 单因素具体值预测

        //获取参数
        private void btn_check_fwv3_Click(object sender, RoutedEventArgs e)
        {
            tongjistart.Text = "";
            tongjistart1.Text = "";
            tongjistart2.Text = "";
            K.Text = "";
            K1.Text = "";
            K2.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }


                // 平滑指数
                if (pinghua.Text == "")
                {
                    pinghua_a[0] = float.Parse(pinghua1.Text); //左区间
                    pinghua_a[1] = float.Parse(pinghua2.Text); //右区间
                    pinghua_l = float.Parse(pinghual.Text);  //步长
                }
                else
                {
                    pinghua_a[0] = float.Parse(pinghua.Text);
                    pinghua_a[1] = float.Parse(pinghua.Text);
                    pinghua_l = 1;
                }

                // 趋势指数
                if (trend.Text == "")
                {
                    trend_b[0] = float.Parse(trend1.Text);
                    trend_b[1] = float.Parse(trend2.Text);
                    trend_bl = float.Parse(trendL.Text); // 步长
                }
                else
                {
                    trend_b[0] = float.Parse(trend.Text);
                    trend_b[1] = float.Parse(trend.Text);
                    trend_bl = 1;
                }

                // 区间数
                if (qujianNum.Text == "")
                {
                    qujian_z[0] = Int32.Parse(qujianNum1.Text);
                    qujian_z[1] = Int32.Parse(qujianNum2.Text);

                }
                else
                {
                    qujian_z[0] = Int32.Parse(qujianNum.Text);
                    qujian_z[1] = Int32.Parse(qujianNum.Text);
                }

                //滚动次数T
                roll_t = Int32.Parse(rollNum.Text);
                roll_l = float.Parse(rolll.Text);
                // 统计时长M范围
                if (M.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2.Text); // 统计时长m2
                    stat_ml = Int32.Parse(M_ml.Text); // 统计时长步长

                }
                else
                {
                    statistics_m[0] = Int32.Parse(M.Text);
                    statistics_m[1] = Int32.Parse(M.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围（因素）
                if (tongjistart_fwv3.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fwv3.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fwv3.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fwv3.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fwv3.Text);
                }


                // 关联步长（因素）
                if (K_fwv3.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fwv3.Text);
                    relevance_k[1] = Int32.Parse(K2_fwv3.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fwv3.Text);
                    relevance_k[1] = Int32.Parse(K_fwv3.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fcv3.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fcv3.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fcv3.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fcv3.Text);
                    valid_n[1] = Int32.Parse(valid_n_fcv3.Text);
                    //flagN = true;
                }
                if (value_FCv_CK3.IsChecked == true)
                {
                    if (qushu_fwv3.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fwv3.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fwv3.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fwv3.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fwv3.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fwv3.Text);
                    rollb_fwl = float.Parse(rollL_fwv3.Text);

                    if (dengnumberDivide_FCv_CK3.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FCv_CK3.IsChecked == true)
                {
                    if (change_fwv3.Text == "")
                    {
                        cg[0] = float.Parse(change1_fwv3.Text);
                        cg[1] = float.Parse(change2_fwv3.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fwv3.Text);
                        cg[1] = float.Parse(change_fwv3.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }
                if (dengnumberDivide_TG_CK.IsChecked == true)
                {
                    DengNumFenQu = true;
                }
                else
                {
                    DengNumFenQu = false;
                }
                err[0] = -float.Parse(err1.Text);
                err[1] = float.Parse(err1.Text);

                deletBaifen = float.Parse(delet_fcv3.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
        }
        // 计算
        private void btn_math_fwv3_Click(object sender, RoutedEventArgs e)
        {
            SingleFactorValue singleFactorValue3;
            if (value_FCv_CK3.IsChecked == true)
            {
                singleFactorValue3 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, rollb_fw, rollb_fwl, DengNumFenQU_FC, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }
            else
            {
                singleFactorValue3 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }

            singleFactorValue3.FWdataList = FWdataList3;
            singleFactorValue3.listData = listData_factor3;
            singleFactorValue3.SingleFactorCalculate();
            TestRight_fwv3.Text = singleFactorValue3.TestRight.ToString();
            VerifyRight_fwv3.Text = singleFactorValue3.VerifyRight.ToString();

            FactorName = CBFC3.Text;
            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FCv_CK3.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorValue3.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue3.test_index - singleFactorValue3.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue3.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue3.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue3.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue3.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorValue3.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue3.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue3.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue3.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue3.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue3.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue3.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue3.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue3.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(值分区).txt");

                if (singleFactorValue3.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("训练集：" + (singleFactorValue3.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue3.test_index - singleFactorValue3.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue3.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue3.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue3.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue3.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素区间数：" + singleFactorValue3.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue3.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue3.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue3.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue3.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue3.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue3.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorValue3.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorValue3.roll_l.ToString());
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue3.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue3.VerifyRight.ToString());
                sw.Close();
            }

        }

        private void value_FCv_CK3_Checked(object sender, RoutedEventArgs e)
        {
            trend_FCv_CK3.IsChecked = false;
            change_fwv3.IsEnabled = false;
            change1_fwv3.IsEnabled = false;
            change2_fwv3.IsEnabled = false;
            qushu_fwv3.IsEnabled = true;
            qushu1_fwv3.IsEnabled = true;
            qushu2_fwv3.IsEnabled = true;
            roll_fwv3.IsEnabled = true;
            rollL_fwv3.IsEnabled = true;
            dengnumberDivide_FCv_CK3.IsEnabled = true;
            dengzhiDivide_FCv_CK3.IsEnabled = true;
        }

        private void trend_FCv_CK3_Checked(object sender, RoutedEventArgs e)
        {
            value_FCv_CK3.IsChecked = false;
            change_fwv3.IsEnabled = true;
            change1_fwv3.IsEnabled = true;
            change2_fwv3.IsEnabled = true;
            qushu_fwv3.IsEnabled = false;
            qushu1_fwv3.IsEnabled = false;
            qushu2_fwv3.IsEnabled = false;
            roll_fwv3.IsEnabled = false;
            rollL_fwv3.IsEnabled = false;

            dengnumberDivide_FCv_CK3.IsChecked = false;
            dengzhiDivide_FCv_CK3.IsChecked = false;
            dengnumberDivide_FCv_CK3.IsEnabled = false;
            dengzhiDivide_FCv_CK3.IsEnabled = false;
        }

        private void dengnumberDivide_FCv_CK3_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FCv_CK3.IsChecked = false;
        }

        private void dengzhiDivide_FCv_CK3_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FCv_CK3.IsChecked = false;
        }

        // 读取参数
        private void btn_readParm_fwv3_Click(object sender, RoutedEventArgs e)
        {
            if (CBFC3.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC3.Text;
                if (trend_FCv_CK3.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fwv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                    qushu_fwv3.Text = "";
                    qushu1_fwv3.Text = "";
                    qushu2_fwv3.Text = "";
                    roll_fwv3.Text = "";
                    rollL_fwv3.Text = "";


                }
                else if (value_FCv_CK3.IsChecked == true)
                {
                    change_fwv3.Text = "";
                    change1_fwv3.Text = "";
                    change2_fwv3.Text = "";
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    string strr = line.Trim().Split('：')[1];
                    if (strr == "等数量")
                    {
                        dengnumberDivide_FCv_CK3.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_FCv_CK3.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fwv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    roll_fwv3.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fwv3.Text = line.Trim().Split('：')[1];
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }
        }

        #endregion
        // 综合分析
        private void btn_ZongheFC3_Click(object sender, RoutedEventArgs e)
        {
            int testqushi_n = 0;
            int yanzhengqushi_n = 0;
            float qushizhi = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                qushizhi = listData_factor3[i].YuceZhenZhi - listData_factor3[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor3[i].LabelYuCe == lablelist[0])
                {
                    testqushi_n++;
                    listData_factor3[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor3[i].LabelYuCe == lablelist[2])
                {
                    testqushi_n++;
                    listData_factor3[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor3[i].LabelYuCe == lablelist[1])
                {
                    testqushi_n++;
                    listData_factor3[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor3[i].Consistency = "No";
                }
            }

            for (int i = test_index + 1; i < listData_factor3.Count; i++)
            {
                qushizhi = listData_factor3[i].YuceZhenZhi - listData_factor3[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor3[i].LabelYuCe == lablelist[0])
                {
                    yanzhengqushi_n++;
                    listData_factor3[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor3[i].LabelYuCe == lablelist[2])
                {
                    yanzhengqushi_n++;
                    listData_factor3[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor3[i].LabelYuCe == lablelist[1])
                {
                    yanzhengqushi_n++;
                    listData_factor3[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor3[i].Consistency = "No";
                }
            }

            //  MessageBox.Show("训练集一致性：" + ((float)testqushi_n / (test_index - train_index)).ToString());
            // MessageBox.Show("验证集一致性：" + ((float)yanzhengqushi_n / (listData.Count - test_index - 1)).ToString());
            test_yizhiFC3.Text = ((float)testqushi_n / (test_index - train_index)).ToString();
            yanzheng_yizhiFC3.Text = ((float)yanzhengqushi_n / (listData_factor3.Count - test_index - 1)).ToString();

        }
        #endregion

        #region 单因素预测4

        #region 单因素趋势预测

        // 读取风温数据表
        private void btn_fw4_Click(object sender, RoutedEventArgs e)
        {
            // List<int[]> a = new List<int[]> {new int[]{1,2,3},new int[]{4,5,6}};
            //List<int[]> b = new List<int[]>();
            //a.ForEach(i => b.Add(i));
            // b[0][0] = 0;
            FWdataList4.Clear();
            listData_factor4.Clear();
            listDataStart.ForEach(i => listData_factor4.Add(new Datalist(i)));
            ListFengWen4.ItemsSource = listData_factor4;
            Microsoft.Win32.OpenFileDialog dialog = new OpenFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fileNameFW = dialog.FileName;
            //int indx = 0; 

            if (fileNameFW != "")
            {
                FileStream fileStream = new FileStream(fileNameFW, FileMode.Open, FileAccess.ReadWrite);
                HSSFWorkbook workbook = new HSSFWorkbook(fileStream); //获取excle数据
                ISheet sheet = workbook.GetSheetAt(0); //根据表名获取表
                IRow row;

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    if (row != null)
                    {

                        FactorData dl = new FactorData();
                        dl.Id = i;
                        try { dl.Time = Convert.ToDateTime(row.GetCell(1).ToString().Replace('/', '-')); }
                        catch
                        {
                            string[] time_str = row.GetCell(1).ToString().Split('/');
                            string new_time = "20" + time_str[2] + "-" + time_str[0] + "-" + time_str[1];
                            dl.Time = Convert.ToDateTime(new_time);
                        }
                        dl.Temperature = float.Parse(row.GetCell(2).ToString());
                        FWdataList4.Add(dl);

                    }
                    else
                    {
                        break;
                    }
                }
                workbook.Close();
                fileStream.Close();

            }

        }

        // 获取参数设置
        private void btn_check_fw4_Click(object sender, RoutedEventArgs e)
        {

            K_t.Text = "";
            K1_t.Text = "";
            K2_t.Text = "";
            tongjistart_t.Text = "";
            tongjistart1_t.Text = "";
            tongjistart2_t.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号
                jieding_t = float.Parse(Jieding_t.Text); // 界定

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }

                // 统计时长M范围
                if (M_t.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1_t.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2_t.Text); // 统计时长m2
                    stat_ml = Int32.Parse(ML_t.Text); // 统计时长步长
                }
                else
                {
                    statistics_m[0] = Int32.Parse(M_t.Text);
                    statistics_m[1] = Int32.Parse(M_t.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围
                if (tongjistart_fw4.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fw4.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fw4.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fw4.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fw4.Text);
                }


                // 关联步长
                if (K_fw4.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fw4.Text);
                    relevance_k[1] = Int32.Parse(K2_fw4.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fw4.Text);
                    relevance_k[1] = Int32.Parse(K_fw4.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fc4.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fc4.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fc4.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fc4.Text);
                    valid_n[1] = Int32.Parse(valid_n_fc4.Text);
                    //flagN = true;
                }
                if (value_FC_CK4.IsChecked == true)
                {
                    if (qushu_fw4.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fw4.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fw4.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fw4.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fw4.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fw4.Text);
                    rollb_fwl = float.Parse(rollL_fw4.Text);

                    if (dengnumberDivide_FC_CK4.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FC_CK4.IsChecked == true)
                {
                    if (change_fw4.Text == "")
                    {
                        cg[0] = float.Parse(change1_fw4.Text);
                        cg[1] = float.Parse(change2_fw4.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fw4.Text);
                        cg[1] = float.Parse(change_fw4.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }

                FactorName = CBFC4.Text;
                deletBaifen = float.Parse(delet_fc4.Text);



            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
            //Lable();
            //MessageBox.Show(train_index_t.ToString());
            //MessageBox.Show(test_index_t.ToString());
            //MessageBox.Show(jieding_t.ToString());
            //MessageBox.Show(statistics_m_t[0].ToString());
            //MessageBox.Show(statistics_m_t[1].ToString());
            //MessageBox.Show(stat_ml_t.ToString());
            //MessageBox.Show(stat_start_t[0].ToString());
            //MessageBox.Show(stat_start_t[1].ToString());
            //MessageBox.Show(relevance_k_t[0].ToString());
            //MessageBox.Show(relevance_k_t[1].ToString());
            //MessageBox.Show(valid_n_t[0].ToString());
            //MessageBox.Show(valid_n_t[1].ToString());
            //MessageBox.Show(fwQunjian[0].ToString());
            //MessageBox.Show(fwQunjian[1].ToString());
        }

        // 计算
        private void btn_math_fw4_Click(object sender, RoutedEventArgs e)
        {

            SingleFactorTrend singleFactorTrend4;
            if (value_FC_CK4.IsChecked == true)
            {
                singleFactorTrend4 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, jieding_t, rollb_fw, rollb_fwl, DengNumFenQU_FC);

            }
            else
            {
                singleFactorTrend4 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, jieding_t);

            }
            singleFactorTrend4.FWdataList = FWdataList4;
            singleFactorTrend4.listData = listData_factor4;
            singleFactorTrend4.SingleFactorCalculate();
            TestRight_fw4.Text = singleFactorTrend4.TestRight.ToString();
            VerifyRight_fw4.Text = singleFactorTrend4.VerifyRight.ToString();

            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FC_CK4.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorTrend4.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend4.test_index - singleFactorTrend4.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend4.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend4.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend4.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend4.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend4.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorTrend4.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend4.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend4.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend4.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(值分区).txt");
                sw.Write("训练集：" + (singleFactorTrend4.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend4.test_index - singleFactorTrend4.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend4.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend4.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend4.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend4.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend4.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素分区数：" + singleFactorTrend4.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorTrend4.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorTrend4.roll_l.ToString());
                sw.WriteLine();
                if (singleFactorTrend4.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend4.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend4.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend4.VerifyRight.ToString());
                sw.Close();
            }

        }

        // 写入excel
        private void btn_save_fw4_Click(object sender, RoutedEventArgs e)
        {
            #region 写入excel
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fname = dialog.FileName;
            if (fname != "")
            {
                // FileStream fileStream2 = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                HSSFWorkbook workbook1 = new HSSFWorkbook(); //获取excle数据
                ISheet sheet1;
                try
                {
                    sheet1 = workbook1.CreateSheet("Predict"); //根据表名第一个表
                }
                catch
                {
                    sheet1 = workbook1.GetSheet("Predict");
                }


                IRow row = sheet1.CreateRow(0);
                ICell headCell = row.CreateCell(0);
                headCell.SetCellValue("序号");
                headCell = row.CreateCell(1);
                headCell.SetCellValue("出铁时间");
                //headCell = row.CreateCell(2);
                //headCell.SetCellValue("出铁结束时间");
                headCell = row.CreateCell(2);
                headCell.SetCellValue("铁水中SI含量");
                headCell = row.CreateCell(3);
                headCell.SetCellValue("趋势预测值");
                headCell = row.CreateCell(4);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(5);
                headCell.SetCellValue("具体值预测");
                headCell = row.CreateCell(6);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(7);
                headCell.SetCellValue("一致性");

                HSSFCell[] Cell = new HSSFCell[8];
                int excRow = 1; // 行号
                //int RowNum =sheet1.LastRowNum;
                for (int j = 0; j < listData_factor4.Count; j++)
                {
                    row = sheet1.CreateRow(excRow);


                    for (int i = 0; i <= 7; i++)
                    {
                        Cell[i] = (HSSFCell)row.CreateCell(i);
                    }

                    Cell[0].SetCellValue(excRow);
                    Cell[1].SetCellValue(listData_factor4[j].POPtime);
                    //Cell[2].SetCellValue(listData[j].POPfinish);
                    Cell[2].SetCellValue(listData_factor4[j].Rhmsi);

                    if (j > train_index)
                    {
                        Cell[3].SetCellValue(listData_factor4[j].LabelYuCe);
                        Cell[4].SetCellValue(listData_factor4[j].Trust_t);
                        Cell[5].SetCellValue(listData_factor4[j].YuceZhenZhi);
                        Cell[6].SetCellValue(listData_factor4[j].Trust_v);
                        Cell[7].SetCellValue(listData_factor4[j].Consistency);
                    }
                    else
                    {
                        Cell[3].SetCellValue("");
                        Cell[4].SetCellValue("");
                        Cell[5].SetCellValue("");
                        Cell[6].SetCellValue("");
                        Cell[7].SetCellValue("");


                    }

                    excRow++;
                }


            #endregion
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
                workbook1.Write(fs);
                //fileStream2.Close();
                fs.Close();
                workbook1.Close();

                MessageBox.Show("完成");
            }
            else
            {
                MessageBox.Show("请输入正确的保存文件名");
            }
        }

        // 参数配置文件读取
        private void btn_readParm_fw4_Click(object sender, RoutedEventArgs e)
        {

            if (CBFC4.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC4.Text;
                if (trend_FC_CK4.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fc4.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    qushu_fw4.Text = "";
                    qushu1_fw4.Text = "";
                    qushu2_fw4.Text = "";
                    roll_fw4.Text = "";
                    rollL_fw4.Text = "";

                }
                else if (value_FC_CK4.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    roll_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fw4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_FC_CK4.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_FC_CK4.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    delet_fc4.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    change_fw4.Text = "";
                    change1_fw4.Text = "";
                    change2_fw4.Text = "";

                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }


        }

        // 值分区的checked
        private void value_FC_CK4_Checked(object sender, RoutedEventArgs e)
        {
            trend_FC_CK4.IsChecked = false;
            change_fw4.IsEnabled = false;
            change1_fw4.IsEnabled = false;
            change2_fw4.IsEnabled = false;
            qushu_fw4.IsEnabled = true;
            qushu1_fw4.IsEnabled = true;
            qushu2_fw4.IsEnabled = true;
            roll_fw4.IsEnabled = true;
            rollL_fw4.IsEnabled = true;
            dengnumberDivide_FC_CK4.IsEnabled = true;
            dengzhiDivide_FC_CK4.IsEnabled = true;

        }

        private void trend_FC_CK4_Checked(object sender, RoutedEventArgs e)
        {
            value_FC_CK4.IsChecked = false;
            change_fw4.IsEnabled = true;
            change1_fw4.IsEnabled = true;
            change2_fw4.IsEnabled = true;
            qushu_fw4.IsEnabled = false;
            qushu1_fw4.IsEnabled = false;
            qushu2_fw4.IsEnabled = false;
            roll_fw4.IsEnabled = false;
            rollL_fw4.IsEnabled = false;

            dengnumberDivide_FC_CK4.IsChecked = false;
            dengzhiDivide_FC_CK4.IsChecked = false;
            dengnumberDivide_FC_CK4.IsEnabled = false;
            dengzhiDivide_FC_CK4.IsEnabled = false;


        }

        private void dengnumberDivide_FC_CK4_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FC_CK4.IsChecked = false;
        }

        private void dengzhiDivide_FC_CK4_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FC_CK4.IsChecked = false;
        }
        #endregion

        #region 单因素具体值预测

        //获取参数
        private void btn_check_fwv4_Click(object sender, RoutedEventArgs e)
        {
            tongjistart.Text = "";
            tongjistart1.Text = "";
            tongjistart2.Text = "";
            K.Text = "";
            K1.Text = "";
            K2.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }


                // 平滑指数
                if (pinghua.Text == "")
                {
                    pinghua_a[0] = float.Parse(pinghua1.Text); //左区间
                    pinghua_a[1] = float.Parse(pinghua2.Text); //右区间
                    pinghua_l = float.Parse(pinghual.Text);  //步长
                }
                else
                {
                    pinghua_a[0] = float.Parse(pinghua.Text);
                    pinghua_a[1] = float.Parse(pinghua.Text);
                    pinghua_l = 1;
                }

                // 趋势指数
                if (trend.Text == "")
                {
                    trend_b[0] = float.Parse(trend1.Text);
                    trend_b[1] = float.Parse(trend2.Text);
                    trend_bl = float.Parse(trendL.Text); // 步长
                }
                else
                {
                    trend_b[0] = float.Parse(trend.Text);
                    trend_b[1] = float.Parse(trend.Text);
                    trend_bl = 1;
                }

                // 区间数
                if (qujianNum.Text == "")
                {
                    qujian_z[0] = Int32.Parse(qujianNum1.Text);
                    qujian_z[1] = Int32.Parse(qujianNum2.Text);

                }
                else
                {
                    qujian_z[0] = Int32.Parse(qujianNum.Text);
                    qujian_z[1] = Int32.Parse(qujianNum.Text);
                }

                //滚动次数T
                roll_t = Int32.Parse(rollNum.Text);
                roll_l = float.Parse(rolll.Text);
                // 统计时长M范围
                if (M.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2.Text); // 统计时长m2
                    stat_ml = Int32.Parse(M_ml.Text); // 统计时长步长

                }
                else
                {
                    statistics_m[0] = Int32.Parse(M.Text);
                    statistics_m[1] = Int32.Parse(M.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围（因素）
                if (tongjistart_fwv4.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fwv4.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fwv4.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fwv4.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fwv4.Text);
                }


                // 关联步长（因素）
                if (K_fwv4.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fwv4.Text);
                    relevance_k[1] = Int32.Parse(K2_fwv4.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fwv4.Text);
                    relevance_k[1] = Int32.Parse(K_fwv4.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fcv4.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fcv4.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fcv4.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fcv4.Text);
                    valid_n[1] = Int32.Parse(valid_n_fcv4.Text);
                    //flagN = true;
                }
                if (value_FCv_CK4.IsChecked == true)
                {
                    if (qushu_fwv4.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fwv4.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fwv4.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fwv4.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fwv4.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fwv4.Text);
                    rollb_fwl = float.Parse(rollL_fwv4.Text);

                    if (dengnumberDivide_FCv_CK4.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FCv_CK4.IsChecked == true)
                {
                    if (change_fwv4.Text == "")
                    {
                        cg[0] = float.Parse(change1_fwv4.Text);
                        cg[1] = float.Parse(change2_fwv4.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fwv4.Text);
                        cg[1] = float.Parse(change_fwv4.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }
                if (dengnumberDivide_TG_CK.IsChecked == true)
                {
                    DengNumFenQu = true;
                }
                else
                {
                    DengNumFenQu = false;
                }
                err[0] = -float.Parse(err1.Text);
                err[1] = float.Parse(err1.Text);

                deletBaifen = float.Parse(delet_fcv4.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
        }
        // 计算
        private void btn_math_fwv4_Click(object sender, RoutedEventArgs e)
        {
            SingleFactorValue singleFactorValue4;
            if (value_FCv_CK4.IsChecked == true)
            {
                singleFactorValue4 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, rollb_fw, rollb_fwl, DengNumFenQU_FC, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }
            else
            {
                singleFactorValue4 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }

            singleFactorValue4.FWdataList = FWdataList4;
            singleFactorValue4.listData = listData_factor4;
            singleFactorValue4.SingleFactorCalculate();
            TestRight_fwv4.Text = singleFactorValue4.TestRight.ToString();
            VerifyRight_fwv4.Text = singleFactorValue4.VerifyRight.ToString();

            FactorName = CBFC4.Text;
            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FCv_CK4.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorValue4.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue4.test_index - singleFactorValue4.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue4.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue4.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue4.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue4.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorValue4.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue4.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue4.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue4.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue4.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue4.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue4.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue4.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue4.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(值分区).txt");

                if (singleFactorValue4.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("训练集：" + (singleFactorValue4.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue4.test_index - singleFactorValue4.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue4.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue4.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue4.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue4.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素区间数：" + singleFactorValue4.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue4.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue4.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue4.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue4.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue4.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue4.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorValue4.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorValue4.roll_l.ToString());
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue4.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue4.VerifyRight.ToString());
                sw.Close();
            }

        }

        private void value_FCv_CK4_Checked(object sender, RoutedEventArgs e)
        {
            trend_FCv_CK4.IsChecked = false;
            change_fwv4.IsEnabled = false;
            change1_fwv4.IsEnabled = false;
            change2_fwv4.IsEnabled = false;
            qushu_fwv4.IsEnabled = true;
            qushu1_fwv4.IsEnabled = true;
            qushu2_fwv4.IsEnabled = true;
            roll_fwv4.IsEnabled = true;
            rollL_fwv4.IsEnabled = true;
            dengnumberDivide_FCv_CK4.IsEnabled = true;
            dengzhiDivide_FCv_CK4.IsEnabled = true;
        }

        private void trend_FCv_CK4_Checked(object sender, RoutedEventArgs e)
        {
            value_FCv_CK4.IsChecked = false;
            change_fwv4.IsEnabled = true;
            change1_fwv4.IsEnabled = true;
            change2_fwv4.IsEnabled = true;
            qushu_fwv4.IsEnabled = false;
            qushu1_fwv4.IsEnabled = false;
            qushu2_fwv4.IsEnabled = false;
            roll_fwv4.IsEnabled = false;
            rollL_fwv4.IsEnabled = false;


            dengnumberDivide_FCv_CK4.IsChecked = false;
            dengzhiDivide_FCv_CK4.IsChecked = false;
            dengnumberDivide_FCv_CK4.IsEnabled = false;
            dengzhiDivide_FCv_CK4.IsEnabled = false;
        }

        private void dengnumberDivide_FCv_CK4_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FCv_CK4.IsChecked = false;
        }

        private void dengzhiDivide_FCv_CK4_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FCv_CK4.IsChecked = false;
        }

        // 读取参数
        private void btn_readParm_fwv4_Click(object sender, RoutedEventArgs e)
        {
            if (CBFC4.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC4.Text;
                if (trend_FCv_CK4.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fwv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                    qushu_fwv4.Text = "";
                    qushu1_fwv4.Text = "";
                    qushu2_fwv4.Text = "";
                    roll_fwv4.Text = "";
                    rollL_fwv4.Text = "";


                }
                else if (value_FCv_CK4.IsChecked == true)
                {
                    change_fwv4.Text = "";
                    change1_fwv4.Text = "";
                    change2_fwv4.Text = "";
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    string strr = line.Trim().Split('：')[1];
                    if (strr == "等数量")
                    {
                        dengnumberDivide_FCv_CK4.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_FCv_CK4.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fwv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    roll_fwv4.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fwv4.Text = line.Trim().Split('：')[1];
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }
        }

        #endregion
        // 综合分析
        private void btn_ZongheFC4_Click(object sender, RoutedEventArgs e)
        {
            int testqushi_n = 0;
            int yanzhengqushi_n = 0;
            float qushizhi = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                qushizhi = listData_factor4[i].YuceZhenZhi - listData_factor4[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor4[i].LabelYuCe == lablelist[0])
                {
                    testqushi_n++;
                    listData_factor4[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor4[i].LabelYuCe == lablelist[2])
                {
                    testqushi_n++;
                    listData_factor4[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor4[i].LabelYuCe == lablelist[1])
                {
                    testqushi_n++;
                    listData_factor4[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor4[i].Consistency = "No";
                }
            }

            for (int i = test_index + 1; i < listData_factor4.Count; i++)
            {
                qushizhi = listData_factor4[i].YuceZhenZhi - listData_factor4[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor4[i].LabelYuCe == lablelist[0])
                {
                    yanzhengqushi_n++;
                    listData_factor4[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor4[i].LabelYuCe == lablelist[2])
                {
                    yanzhengqushi_n++;
                    listData_factor4[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor4[i].LabelYuCe == lablelist[1])
                {
                    yanzhengqushi_n++;
                    listData_factor4[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor4[i].Consistency = "No";
                }
            }

            //  MessageBox.Show("训练集一致性：" + ((float)testqushi_n / (test_index - train_index)).ToString());
            // MessageBox.Show("验证集一致性：" + ((float)yanzhengqushi_n / (listData.Count - test_index - 1)).ToString());
            test_yizhiFC4.Text = ((float)testqushi_n / (test_index - train_index)).ToString();
            yanzheng_yizhiFC4.Text = ((float)yanzhengqushi_n / (listData_factor4.Count - test_index - 1)).ToString();

        }
        #endregion

        #region 单因素预测5

        #region 单因素趋势预测

        // 读取风温数据表
        private void btn_fw5_Click(object sender, RoutedEventArgs e)
        {
            // List<int[]> a = new List<int[]> {new int[]{1,2,3},new int[]{4,5,6}};
            //List<int[]> b = new List<int[]>();
            //a.ForEach(i => b.Add(i));
            // b[0][0] = 0;
            FWdataList5.Clear();
            listData_factor5.Clear();
            listDataStart.ForEach(i => listData_factor5.Add(new Datalist(i)));
            ListFengWen5.ItemsSource = listData_factor5;
            Microsoft.Win32.OpenFileDialog dialog = new OpenFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fileNameFW = dialog.FileName;
            //int indx = 0; 

            if (fileNameFW != "")
            {
                FileStream fileStream = new FileStream(fileNameFW, FileMode.Open, FileAccess.ReadWrite);
                HSSFWorkbook workbook = new HSSFWorkbook(fileStream); //获取excle数据
                ISheet sheet = workbook.GetSheetAt(0); //根据表名获取表
                IRow row;

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    row = sheet.GetRow(i);
                    if (row != null)
                    {

                        FactorData dl = new FactorData();
                        dl.Id = i;
                        try { dl.Time = Convert.ToDateTime(row.GetCell(1).ToString().Replace('/', '-')); }
                        catch
                        {
                            string[] time_str = row.GetCell(1).ToString().Split('/');
                            string new_time = "20" + time_str[2] + "-" + time_str[0] + "-" + time_str[1];
                            dl.Time = Convert.ToDateTime(new_time);
                        }
                        dl.Temperature = float.Parse(row.GetCell(2).ToString());
                        FWdataList5.Add(dl);

                    }
                    else
                    {
                        break;
                    }
                }
                workbook.Close();
                fileStream.Close();

            }

        }

        // 获取参数设置
        private void btn_check_fw5_Click(object sender, RoutedEventArgs e)
        {

            K_t.Text = "";
            K1_t.Text = "";
            K2_t.Text = "";
            tongjistart_t.Text = "";
            tongjistart1_t.Text = "";
            tongjistart2_t.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号
                jieding_t = float.Parse(Jieding_t.Text); // 界定

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }

                // 统计时长M范围
                if (M_t.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1_t.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2_t.Text); // 统计时长m2
                    stat_ml = Int32.Parse(ML_t.Text); // 统计时长步长
                }
                else
                {
                    statistics_m[0] = Int32.Parse(M_t.Text);
                    statistics_m[1] = Int32.Parse(M_t.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围
                if (tongjistart_fw5.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fw5.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fw5.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fw5.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fw5.Text);
                }


                // 关联步长
                if (K_fw5.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fw5.Text);
                    relevance_k[1] = Int32.Parse(K2_fw5.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fw5.Text);
                    relevance_k[1] = Int32.Parse(K_fw5.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fc5.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fc5.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fc5.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fc5.Text);
                    valid_n[1] = Int32.Parse(valid_n_fc5.Text);
                    //flagN = true;
                }
                if (value_FC_CK5.IsChecked == true)
                {
                    if (qushu_fw5.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fw5.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fw5.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fw5.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fw5.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fw5.Text);
                    rollb_fwl = float.Parse(rollL_fw5.Text);

                    if (dengnumberDivide_FC_CK5.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FC_CK5.IsChecked == true)
                {
                    if (change_fw5.Text == "")
                    {
                        cg[0] = float.Parse(change1_fw5.Text);
                        cg[1] = float.Parse(change2_fw5.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fw5.Text);
                        cg[1] = float.Parse(change_fw5.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }

                FactorName = CBFC5.Text;
                deletBaifen = float.Parse(delet_fc5.Text);



            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
            //Lable();
            //MessageBox.Show(train_index_t.ToString());
            //MessageBox.Show(test_index_t.ToString());
            //MessageBox.Show(jieding_t.ToString());
            //MessageBox.Show(statistics_m_t[0].ToString());
            //MessageBox.Show(statistics_m_t[1].ToString());
            //MessageBox.Show(stat_ml_t.ToString());
            //MessageBox.Show(stat_start_t[0].ToString());
            //MessageBox.Show(stat_start_t[1].ToString());
            //MessageBox.Show(relevance_k_t[0].ToString());
            //MessageBox.Show(relevance_k_t[1].ToString());
            //MessageBox.Show(valid_n_t[0].ToString());
            //MessageBox.Show(valid_n_t[1].ToString());
            //MessageBox.Show(fwQunjian[0].ToString());
            //MessageBox.Show(fwQunjian[1].ToString());
        }

        // 计算
        private void btn_math_fw5_Click(object sender, RoutedEventArgs e)
        {

            SingleFactorTrend singleFactorTrend5;
            if (value_FC_CK5.IsChecked == true)
            {
                singleFactorTrend5 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, jieding_t, rollb_fw, rollb_fwl, DengNumFenQU_FC);

            }
            else
            {
                singleFactorTrend5 = new SingleFactorTrend(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, jieding_t);

            }
            singleFactorTrend5.FWdataList = FWdataList5;
            singleFactorTrend5.listData = listData_factor5;
            singleFactorTrend5.SingleFactorCalculate();
            TestRight_fw5.Text = singleFactorTrend5.TestRight.ToString();
            VerifyRight_fw5.Text = singleFactorTrend5.VerifyRight.ToString();

            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FC_CK5.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorTrend5.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend5.test_index - singleFactorTrend5.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend5.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend5.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend5.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend5.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend5.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorTrend5.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend5.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend5.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend5.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "趋势预测配置文件(值分区).txt");
                sw.Write("训练集：" + (singleFactorTrend5.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorTrend5.test_index - singleFactorTrend5.train_index).ToString());
                sw.WriteLine();
                sw.Write("界定值：" + singleFactorTrend5.jieding.ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorTrend5.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorTrend5.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorTrend5.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorTrend5.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素分区数：" + singleFactorTrend5.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorTrend5.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorTrend5.roll_l.ToString());
                sw.WriteLine();
                if (singleFactorTrend5.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorTrend5.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorTrend5.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorTrend5.VerifyRight.ToString());
                sw.Close();
            }

        }

        // 写入excel
        private void btn_save_fw5_Click(object sender, RoutedEventArgs e)
        {
            #region 写入excel
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();//对话框
            dialog.Filter = "Excel文件|*.xls";
            dialog.ShowDialog();
            string fname = dialog.FileName;
            if (fname != "")
            {
                // FileStream fileStream2 = new FileStream(fname, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                HSSFWorkbook workbook1 = new HSSFWorkbook(); //获取excle数据
                ISheet sheet1;
                try
                {
                    sheet1 = workbook1.CreateSheet("Predict"); //根据表名第一个表
                }
                catch
                {
                    sheet1 = workbook1.GetSheet("Predict");
                }


                IRow row = sheet1.CreateRow(0);
                ICell headCell = row.CreateCell(0);
                headCell.SetCellValue("序号");
                headCell = row.CreateCell(1);
                headCell.SetCellValue("出铁时间");
                //headCell = row.CreateCell(2);
                //headCell.SetCellValue("出铁结束时间");
                headCell = row.CreateCell(2);
                headCell.SetCellValue("铁水中SI含量");
                headCell = row.CreateCell(3);
                headCell.SetCellValue("趋势预测值");
                headCell = row.CreateCell(4);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(5);
                headCell.SetCellValue("具体值预测");
                headCell = row.CreateCell(6);
                headCell.SetCellValue("可信度");
                headCell = row.CreateCell(7);
                headCell.SetCellValue("一致性");

                HSSFCell[] Cell = new HSSFCell[8];
                int excRow = 1; // 行号
                //int RowNum =sheet1.LastRowNum;
                for (int j = 0; j < listData_factor5.Count; j++)
                {
                    row = sheet1.CreateRow(excRow);


                    for (int i = 0; i <= 7; i++)
                    {
                        Cell[i] = (HSSFCell)row.CreateCell(i);
                    }

                    Cell[0].SetCellValue(excRow);
                    Cell[1].SetCellValue(listData_factor5[j].POPtime);
                    //Cell[2].SetCellValue(listData[j].POPfinish);
                    Cell[2].SetCellValue(listData_factor5[j].Rhmsi);

                    if (j > train_index)
                    {
                        Cell[3].SetCellValue(listData_factor5[j].LabelYuCe);
                        Cell[4].SetCellValue(listData_factor5[j].Trust_t);
                        Cell[5].SetCellValue(listData_factor5[j].YuceZhenZhi);
                        Cell[6].SetCellValue(listData_factor5[j].Trust_v);
                        Cell[7].SetCellValue(listData_factor5[j].Consistency);
                    }
                    else
                    {
                        Cell[3].SetCellValue("");
                        Cell[4].SetCellValue("");
                        Cell[5].SetCellValue("");
                        Cell[6].SetCellValue("");
                        Cell[7].SetCellValue("");


                    }

                    excRow++;
                }


            #endregion
                FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);
                workbook1.Write(fs);
                //fileStream2.Close();
                fs.Close();
                workbook1.Close();

                MessageBox.Show("完成");
            }
            else
            {
                MessageBox.Show("请输入正确的保存文件名");
            }
        }

        // 参数配置文件读取
        private void btn_readParm_fw5_Click(object sender, RoutedEventArgs e)
        {

            if (CBFC5.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC5.Text;
                if (trend_FC_CK5.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fc5.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    qushu_fw5.Text = "";
                    qushu1_fw5.Text = "";
                    qushu2_fw5.Text = "";
                    roll_fw5.Text = "";
                    rollL_fw5.Text = "";

                }
                else if (value_FC_CK5.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "趋势预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    Jieding_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M_t.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fc5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    roll_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fw5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_FC_CK5.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_FC_CK5.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    delet_fc5.Text = line.Trim().Split('：')[1];
                    sr.Close();
                    change_fw5.Text = "";
                    change1_fw5.Text = "";
                    change2_fw5.Text = "";

                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }


        }

        // 值分区的checked
        private void value_FC_CK5_Checked(object sender, RoutedEventArgs e)
        {
            trend_FC_CK5.IsChecked = false;
            change_fw5.IsEnabled = false;
            change1_fw5.IsEnabled = false;
            change2_fw5.IsEnabled = false;
            qushu_fw5.IsEnabled = true;
            qushu1_fw5.IsEnabled = true;
            qushu2_fw5.IsEnabled = true;
            roll_fw5.IsEnabled = true;
            rollL_fw5.IsEnabled = true;
            dengnumberDivide_FC_CK5.IsEnabled = true;
            dengzhiDivide_FC_CK5.IsEnabled = true;

        }

        private void trend_FC_CK5_Checked(object sender, RoutedEventArgs e)
        {
            value_FC_CK5.IsChecked = false;
            change_fw5.IsEnabled = true;
            change1_fw5.IsEnabled = true;
            change2_fw5.IsEnabled = true;
            qushu_fw5.IsEnabled = false;
            qushu1_fw5.IsEnabled = false;
            qushu2_fw5.IsEnabled = false;
            roll_fw5.IsEnabled = false;
            rollL_fw5.IsEnabled = false;

            dengnumberDivide_FC_CK5.IsChecked = false;
            dengzhiDivide_FC_CK5.IsChecked = false;
            dengnumberDivide_FC_CK5.IsEnabled = false;
            dengzhiDivide_FC_CK5.IsEnabled = false;


        }

        private void dengnumberDivide_FC_CK5_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FC_CK5.IsChecked = false;
        }

        private void dengzhiDivide_FC_CK5_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FC_CK5.IsChecked = false;
        }
        #endregion

        #region 单因素具体值预测

        //获取参数
        private void btn_check_fwv5_Click(object sender, RoutedEventArgs e)
        {
            tongjistart.Text = "";
            tongjistart1.Text = "";
            tongjistart2.Text = "";
            K.Text = "";
            K1.Text = "";
            K2.Text = "";
            try
            {
                train_index = Int32.Parse(trainN.Text) - 1; // 训练集最后一行行号 :1000条训练集,则最后一条数据下标应为999
                test_index = train_index + Int32.Parse(testN.Text); // 测试集最后一行行号

                if (train_index >= listData.Count)
                {
                    train_index = listData.Count - 10;
                    test_index = train_index + 5;
                }


                // 平滑指数
                if (pinghua.Text == "")
                {
                    pinghua_a[0] = float.Parse(pinghua1.Text); //左区间
                    pinghua_a[1] = float.Parse(pinghua2.Text); //右区间
                    pinghua_l = float.Parse(pinghual.Text);  //步长
                }
                else
                {
                    pinghua_a[0] = float.Parse(pinghua.Text);
                    pinghua_a[1] = float.Parse(pinghua.Text);
                    pinghua_l = 1;
                }

                // 趋势指数
                if (trend.Text == "")
                {
                    trend_b[0] = float.Parse(trend1.Text);
                    trend_b[1] = float.Parse(trend2.Text);
                    trend_bl = float.Parse(trendL.Text); // 步长
                }
                else
                {
                    trend_b[0] = float.Parse(trend.Text);
                    trend_b[1] = float.Parse(trend.Text);
                    trend_bl = 1;
                }

                // 区间数
                if (qujianNum.Text == "")
                {
                    qujian_z[0] = Int32.Parse(qujianNum1.Text);
                    qujian_z[1] = Int32.Parse(qujianNum2.Text);

                }
                else
                {
                    qujian_z[0] = Int32.Parse(qujianNum.Text);
                    qujian_z[1] = Int32.Parse(qujianNum.Text);
                }

                //滚动次数T
                roll_t = Int32.Parse(rollNum.Text);
                roll_l = float.Parse(rolll.Text);
                // 统计时长M范围
                if (M.Text == "")
                {
                    statistics_m[0] = Int32.Parse(M1.Text);//统计时长m1
                    statistics_m[1] = Int32.Parse(M2.Text); // 统计时长m2
                    stat_ml = Int32.Parse(M_ml.Text); // 统计时长步长

                }
                else
                {
                    statistics_m[0] = Int32.Parse(M.Text);
                    statistics_m[1] = Int32.Parse(M.Text);
                    stat_ml = 1;
                }
                // 统计时长起点范围（因素）
                if (tongjistart_fwv5.Text == "")
                {
                    stat_start[0] = Int32.Parse(tongjistart1_fwv5.Text);
                    stat_start[1] = Int32.Parse(tongjistart2_fwv5.Text);
                }
                else
                {
                    stat_start[0] = Int32.Parse(tongjistart_fwv5.Text);
                    stat_start[1] = Int32.Parse(tongjistart_fwv5.Text);
                }


                // 关联步长（因素）
                if (K_fwv5.Text == "")
                {
                    relevance_k[0] = Int32.Parse(K1_fwv5.Text);
                    relevance_k[1] = Int32.Parse(K2_fwv5.Text);
                    //flagK = false;
                }
                else
                {
                    relevance_k[0] = Int32.Parse(K_fwv5.Text);
                    relevance_k[1] = Int32.Parse(K_fwv5.Text);
                    //flagK = true;
                }
                // 有效值个数
                if (valid_n_fcv5.Text == "")
                {
                    valid_n[0] = Int32.Parse(valid_n1_fcv5.Text);
                    valid_n[1] = Int32.Parse(valid_n2_fcv5.Text);
                    //flagN = false;
                }
                else
                {
                    valid_n[0] = Int32.Parse(valid_n_fcv5.Text);
                    valid_n[1] = Int32.Parse(valid_n_fcv5.Text);
                    //flagN = true;
                }
                if (value_FCv_CK5.IsChecked == true)
                {
                    if (qushu_fwv5.Text == "")
                    {
                        fwQunjian[0] = Int32.Parse(qushu1_fwv5.Text);
                        fwQunjian[1] = Int32.Parse(qushu2_fwv5.Text);
                    }
                    else
                    {
                        fwQunjian[0] = Int32.Parse(qushu_fwv5.Text);
                        fwQunjian[1] = Int32.Parse(qushu_fwv5.Text);
                    }
                    rollb_fw = Int32.Parse(roll_fwv5.Text);
                    rollb_fwl = float.Parse(rollL_fwv5.Text);

                    if (dengnumberDivide_FCv_CK5.IsChecked == true)
                    {
                        DengNumFenQU_FC = true;
                    }
                    else
                    {
                        DengNumFenQU_FC = false;
                    }

                }
                else if (trend_FCv_CK5.IsChecked == true)
                {
                    if (change_fwv5.Text == "")
                    {
                        cg[0] = float.Parse(change1_fwv5.Text);
                        cg[1] = float.Parse(change2_fwv5.Text);
                    }
                    else
                    {
                        cg[0] = float.Parse(change_fwv5.Text);
                        cg[1] = float.Parse(change_fwv5.Text);
                    }
                }
                else
                {
                    MessageBox.Show("请选择分区模式");
                }
                if (dengnumberDivide_TG_CK.IsChecked == true)
                {
                    DengNumFenQu = true;
                }
                else
                {
                    DengNumFenQu = false;
                }
                err[0] = -float.Parse(err1.Text);
                err[1] = float.Parse(err1.Text);

                deletBaifen = float.Parse(delet_fcv5.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确的参数");
            };
        }
        // 计算
        private void btn_math_fwv5_Click(object sender, RoutedEventArgs e)
        {
            SingleFactorValue singleFactorValue5;
            if (value_FCv_CK5.IsChecked == true)
            {
                singleFactorValue5 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, fwQunjian, rollb_fw, rollb_fwl, DengNumFenQU_FC, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }
            else
            {
                singleFactorValue5 = new SingleFactorValue(train_index, test_index, relevance_k, valid_n, statistics_m, stat_ml, stat_start, deletBaifen, cg, pinghua_a, pinghua_l, trend_b, trend_bl, qujian_z, err, roll_t, roll_l, DengNumFenQu);

            }

            singleFactorValue5.FWdataList = FWdataList5;
            singleFactorValue5.listData = listData_factor5;
            singleFactorValue5.SingleFactorCalculate();
            TestRight_fwv5.Text = singleFactorValue5.TestRight.ToString();
            VerifyRight_fwv5.Text = singleFactorValue5.VerifyRight.ToString();

            FactorName = CBFC5.Text;
            // 参数保存
            string path = System.AppDomain.CurrentDomain.BaseDirectory; //  \debug\
            if (trend_FCv_CK5.IsChecked == true)
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(趋势分区).txt");
                sw.Write("训练集：" + (singleFactorValue5.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue5.test_index - singleFactorValue5.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue5.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue5.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue5.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue5.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素变化值：" + singleFactorValue5.best_cg.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue5.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue5.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue5.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue5.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue5.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue5.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue5.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue5.VerifyRight.ToString());
                sw.Close();
            }
            else
            {
                StreamWriter sw = File.CreateText(path + FactorName + "具体值预测配置文件(值分区).txt");

                if (singleFactorValue5.DengFenNum)
                {
                    sw.Write("因素数值分区：等数量");
                }
                else
                {
                    sw.Write("因素数值分区：等值");
                }
                sw.WriteLine();
                sw.Write("训练集：" + (singleFactorValue5.train_index + 1).ToString());
                sw.WriteLine();
                sw.Write("测试集：" + (singleFactorValue5.test_index - singleFactorValue5.train_index).ToString());
                sw.WriteLine();
                sw.Write("统计时长：" + singleFactorValue5.best_m.ToString());
                sw.WriteLine();
                sw.Write("统计时长起点：" + singleFactorValue5.best_mst.ToString());
                sw.WriteLine();
                sw.Write("因素关联时长：" + singleFactorValue5.best_k.ToString());
                sw.WriteLine();
                sw.Write("有效值个数：" + singleFactorValue5.best_n.ToString());
                sw.WriteLine();
                sw.Write("因素区间数：" + singleFactorValue5.best_qujian_fw.ToString());
                sw.WriteLine();
                sw.Write("删除比例：" + singleFactorValue5.deletBaifen.ToString());
                sw.WriteLine();
                sw.Write("目标平滑指数：" + singleFactorValue5.best_a.ToString());
                sw.WriteLine();
                sw.Write("目标趋势指数：" + singleFactorValue5.best_b.ToString());
                sw.WriteLine();
                sw.Write("目标区间数：" + singleFactorValue5.best_qushu.ToString());
                sw.WriteLine();
                sw.Write("目标滚动次数：" + singleFactorValue5.roll_target.ToString());
                sw.WriteLine();
                sw.Write("目标滚动步长：" + singleFactorValue5.roll_targetl.ToString());
                sw.WriteLine();
                sw.Write("误差：±" + err[1]);
                sw.WriteLine();
                sw.Write("因素滚动次数：" + singleFactorValue5.roll_t.ToString());
                sw.WriteLine();
                sw.Write("因素滚动步长：" + singleFactorValue5.roll_l.ToString());
                sw.WriteLine();
                if (DengNumFenQu)
                {
                    sw.Write("分区模式：等数量");
                }
                else
                {
                    sw.Write("分区模式：等值");
                }
                sw.WriteLine();
                sw.Write("*********************************************************");
                sw.WriteLine();
                sw.Write("测试集命中率：" + singleFactorValue5.TestRight.ToString());
                sw.WriteLine();
                sw.Write("验证集命中率：" + singleFactorValue5.VerifyRight.ToString());
                sw.Close();
            }

        }

        // 值分区check
        private void value_FCv_CK5_Checked(object sender, RoutedEventArgs e)
        {
            trend_FCv_CK5.IsChecked = false;
            change_fwv5.IsEnabled = false;
            change1_fwv5.IsEnabled = false;
            change2_fwv5.IsEnabled = false;
            qushu_fwv5.IsEnabled = true;
            qushu1_fwv5.IsEnabled = true;
            qushu2_fwv5.IsEnabled = true;
            roll_fwv5.IsEnabled = true;
            rollL_fwv5.IsEnabled = true;
            dengnumberDivide_FCv_CK5.IsEnabled = true;
            dengzhiDivide_FCv_CK5.IsEnabled = true;
        }
        // 趋势分区 check
        private void trend_FCv_CK5_Checked(object sender, RoutedEventArgs e)
        {
            value_FCv_CK5.IsChecked = false;
            change_fwv5.IsEnabled = true;
            change1_fwv5.IsEnabled = true;
            change2_fwv5.IsEnabled = true;
            qushu_fwv5.IsEnabled = false;
            qushu1_fwv5.IsEnabled = false;
            qushu2_fwv5.IsEnabled = false;
            roll_fwv5.IsEnabled = false;
            rollL_fwv5.IsEnabled = false;

            dengnumberDivide_FCv_CK5.IsChecked = false;
            dengzhiDivide_FCv_CK5.IsChecked = false;
            dengnumberDivide_FCv_CK5.IsEnabled = false;
            dengzhiDivide_FCv_CK5.IsEnabled = false;

        }
        // 值分区下 等数量分区
        private void dengnumberDivide_FCv_CK5_Checked(object sender, RoutedEventArgs e)
        {
            dengzhiDivide_FCv_CK5.IsChecked = false;
        }
        // 值分区下 等值分区
        private void dengzhiDivide_FCv_CK5_Checked(object sender, RoutedEventArgs e)
        {
            dengnumberDivide_FCv_CK5.IsChecked = false;
        }

        // 读取参数
        private void btn_readParm_fwv5_Click(object sender, RoutedEventArgs e)
        {
            if (CBFC5.Text == "")
            {
                MessageBox.Show("请先输入因素名称");
            }
            else
            {
                FactorName = CBFC5.Text;
                if (trend_FCv_CK5.IsChecked == true)
                {
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(趋势分区).txt");
                    String line;
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    valid_n_fcv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    change_fwv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_fcv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                    qushu_fwv5.Text = "";
                    qushu1_fwv5.Text = "";
                    qushu2_fwv5.Text = "";
                    roll_fwv5.Text = "";
                    rollL_fwv5.Text = "";


                }
                else if (value_FCv_CK5.IsChecked == true)
                {
                    change_fwv5.Text = "";
                    change1_fwv5.Text = "";
                    change2_fwv5.Text = "";
                    StreamReader sr = new StreamReader(FactorName + "具体值预测配置文件(值分区).txt");
                    String line;
                    line = sr.ReadLine();
                    string strr = line.Trim().Split('：')[1];
                    if (strr == "等数量")
                    {
                        dengnumberDivide_FCv_CK5.IsChecked = true;
                    }
                    else
                    {
                        dengzhiDivide_FCv_CK5.IsChecked = true;
                    }
                    line = sr.ReadLine();
                    trainN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    testN.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    M.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    tongjistart_fwv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    K_fwv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    N.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qushu_fwv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    delet_v.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    pinghua.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    trend.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    qujianNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollNum.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rolll.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    err1.Text = line.Trim().Split('：')[1].Split('±')[1];
                    line = sr.ReadLine();
                    roll_fwv5.Text = line.Trim().Split('：')[1];
                    line = sr.ReadLine();
                    rollL_fwv5.Text = line.Trim().Split('：')[1];
                    string str = line.Trim().Split('：')[1];
                    if (str == "等数量")
                    {
                        dengnumberDivide_TG_CK.IsChecked = true;

                    }
                    else
                    {
                        dengzhiDivide_TG_CK.IsChecked = true;
                    }
                    sr.Close();
                }
                else
                {
                    MessageBox.Show("请先选择分区模式");
                }
            }
        }

        #endregion
        // 综合分析
        private void btn_ZongheFC5_Click(object sender, RoutedEventArgs e)
        {
            int testqushi_n = 0;
            int yanzhengqushi_n = 0;
            float qushizhi = 0;
            for (int i = train_index + 1; i <= test_index; i++)
            {
                qushizhi = listData_factor5[i].YuceZhenZhi - listData_factor5[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor5[i].LabelYuCe == lablelist[0])
                {
                    testqushi_n++;
                    listData_factor5[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor5[i].LabelYuCe == lablelist[2])
                {
                    testqushi_n++;
                    listData_factor5[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor5[i].LabelYuCe == lablelist[1])
                {
                    testqushi_n++;
                    listData_factor5[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor5[i].Consistency = "No";
                }
            }

            for (int i = test_index + 1; i < listData_factor5.Count; i++)
            {
                qushizhi = listData_factor5[i].YuceZhenZhi - listData_factor5[i - 1].Rhmsi;
                if (qushizhi < -jieding_t && listData_factor5[i].LabelYuCe == lablelist[0])
                {
                    yanzhengqushi_n++;
                    listData_factor5[i].Consistency = "Yes";
                }
                else if (qushizhi > jieding_t && listData_factor5[i].LabelYuCe == lablelist[2])
                {
                    yanzhengqushi_n++;
                    listData_factor5[i].Consistency = "Yes";
                }
                else if (qushizhi >= -jieding_t && qushizhi <= jieding_t && listData_factor5[i].LabelYuCe == lablelist[1])
                {
                    yanzhengqushi_n++;
                    listData_factor5[i].Consistency = "Yes";
                }
                else
                {
                    listData_factor5[i].Consistency = "No";
                }
            }

            //  MessageBox.Show("训练集一致性：" + ((float)testqushi_n / (test_index - train_index)).ToString());
            // MessageBox.Show("验证集一致性：" + ((float)yanzhengqushi_n / (listData.Count - test_index - 1)).ToString());
            test_yizhiFC5.Text = ((float)testqushi_n / (test_index - train_index)).ToString();
            yanzheng_yizhiFC5.Text = ((float)yanzhengqushi_n / (listData_factor5.Count - test_index - 1)).ToString();

        }
        #endregion

       


    }
}
    
