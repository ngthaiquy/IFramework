﻿using System;
using System.Data;
using System.Threading.Tasks;
using IFramework.Config;
using IFramework.Exceptions;
using IFramework.Infrastructure.Logging;
using IFramework.IoC;

namespace IFramework.Infrastructure
{
    public class ApiResult
    {
        public ApiResult()
        {
            Success = true;
            ErrorCode = 0;
        }

        public ApiResult(object errorCode, string message = null)
        {
            ErrorCode = errorCode;
            Message = message;
            Success = false;
        }

        public bool Success { get; set; }
        public object ErrorCode { get; set; }
        public string Message { get; set; }
    }

    public class ApiResult<TResult> : ApiResult
    {
        public ApiResult()
        {
            Success = true;
        }

        public ApiResult(TResult result)
            : this()
        {
            Result = result;
        }

        public ApiResult(object errorCode, string message = null)
            : base(errorCode, message) { }

        public TResult Result { get; set; }
    }

    public static class ExceptionManager
    {
        private static readonly ILogger _logger = IoCFactory.IsInit()
                                                      ? IoCFactory.Resolve<ILoggerFactory>().Create(typeof(ExceptionManager))
                                                      : null;

        private static string _UnKnownMessage = ErrorCode.UnknownError.ToString();

        public static void SetUnKnownMessage(string unknownMessage)
        {
            _UnKnownMessage = unknownMessage;
        }


        private static string GetUnknownErrorMessage(Exception ex)
        {
            var unknownErrorMessage = _UnKnownMessage;
            var compliationSection = Configuration.GetCompliationSection();
            if (compliationSection != null && compliationSection.Debug)
            {
                unknownErrorMessage = ex.Message;
            }
            return unknownErrorMessage;
        }


        private static string GetExceptionMessage(Exception ex)
        {
            return GetUnknownErrorMessage(ex);
        }

        public static async Task<ApiResult<T>> ProcessAsync<T>(Func<Task<T>> func,
                                                               bool continueOnCapturedContext = false,
                                                               bool needRetry = false,
                                                               int retryCount = 50,
                                                               Func<Exception, string> getExceptionMessage = null)
        {
            ApiResult<T> apiResult = null;
            getExceptionMessage = getExceptionMessage ?? GetExceptionMessage;

            do
            {
                try
                {
                    var result = await func().ConfigureAwait(continueOnCapturedContext);
                    apiResult = new ApiResult<T>(result);
                    needRetry = false;
                }
                catch (Exception ex)
                {
                    if (!(ex is OptimisticConcurrencyException) || !needRetry)
                    {
                        var baseException = ex.GetBaseException();
                        if (baseException is DomainException)
                        {
                            var sysException = baseException as DomainException;
                            apiResult = new ApiResult<T>(sysException.ErrorCode, getExceptionMessage(sysException));
                            _logger?.Debug(ex);
                        }
                        else
                        {
                            apiResult = new ApiResult<T>(ErrorCode.UnknownError, getExceptionMessage(ex));
                            _logger?.Error(ex);
                        }
                        needRetry = false;
                    }
                }
            } while (needRetry && retryCount-- > 0);
            return apiResult;

            #region Old Method for .net 4

            /*
             * old method for .net 4
            return func().ContinueWith<Task<ApiResult<T>>>(t =>
            {
                ApiResult<T> apiResult = null;
                try
                {
                    if (t.IsFaulted)
                    {
                        throw t.Exception.GetBaseException();
                    }
                    needRetry = false;
                    apiResult = new ApiResult<T>(t.Result);
                }
                catch (Exception ex)
                {
                    if (!(ex is OptimisticConcurrencyException) || !needRetry)
                    {
                        var baseException = ex.GetBaseException();
                        if (baseException is SysException)
                        {
                            var sysException = baseException as SysException;
                            apiResult = new ApiResult<T>(sysException.ErrorCode, sysException.Message);
                        }
                        else
                        {
                            apiResult = new ApiResult<T>(ErrorCode.UnknownError, baseException.Message);
                            _logger.Error(ex);
                        }
                        needRetry = false;
                    }
                }
                if (needRetry)
                {
                    return ProcessAsync(func, needRetry);
                }

                return Task.FromResult(apiResult);
            }).Unwrap();
            */

            #endregion
        }

