using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CrossHairPlus.Models;

namespace CrossHairPlus.Services
{
    /// <summary>
    /// Manages process detection and settings (injection disabled for now)
    /// </summary>
    public class InjectionManager : IDisposable
    {
        private bool _isDisposed;

        // Known game executable names for auto-detection
        private static readonly string[] KnownGames = new[]
        {
            "KingdomCome",
            "cs2",
            "valorant",
            "fortnite",
            "pubg",
            "apex Legends",
            "overwatch2",
            "destiny2",
            "dota2",
            "warframe"
        };

        public event EventHandler<bool>? OnInjectionStateChanged;
        public event EventHandler<string>? OnStatusChanged;

        public bool IsInjected => false;
        public Process? InjectedProcess => null;

        /// <summary>
        /// Gets a list of running game processes
        /// </summary>
        public List<ProcessInfo> GetRunningGames()
        {
            var games = new List<ProcessInfo>();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    if (process.Id == 0 || process.Id == 4) continue;
                    if (string.IsNullOrEmpty(process.ProcessName)) continue;

                    var name = process.ProcessName.ToLowerInvariant();

                    if (KnownGames.Any(g => name.Contains(g.ToLowerInvariant())))
                    {
                        games.Add(new ProcessInfo
                        {
                            Id = process.Id,
                            Name = process.ProcessName,
                            Title = GetProcessTitle(process)
                        });
                    }
                }
                catch
                {
                    // Skip processes we can't access
                }
            }

            return games.OrderBy(g => g.Name).ToList();
        }

        private string GetProcessTitle(Process process)
        {
            try
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    return process.MainWindowTitle;
                }
            }
            catch { }
            return "";
        }

        /// <summary>
        /// Injection is disabled - overlay hook code needs cleanup
        /// </summary>
        public bool Inject(Process targetProcess)
        {
            OnStatusChanged?.Invoke(this, "Injection is temporarily disabled");
            return false;
        }

        /// <summary>
        /// Not available - injection disabled
        /// </summary>
        public bool SendSettings(CrosshairSettings settings)
        {
            return false;
        }

        /// <summary>
        /// Not available - injection disabled
        /// </summary>
        public void Uninject()
        {
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Information about a running game process
    /// </summary>
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";

        public override string ToString() => $"{Name} (ID: {Id})";
    }
}
