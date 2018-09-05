using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Media;
using System.IO;
using System.Linq;

namespace Mixtured_Semicondutor
{
    public partial class AdmixturedSemicondutor : Form
    {
        InputParameters Parameters = new InputParameters();
        OutValues OValues;
        List<double> Temperatures;

        double Nd_min = 1e15;
        double Nd_max = 1e19;
        double Na_min = 1e15;
        double Na_max = 1e19;
        double T_min = 20;
        double T_max = 1500;
        double dT = 5;

        double Cn_GaAs = 5.02e12;
        double Tn_GaAs = 4.94e-3;
        double Cp_GaAs = 2.53e12;
        double Tp_GaAs = 1.27e-3;
        double Eg_GaAs = 1.42;
        double mc_GaAs = 0.85;
        double mv_GaAs = 0.53;
        double Nd0_GaAs = 1e17;
        double Na0_GaAs = 5e17;
        double Ed_GaAs = 0.01;
        double Ea_GaAs = 0.09;

        double Cn_Ge = 4e12;
        double Tn_Ge = 2.3e-4;
        double Cp_Ge = 4.4e12;
        double Tp_Ge = 1.5e-4;
        double Eg_Ge = 0.66;
        double mc_Ge = 0.22;
        double mv_Ge = 0.34;
        double Nd0_Ge = 1e17;
        double Na0_Ge = 5e17;
        double Ed_Ge = 0.029;
        double Ea_Ge = 0.012;


        public AdmixturedSemicondutor()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            SetMaterial(0);
            SetParameters(0);
            UpdateInterfaceParameters();
            Name_Axis();
        }

        new void Update()
        {
            Calc();
            Draw();
        }

        void Calc()
        {
            OValues = SCalc.Calc(Parameters);
        }

        void Draw()
        {
            List<int> steps;
            FormSteps(StepsNumber(), out steps);

            //List<double> Temperatures;
            FormNeededTemperatures(steps, out Temperatures);

            ClearCharts();

            DisplayCharts(Temperatures);        
        }

        private void FormSteps(int N, out List<int> steps)
        {
            steps = new List<int>();

            for (int i = 0; i < N; i++)
            {
                steps.Add(i);
            }
        }

        private void FormT(List<int> steps, ref List<double> T)
        {
            foreach (var step in steps)
            {
                T.Add(Parameters.T_min + Parameters.dT * step);
            }
        }

        private void FormKT(List<int> steps, ref List<double> kT)
        {
            foreach (double step in steps)
            {
                kT.Add(1 / (SCalc.k * step));
            }
        }

        private Plot DisplayChart(List<double> t, List<double> values, Chart chart, int series)
        {
            List<double> val;
            FormIfLogValues(values, out val);

            var Plot = new Plot(t, val, chart, series);
            return Plot;
        }

        private Plot DisplayChartRadio(RadioButton button, List<double> t, List<double> values_true, List<double> values_false, Chart chart, int series)
        {
            List<double> val;

            if (button.Checked)
            {
                FormIfLogValues(values_true, out val);
                var Plot = new Plot(t, val, chart, series);
                return Plot;
            }
            else
            {
                FormIfLogValues(values_false, out val);

                var Plot = new Plot(t, val, chart, series);
                return Plot;
            }
        }

        private void ClearCharts()
        {
            Chart_Fermi.Series.Clear();
            Chart_Charges.Series.Clear();
            Chart_Mixtures.Series.Clear();
            Chart_Mobility.Series.Clear();
            Chart_Unit.Series.Clear();
        }

