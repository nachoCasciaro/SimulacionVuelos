using System;
using System.Collections.Generic;
using System.Linq;

namespace SimulacionVuelos
{
    class Program
    {
        static void Main(string[] args)
        {
            int T = 0;
            int TPLL = 0;
            int TF = 1000000;
            List<int> DA = new List<int>();
            List<int> TPD = new List<int>();
            int IA;
            int DE;
            int CLL = 0;
            int TR = 0;
            int RAR = 0;
            int TMAX = 4000;
            int SC = 0;
            int ARRNC = 0;
            int ARRNE = 0;
            int CESC = 0;
            int CE1 = 0;
            int CE2 = 0;
            int CE3 = 0;
            int TCEC = 0;
            int TCE3 = 0;
            int TCE2 = 0;
            int TCE1 = 0;

            InicializarDisponibilidadAsientos(ref DA);
            InicializarTPD(ref TPD);

            Random r = new Random();
            do
            {

                int I = BuscarMenorTPD(TPD);
                if (TPLL <= TPD.ElementAt(I))
                {
                    //Rama llegada
                    T = TPLL;
                    IA = generarIA();
                    TPLL = T + IA;
                    if (HayAsientosLibres(DA))
                    {
                        //Compra o reserva
                        CLL++;
                        double R = r.NextDouble();

                        int asientoAComprarOReservar;
                        if (R <= 0.6)
                        {
                            //Me fijo en primera
                            asientoAComprarOReservar = ReservarAsientoEnPrimeraOResto(DA, ref SC, ref ARRNC);
                        }
                        else if (R <= 0.7)
                        {
                            //Me fijo en ejecutiva
                            asientoAComprarOReservar = ReservarAsientoEnEjecutivaOResto(DA, ref SC, ref ARRNC);
                        }
                        else
                        {
                            //Me fijo en turista
                            asientoAComprarOReservar = ReservarAsientoEnTuristaOResto(DA, ref SC, ref ARRNC);
                        }

                        CompraOReserva(ref TPD, ref DA, ref TR, asientoAComprarOReservar, ref RAR, TMAX, T);
                    }
                    else
                    {
                        //COla de espera
                        ColaDeEsperaOArrepentido(ref CLL, ref ARRNE, ref CESC, ref CE3, ref CE2, ref CE1, ref TCEC, ref TCE3, ref TCE2, ref TCE1);
                    }
                }
                else
                {
                    //Rama definicion de asiento
                    T = TPD.ElementAt(I);

                    //El 70% confirma la reserva, el resto la cancela y se elige por prioridad de la cola de espera
                    double r2 = r.NextDouble();
                    if (r2 < 0.7)
                    {
                        DA[I] = 1;
                    }
                    else
                    {
                        bool hayGenteEsperando = CESC + CE1 + CE2 + CE3 > 0;
                        if (hayGenteEsperando)
                        {
                            ElegirPersonaDeLaColaDeEsperaPorPrioridad(ref CESC, ref CE3, ref CE2, ref CE1);

                            PreguntarSiElAsientoLiberadoEsElAdecuado(I, ref SC, ref ARRNC);

                            CompraOReserva(ref TPD, ref DA, ref TR, I, ref RAR, TMAX, T);
                        }
                    }

                }

                Console.WriteLine(T);
            } while (T < TF);

            CalcularResultados(SC, CLL, ARRNC, ARRNE, CESC, CE1, CE2, CE3, TCEC, TCE3, TCE2, TCE1, RAR, TR);
        }

        public static void InicializarDisponibilidadAsientos(ref List<int> asientos)
        {
            for (int i = 0; i < 150; i++)
            {
                asientos.Add(0);
            }
        }

        public static void InicializarTPD(ref List<int> tpds)
        {
            for (int i = 0; i < 150; i++)
            {
                tpds.Add(999999999);
            }
        }

        public static int BuscarMenorTPD(List<int> tpds)
        {
            int smallestValue = tpds.OrderBy(x => x).FirstOrDefault();
            return tpds.IndexOf(smallestValue);
        }

