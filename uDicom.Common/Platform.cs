/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: Platform.cs
////
//// Summary:
////
////
//// Date: 2014/08/19
//////////////////////////////////////////////////////////////////////////
#region License

// Copyright (c) 2011 - 2013, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UIH.Dicom.Common;

namespace uDicom.Common
{
    public enum LogLevel
    {
        /// <summary>
        /// Debug log level.
        /// </summary>
        Debug,

        /// <summary>
        /// Info log level.
        /// </summary>
        Info,

        /// <summary>
        /// Warning log level.
        /// </summary>
        Warn,

        /// <summary>
        /// Error log level.
        /// </summary>
        Error,

        /// <summary>
        /// Fatal log level.
        /// </summary>
        Fatal
    }

    public interface IDuplexServiceProvider
    {
        object GetService(Type type, object callback);
    }

    public class Platform
    {
        private static object _syncRoot = new Object();

        private static readonly ILog _log = LogManager.GetLog(typeof(Platform).ToString());
        private static readonly object _namedLogLock = new object();
        private static readonly Dictionary<string, ILog> _namedLogs = new Dictionary<string, ILog>();

        private static volatile IServiceProvider[] _serviceProviders;
        private static volatile IDuplexServiceProvider[] _duplexServiceProviders;

        protected static Platform _instance;
        public static Platform Instance 
        { 
            get 
            { 
                return _instance; 
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (null == _instance)
                    _instance = value;
                else
                    throw new ArgumentOutOfRangeException("PlatformInstance can be set only once.");
            }
        }

        #region Service Provision

        /// <summary>
        /// Obtains an instance of the specified service for use by the application.
        /// </summary>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
        /// <typeparam name="TService">The type of service to obtain.</typeparam>
        /// <returns>An instance of the specified service.</returns>
        /// <exception cref="UnknownServiceException">The requested service cannot be provided.</exception>
        public static TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }

        /// <summary>
        /// For use with the <see cref="GetService{TService}(WithServiceDelegate{TService})"/> method.
        /// </summary>
        public delegate void WithServiceDelegate<T>(T service);

        /// <summary>
        /// Obtains an instance of the specified service for use by the application.  
        /// </summary>
        /// <remarks>
        /// <para>
        /// Instead of returning the service directly, this overload passes the service to the specified delegate for use.
        /// When the delegate returns, this method automatically takes care of determing whether the service implements <see cref="IDisposable"/>
        /// and calling <see cref="IDisposable.Dispose"/> if it does.  The delegate must not cache the returned service
        /// because it may be disposed as soon as the delegate returns.  For the single-use scenario, this overload is preferred
        /// to the other overloads because it automatically manages the lifecycle of the service object.
        /// </para>
        /// <para>
        /// This method is thread-safe.
        /// </para>
        /// </remarks>
        /// <typeparam name="TService">The service to obtain.</typeparam>
        /// <param name="proc">A delegate that will receive the service for one-time use.</param>
        public static void GetService<TService>(WithServiceDelegate<TService> proc)
        {
            var service = GetService<TService>();

            try
            {
                proc(service);
            }
            finally
            {
                if (service is IDisposable)
                {
                    try
                    {
                        (service as IDisposable).Dispose();
                    }
                    catch (Exception e)
                    {
                        // do not allow exceptions thrown from Dispose() because it may have the effect of
                        // hiding an exception that was thrown from the service itself
                        // if the service fails to dispose properly, we don't care, just log it and move on
                        Log(LogLevel.Error, e);
                    }
                }
            }
        }

