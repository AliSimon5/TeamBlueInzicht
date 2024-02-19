using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTelefoonKosten.Core
{
    public struct BelTarieven
    {
        // Declaring different data types
        public double dStartTarief;
        public double dMinuutTarief;
    }
    internal class BelTarievenManager
    {
        /// <summary>
        /// Berekend de tarieven door middel van beltijd en totalekosten in te voeren van twee telefoongesprekken.
        /// </summary>
        /// <param name="argSecondsCall1">Beltijd telefoongesprek 1</param>
        /// <param name="argTotalCostCall1">Totale kosten telefoongesprek 1</param>
        /// <param name="argSecondsCall2">Beltijd telefoongesprek 2</param>
        /// <param name="argTotalCostCall2">Totale kosten telefoongesprek 2</param>
        /// <returns></returns>
        internal static BelTarieven CalculateTarieven(double argSecondsCall1, double argTotalCostCall1, double argSecondsCall2, double argTotalCostCall2, int argRoundDecimals = 4)
        {
            var tempT1 = argSecondsCall1 / 60;
            var tempC1 = argTotalCostCall1;
            var tempT2 = argSecondsCall2 / 60;
            var tempC2 = argTotalCostCall2;

            var tempB = BerekenMinuutTariefB(tempT1, tempC1, tempT2, tempC2);

            var tempA = BerekenStartTariefA(tempB, tempT1, tempC1);

            // Test berekening om weer uit te komen op C1 en C2
            var tempCost1 = tempA + (tempB * tempT1);
            var tempCost2 = tempA + (tempB * tempT2);

            tempA = Math.Round(tempA, argRoundDecimals);
            tempB = Math.Round(tempB, argRoundDecimals);

            BelTarieven tempTarieven = new BelTarieven() { dStartTarief = tempA, dMinuutTarief = tempB };

            return tempTarieven;
        }

        private static double BerekenStartTariefA(double argBMinuutTarief, double argT1Minuut, double argC1TotaalKosten)
        {
            // In stappen Beschrijven

            // A = 0.00538C1 - (0.11667T1 * B)

            var tempA = argC1TotaalKosten - (argT1Minuut * argBMinuutTarief);
            if (tempA.ToString().StartsWith("-"))
                tempA = (argT1Minuut * argBMinuutTarief) - argC1TotaalKosten;
            return tempA;
        }

        private static double BerekenMinuutTariefB(double argT1Minuut, double argC1TotaalKosten, double argT2Minuut, double argC2TotaalKosten)
        {
            // In stappen beschrijven

            // A + (B * 0.11667T1) = 0.00538 C1
            // A + (B * 0.05T2) = 0.00288 C2

            // A = 0.00538C1 - (B * 0.11667T1)
            // A = 0.00288C2 - (B * 0.05T2)

            // 0.00538C1 - (B * 0.11667T1) =  0.00288C2 - (B * 0.05T2)

            // 0.00538C1 - 0.11667T1B =  0.00288C2 - 0.05T2B
            // 0.00538C1 = 0.00288C2 - 0.05T2B + 0.11667T1B

            // 0.00538C1 = 0.00288C2 + 0.06667T3B
            var tempT3 = -argT2Minuut + argT1Minuut;

            // 0.00538C1 - 0.00288C2 = 0.06667T3B
            // 0.0025D1 = 0.06667T3B
            var tempD1 = argC1TotaalKosten - argC2TotaalKosten;

            // B = 0.0025D1 / 0.06667T3

            var tempB = tempD1 / tempT3;

            return tempB;
        }
    }
}
