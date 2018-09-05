using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mixtured_Semicondutor
{

    public struct InputParameters
    {
        public double Cn;
        public double Tn;
        public double Cp;
        public double Tp;

        public double Eg;
        public double mc;
        public double mv;
        public double Nd0;
        public double Na0;
        public double Ed;
        public double Ea;

        public double T_min;
        public double T_max;
        public double dT;
    };

    public struct OutValues
    {
        public List<double> T;
        public List<double> Fermi;
        public List<double> n;
        public List<double> p;
        public List<double> Nd_p;
        public List<double> Na_m;
        public List<double> u_n;
        public List<double> u_p;
        public List<double> sigma_n;
        public List<double> sigma_p;
    };

    public enum Material
    {
        GaAs = 0,
        Ge = 1
    }

    public class SCalc
    {
        internal static double k = 8.62 * Math.Pow(10, -5.0);
        internal static double e = 1;

        internal static double energy_exp(double energy_difference, double _t)
        {
            return Math.Exp(energy_difference / (k * _t));
        }

        internal static double fermi(double _e, double _x, double _t)
        {
            return 1 / (1 + energy_exp(_e - _x, _t));
        }

        //internal static Func<InputParameters, double, double, double> Ndp = (_p, _t, _x) => (_p.Nd0 / (1 + Math.Exp((_p.Eg - _p.Ed - _x) / (k * _t))));
        internal static double Ndp(InputParameters _p, double _t, double _x)
        {
            double a =_p.Nd0 / (1 + Math.Exp((/*_p.Eg -*/_p.Ed - _x) / (k * _t)));
            return (a);
        }

        internal static double NewNdp(InputParameters _p, double _t, double _x)
        {
            return _p.Nd0 * (1 - fermi(_p.Eg - _p.Ed, _x, _t));
        }

        //internal static Func<InputParameters, double, double, double> Nam = (_p, _t, _x) => (_p.Na0 / (1 + Math.Exp((_p.Ea - _x) / (k * _t))));
        internal static double Nam(InputParameters _p, double _t, double _x)
        {
            double a = _p.Na0 / (1 + Math.Exp((_p.Ea - _x) / (k * _t)));
            return (a);
        }

        internal static double NewNam(InputParameters _p, double _t, double _x)
        {
            return _p.Na0 * fermi(_p.Ea, _x, _t);
        }

        internal static Func<InputParameters, double, double> Nc = (_p, _t) => (2.51 * Math.Pow(10, 19.0) * Math.Pow(_p.mc, 1.5) * Math.Pow(_t / 300.0, 1.5));
        internal static Func<InputParameters, double, double, double> n = (_p, _t, _x) => (Nc(_p, _t) * Math.Exp(/*-_x*/_x - _p.Eg / (k * _t)));

        internal static double Newn(InputParameters _p, double _t, double _x)
        {
            return Nc(_p, _t) * energy_exp(_x - _p.Eg, _t);
        }

        internal static Func<InputParameters, double, double> Nv = (_p, _t) => (2.51 * Math.Pow(10, 19.0) * Math.Pow(_p.mv, 1.5) * Math.Pow(_t / 300.0, 1.5));
        internal static Func<InputParameters, double, double, double> p = (_p, _t, _x) => (Nv(_p, _t) * Math.Exp(-_x / (k * _t)));

        internal static double Newp(InputParameters _p, double _t, double _x)
        {
            return Nv(_p, _t) * energy_exp(0 - _x, _t);
        }

        internal static double un(InputParameters _p, double _t)
        {
            return (_p.Cn / (Math.Pow((_t / _p.Tn), 1.5) + _p.Nd0 * Math.Pow(_p.Tn / _t, 1.5)));
        }

        internal static double up(InputParameters _p, double _t)
        {
            return (_p.Cp / (Math.Pow((_t / _p.Tp), 1.5) + _p.Na0 * Math.Pow(_p.Tp / _t, 1.5)));
        }

        internal static double sigma_n(InputParameters _p, double _t)
        {
            return Newn(_p, _t, Fermi(_p, _t)) * e * un(_p, _t);
        }

        internal static double sigma_p(InputParameters _p, double _t)
        {
            return Newp(_p, _t, Fermi(_p, _t)) * e * up(_p, _t);
        }

        internal static double Dichotomy(Func<double, double> f, double _a, double _b, double _eps)
        {
            double c = (_a + _b) / 2;
            while (Math.Abs(_b - _a) > _eps)
            {
                if (f(_a) * f(c) <= 0) _b = c;
                else _a = c;
                c = (_a + _b) / 2;
            }
            return (_a + _b) / 2;
        }

        internal static double NDick(Func<double, double> f, double _a, double _b, double _eps)
        {
            double c;
            while (Math.Abs(_b - _a) > _eps)
            {
                c = (_a + _b) / 2;
                if (f(c) >= 0)
                    _b = c;
                else
                    _a = c;
            }
            return (_a + _b) / 2;
        }

        internal static double Fermi(InputParameters _p, double _t)
        {
            //Func<double, double> fermi = (_x) => (-Ndp(_p, _t, _x) - p(_p, _t, _x) + (Nam(_p, _t, _x) + n(_p, _t, _x)) + _p.Na0);
            Func<double, double> fermi = (_x) => (-NewNdp(_p, _t, _x) - Newp(_p, _t, _x) + (NewNam(_p, _t, _x) + Newn(_p, _t, _x))/* + _p.Na0*//* + 5e16*/);
            //if (true)
            //    ;
            //return (Dichotomy(fermi, /*2 * -_p.Eg*/0, /*2 * */_p.Eg * 100, 1e-5));
            return (Dichotomy(fermi, -_p.Eg, 2 * _p.Eg, 1e-5));

            //double _x = 0.6;
            //return (-NewNdp(_p, _t, _x) - Newp(_p, _t, _x) + (NewNam(_p, _t, _x) + Newn(_p, _t, _x)));
            //return (-Ndp(_p, _t, _x) - p(_p, _t, _x) + (Nam(_p, _t, _x) + n(_p, _t, _x)));
            //return p(_p, _t, _x);
        }

        internal static double Fermi_KOSTYL(InputParameters _p, double _t)
        {
            //Func<double, double> fermi = (_x) => (-Ndp(_p, _t, _x) - p(_p, _t, _x) + (Nam(_p, _t, _x) + n(_p, _t, _x)) + _p.Na0);
            Func<double, double> fermi = (_x) => (-NewNdp(_p, _t, _x) - Newp(_p, _t, _x) + (NewNam(_p, _t, _x) + Newn(_p, _t, _x))/* + _p.Na0*//* + 5e16*/);
            //if (true)
            //    ;
            //return (Dichotomy(fermi, /*2 * -_p.Eg*/0, /*2 * */_p.Eg * 100, 1e-5));
            return (Dichotomy(fermi, -_p.Eg, 2 * _p.Eg, 1e-5));

            //double _x = 0.6;
            //return (-NewNdp(_p, _t, _x) - Newp(_p, _t, _x) + (NewNam(_p, _t, _x) + Newn(_p, _t, _x)));
            //return (-Ndp(_p, _t, _x) - p(_p, _t, _x) + (Nam(_p, _t, _x) + n(_p, _t, _x)));
            //return p(_p, _t, _x);
        }

        internal static OutValues Calc(InputParameters _p)
        {
            OutValues OValues = new OutValues();

            OValues.Fermi = new List<double>();
            OValues.n = new List<double>();
            OValues.Na_m = new List<double>();
            OValues.Nd_p = new List<double>();
            OValues.p = new List<double>();
            OValues.sigma_n = new List<double>();
            OValues.sigma_p = new List<double>();
            OValues.T = new List<double>();
            OValues.u_n = new List<double>();
            OValues.u_p = new List<double>();

            int N = (int)((_p.T_max - _p.T_min) / _p.dT);
            var steps = new List<int>();
            for (int i = 0; i < N; i++)
            {
                steps.Add(i);
            }

            var T = new List<double>();
            foreach (var step in steps)
            {
                T.Add(_p.T_min + _p.dT * step);
            }

            foreach (var step in steps)
            {
                var f = Fermi(_p, T[step]);
                var f_K = Fermi_KOSTYL(_p, T[step]);
                OValues.Fermi.Add(f);

                OValues.n.Add(Newn(_p, T[step], f)/* * 1e2*/);
                OValues.p.Add(Newp(_p, T[step], f));
                OValues.Nd_p.Add(NewNdp(_p, T[step], f_K));
                OValues.Na_m.Add(NewNam(_p, T[step], f_K));
                OValues.u_n.Add(un(_p, T[step]));
                OValues.u_p.Add(up(_p, T[step]));
                OValues.sigma_n.Add(sigma_n(_p, T[step]));
                OValues.sigma_p.Add(sigma_p(_p, T[step]));
            }

            return OValues;
        }
    }
}