        /// <summary>
        /// Obtains an instance of the specified service for use by the application.  
        /// </summary>
        /// <remarks>
        /// <para>
        /// Instead of returning the service directly, this overload passes the service to the specified delegate for use.
        /// When the delegate returns, this method automatically takes care of determing whether the service implements <see cref="IDisposable"/>
        /// and calling <see cref="IDisposable.Dispose"/> if it does.  The delegate must not cache the returned service
        /// because it may be disposed as soon as the delegate returns.  For the single-use scenario, this overload is preferred
        /// to the other overloads because it automatically manages the lifecycle of the service object.
        /// </para>
        /// <para>
        /// This method is thread-safe.
        /// </para>
        /// </remarks>
        /// <typeparam name="TService">The service to obtain.</typeparam>
        /// <typeparam name="TResult">The type of the function result.</typeparam>
        /// <param name="func">A delegate that will receive the service for one-time use.</param>
        public static TResult GetService<TService, TResult>(Func<TService, TResult> func)
        {
            TResult result = default(TResult);
            GetService<TService>(svc => result = func.Invoke(svc));
            return result;
        }

        /// <summary>
        /// Obtains an instance of the specified service for use by the application.
        /// </summary>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
        /// <param name="service">The type of service to obtain.</param>
        /// <returns>An instance of the specified service.</returns>
        /// <exception cref="UnknownServiceException">The requested service cannot be provided.</exception>
        public static object GetService(Type service)
        {
            // load all service providers if not yet loaded
            if (_serviceProviders == null)
            {
                lock (_syncRoot)
                {
                    if (_serviceProviders == null)
                    {
                        _serviceProviders = IoC.GetAll<IServiceProvider>().ToArray();
                    }
                }
            }

            // attempt to instantiate the requested service
            foreach (IServiceProvider sp in _serviceProviders)
            {
                // the service provider itself may not be thread-safe, so we need to ensure only one thread will access it
                // at a time
                lock (sp)
                {
                    object impl = sp.GetService(service);
                    if (impl != null)
                        return impl;
                }
            }

            var message = string.Format("No service provider was found that can provide the service {0}.", service.FullName);
            throw new Exception(message);
        }

        /// <summary>
        /// Obtains an instance of the specified duplex service for use by the application.
        /// </summary>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
        /// <typeparam name="TService">The type of service to obtain.</typeparam>
        /// <typeparam name="TCallback">The type of the callback contract.</typeparam>
        /// <param name="callback">An object that implements the callback contract.</param>
        /// <returns>An instance of the specified service.</returns>
        /// <exception cref="UnknownServiceException">The requested service cannot be provided.</exception>
        public static TService GetDuplexService<TService, TCallback>(TCallback callback)
        {
            return (TService)GetDuplexService(typeof(TService), callback);
        }

        /// <summary>
        /// Obtains an instance of the specified duplex service for use by the application.
        /// </summary>
        /// <remarks>
        /// This method is thread-safe.
        /// </remarks>
        /// <param name="service">The type of service to obtain.</param>
        /// <param name="callback">An object implementing the callback service contract.</param>
        /// <returns>An instance of the specified service.</returns>
        /// <exception cref="UnknownServiceException">The requested service cannot be provided.</exception>
        public static object GetDuplexService(Type service, object callback)
        {
            CheckForNullReference(callback, "callback");

            // load all service providers if not yet loaded
            if (_duplexServiceProviders == null)
            {
                lock (_syncRoot)
                {
                    if (_duplexServiceProviders == null)
                    {
                        _duplexServiceProviders = IoC.GetAll<IDuplexServiceProvider>().ToArray();
                    }
                }
            }

            // attempt to instantiate the requested service
            foreach (IDuplexServiceProvider sp in _duplexServiceProviders)
            {
                // the service provider itself may not be thread-safe, so we need to ensure only one thread will access it
                // at a time
                lock (sp)
                {
                    object impl = sp.GetService(service, callback);
                    if (impl != null)
                        return impl;
                }
            }

            var message = string.Format("No duplex service provider was found that can provide the service {0}.", service.FullName);
            throw new Exception(message);
        }

        #endregion


        #region Logging

        /// <summary>
        /// Determines if the specified <see cref="LogLevel"/> is enabled.
        /// </summary>
        /// <param name="category">The logging level to check.</param>
        /// <returns>true if the <see cref="LogLevel"/> is enabled, or else false.</returns>
        public static bool IsLogLevelEnabled(LogLevel category)
        {
            return IsLogLevelEnabled((string)null, category);
        }

