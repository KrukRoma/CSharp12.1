using System;
using System.Collections.Generic;
using System.Timers;
using Timer = System.Timers.Timer;
namespace CSharp12._1
{
    public class Exchange
    {
        private readonly Random _random = new Random();
        private double _currentRate;
        private readonly Timer _timer;

        public event Action<double> RateChanged;
        public event Action<double> MaxRateReached;
        public event Action<double> MinRateReached;

        public double MaxRate { get; set; }
        public double MinRate { get; set; }

        public Exchange(double initialRate, double maxRate, double minRate, int interval)
        {
            _currentRate = initialRate;
            MaxRate = maxRate;
            MinRate = minRate;
            _timer = new Timer(interval);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            ChangeRate();
        }

        private void ChangeRate()
        {
            _currentRate += _random.NextDouble() * 2 - 1; 
            RateChanged?.Invoke(_currentRate);

            if (_currentRate >= MaxRate)
            {
                MaxRateReached?.Invoke(_currentRate);
            }
            else if (_currentRate <= MinRate)
            {
                MinRateReached?.Invoke(_currentRate);
            }
        }
    }

    public class Trader
    {
        public string Name { get; }
        public double UahAmount { get; private set; }
        public double UsdAmount { get; private set; }
        public double CryptoAmount { get; private set; }

        public Trader(string name, double initialUah, double initialUsd)
        {
            Name = name;
            UahAmount = initialUah;
            UsdAmount = initialUsd;
            CryptoAmount = 0;
        }

        public void OnRateChanged(double rate)
        {
            Console.WriteLine($"{Name}: Rate changed to {rate}");
        }

        public void OnMaxRateReached(double rate)
        {
            double amountToSell = UsdAmount * 0.5; 
            UahAmount += amountToSell * rate;
            UsdAmount -= amountToSell;
            Console.WriteLine($"{Name}: Max rate reached. Sold {amountToSell} USD. New UAH balance: {UahAmount}");
        }

        public void OnMinRateReached(double rate)
        {
            
            double amountToBuy = UahAmount / rate * 0.5; 
            UahAmount -= amountToBuy * rate;
            UsdAmount += amountToBuy;
            Console.WriteLine($"{Name}: Min rate reached. Bought {amountToBuy} USD. New USD amount: {UsdAmount}");
        }

        public void BuyCrypto(double rate)
        {
            double usdToSpend = UsdAmount * 0.1; 
            double cryptoToBuy = usdToSpend / rate;
            UsdAmount -= usdToSpend;
            CryptoAmount += cryptoToBuy;
            Console.WriteLine($"{Name}: Bought {cryptoToBuy} crypto. New crypto amount: {CryptoAmount}");
        }
    }

    internal class Program
    {
        private static void Main()
        {
            var exchange = new Exchange(100, 120, 80, 1000);
            var trader1 = new Trader("Alice", 10000, 5000);
            var trader2 = new Trader("Bob", 20000, 10000);

            exchange.RateChanged += trader1.OnRateChanged;
            exchange.RateChanged += trader2.OnRateChanged;

            exchange.MaxRateReached += trader1.OnMaxRateReached;
            exchange.MaxRateReached += trader2.OnMaxRateReached;

            exchange.MinRateReached += trader1.OnMinRateReached;
            exchange.MinRateReached += trader2.OnMinRateReached;

            exchange.Start();

            System.Threading.Thread.Sleep(5000);

            trader1.BuyCrypto(1000); 
            trader2.BuyCrypto(1000); 

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            exchange.Stop();
        }
    }
}