        private void DisplayCharts(List<double> Temperatures)
        {
            DisplayChart(Temperatures, OValues.Fermi, Chart_Fermi, 0);
            DisplayChartRadio(Radio_Conc_Electron, Temperatures, OValues.n, OValues.p, Chart_Charges, 0);
            DisplayChartRadio(Radio_Ad_Acc, Temperatures, OValues.Nd_p, OValues.Na_m, Chart_Mixtures, 0);
            DisplayChartRadio(Radio_Mob_Electron, Temperatures, OValues.u_n, OValues.u_p, Chart_Mobility, 0);
            if (CheckBox_SumConduct.Checked)
            {
                var summ = new List<double>();

                foreach (var el in OValues.sigma_p.Zip(OValues.sigma_n, Tuple.Create))
                {
                    summ.Add(el.Item1 + el.Item2);
                }

                DisplayChart(Temperatures, summ, Chart_Unit, 0);
            }
            else
            {
                DisplayChartRadio(Radio_Unit_Electron, Temperatures, OValues.sigma_n, OValues.sigma_p, Chart_Unit, 0);
            }
        }

        private void FormNeededTemperatures(List<int> steps, out List<double> Temperatures)
        {
            Temperatures = new List<double>();

            if (Radio_T.Checked)
            {
                FormT(steps, ref Temperatures);
            }
            else
            {
                FormKT(steps, ref Temperatures);
            }
        }

        private void FormIfLogValues(List<double> values, out List<double> outValues)
        {
            outValues = new List<double>();

            if (CheckBox_LogScale.Checked)
            {
                foreach (var item in values)
                {
                    outValues.Add(Math.Log(item));
                }
            }
            else
            {
                outValues = values;
            }
        }

        private int StepsNumber()
        {
            return (int)((Parameters.T_max - Parameters.T_min) / Parameters.dT);
        }

        private void SetMaterial(Material material)
        {
            ComboBox_Material.SelectedIndex = (int)material;
        }

        private void Button_Reset_Click(object sender, EventArgs e)
        {
            SetParameters(0);
            UpdateInterfaceParameters();

            CheckBox_Na.Checked = true;
            CheckBox_Nd.Checked = true;
            //a;skhsjdkbfkbs
        }

        private void SetParameters(Material material)
        {
            if (material == Material.GaAs)
            {
                Parameters.Cn = Cn_GaAs;
                Parameters.Tn = Tn_GaAs;
                Parameters.Cp = Cp_GaAs;
                Parameters.Tp = Tp_GaAs;
                Parameters.Ea = Ea_GaAs;
                Parameters.Ed = Ed_GaAs;
                Parameters.Eg = Eg_GaAs;
                Parameters.mc = mc_GaAs;
                Parameters.mv = mv_GaAs;
                Parameters.Na0 = Na0_GaAs;
                Parameters.Nd0 = Nd0_GaAs;
            }

            if (material == Material.Ge)
            {
                Parameters.Cn = Cn_Ge;
                Parameters.Tn = Tn_Ge;
                Parameters.Cp = Cp_Ge;
                Parameters.Tp = Tp_Ge;
                Parameters.Ea = Ea_Ge;
                Parameters.Ed = Ed_Ge;
                Parameters.Eg = Eg_Ge;
                Parameters.mc = mc_Ge;
                Parameters.mv = mv_Ge;
                Parameters.Na0 = Na0_Ge;
                Parameters.Nd0 = Nd0_Ge;
            }
            
            Parameters.dT = dT;
            Parameters.T_max = T_max;
            Parameters.T_min = T_min;
        }

        private void UpdateInterfaceParameters()
        {
            NumUD_dT.Value = (decimal)Parameters.dT;
            NumUD_Ea.Value = (decimal)Parameters.Ea;
            NumUD_Ed.Value = (decimal)Parameters.Ed;
            NumUD_Eg.Value = (decimal)Parameters.Eg;
            NumUD_mc.Value = (decimal)Parameters.mc;
            NumUD_mv.Value = (decimal)Parameters.mv;
            NumUD_T_max.Value = (decimal)Parameters.T_max;
            NumUD_T_min.Value = (decimal)Parameters.T_min;

            UpdateTrackBars();
        }
        
        private void UpdateTrackBars()
        {
            UpdateTrackBar(TrackBar_Na, Parameters.Na0, Na_min, Na_max);
            UpdateTrackBar(TrackBar_Nd, Parameters.Nd0, Nd_min, Nd_max);
        }

