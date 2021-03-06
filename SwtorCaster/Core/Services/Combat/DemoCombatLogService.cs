namespace SwtorCaster.Core.Services.Combat
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using Caliburn.Micro;
    using Domain.Log;
    using Events;
    using Extensions;
    using Factory;
    using Images;
    using Parsing;
    using Settings;

    public class DemoCombatLogService : ICombatLogService
    {
        private readonly ICombatLogViewModelFactory _logViewModelFactory;
        private readonly ICombatLogParser _logParser;
        private readonly ISettingsService _settingsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IEventService _eventService;

        public bool IsRunning { get; private set; }
        private Thread _parserThread;

        public DemoCombatLogService(
            ISettingsService settingsService,
            IEventAggregator eventAggregator,
            IEventService eventService,
            ICombatLogViewModelFactory logViewModelFactory,
            ICombatLogParser logParser)
        {
            _settingsService = settingsService;
            _eventAggregator = eventAggregator;
            _eventService = eventService;
            _logViewModelFactory = logViewModelFactory;
            _logParser = logParser;
        }

        public void Start()
        {
            if (_parserThread != null) return;
            _parserThread = new Thread(Run);
            _parserThread.Start();
        }

        private void Run()
        {
            IsRunning = File.Exists(_settingsService.Settings.CombatLogFile);

            if (IsRunning)
            {
                using (var fs = new FileStream(_settingsService.Settings.CombatLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    using (var reader = new StreamReader(fs))
                    {
                        while (IsRunning)
                        {
                            var value = reader.ReadLine();
                            TryRead(value);

                            if (reader.EndOfStream)
                            {
                                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                            }

                            Thread.Sleep(250);
                        }
                    }
                }
            }
        }

        private void TryRead(string value)
        {
            try
            {
                var combatLogEvent = _logParser.Parse(value);
                _eventService.Handle(combatLogEvent);
                Application.Current.Dispatcher.Invoke(() => Render(combatLogEvent));
            }
            catch (Exception e)
            {

            }
        }

        private void Render(CombatLogEvent combatLogEvent)
        {
            var logLine = _logViewModelFactory.Create(combatLogEvent);
            _eventAggregator.PublishOnUIThread(logLine);
        }

        public void Stop()
        {
            IsRunning = false;
            if (_parserThread == null) return;
            _parserThread.Abort();
            _parserThread = null;
        }
    }
}