        /// <summary>
        /// Determines if the specified <see cref="LogLevel"/> is enabled for the named log.
        /// </summary>
        /// <param name="logName">The name of the log.</param>
        /// <param name="category">The logging level to check.</param>
        /// <returns>true if the <see cref="LogLevel"/> is enabled, or else false.</returns>
        public static bool IsLogLevelEnabled(string logName, LogLevel category)
        {
            var log = GetLog(logName);
            return IsLogLevelEnabled(log, category);
        }

        private static bool IsLogLevelEnabled(ILog log, LogLevel category)
        {
            //switch (category)
            //{
            //    case LogLevel.Debug:
            //        return log.IsDebugEnabled;
            //    case LogLevel.Info:
            //        return log.IsInfoEnabled;
            //    case LogLevel.Warn:
            //        return log.IsWarnEnabled;
            //    case LogLevel.Error:
            //        return log.IsErrorEnabled;
            //    case LogLevel.Fatal:
            //        return log.IsFatalEnabled;
            //}

            return true;
        }

        /// <summary>
        /// Logs the specified message at the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="category">The logging level.</param>
        /// <param name="message">The message to be logged.</param>
        public static void Log(LogLevel category, object message)
        {
            // Just return without formatting if the log level isn't enabled
            if (!IsLogLevelEnabled(category)) return;

            var ex = message as Exception;
            if (ex != null)
            {
                Log(_log, category, ex, null, null);
            }
            else
            {
                switch (category)
                {
                    case LogLevel.Debug:
                        _log.Debug(message.ToString());
                        break;
                    case LogLevel.Info:
                        _log.Info(message.ToString());
                        break;
                    case LogLevel.Warn:
                        _log.Warn(message.ToString());
                        break;
                    case LogLevel.Error:
                        _log.Error(message.ToString());
                        break;
                    case LogLevel.Fatal:
                        _log.Fatal(message.ToString());
                        break;
                }
            }
        }

        /// <summary>
        /// Logs the specified message at the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="category">The log level.</param>
        /// <param name="message">Format message, as used with <see cref="System.Text.StringBuilder"/>.</param>
        /// <param name="args">Optional arguments used with <paramref name="message"/>.</param>
        public static void Log(LogLevel category, String message, params object[] args)
        {
            Log(_log, category, null, message, args);
        }

        /// <summary>
        /// Logs the specified exception at the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="ex">The exception to log.</param>
        /// <param name="category">The log level.</param>
        /// <param name="message">Format message, as used with <see cref="System.Text.StringBuilder"/>.</param>
        /// <param name="args">Optional arguments used with <paramref name="message"/>.</param>
        public static void Log(LogLevel category, Exception ex, String message, params object[] args)
        {
            Log(_log, category, ex, message, args);
        }

        /// <summary>
        /// Logs the specified message at the specified <see cref="LogLevel"/>, to the log with the specified name.
        /// </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="logName"> </param>
        /// <param name="category">The log level.</param>
        /// <param name="message">Format message, as used with <see cref="System.Text.StringBuilder"/>.</param>
        /// <param name="args">Optional arguments used with <paramref name="message"/>.</param>
        public static void Log(string logName, LogLevel category, String message, params object[] args)
        {
            Log(logName, category, null, message, args);
        }

        /// <summary>
        /// Logs the specified exception at the specified <see cref="LogLevel"/>, to the log with the specified name.
        /// </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="ex">The exception to log.</param>
        /// <param name="logName">A named log.</param>
        /// <param name="category">The log level.</param>
        /// <param name="message">Format message, as used with <see cref="System.Text.StringBuilder"/>.</param>
        /// <param name="args">Optional arguments used with <paramref name="message"/>.</param>
        public static void Log(string logName, LogLevel category, Exception ex, String message, params object[] args)
        {
            Log(GetLog(logName), category, ex, message, args);
        }

        private static void Log(ILog log, LogLevel category, Exception ex, String message, object[] args)
        {
            if (IsLogLevelEnabled(log, category))
                Log(log, category, ex, GetLogMessage(ex != null, message, args));
        }

