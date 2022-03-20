/* MIT License

Copyright (c) 2021 JotunnLib Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using BepInEx.Logging;

namespace FTKAPI.Utils {
    /// <summary>
    ///     A namespace wide Logger class, which automatically creates a ManualLogSource
    ///     for every Class from which it is being called.
    /// </summary>
    public class Logger {
        /// <summary>
        ///     Add DateTime to the log output
        /// </summary>
        public static bool ShowDate = false;

        private static Logger instance;

        private readonly Dictionary<string, ManualLogSource> logger = new Dictionary<string, ManualLogSource>();

        /// <summary>
        ///     Singleton init
        /// </summary>
        internal static void Init() {
            if (instance == null) {
                instance = new Logger();
            }
        }

        /// <summary>
        ///     Remove and clear all Logger instances
        /// </summary>
        internal static void Destroy() {
            LogDebug("Destroying Logger");

            foreach (var entry in instance.logger) {
                BepInEx.Logging.Logger.Sources.Remove(entry.Value);
            }

            instance.logger.Clear();
        }

        /// <summary>
        ///     Get or create a <see cref="ManualLogSource"/> with the callers <see cref="Type.FullName"/>
        /// </summary>
        /// <returns>A BepInEx <see cref="ManualLogSource"/></returns>
        private ManualLogSource GetLogger() {
            var type = new StackFrame(3).GetMethod().DeclaringType;

            ManualLogSource ret;
            if (!this.logger.TryGetValue(type.FullName, out ret)) {
                ret = BepInEx.Logging.Logger.CreateLogSource(type.FullName);
                this.logger.Add(type.FullName, ret);
            }

            return ret;
        }

        private static void Log(LogLevel level, object data) {
            if (ShowDate) {
                instance.GetLogger().Log(level, $"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {data}");
            }
            else {
                instance.GetLogger().Log(level, data);
            }
        }

        /// <summary>
        ///     Logs a message with <see cref="BepInEx.Logging.LogLevel.Fatal"/> level.
        /// </summary>
        /// <param name="data">Data to log</param>
        public static void LogFatal(object data) => Log(LogLevel.Fatal, data);

        /// <summary>
        ///     Logs a message with <see cref="BepInEx.Logging.LogLevel.Fatal"/> level.
        /// </summary>
        /// <param name="data">Data to log</param>
        public static void LogError(object data) => Log(LogLevel.Error, data);

        /// <summary>
        ///     Logs a message with <see cref="BepInEx.Logging.LogLevel.Warning"/> level.
        /// </summary>
        /// <param name="data">Data to log</param>
        public static void LogWarning(object data) => Log(LogLevel.Warning, data);

        /// <summary>
        ///     Logs a message with <see cref="BepInEx.Logging.LogLevel.Message"/> level.
        /// </summary>
        /// <param name="data">Data to log</param>
        public static void LogMessage(object data) => Log(LogLevel.Message, data);

        /// <summary>
        ///     Logs a message with <see cref="BepInEx.Logging.LogLevel.Info"/> level.
        /// </summary>
        /// <param name="data">Data to log</param>
        public static void LogInfo(object data) => Log(LogLevel.Info, data);

        /// <summary>
        ///     Logs a message with <see cref="BepInEx.Logging.LogLevel.Debug"/> level.
        /// </summary>
        /// <param name="data">Data to log</param>
        public static void LogDebug(object data) => Log(LogLevel.Debug, data);
    }
}