        private void UpdateTrackBar(TrackBar tb, double value, double min, double max)
        {
            tb.Value = (int)Math.Round(Map(min, max, tb.Minimum, tb.Maximum, value), 1);
        }

        private void NumUD_Eg_ValueChanged(object sender, EventArgs e)
        {
            Parameters.Eg = (double)NumUD_Eg.Value;
            Update();
        }

        private void NumUD_mc_ValueChanged(object sender, EventArgs e)
        {
            Parameters.mc = (double)NumUD_mc.Value;
            Update();
        }

        private void NumUD_mv_ValueChanged(object sender, EventArgs e)
        {
            Parameters.mv = (double)NumUD_mv.Value;
            Update();
        }

        private void NumUD_Ea_ValueChanged(object sender, EventArgs e)
        {
            Parameters.Ea = (double)NumUD_Ea.Value;
            Update();
        }

        private void NumUD_Ed_ValueChanged(object sender, EventArgs e)
        {
            Parameters.Ed = (double)NumUD_Ed.Value;
            Update();
        }

        private void NumUD_T_min_ValueChanged(object sender, EventArgs e)
        {
            Parameters.T_min = (double)NumUD_T_min.Value;
            Update();
        }

        private void NumUD_T_max_ValueChanged(object sender, EventArgs e)
        {
            Parameters.T_max = (double)NumUD_T_max.Value;
            Update();
        }

        private void NumUD_dT_ValueChanged(object sender, EventArgs e)
        {
            Parameters.dT = (double)NumUD_dT.Value;
            Update();
        }

        private void TrackBar_Na_ValueChanged(object sender, EventArgs e)
        {
            Handle_TrackBarValue(TrackBar_Na, TextBox_Na, ref Parameters.Na0, Na_min, Na_max);
            Update();
        }

        private void TrackBar_Nd_ValueChanged(object sender, EventArgs e)
        {
            Handle_TrackBarValue(TrackBar_Nd, TextBox_Nd, ref Parameters.Nd0, Nd_min, Nd_max);
            Update();
        }

        private void CheckBox_Nx_CheckedChange(CheckBox chBox, ref double Nx, ref double Ex, NumericUpDown numUD, TrackBar tb, TextBox box, double Nx_min, double Nx_max)
        {
            if (!chBox.Checked)
            {
                Nx = 0;
                tb.Enabled = false;
                Ex = 0;
                numUD.Enabled = false;
            }
            else
            {
                tb.Enabled = true;
                Handle_TrackBarValue(tb, box, ref Nx, Nx_min, Nx_max);
                numUD.Enabled = true;
                Ex = (double)numUD.Value;
            }
            Update();
        }

