using UnityEngine;

namespace iffnsStuff.iffnsPhysics
{
    public class IffnsAtmosphere
    {
        //Valid up to 32km
        //ToDo: Check values when using ground temperatures other than 15°C

        /*
        Based on ICAO standard atmosphere:
        ----------------------------------

        Sources:
        - https://en.wikipedia.org/wiki/International_Standard_Atmosphere#ICAO_Standard_Atmosphere
        - https://en.wikipedia.org/wiki/Speed_of_sound#Practical_formula_for_dry_air
        - https://en.wikipedia.org/wiki/Barometric_formula
            
        3 layers:
        - Troposphere below 11km: Temerature decreases linearly
        - Tropopause between 11..20km: Temerature remains constant
        - Stratosphere above 20...32km: Temperature increases lnearly

        Values:
        - Temperature calculated based on altitude and laps rate
        - Mach number calculated based on temperature
        - Pressure calculated using the barometric formula
        - Equivalent air speed based on density ratio

        Units:
        - Temperature in K
        - Air pressure in Pa
        - Altitude in m
        - Speed in m/s
        - Lapse rate in K/m
        - Acceleratiion in m/s^2

        */

        const float EndOfTroposphereAltitude = 11000;
        const float EndOfTropopauseAltitude = 20000;
        const float gravitationAcceleration = 9.80665f;
        const float MolarMassOfAir = 0.0289644f;
        const float UniversalGasConstant = 8.3144598f;

        const float TroposphereLapseRate = -0.006516112569f;
        const float StratosphereLapseRate = 0.0009842519685f;

        readonly float seaLevelTemperature;
        readonly float seaLevelAirPressure;

        readonly float EndOfTroposphereTemperature;
        readonly float EndOfTropospherePressure;
        readonly float EndOfTropopausePressure;

        public IffnsAtmosphere(float seaLevelTemperatureK = 273.15f + 15f, float seaLevelAirPressurePa = 101325f)
        {
            seaLevelTemperature = seaLevelTemperatureK;
            seaLevelAirPressure = seaLevelAirPressurePa;

            EndOfTroposphereTemperature = seaLevelTemperatureK + TroposphereLapseRate * EndOfTroposphereAltitude;

            EndOfTropospherePressure = GetTropospherePressure(EndOfTroposphereAltitude);

            EndOfTropopausePressure = GetTropopausePressure(EndOfTropopauseAltitude);
        }

        public float TemperatureAtAltitude(float altitude)
        {
            if (altitude < EndOfTroposphereAltitude)
            {
                return GetTroposphereTemperature(altitude);
            }
            else if (altitude < EndOfTropopauseAltitude)
            {
                return EndOfTroposphereTemperature;
            }
            else
            {
                return GetStratosphereTemperature(altitude);
            }
        }

        public float SpeedOfSoundAtAltitude(float altitude)
        {
            // Based only on air temperature
            return 20.05f * Mathf.Sqrt(TemperatureAtAltitude(altitude));
        }

        public float MachNumber(float speed, float altitude)
        {
            return speed / SpeedOfSoundAtAltitude(altitude);
        }

        public float AirPressureAtAltitude(float altitude)
        {
            if (altitude < EndOfTroposphereAltitude)
            {
                return GetTropospherePressure(altitude);
            }
            else if (altitude < EndOfTropopauseAltitude)
            {
                return GetTropopausePressure(altitude);
            }
            else
            {
                return GetStratospherePressure(altitude);
            }
        }

        public float PressureRatio(float altitude)
        {
            return AirPressureAtAltitude(altitude) / seaLevelAirPressure;
        }

        public float TemperatureRatio(float altitude)
        {
            return TemperatureAtAltitude(altitude) / seaLevelTemperature;
        }

        public float DensityRatio(float altitude)
        {
            return PressureRatio(altitude) / TemperatureRatio(altitude);
        }

        public float EquivalentAirSpeed(float trueAirSpeed, float altitude)
        {
            return trueAirSpeed * Mathf.Sqrt(DensityRatio(altitude));
        }

        public float TrueAirSpeed(float equivalentAirSpeed, float altitude)
        {
            return equivalentAirSpeed / Mathf.Sqrt(DensityRatio(altitude));
        }

        float GetTroposphereTemperature(float altitude)
        {
            return seaLevelTemperature + altitude * TroposphereLapseRate;
        }

        float GetTropospherePressure(float altitude)
        {
            float temperature = GetTroposphereTemperature(altitude);

            //Barometric formula if temperature laps rate is not 0
            float temperatureRatio = temperature / seaLevelTemperature;

            float exponent = -gravitationAcceleration * MolarMassOfAir / UniversalGasConstant / TroposphereLapseRate;

            return seaLevelAirPressure * Mathf.Pow(temperatureRatio, exponent);
        }

        float GetTropopausePressure(float altitude)
        {
            //Barometric formula if temperature laps rate is 0
            float exponent = -gravitationAcceleration * MolarMassOfAir * (altitude - EndOfTroposphereAltitude) / UniversalGasConstant / EndOfTroposphereTemperature;

            return EndOfTropospherePressure * Mathf.Exp(exponent);
        }

        float GetStratosphereTemperature(float altitude)
        {
            return EndOfTroposphereTemperature + altitude * StratosphereLapseRate;
        }

        float GetStratospherePressure(float altitude)
        {
            float temperature = GetStratosphereTemperature(altitude);

            //Barometric formula if temperature laps rate is not 0
            float temperatureRatio = temperature / EndOfTroposphereTemperature;

            float exponent = -gravitationAcceleration * MolarMassOfAir / UniversalGasConstant / StratosphereLapseRate;

            return EndOfTropopausePressure * Mathf.Pow(temperatureRatio, exponent);
        }
    }
}