        private static void Log(ILog log, LogLevel category, Exception ex, string message)
        {
            if (log == null) return;

            if (ex == null)
            {
                switch (category)
                {
                    case LogLevel.Debug:
                        log.Debug(message);
                        break;
                    case LogLevel.Info:
                        log.Info(message);
                        break;
                    case LogLevel.Warn:
                        log.Warn(message);
                        break;
                    case LogLevel.Error:
                        log.Error(message);
                        break;
                    case LogLevel.Fatal:
                        log.Fatal(message);
                        break;
                }
            }
            else
            {
                switch (category)
                {
                    case LogLevel.Debug:
                        log.Debug(message, ex);
                        break;
                    case LogLevel.Info:
                        log.Info(message, ex);
                        break;
                    case LogLevel.Warn:
                        log.Warn(message, ex);
                        break;
                    case LogLevel.Error:
                        log.Error(message, ex);
                        break;
                    case LogLevel.Fatal:
                        log.Fatal(message, ex);
                        break;
                }
            }
        }

        private static string GetLogMessage(bool isExceptionLog, string message, params object[] args)
        {
            var sb = new StringBuilder();
            if (isExceptionLog)
            {
                sb.AppendLine("Exception thrown"); // note: it's log. Keep it in English
                sb.AppendLine();
            }

            if (!String.IsNullOrEmpty(message))
            {
                if (args == null || args.Length == 0)
                    sb.Append(message);
                else
                    sb.AppendFormat(message, args);
            }

            return sb.ToString();
        }

        private static ILog GetLog(string name)
        {
            if (String.IsNullOrEmpty(name))
                return _log;

            lock (_namedLogLock)
            {
                ILog log;
                if (!_namedLogs.TryGetValue(name, out log))
                {
                    log = LogManager.GetLog(name);
                    if (log != null)
                        _namedLogs[name] = log;
                }

                return log ?? _log;
            }
        }

        #endregion

        /// <summary>
        /// Checks if a string is empty.
        /// </summary>
        /// <param name="variable">The string to check.</param>
        /// <param name="variableName">The variable name of the string to checked.</param>
        /// <exception cref="ArgumentNullException"><paramref name="variable"/> or or <paramref name="variableName"/>
        /// is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="variable"/> is zero length.</exception>
        public static void CheckForEmptyString(string variable, string variableName)
        {
            CheckForNullReference(variable, variableName);
            CheckForNullReference(variableName, "variableName");

            if (variable.Length == 0)
                throw new ArgumentException(String.Format(SR.ExceptionEmptyString, variableName));
        }

        /// <summary>
        /// Checks if an object reference is null.
        /// </summary>
        /// <param name="variable">The object reference to check.</param>
        /// <param name="variableName">The variable name of the object reference to check.</param>
        /// <remarks>Use for checking if an input argument is <b>null</b>.  To check if a member variable
        /// is <b>null</b> (i.e., to see if an object is in a valid state), use <b>CheckMemberIsSet</b> instead.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="variable"/> or <paramref name="variableName"/>
        /// is <b>null</b>.</exception>
        public static void CheckForNullReference(object variable, string variableName)
        {
            if (variableName == null)
                throw new ArgumentNullException("variableName");

            if (null == variable)
                throw new ArgumentNullException(variableName);
        }

        /// <summary>
        /// Checks if an object is of the expected type.
        /// </summary>
        /// <param name="variable">The object to check.</param>
        /// <param name="type">The variable name of the object to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="variable"/> or <paramref name="type"/>
        /// is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not the expected type.</exception>
        public static void CheckExpectedType(object variable, Type type)
        {
            CheckForNullReference(variable, "variable");
            CheckForNullReference(type, "type");

            if (!type.IsAssignableFrom(variable.GetType()))
                throw new ArgumentException(String.Format(SR.ExceptionExpectedType, type.FullName));
        }