        public static bool HayAsientosLibres(List<int> asientos)
        {
            return asientos.Any(x => x == 0);
        }

        public static int ReservarAsientoEnPrimeraOResto(List<int> asientos, ref int SC, ref int ARRNC)
        {
            int asiento = BuscarAsientoLibreEnEjecutiva(asientos);

            if (asiento == 0)
            {
                //No encontro en Primera busco en ejecutiva o turista
                asiento = BuscarAsientoLibreEnEjecutivaOTurista(asientos);

                // El 80% acepta la clase inferior, el resto se arrepiente
                Random r = new Random();
                double R = r.NextDouble();

                if (R <= 0.8)
                {
                    //Generar PSCI 
                    int PSCI = 60; //TODO Cambiar FDP
                    SC += PSCI;
                }
                else
                {
                    ARRNC++;
                }
            }
            else
            {
                //Encontro en primera
                SC += 100;
            }

            //Devuelvo la posicion del asiento asi se compra o reserva luego
            return asiento;
        }

        public static int ReservarAsientoEnEjecutivaOResto(List<int> asientos, ref int SC, ref int ARRNC)
        {
            int asiento = BuscarAsientoLibreEnEjecutiva(asientos);

            if (asiento == 0)
            {
                //No encontro en Ejecutiva busco en Primera
                asiento = BuscarAsientoLibreEnPrimera(asientos);

                if (asiento == 0)
                {
                    //Si no encuentra en primera, busco en turista
                    asiento = BuscarAsientoLibreEnETurista(asientos);

                    if (asiento == 0)
                    {
                        // No hay asiento de turista
                        ARRNC++;
                    }
                    else
                    {
                        Random r4 = new Random();
                        double R4 = r4.NextDouble();

                        if (R4 < 0.2)
                        {
                            //No acepta el asiento de turisa
                            ARRNC++;
                        }
                        else
                        {
                            // Acepta asiento en turista
                            int PSCI2 = 55; //TODO GENERAR FDP
                            SC += PSCI2;
                        }
                    }
                }
                else
                {
                    //Si encuentra en primera, el 70% acepta, el resto pregunto si quiere turista

                    Random r2 = new Random();
                    double R2 = r2.NextDouble();

                    if (R2 <= 0.3)
                    {
                        asiento = BuscarAsientoLibreEnETurista(asientos);

                        if (asiento == 0)
                        {
                            //No encuentra en turista
                            ARRNC++;
                        }
                        else
                        {
                            Random r3 = new Random();
                            double R3 = r3.NextDouble();

                            if (R3 < 0.2)
                            {
                                // No acepta el asiento en turista
                                ARRNC++;
                            }
                            else
                            {
                                //Acepta clase inferior
                                int PSCI = 55; //TODO GENERAR FDP
                                SC += PSCI;

                            }
                        }
                    }
                    else
                    {
                        int PSCS = 55; //TODO GENERAR FDP
                        SC += PSCS;
                    }
                }

                // El 80% acepta la clase inferior, el resto se arrepiente
                Random r = new Random();
                double R = r.NextDouble();

                if (R <= 80)
                {
                    //Generar PSCI 
                    int PSCI = 60; //TODO Cambiar FDP
                    SC += PSCI;
                }
                else
                {
                    ARRNC++;
                }
            }
            else
            {
                //Encontro en ejecutiva
                SC += 100;
            }

            //Devuelvo la posicion del asiento asi se compra o reserva luego
            return asiento;
        }

        public static int ReservarAsientoEnTuristaOResto(List<int> asientos, ref int SC, ref int ARRNC)
        {
            int asiento = BuscarAsientoLibreEnETurista(asientos);

            if (asiento == 0)
            {
                //No encontro en Turista busco en ejecutiva o primera
                asiento = BuscarAsientoLibreEnPrimeraOEjecutiva(asientos);

                // El 80% acepta la clase superior, el resto se arrepiente
                Random r = new Random();
                double R = r.NextDouble();

                if (R <= 0.7)
                {
                    //Generar PSCS
                    int PSCS = 60; //TODO Cambiar FDP
                    SC += PSCS;
                }
                else
                {
                    ARRNC++;
                }
            }
            else
            {
                //Encontro en turista
                SC += 100;
            }

            //Devuelvo la posicion del asiento asi se compra o reserva luego
            return asiento;
        }

