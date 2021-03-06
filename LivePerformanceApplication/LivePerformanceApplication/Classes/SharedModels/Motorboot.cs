﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivePerformanceApplication.Classes.SharedModels
{
    public class Motorboot : IBoot
    {
        public int Id { get; set; }

        public string Naam { get; set; }

        public double Prijs { get; set; }

        public string SoortBoot { get; set; }

        public double Tankinhoud { get; set; }

        public Motorboot(int id, string naam, double prijs, string soortBoot, double tankinhoud)
        {
            Id = id;
            Naam = naam;
            Prijs = prijs;
            SoortBoot = soortBoot;
            Tankinhoud = tankinhoud;
        }

        /// <summary>
        /// Returns action radius of certain amount of liters
        /// </summary>
        /// <param name="tankInhoud"></param>
        /// <returns></returns>
        public double BerekenActieRadius(double tankInhoud)
        {
            return Convert.ToDouble(tankInhoud * 15);
        }

        public override string ToString()
        {
            return Naam + " Prijs:" + Prijs + " Soort:" + " Tankinhoud:" + SoortBoot;
        }
    }
}