        /// <summary>
        /// Checks if a cast is valid.
        /// </summary>
        /// <param name="castOutput">The object resulting from the cast.</param>
        /// <param name="castInputName">The variable name of the object that was cast.</param>
        /// <param name="castTypeName">The name of the type the object was cast to.</param>
        /// <remarks>
        /// <para>To use this method, casts have to be done using the <b>as</b> operator.  The
        /// method depends on failed casts resulting in <b>null</b>.</para>
        /// <para>This method has been deprecated since it does not actually perform any
        /// cast checking itself and entirely relies on correct usage (which is not apparent
        /// through the Visual Studio Intellisence feature) to function as an exception message
        /// formatter. The recommended practice is to use the <see cref="CheckExpectedType"/>
        /// if the cast output need not be consumed, or use the direct cast operator instead.</para>
        /// </remarks>
        /// <example>
        /// <code>
        /// [C#]
        /// layer = new GraphicLayer();
        /// GraphicLayer graphicLayer = layer as GraphicLayer;
        /// // No exception thrown
        /// Platform.Instance.CheckForInvalidCast(graphicLayer, "layer", "GraphicLayer");
        ///
        /// ImageLayer image = layer as ImageLayer;
        /// // InvalidCastException thrown
        /// Platform.Instance.CheckForInvalidCast(image, "layer", "ImageLayer");
        /// </code>
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="castOutput"/>,
        /// <paramref name="castInputName"/>, <paramref name="castTypeName"/> is <b>null</b>.</exception>
        /// <exception cref="InvalidCastException">Cast is invalid.</exception>
        [Obsolete("Use Platform.Instance.CheckExpectedType or perform a direct cast instead.")]
        public static void CheckForInvalidCast(object castOutput, string castInputName, string castTypeName)
        {
            CheckForNullReference(castOutput, "castOutput");
            CheckForNullReference(castInputName, "castInputName");
            CheckForNullReference(castTypeName, "castTypeName");

            if (castOutput == null)
                throw new InvalidCastException(String.Format(SR.ExceptionInvalidCast, castInputName, castTypeName));
        }

        /// <summary>
        /// Checks if a value is positive.
        /// </summary>
        /// <param name="n">The value to check.</param>
        /// <param name="variableName">The variable name of the value to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="variableName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="n"/> &lt;= 0.</exception>
        public static void CheckPositive(int n, string variableName)
        {
            CheckForNullReference(variableName, "variableName");

            if (n <= 0)
                throw new ArgumentException(SR.ExceptionArgumentNotPositive, variableName);
        }

        /// <summary>
        /// Checks if a value is true.
        /// </summary>
        /// <param name="testTrueCondition">The value to check.</param>
        /// <param name="conditionName">The name of the condition to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="conditionName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="testTrueCondition"/> is  <b>false</b>.</exception>
        public static void CheckTrue(bool testTrueCondition, string conditionName)
        {
            CheckForNullReference(conditionName, "conditionName");

            if (testTrueCondition != true)
                throw new ArgumentException(String.Format(SR.ExceptionConditionIsNotMet, conditionName));
        }

        /// <summary>
        /// Checks if a value is false.
        /// </summary>
        /// <param name="testFalseCondition">The value to check.</param>
        /// <param name="conditionName">The name of the condition to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="conditionName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="testFalseCondition"/> is  <b>true</b>.</exception>
        public static void CheckFalse(bool testFalseCondition, string conditionName)
        {
            CheckForNullReference(conditionName, "conditionName");

            if (testFalseCondition != false)
                throw new ArgumentException(String.Format(SR.ExceptionConditionIsNotMet, conditionName));
        }

        /// <summary>
        /// Checks if a value is positive.
        /// </summary>
        /// <param name="x">The value to check.</param>
        /// <param name="variableName">The variable name of the value to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="variableName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="x"/> &lt;= 0.</exception>
        public static void CheckPositive(float x, string variableName)
        {
            CheckForNullReference(variableName, "variableName");

            if (x <= 0.0f)
                throw new ArgumentException(SR.ExceptionArgumentNotPositive, variableName);
        }