        public static int BuscarAsientoLibreEnPrimera(List<int> asientos)
        {
            for (int i = 0; i < 8; i++)
            {
                if (asientos.ElementAt(i) == 0)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int BuscarAsientoLibreEnEjecutivaOTurista(List<int> asientos)
        {
            for (int i = 8; i < 150; i++)
            {
                if (asientos.ElementAt(i) == 0)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int BuscarAsientoLibreEnEjecutiva(List<int> asientos)
        {
            for (int i = 8; i < 30; i++)
            {
                if (asientos.ElementAt(i) == 0)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int BuscarAsientoLibreEnETurista(List<int> asientos)
        {
            for (int i = 30; i < 150; i++)
            {
                if (asientos.ElementAt(i) == 0)
                {
                    return i;
                }
            }

            return 0;
        }

        public static int BuscarAsientoLibreEnPrimeraOEjecutiva(List<int> asientos)
        {
            for (int i = 0; i < 30; i++)
            {
                if (asientos.ElementAt(i) == 0)
                {
                    return i;
                }
            }

            return 0;
        }


        public static void CompraOReserva(ref List<int> tpds, ref List<int> da, ref int TR, int I, ref int RAR, int TMAX, int T)
        {
            Random r = new Random();
            double R = r.NextDouble();

            if (R <= 0.7)
            {
                //Reserva
                TR = TR + 1;
                da[I] = 2;
                int DE = generarDE();

                if (DE < TMAX)
                {
                    tpds[I] = T + DE;
                }
                else
                {
                    da[I] = 0;
                    RAR++;
                }
            }
            else
            {
                da[I] = 1;
            }

        }

        public static void ColaDeEsperaOArrepentido(ref int CLL, ref int ARRNE, ref int CESC, ref int CE3, ref int CE2, ref int CE1, ref int TCEC, ref int TCE3, ref int TCE2, ref int TCE1)
        {
            Random r = new Random();
            double R = r.NextDouble();

            if (R < 0.8)
            {
                // Meter en la cola de espera
                CLL++;

                ColaDeEspera(ref CESC, ref CE3, ref CE2, ref CE1, ref TCEC, ref TCE3, ref TCE2, ref TCE1);
            }
            else
            {
                // No quiere esperar, se arrepiente
                ARRNE++;
            }
        }

        public static void ColaDeEspera(ref int CESC, ref int CE3, ref int CE2, ref int CE1, ref int TCEC, ref int TCE3, ref int TCE2, ref int TCE1)
        {
            Random r = new Random();
            double R1 = r.NextDouble();
            double R2 = r.NextDouble();
            double R3 = r.NextDouble();

            if (R1 < 0.5)
            {
                CESC++;
                TCEC++;
            }
            else if (R2 < 0.7)
            {
                CE3++;
                TCE3++;
            }
            else if (R3 < 0.85)
            {
                CE2++;
                TCE2++;
            }
            else
            {
                CE1++;
                TCE1++;
            }
        }

        public static void ElegirPersonaDeLaColaDeEsperaPorPrioridad(ref int CESC, ref int CE3, ref int CE2, ref int CE1)
        {
            if (CE1 > 0)
            {
                CE1--;
            }
            else if (CE2 > 0)
            {
                CE2--;
            }
            else if (CE3 > 0)
            {
                CE3--;
            }
            else
            {
                CESC--;
            }
        }

        public static void PreguntarSiElAsientoLiberadoEsElAdecuado(int I, ref int SC, ref int ARRNC)
        {
            Random r = new Random();
            double R = r.NextDouble();
            double R3 = r.NextDouble();

            //El 60% desea un asiento de primera
            if (R < 0.6)
            {
                // Me fijo si el asiento liberado es de primera
                if (I >= 0 && I <= 7)
                {
                    // El asiento liberado es el que preferia el cliente
                    SC += 90;
                }
                else
                {
                    // El asiento liberado no es de primera, es una clase inferior
                    // El 80% acepta una clase inferior, el resto se arrepiente
                    double R2 = r.NextDouble();
                    if (R2 < 0.8)
                    {
                        SC += 80;
                    }
                    else
                    {
                        ARRNC++;
                    }
                }

                //El 70% desea un asiento de ejecutiva REVEER LOS PORCENTAJES
            }
            else if (R3 < 0.7)
            {
                // Me fijo si el asiento liberado es de ejecutiva
                if (I >= 8 && I <= 29)
                {
                    // El asiento liberado es el que preferia el cliente
                    SC += 90;
                }
                // Me fijo si el asiento liberado es de turista
                else if (I >= 30 && I <= 149)
                {
                    // El 80% acepta una clase inferior, el resto se arrepiente
                    double R2 = r.NextDouble();
                    if (R2 < 0.8)
                    {
                        SC += 80;
                    }
                    else
                    {
                        ARRNC++;
                    }
                }
                // El asiento liberado es de primera
                else
                {
                    // El 70% acepta una clase superior resto se arrepiente
                    double R2 = r.NextDouble();
                    if (R2 < 0.7)
                    {
                        SC += 80;
                    }
                    else
                    {
                        ARRNC++;
                    }
                }

            }
            //El resto desea un asiento de turista 
            else
            {
                // Me fijo si el asiento liberado es de turista
                if (I >= 30 && I <= 149)
                {
                    // El asiento liberado es el que preferia el cliente
                    SC += 90;
                }
                else
                {
                    // El 70% acepta una clase superior resto se arrepiente
                    double R2 = r.NextDouble();
                    if (R2 < 0.7)
                    {
                        SC += 80;
                    }
                    else
                    {
                        ARRNC++;
                    }
                }
            }
        }

        public static void CalcularResultados(int SC, int CLL, int ARRNC, int ARRNE, int CESC, int CE1, int CE2, int CE3, int TCEC, int TCE3, int TCE2, int TCE1, int RAR, int TR)
        {
            long PS;
            int PCE1;
            int PCE2;
            int PCE3;
            int PCEC;
            int PARRNE;
            int PARRNC;
            int PRRA;

            PS = SC / CLL;
            PCE1 = CE1 * 100 / TCE1;
            PCE2 = CE2 * 100 / TCE2;
            PCE3 = CE3 * 100 / TCE3;
            PCEC = CESC * 100 / TCEC;
            PARRNE = ARRNE * 100 / CLL;
            PARRNC = ARRNC * 100 / CLL;
            PRRA = RAR * 100 / TR;

            Console.WriteLine("El PS es: " + PS);
            Console.WriteLine("El PCE1 es: " + PCE1);
            Console.WriteLine("El PCE2 es: " + PCE2);
            Console.WriteLine("El PCE3 es: " + PCE3);
            Console.WriteLine("El PCEC es: " + PCEC);
            Console.WriteLine("El PARRNE es: " + PARRNE);
            Console.WriteLine("El PARRNC es: " + PARRNC);
            Console.WriteLine("El PRRA es: " + PRRA);
        }


        public static int generarIA()
        {
            Random r = new Random();
            double R = r.NextDouble();
            return Convert.ToInt32(Math.Log(-R + 1) / (-0.0046));
        }

        public static int generarDE()
        {
            Random r = new Random();
            double R = r.NextDouble();
            //Cambiar esto
            return 203074 * R + ;
        }

        public static double generarPSPS()
        {
            Random r = new Random();
            double R = r.NextDouble();
            return 21 * R + 63;
        }

        public static double generarPSPI()
        {
            Random r = new Random();
            double R = r.NextDouble();
            return 20 * R + 5;
        }
    }

}