        public static async Task<ApiResult> ProcessAsync(Func<Task> func,
                                                         bool continueOnCapturedContext = false,
                                                         bool needRetry = false,
                                                         int retryCount = 50,
                                                         Func<Exception, string> getExceptionMessage = null)
        {
            getExceptionMessage = getExceptionMessage ?? GetExceptionMessage;
            ApiResult apiResult = null;
            do
            {
                try
                {
                    await func().ConfigureAwait(continueOnCapturedContext);
                    needRetry = false;
                    apiResult = new ApiResult();
                }
                catch (Exception ex)
                {
                    if (!(ex is OptimisticConcurrencyException) || !needRetry)
                    {
                        var baseException = ex.GetBaseException();
                        if (baseException is DomainException)
                        {
                            var sysException = baseException as DomainException;
                            apiResult = new ApiResult(sysException.ErrorCode, getExceptionMessage(sysException));
                        }
                        else
                        {
                            apiResult = new ApiResult(ErrorCode.UnknownError, getExceptionMessage(ex));
                            _logger?.Error(ex);
                        }
                        needRetry = false;
                    }
                }
            } while (needRetry && retryCount-- > 0);
            return apiResult;

            #region Old Method for .net 4

            //return func().ContinueWith<Task<ApiResult>>(t =>
            //{
            //    ApiResult apiResult = null;
            //    try
            //    {
            //        if (t.IsFaulted)
            //        {
            //            throw t.Exception.GetBaseException();
            //        }
            //        needRetry = false;
            //        apiResult = new ApiResult();
            //    }
            //    catch (Exception ex)
            //    {
            //        if (!(ex is OptimisticConcurrencyException) || !needRetry)
            //        {
            //            var baseException = ex.GetBaseException();
            //            if (baseException is SysException)
            //            {
            //                var sysException = baseException as SysException;
            //                apiResult = new ApiResult(sysException.ErrorCode, sysException.Message);
            //            }
            //            else
            //            {
            //                apiResult = new ApiResult(ErrorCode.UnknownError, baseException.Message);
            //                _logger.Error(ex);
            //            }
            //            needRetry = false;
            //        }
            //    }
            //    if (needRetry)
            //    {
            //        return ProcessAsync(func, needRetry);
            //    }

            //    return Task.FromResult(apiResult);
            //}).Unwrap();

            #endregion
        }

        public static ApiResult Process(Action action,
                                        bool needRetry = false,
                                        int retryCount = 50,
                                        Func<Exception, string> getExceptionMessage = null)
        {
            ApiResult apiResult = null;
            getExceptionMessage = getExceptionMessage ?? GetExceptionMessage;
            do
            {
                try
                {
                    action();
                    apiResult = new ApiResult();
                    needRetry = false;
                }
                catch (Exception ex)
                {
                    if (!(ex is OptimisticConcurrencyException) || !needRetry)
                    {
                        var baseException = ex.GetBaseException();
                        if (baseException is DomainException)
                        {
                            var sysException = baseException as DomainException;
                            apiResult = new ApiResult(sysException.ErrorCode, getExceptionMessage(sysException));
                        }
                        else
                        {
                            apiResult = new ApiResult(ErrorCode.UnknownError, getExceptionMessage(ex));
                            _logger?.Error(ex);
                        }
                        needRetry = false;
                    }
                }
            } while (needRetry && retryCount-- > 0);
            return apiResult;
        }

        public static ApiResult<T> Process<T>(Func<T> func,
                                              bool needRetry = false,
                                              int retryCount = 50,
                                              Func<Exception, string> getExceptionMessage = null)
        {
            ApiResult<T> apiResult = null;
            getExceptionMessage = getExceptionMessage ?? GetExceptionMessage;
            do
            {
                try
                {
                    var result = func();
                    needRetry = false;
                    if (result != null)
                    {
                        apiResult = new ApiResult<T>(result);
                    }
                    else
                    {
                        apiResult = new ApiResult<T>();
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is OptimisticConcurrencyException) || !needRetry)
                    {
                        var baseException = ex.GetBaseException();
                        if (baseException is DomainException)
                        {
                            var sysException = baseException as DomainException;
                            apiResult = new ApiResult<T>(sysException.ErrorCode, getExceptionMessage(sysException));
                        }
                        else
                        {
                            apiResult = new ApiResult<T>(ErrorCode.UnknownError, getExceptionMessage(ex));
                            _logger?.Error(ex);
                        }
                        needRetry = false;
                    }
                }
            } while (needRetry && retryCount-- > 0);
            return apiResult;
        }
    }
}