        private void CheckBox_Na_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox_Nx_CheckedChange(CheckBox_Na, ref Parameters.Na0, ref Parameters.Ea, NumUD_Ea, TrackBar_Na, TextBox_Na, Na_min, Na_max);
        }

        private void CheckBox_Nd_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox_Nx_CheckedChange(CheckBox_Nd, ref Parameters.Nd0, ref Parameters.Ed, NumUD_Ed, TrackBar_Nd, TextBox_Nd, Nd_min, Nd_max);
        }

        private void Handle_TrackBarValue(TrackBar bar, TextBox box, ref double value, double min, double max)
        {
            double tb = bar.Value;
            double tb_min = bar.Minimum - 1;
            double tb_max = bar.Maximum;

            value = Map(tb_min, tb_max, min, max, tb);

            box.Text = ToTensPower(value);
        }

        private double Map(double a_min, double a_max, double b_min, double b_max, double a)
        {
            return ((a - a_min) / Math.Abs(a_max - a_min)) * (Math.Abs(b_max - b_min) + b_min);
        }

        private string ToTensPower(double num)
        {
            int pow = TensPower(num);

            return Math.Round(num / (Math.Pow(10, pow)), 2).ToString() + " e" + pow.ToString();
        }

        private int TensPower(double num)
        {
            int pow = 0;

            while(num > 1)
            {
                num /= 10;
                pow++;
            }

            return pow;
        }

        private void Radio_Conc_Electron_CheckedChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void Radio_Ad_Donor_CheckedChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void Radio_Mob_Electron_CheckedChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void Radio_Unit_Electron_CheckedChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void Radio_T_CheckedChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void CheckBox_LogScale_CheckedChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void ComboBox_Material_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetParameters((Material)ComboBox_Material.SelectedIndex);
            UpdateInterfaceParameters();
            Update();
        }

        private void BUTTON_Click(object sender, EventArgs e)
        {
            Random r = new Random();

            //MessageBox.Show("Тааааааааа");
            //MessageBox.Show("Шааааааааа");
            //MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory + @"files\" + r.Next(1, 5).ToString() + ".wav");


            SoundPlayer simpleSound = new SoundPlayer(AppDomain.CurrentDomain.BaseDirectory + @"files\file_" + r.Next(1, 6).ToString()/* + ".wav"*/);
            simpleSound.Play();
        }

        private void Name_Axis()
        {
            Chart_Fermi.ChartAreas[0].AxisX.Title = "T, K";
            Chart_Fermi.ChartAreas[0].AxisY.Title = "E, eV";
            Chart_Fermi.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);

            Chart_Charges.ChartAreas[0].AxisX.Title = "T, K";
            Chart_Charges.ChartAreas[0].AxisY.Title = "n, cm^-3";
            Chart_Charges.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);

            Chart_Mixtures.ChartAreas[0].AxisX.Title = "T, K";
            Chart_Mixtures.ChartAreas[0].AxisY.Title = "n, cm^-3";
            Chart_Mixtures.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);

            Chart_Mobility.ChartAreas[0].AxisX.Title = "T, K";
            Chart_Mobility.ChartAreas[0].AxisY.Title = "cm^2*V^-1*s^-1";
            Chart_Mobility.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);

            Chart_Unit.ChartAreas[0].AxisX.Title = "T, K";
            Chart_Unit.ChartAreas[0].AxisY.Title = "cm^-1*V^-1*s^-1";
            Chart_Unit.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);
        }

        private void CheckBox_SumConduct_CheckedChanged(object sender, EventArgs e)
        {
            if(CheckBox_SumConduct.Checked)
            {
                Radio_Unit_Electron.Enabled = false;
                Radio_Unit_Holes.Enabled = false;
            }
            else
            {
                Radio_Unit_Electron.Enabled = true;
                Radio_Unit_Holes.Enabled = true;
            }

            Update();
        }

        private void Button_Save_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "data";
            sfd.Filter = "csv files (*.csv)|*.csv";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(sfd.FileName);

                if (tabControl1.SelectedTab == tabControl1.TabPages["Tab_Fermi"])
                {
                    foreach (var el in OValues.Fermi.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());

                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Charges"]) && (Radio_Conc_Electron.Checked))
                {
                    foreach (var el in OValues.n.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());

                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Charges"]) && (Radio_Conc_Holes.Checked))
                {
                    foreach (var el in OValues.p.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());

                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Mixtures"]) && (Radio_Ad_Donor.Checked))
                {
                    foreach (var el in OValues.Nd_p.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());

                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Mixtures"]) && (Radio_Ad_Acc.Checked))
                {
                    foreach (var el in OValues.Na_m.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());

                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Mobility"]) && (Radio_Mob_Electron.Checked))
                {
                    foreach (var el in OValues.u_n.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());
                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Mobility"]) && (Radio_Mob_Holes.Checked))
                {
                    foreach (var el in OValues.u_p.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());
                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Conductivity"]) && (Radio_Unit_Electron.Checked))
                {
                    foreach (var el in OValues.sigma_n.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());
                    }
                }

                if ((tabControl1.SelectedTab == tabControl1.TabPages["Tab_Conductivity"]) && (Radio_Unit_Holes.Checked))
                {
                    foreach (var el in OValues.sigma_p.Zip(Temperatures, Tuple.Create))
                    {
                        sw.WriteLine(el.Item2.ToString() + ";" + el.Item1.ToString());
                    }
                }
                sw.Close();
            }
        }
    }
}
