using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursach_RPVS_2022
{
    internal class Phisics_model
    {
        double amplitude = 0.05;
        double omega_static;
        double coords_static;
        
        double g = 9.8;

        double coord_not_static;
        double omega_not_static;
        double Be;
        double r = 1;
        double massa;
        public Phisics_model()
        {
        }

        public double Calculate_static(double ro_liquid, double t, double ro_prob, double H)
        {
            omega_static = Math.Sqrt((ro_liquid * g)/(ro_prob * H));
            coords_static = amplitude * Math.Cos(omega_static * t);
            return coords_static;
        }
        public double Calculate_not_static(double ro_liquid, double t, double ro_prob, double size, double H)
        {
            massa = ro_prob * (size * H);
            Be = r / (2 * massa);
            omega_not_static = Math.Sqrt((ro_liquid * g)/(ro_prob * H));
            coord_not_static = amplitude * Math.Exp((-Be) * t)* Math.Cos(omega_not_static * t);
            return coord_not_static;
        }

    }
}