        /// <summary>
        /// Checks if a value is positive.
        /// </summary>
        /// <param name="x">The value to check.</param>
        /// <param name="variableName">The variable name of the value to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="variableName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="x"/> &lt;= 0.</exception>
        public static void CheckPositive(double x, string variableName)
        {
            CheckForNullReference(variableName, "variableName");

            if (x <= 0.0d)
                throw new ArgumentException(SR.ExceptionArgumentNotPositive, variableName);
        }

        /// <summary>
        /// Checks if a value is non-negative.
        /// </summary>
        /// <param name="n">The value to check.</param>
        /// <param name="variableName">The variable name of the value to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="variableName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="n"/> &lt; 0.</exception>
        public static void CheckNonNegative(int n, string variableName)
        {
            CheckForNullReference(variableName, "variableName");

            if (n < 0)
                throw new ArgumentException(SR.ExceptionArgumentNegative, variableName);
        }

        /// <summary>
        /// Checks if a value is within a specified range.
        /// </summary>
        /// <param name="argumentValue">Value to be checked.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="variableName">Variable name of value to be checked.</param>
        /// <remarks>Checks if <paramref name="min"/> &lt;= <paramref name="argumentValue"/> &lt;= <paramref name="max"/></remarks>
        /// <exception cref="ArgumentNullException"><paramref name="variableName"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="argumentValue"/> is not within the
        /// specified range.</exception>
        public static void CheckArgumentRange(int argumentValue, int min, int max, string variableName)
        {
            CheckForNullReference(variableName, "variableName");

            if (argumentValue < min || argumentValue > max)
                throw new ArgumentOutOfRangeException(String.Format(SR.ExceptionArgumentOutOfRange, argumentValue, min, max, variableName));
        }

        /// <summary>
        /// Checks if an index is within a specified range.
        /// </summary>
        /// <param name="index">Index to be checked</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="obj">Object being indexed.</param>
        /// <remarks>Checks if <paramref name="min"/> &lt;= <paramref name="index"/> &lt;= <paramref name="max"/>.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <b>null</b>.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is not within the
        /// specified range.</exception>
        public static void CheckIndexRange(int index, int min, int max, object obj)
        {
            CheckForNullReference(obj, "obj");

            if (index < min || index > max)
                throw new IndexOutOfRangeException(String.Format(SR.ExceptionIndexOutOfRange, index, min, max, obj.GetType().Name));
        }

        /// <summary>
        /// Checks if a field or property is null.
        /// </summary>
        /// <param name="variable">Field or property to be checked.</param>
        /// <param name="variableName">Name of field or property to be checked.</param>
        /// <remarks>Use this method in your classes to verify that the object
        /// is not in an invalid state by checking that various fields and/or properties
        /// have been set, i.e., are not null.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="variableName"/> is <b>null</b>.</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="variable"/> is <b>null</b>.</exception>
        public static void CheckMemberIsSet(object variable, string variableName)
        {
            CheckForNullReference(variableName, "variableName");

            if (variable == null)
                throw new InvalidOperationException(String.Format(SR.ExceptionMemberNotSet, variableName));
        }

        /// <summary>
        /// Checks if a field or property is null.
        /// </summary>
        /// <param name="variable">Field or property to be checked.</param>
        /// <param name="variableName">Name of field or property to be checked.</param>
        /// <param name="detailedMessage">A more detailed and informative message describing
        /// why the object is in an invalid state.</param>
        /// <remarks>Use this method in your classes to verify that the object
        /// is not in an invalid state by checking that various fields and/or properties
        /// have been set, i.e., are not null.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="variableName"/> is <b>null</b>.</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="variable"/> is <b>null</b>.</exception>
        public static void CheckMemberIsSet(object variable, string variableName, string detailedMessage)
        {
            CheckForNullReference(variableName, "variableName");
            CheckForNullReference(detailedMessage, "detailedMessage");

            if (variable == null)
                throw new InvalidOperationException(String.Format(SR.ExceptionMemberNotSetVerbose, variableName, detailedMessage));
        }
    }
}
