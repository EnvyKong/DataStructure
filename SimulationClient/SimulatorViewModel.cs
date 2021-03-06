﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructure.ServerSimulation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Input;

namespace SimulationClient
{
    public class SimulatorViewModel : INotifyPropertyChanged
    {
        #region field
        private TaskGenerator generator;
        private LoadBalancer balancer;
        private DispatcherTimer timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public static RoutedCommand StartCommand = new RoutedCommand("Start", typeof(SimulatorViewModel),
            new InputGestureCollection() { new KeyGesture(Key.N, ModifierKeys.Control) });

        public static RoutedCommand StopCommand = new RoutedCommand("Stop", typeof(SimulatorViewModel),
            new InputGestureCollection() { new KeyGesture(Key.W, ModifierKeys.Control) });

        public static RoutedCommand ResetCommand = new RoutedCommand("Reset", typeof(SimulatorViewModel),
            new InputGestureCollection() { new KeyGesture(Key.R, ModifierKeys.Control) });

        public static RoutedCommand AboutCommand = new RoutedCommand("About", typeof(SimulatorViewModel),
            new InputGestureCollection() { new KeyGesture(Key.A, ModifierKeys.Control) });

        #endregion field

        #region property
        public ObservableCollection<Server> Cluster { get; set; }
        public Simulator Simulator { get; private set; }

        public RelayCommand AddServerCommand { get; private set; }
        public RelayCommand DeleteServerCommand { get; private set; }
        public RelayCommand ConfigCommand { get; private set; }

        #endregion property

        #region constructor
        public SimulatorViewModel()
        {
            Cluster = new ObservableCollection<Server>();
            generator = new TaskGenerator();
            balancer = new LoadBalancer();
            Simulator = new Simulator();
            AddServerCommand = new RelayCommand(AddServerExecute);
            DeleteServerCommand = new RelayCommand(DeleteServerExecute);
            ConfigCommand = new RelayCommand(ConfigExecute);
            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            timer.Tick += Refresh;
        }

        #endregion constructor

        #region method
        public void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void Refresh(object sender, EventArgs e)
        {
            Simulator.Simulate(balancer, generator);
        }
        
        public void AddServer(Server server)
        {
            Cluster.Add(server);
            balancer.AddServer(server);
        }

        public void DeleteServer(Server server)
        {

            Cluster.Remove(server);
            balancer.RemoveServer(server);
        }


        public void StartSimulation()
        {
            if (Cluster.Count == 0)
                AddServer(new Server());
            timer.Start();
        }

        public void StopSimulation()
        {
            timer.Stop();
        }

        public void ResetSimulation()
        {
            timer.Stop();
            Cluster.Clear();
            Server.Reset();
            Simulator.Reset();
            balancer.Reset();
            generator.Reset();
        }

        private void AddServerExecute()
        {
            AddServer(new Server());
        }

        private void DeleteServerExecute()
        {
            DeleteServer(Cluster.Last());
        }

        private void ConfigExecute()
        {
            timer.Stop();
            var config = new Config(timer.Interval.Milliseconds)
            {
                Possibility = Simulator.Possibility,
                Frequency = Simulator.Frequency,
                MinMemory = generator.MinMemory,
                MaxMemory = generator.MaxMemory,
                MinCpu = generator.MinCpu,
                MaxCpu = generator.MaxCpu,
                MinTime = generator.MinTime,
                MaxTime = generator.MaxTime
            };
            if (config.ShowDialog() == true)
            {
                generator.Config(config.MinMemory, config.MaxMemory,
                    config.MinCpu, config.MaxCpu,
                    config.MinTime, config.MaxTime);
                Simulator.Frequency = config.Frequency;
                Simulator.Possibility = config.Possibility;
                timer.Interval = new TimeSpan(0, 0, 0, 0, config.RefreshTime);
            }
            timer.Start();
        }

        #endregion method
    }
}
